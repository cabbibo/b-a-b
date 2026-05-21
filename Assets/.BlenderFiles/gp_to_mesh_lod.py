"""
gp_to_mesh_lod.py  -  V2 (tubes + vertex colors + stroke dropping)

Convert Grease Pencil strokes into a tube mesh with curvature-weighted vertex
distribution and a fixed total vertex budget.

Parameters
  Total Stroke Verts Total spine sample count across all strokes.
                     True mesh vert count = Total Stroke Verts * Ring Verts.
  Curve Importance   0 = uniform by arc length; higher = more verts on bends.
  Tube Scale         Multiplier on per-point thickness to get world-space radius.
  Ring Verts         Vertices around each tube's cross-section (>= 3).

If the budget doesn't allow >= 2 spine samples per stroke, the shortest/thinnest
strokes are dropped (scored by arc_length * mean_thickness).
"""

bl_info = {
    "name": "GP -> Mesh LOD",
    "author": "Isaac",
    "version": (2, 0),
    "blender": (3, 6, 0),
    "location": "View3D > Sidebar > GP LOD",
    "description": "Convert Grease Pencil strokes to a tube mesh with curvature-weighted verts.",
    "category": "Object",
}

import bpy
import numpy as np

COLOR_ATTR_NAME = "Col"

# LOD bake schedule. Each entry: (total_verts multiplier, ring_verts).
# LOD0 keeps the full budget and tube cross-section; later LODs progressively
# strip spine samples and shrink the ring (eventually down to a flat ribbon).
LOD_SCHEDULE = [
    (1.00, 6),  # LOD0 - full tube
    (0.50, 4),  # LOD1
    (0.25, 2),  # LOD2 - ribbon
    (0.10, 2),  # LOD3 - sparse ribbon
]

# Shared state for the modal bake-set, read by the sidebar panel for the
# in-panel progress bar. Module-level so the panel can see it without needing
# a registered Scene property.
_BAKE_PROGRESS = {"active": False, "step": 0, "total": 0, "msg": ""}


# --------------------------------------------------------------------------- #
# Stroke data readers                                                         #
# --------------------------------------------------------------------------- #
def _log_classic_point_props(obj):
    """Print all RNA properties available on a classic GP stroke point."""
    for layer in obj.data.layers:
        if layer.active_frame and layer.active_frame.strokes:
            s = layer.active_frame.strokes[0]
            if s.points:
                p = s.points[0]
                print("[gp_to_mesh_lod] classic point properties:")
                for prop in p.bl_rna.properties:
                    if prop.identifier == "rna_type":
                        continue
                    extra = ""
                    try:
                        v = getattr(p, prop.identifier)
                        extra = f" = {v!r}"
                    except Exception:
                        pass
                    print(f"    - {prop.identifier!r}  ({prop.type}){extra}")
                return


def _find_classic_color_prop(obj):
    """Find a per-point RGBA color property name on classic GP points.
    Returns property identifier or None."""
    for layer in obj.data.layers:
        if layer.active_frame and layer.active_frame.strokes:
            s = layer.active_frame.strokes[0]
            if not s.points:
                continue
            p = s.points[0]
            rna = {prop.identifier: prop for prop in p.bl_rna.properties}
            if "vertex_color" in rna:
                return "vertex_color"
            for name, prop in rna.items():
                if "color" in name.lower() and getattr(prop, "array_length", 0) in (3, 4):
                    return name
            return None
    return None


def _log_drawing_attrs(drawing):
    """Print every attribute on a v3 Drawing along with its domain/type."""
    print("[gp_to_mesh_lod] v3 drawing attributes:")
    for a in drawing.attributes:
        print(f"    - {a.name!r}  domain={a.domain}  type={a.data_type}")


def _find_v3_color_attr(drawing):
    """Find a color attribute on a v3 Drawing. Returns name or None."""
    attrs = drawing.attributes
    for n in ("vertex_color", "Col", "color"):
        if n in attrs:
            return n
    for a in attrs:
        if "color" in a.name.lower() and a.domain == 'POINT':
            return a.name
    return None


def _read_classic_stroke(stroke, color_prop):
    """Return dict {pos (N,3), col (N,4), thick (N,)} for one classic GP stroke,
    or None if too short."""
    n = len(stroke.points)
    if n < 2:
        return None
    pos_flat = np.empty(n * 3, dtype=np.float32)
    stroke.points.foreach_get("co", pos_flat)
    pos = pos_flat.reshape(n, 3).astype(np.float64)

    pressure = np.empty(n, dtype=np.float32)
    try:
        stroke.points.foreach_get("pressure", pressure)
    except Exception:
        pressure.fill(1.0)
    line_width = float(getattr(stroke, "line_width", 100.0))
    thick = pressure.astype(np.float64) * line_width / 1000.0

    col = np.ones((n, 4), dtype=np.float64)
    if color_prop:
        col_flat = np.empty(n * 4, dtype=np.float32)
        try:
            stroke.points.foreach_get(color_prop, col_flat)
            col = col_flat.reshape(n, 4).astype(np.float64)
        except Exception:
            pass

    return {"pos": pos, "col": col, "thick": thick}


def _read_v3_drawing(drawing, color_attr_name):
    """Return list of {pos, col, thick} for every stroke in a v3 Drawing.
    Tries bulk attribute reads first; falls back to per-point if needed."""
    out = []
    attrs = drawing.attributes

    def _bulk(name, comp_count, comp_key):
        try:
            data = attrs[name].data
        except (KeyError, AttributeError):
            return None
        flat = np.empty(len(data) * comp_count, dtype=np.float32)
        try:
            data.foreach_get(comp_key, flat)
        except (TypeError, AttributeError):
            return None
        if comp_count == 1:
            return flat
        return flat.reshape(-1, comp_count)

    all_pos    = _bulk("position", 3, "vector")
    all_radius = _bulk("radius",   1, "value")
    all_color  = _bulk(color_attr_name, 4, "color") if color_attr_name else None

    cursor = 0
    for s in drawing.strokes:
        n = len(s.points)
        if n < 2:
            cursor += n
            continue

        if all_pos is not None:
            pos = all_pos[cursor:cursor + n].astype(np.float64)
        else:
            pos = np.empty((n, 3), dtype=np.float64)
            for i, p in enumerate(s.points):
                co = p.position
                pos[i, 0] = co[0]; pos[i, 1] = co[1]; pos[i, 2] = co[2]

        if all_radius is not None:
            thick = all_radius[cursor:cursor + n].astype(np.float64)
        else:
            thick = np.empty(n, dtype=np.float64)
            for i, p in enumerate(s.points):
                thick[i] = getattr(p, "radius", 0.01)

        if all_color is not None:
            col = all_color[cursor:cursor + n].astype(np.float64)
        else:
            col = np.ones((n, 4), dtype=np.float64)
            for i, p in enumerate(s.points):
                vc = getattr(p, "vertex_color", None)
                if vc is not None:
                    col[i] = [vc[0], vc[1], vc[2], vc[3]]

        out.append({"pos": pos, "col": col, "thick": thick})
        cursor += n

    return out


def gather_stroke_data_world(obj):
    """Returns (raw, stats). raw is a list of dicts with world-space positions,
    per-point colors, and per-point thicknesses. stats tracks how many strokes
    were encountered and how many were dropped at the read stage."""
    M = np.array(obj.matrix_world)
    raw = []
    stats = {"strokes_total": 0, "strokes_short": 0}
    if obj.type == 'GPENCIL':
        _log_classic_point_props(obj)
        color_prop = _find_classic_color_prop(obj)
        print(f"[gp_to_mesh_lod] classic color property -> {color_prop!r}")
        for layer in obj.data.layers:
            frame = layer.active_frame
            if frame is None:
                continue
            for s in frame.strokes:
                stats["strokes_total"] += 1
                d = _read_classic_stroke(s, color_prop)
                if d is None:
                    stats["strokes_short"] += 1
                    continue
                raw.append(d)
    elif obj.type == 'GREASEPENCIL':
        for layer in obj.data.layers:
            frame = layer.current_frame()
            if frame is None:
                continue
            drawing = frame.drawing
            _log_drawing_attrs(drawing)
            color_attr = _find_v3_color_attr(drawing)
            print(f"[gp_to_mesh_lod] v3 color attribute -> {color_attr!r}")
            n_before = len(raw)
            n_strokes = len(drawing.strokes)
            raw.extend(_read_v3_drawing(drawing, color_attr))
            n_added = len(raw) - n_before
            stats["strokes_total"] += n_strokes
            stats["strokes_short"] += n_strokes - n_added
    else:
        raise RuntimeError(f"Active object {obj!r} is not a Grease Pencil object.")

    # Transform positions to world space (matrix_world has uniform-ish scale assumed;
    # thickness is left unscaled — tube_scale calibrates).
    for d in raw:
        pos = d["pos"]
        homo = np.hstack([pos, np.ones((len(pos), 1))])
        d["pos"] = (homo @ M.T)[:, :3]
    return raw, stats


# --------------------------------------------------------------------------- #
# Math helpers                                                                #
# --------------------------------------------------------------------------- #
def stroke_weights(pts, thick, curve_importance, thickness_delta_importance):
    deltas  = np.diff(pts, axis=0)
    seg_len = np.linalg.norm(deltas, axis=1)
    safe    = np.where(seg_len > 0, seg_len, 1.0)
    tangs   = deltas / safe[:, None]
    if len(tangs) >= 2:
        dots    = np.clip((tangs[:-1] * tangs[1:]).sum(axis=1), -1.0, 1.0)
        turning = np.arccos(dots)
    else:
        turning = np.zeros(0)
    seg_turn = np.zeros_like(seg_len)
    if turning.size:
        seg_turn[:-1] += 0.5 * turning
        seg_turn[1:]  += 0.5 * turning

    # |delta thickness| per segment
    if len(thick) >= 2:
        seg_dthick = np.abs(np.diff(thick))
    else:
        seg_dthick = np.zeros_like(seg_len)

    seg_w = (seg_len
             + curve_importance * seg_turn
             + thickness_delta_importance * seg_dthick)
    cum_w = np.concatenate([[0.0], np.cumsum(seg_w)])
    arc_len = float(seg_len.sum())
    return cum_w, cum_w[-1], arc_len


def compute_resample_keys(cum_w, n):
    n = max(2, n)
    targets = np.linspace(0.0, cum_w[-1], n)
    idx = np.clip(np.searchsorted(cum_w, targets, side='right') - 1, 0, len(cum_w) - 2)
    w_lo, w_hi = cum_w[idx], cum_w[idx + 1]
    span = np.where((w_hi - w_lo) > 0, w_hi - w_lo, 1.0)
    t = (targets - w_lo) / span
    return idx, t


def interp_along(values, idx, t):
    lo = values[idx]
    hi = values[idx + 1]
    if values.ndim == 1:
        return lo + t * (hi - lo)
    return lo + t[:, None] * (hi - lo)


def largest_remainder(weights, total, min_each=2):
    n = len(weights)
    if n == 0:
        return []
    if total < n * min_each:
        return [min_each] * n
    rem = total - n * min_each
    w = np.asarray(weights, dtype=np.float64)
    ws = w.sum() or 1.0
    exact = rem * w / ws
    floor = np.floor(exact).astype(int)
    leftover = rem - int(floor.sum())
    frac = exact - floor
    extra = np.zeros(n, dtype=int)
    if leftover > 0:
        extra[np.argsort(-frac)[:leftover]] = 1
    return (np.full(n, min_each) + floor + extra).tolist()


# --------------------------------------------------------------------------- #
# Tube construction                                                           #
# --------------------------------------------------------------------------- #
def _frames_along(spine):
    """Parallel-transport frames along an open polyline.
    Returns (tangents, normals, binormals) each (S,3)."""
    S = len(spine)
    deltas = np.diff(spine, axis=0)
    seg_t = deltas / np.maximum(np.linalg.norm(deltas, axis=1, keepdims=True), 1e-12)
    tangents = np.empty_like(spine)
    tangents[0] = seg_t[0]
    tangents[-1] = seg_t[-1]
    if S > 2:
        avg = seg_t[:-1] + seg_t[1:]
        tangents[1:-1] = avg / np.maximum(np.linalg.norm(avg, axis=1, keepdims=True), 1e-12)

    # Initial normal: perpendicular to t0
    t0 = tangents[0]
    ref = np.array([0.0, 0.0, 1.0]) if abs(t0[2]) < 0.9 else np.array([1.0, 0.0, 0.0])
    n0 = np.cross(t0, ref)
    n0 /= max(np.linalg.norm(n0), 1e-12)

    normals = np.empty_like(spine)
    normals[0] = n0
    for i in range(1, S):
        t_prev, t_cur = tangents[i - 1], tangents[i]
        axis = np.cross(t_prev, t_cur)
        sin_a = np.linalg.norm(axis)
        cos_a = float(np.clip(np.dot(t_prev, t_cur), -1.0, 1.0))
        n_prev = normals[i - 1]
        if sin_a < 1e-8:
            n_new = n_prev
        else:
            axis /= sin_a
            # Rodrigues rotation of n_prev around axis by angle (sin_a, cos_a)
            n_new = (n_prev * cos_a
                     + np.cross(axis, n_prev) * sin_a
                     + axis * float(np.dot(axis, n_prev)) * (1.0 - cos_a))
        # Re-project onto plane perpendicular to current tangent
        n_new = n_new - t_cur * float(np.dot(n_new, t_cur))
        nrm = np.linalg.norm(n_new)
        normals[i] = n_new / nrm if nrm > 1e-12 else n_prev

    binormals = np.cross(tangents, normals)
    return tangents, normals, binormals


def build_tube(spine, radii, R):
    """Return (verts (S*R,3), quads, start_cap, end_cap).
    R == 2 -> ribbon mode (single quad per segment, no caps).
    R >= 3 -> tube mode (ring of quads + n-gon end caps)."""
    S = len(spine)
    _, normals, binormals = _frames_along(spine)

    angles = np.linspace(0.0, 2.0 * np.pi, R, endpoint=False)
    cos_a = np.cos(angles)
    sin_a = np.sin(angles)

    ring = (cos_a[None, :, None] * normals[:, None, :]
            + sin_a[None, :, None] * binormals[:, None, :])
    verts = spine[:, None, :] + radii[:, None, None] * ring
    verts_flat = verts.reshape(S * R, 3)

    if R < 3:
        # Ribbon: one quad per segment connecting (+normal, -normal) pairs.
        s_idx = np.arange(S - 1)
        v00 = s_idx * R
        v01 = s_idx * R + 1
        v11 = (s_idx + 1) * R + 1
        v10 = (s_idx + 1) * R
        quads = np.stack([v00, v01, v11, v10], axis=1)
        return verts_flat, quads, None, None

    s_idx = np.repeat(np.arange(S - 1), R)
    r_idx = np.tile(np.arange(R), S - 1)
    r2 = (r_idx + 1) % R
    v00 = s_idx * R + r_idx
    v01 = s_idx * R + r2
    v11 = (s_idx + 1) * R + r2
    v10 = (s_idx + 1) * R + r_idx
    quads = np.stack([v00, v01, v11, v10], axis=1)

    start_cap = tuple(range(R - 1, -1, -1))
    end_cap = tuple(range((S - 1) * R, S * R))
    return verts_flat, quads, start_cap, end_cap


# --------------------------------------------------------------------------- #
# Conversion                                                                  #
# --------------------------------------------------------------------------- #
def run_conversion(gp_obj, total_verts, curve_importance, tube_scale, ring_verts,
                   thickness_delta_importance, output_suffix="_LOD0"):
    R = max(2, int(ring_verts))
    # total_verts is the total *spine* sample count across all strokes.
    # True mesh vert count = total_verts * R (plus zero for caps, which are n-gons).
    spine_budget = max(3, int(total_verts))

    raw, stats = gather_stroke_data_world(gp_obj)
    if not raw:
        raise RuntimeError("No usable strokes on active object.")

    # Zero-thickness fallback: if a stroke has all-zero per-point radius
    # (Blender's line tool stores radius=0 even though it renders visibly),
    # substitute the median nonzero thickness across the whole drawing.
    all_thick_nonzero = []
    for d in raw:
        if d["thick"].size:
            all_thick_nonzero.append(d["thick"][d["thick"] > 1e-9])
    pooled = np.concatenate(all_thick_nonzero) if all_thick_nonzero else np.array([])
    fallback = float(np.median(pooled)) if pooled.size else 0.01
    fallback_count = 0
    for d in raw:
        if d["thick"].size and float(d["thick"].max()) < 1e-9:
            d["thick"] = np.full_like(d["thick"], fallback)
            d["thick_fallback"] = True
            fallback_count += 1
        else:
            d["thick_fallback"] = False
    stats["thick_fallback"] = fallback_count
    if fallback_count:
        print(f"[gp_to_mesh_lod] {fallback_count} stroke(s) had zero thickness "
              f"-> filled with median {fallback:.4f}")

    enriched = []
    zero_weight = 0
    for d in raw:
        pos = d["pos"]
        cum_w, tot_w, arc_len = stroke_weights(
            pos, d["thick"], curve_importance, thickness_delta_importance)
        if tot_w <= 0 or arc_len <= 0:
            zero_weight += 1
            continue
        mean_thick = float(np.mean(d["thick"])) if d["thick"].size else 0.0
        score = arc_len * max(mean_thick, 1e-9)
        d.update({"cum_w": cum_w, "tot_w": tot_w, "arc_len": arc_len,
                  "mean_thick": mean_thick, "score": score})
        enriched.append(d)
    stats["zero_weight"] = zero_weight

    if not enriched:
        raise RuntimeError("All strokes had zero weight.")

    # Decide kept vs dropped by score. Min spine samples per stroke = 3.
    max_n = max(1, spine_budget // 3)
    n_enriched = len(enriched)
    if n_enriched > max_n:
        scores = np.array([d["score"] for d in enriched])
        keep_set = set(np.argsort(-scores)[:max_n].tolist())
    else:
        keep_set = set(range(n_enriched))
    dropped = n_enriched - len(keep_set)
    stats["dropped_by_budget"] = dropped
    stats["kept"] = len(keep_set)

    # Per-stroke detail (capped, so the console doesn't drown).
    MAX_LOG = 40
    print(f"[gp_to_mesh_lod] per-stroke detail (showing up to {MAX_LOG} of {n_enriched}):")
    for i, d in enumerate(enriched[:MAX_LOG]):
        flag = "KEEP" if i in keep_set else "DROP"
        thick = d["thick"]
        t_min = float(thick.min()) if thick.size else 0.0
        t_max = float(thick.max()) if thick.size else 0.0
        # Effective turning across whole stroke (rough straightness check)
        pos = d["pos"]
        if len(pos) >= 3:
            deltas = np.diff(pos, axis=0)
            tn = deltas / np.maximum(np.linalg.norm(deltas, axis=1, keepdims=True), 1e-12)
            total_turn = float(np.sum(np.arccos(np.clip(
                (tn[:-1] * tn[1:]).sum(axis=1), -1.0, 1.0))))
        else:
            total_turn = 0.0
        fb = " FB" if d.get("thick_fallback") else ""
        print(f"  {flag}{fb} #{i}: pts={len(pos)} "
              f"arc_len={d['arc_len']:.4f} "
              f"thick=[{t_min:.4f},{t_max:.4f}] "
              f"total_turn={total_turn:.3f}rad "
              f"score={d['score']:.4f}")
    if n_enriched > MAX_LOG:
        print(f"  ... and {n_enriched - MAX_LOG} more")

    print(f"[gp_to_mesh_lod] stroke breakdown: "
          f"{stats['strokes_total']} found  "
          f"- {stats['strokes_short']} too short (<2 pts)  "
          f"- {stats['zero_weight']} zero weight  "
          f"- {stats['dropped_by_budget']} dropped by budget  "
          f"= {stats['kept']} kept.")

    enriched = [d for i, d in enumerate(enriched) if i in keep_set]

    # Per-stroke allocation is by arc length ONLY. The importance values
    # (curvature, thickness-delta) drive *where* samples land within each
    # stroke via cum_w, but don't steal budget from neighbors.
    spine_alloc = largest_remainder([d["arc_len"] for d in enriched], spine_budget, min_each=3)

    all_verts, all_faces, all_colors = [], [], []
    vbase = 0
    for d, n_spine in zip(enriched, spine_alloc):
        idx, t = compute_resample_keys(d["cum_w"], n_spine)
        spine  = interp_along(d["pos"],   idx, t)
        thick  = interp_along(d["thick"], idx, t)
        colors = interp_along(d["col"],   idx, t)
        radii  = thick * float(tube_scale)

        verts, quads, start_cap, end_cap = build_tube(spine, radii, R)
        all_verts.append(verts)

        # Broadcast each spine sample's color across the R verts of its ring
        all_colors.append(np.repeat(colors, R, axis=0))

        all_faces.extend((quads + vbase).tolist())
        if start_cap is not None:
            all_faces.append(tuple(i + vbase for i in start_cap))
        if end_cap is not None:
            all_faces.append(tuple(i + vbase for i in end_cap))
        vbase += verts.shape[0]

    verts_concat = np.concatenate(all_verts, axis=0)
    colors_concat = np.concatenate(all_colors, axis=0)

    # Find or create output object/mesh by deterministic name.
    mesh_name = gp_obj.name + output_suffix
    out_obj = bpy.data.objects.get(mesh_name)
    if out_obj is None or out_obj.type != 'MESH':
        me = bpy.data.meshes.new(mesh_name)
        out_obj = bpy.data.objects.new(mesh_name, me)
        coll = gp_obj.users_collection[0] if gp_obj.users_collection else bpy.context.scene.collection
        coll.objects.link(out_obj)
    else:
        me = out_obj.data
        me.clear_geometry()
        while me.color_attributes:
            me.color_attributes.remove(me.color_attributes[0])

    # Parent under the GP object so LODs nest in the outliner. Verts are in
    # world space, so cancel the parent's world transform with parent_inverse
    # to keep the mesh at world origin where its verts already live.
    if out_obj.parent != gp_obj:
        out_obj.parent = gp_obj
        out_obj.matrix_parent_inverse = gp_obj.matrix_world.inverted()

    me.from_pydata(verts_concat.tolist(), [], all_faces)
    me.update()

    # Shade smooth (set use_smooth on every polygon in one bulk write).
    if len(me.polygons):
        smooth_flags = np.ones(len(me.polygons), dtype=bool)
        me.polygons.foreach_set("use_smooth", smooth_flags)
        me.update()

    # Vertex colors
    ca = me.color_attributes.new(name=COLOR_ATTR_NAME, type='FLOAT_COLOR', domain='POINT')
    ca.data.foreach_set("color", colors_concat.astype(np.float32).ravel())

    print(f"[gp_to_mesh_lod] '{out_obj.name}': "
          f"{len(verts_concat)} mesh verts "
          f"({sum(spine_alloc)} spine x {R} ring), "
          f"{len(enriched)} strokes kept"
          + (f" ({dropped} dropped)" if dropped else "")
          + f", tube_scale={tube_scale}, ci={curve_importance}.")
    return out_obj


# --------------------------------------------------------------------------- #
# Blender types                                                               #
# --------------------------------------------------------------------------- #
def _is_gp(obj):
    return obj is not None and obj.type in {'GPENCIL', 'GREASEPENCIL'}


def _calc_recommendations(gp_obj):
    """Inspect the GP source and return a recommended total_verts.

    Looks at total source point count and curvature density (radians of
    turning per source point) to scale the recommendation between 0.5x
    (mostly straight) and 1.0x (very curvy) of the source count.

    Returns (source_points, total_turn_rad, turn_per_point, recommended)."""
    raw, _ = gather_stroke_data_world(gp_obj)
    if not raw:
        return 0, 0.0, 0.0, 0
    source_pts = sum(len(d["pos"]) for d in raw)
    total_turn = 0.0
    for d in raw:
        pos = d["pos"]
        if len(pos) < 3:
            continue
        deltas  = np.diff(pos, axis=0)
        seg_len = np.linalg.norm(deltas, axis=1)
        tangs   = deltas / np.maximum(seg_len[:, None], 1e-12)
        dots    = np.clip((tangs[:-1] * tangs[1:]).sum(axis=1), -1.0, 1.0)
        total_turn += float(np.sum(np.arccos(dots)))
    turn_per_point = total_turn / max(1, source_pts)
    # 0.3 rad/pt (~17 deg avg turn per source point) is "definitely curvy"
    norm = min(1.0, turn_per_point / 0.3)
    factor = 0.5 + 0.5 * norm
    recommended = max(100, int(round(source_pts * factor)))
    return source_pts, total_turn, turn_per_point, recommended


def _lod_outputs_for(gp_obj):
    """All existing mesh outputs whose name matches '<gp>_LOD*'."""
    prefix = gp_obj.name + "_LOD"
    return [o for o in bpy.data.objects if o.name.startswith(prefix) and o.type == 'MESH']


def _delete_lod_outputs(gp_obj):
    for o in _lod_outputs_for(gp_obj):
        mesh = o.data
        bpy.data.objects.remove(o, do_unlink=True)
        if mesh is not None and mesh.users == 0:
            bpy.data.meshes.remove(mesh)


class GPLODSettings(bpy.types.PropertyGroup):
    total_verts: bpy.props.IntProperty(
        name="Total Stroke Verts",
        default=1000, min=2, soft_max=200000,
        description="Total spine sample count for LOD0. True mesh verts = this x Ring Verts.",
    )
    curve_importance: bpy.props.FloatProperty(
        name="Curve Importance",
        default=5.0, min=0.0, soft_max=50.0,
        description="0 = uniform along arc length. Higher = more verts on curvy regions.",
    )
    thickness_delta_importance: bpy.props.FloatProperty(
        name="Thickness Delta Importance",
        default=10.0, min=0.0, soft_max=200.0,
        description="Higher = more verts where stroke thickness changes (good for tapers on otherwise-straight strokes).",
    )
    tube_scale: bpy.props.FloatProperty(
        name="Tube Scale",
        default=1.0, min=0.0, soft_max=10.0,
        description="Multiplier on per-point thickness to get world-space tube radius",
    )
    ring_verts: bpy.props.IntProperty(
        name="Ring Verts",
        default=6, min=2, soft_max=32,
        description="Cross-section verts for the single Bake. 2 = flat ribbon (no caps). The LOD Set ignores this and follows LOD_SCHEDULE.",
    )
    lod_size_expansion: bpy.props.FloatProperty(
        name="Min LOD Size Multiplier",
        default=2.0, min=1.0, soft_max=8.0,
        description="Tube radius is scaled from 1x at LOD0 up to this value at the lowest LOD, to mask visual gaps when verts are sparse.",
    )


class OBJECT_OT_gp_to_mesh_lod_bake(bpy.types.Operator):
    """Bake the LOD0 mesh only, using the current Ring Verts setting."""
    bl_idname = "object.gp_to_mesh_lod_bake"
    bl_label = "Bake LOD0"
    bl_options = {'REGISTER', 'UNDO'}

    total_verts: bpy.props.IntProperty(name="Total Stroke Verts", default=1000, min=2, soft_max=200000)
    curve_importance: bpy.props.FloatProperty(name="Curve Importance", default=5.0, min=0.0, soft_max=50.0)
    thickness_delta_importance: bpy.props.FloatProperty(name="Thickness Delta Importance", default=10.0, min=0.0, soft_max=200.0)
    tube_scale: bpy.props.FloatProperty(name="Tube Scale", default=1.0, min=0.0, soft_max=10.0)
    ring_verts: bpy.props.IntProperty(name="Ring Verts", default=6, min=2, soft_max=32)

    @classmethod
    def poll(cls, context):
        return _is_gp(context.active_object)

    def invoke(self, context, event):
        s = context.active_object.gp_lod_settings
        self.total_verts                = s.total_verts
        self.curve_importance           = s.curve_importance
        self.thickness_delta_importance = s.thickness_delta_importance
        self.tube_scale                 = s.tube_scale
        self.ring_verts                 = s.ring_verts
        return self.execute(context)

    def execute(self, context):
        gp_obj = context.active_object
        try:
            run_conversion(gp_obj, self.total_verts, self.curve_importance,
                           self.tube_scale, self.ring_verts,
                           self.thickness_delta_importance,
                           output_suffix="_LOD0")
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}
        s = gp_obj.gp_lod_settings
        s.total_verts                = self.total_verts
        s.curve_importance           = self.curve_importance
        s.thickness_delta_importance = self.thickness_delta_importance
        s.tube_scale                 = self.tube_scale
        s.ring_verts                 = self.ring_verts
        return {'FINISHED'}


def _bake_set_iter(gp_name, total_verts, curve_importance,
                   thickness_delta_importance, tube_scale, lod_size_expansion):
    """Generator: yields (step, total, message) for one LOD per next() call.

    Each LOD's mesh data is refilled in place (preserving materials, parent,
    and outliner position). After all schedule items are processed, any
    leftover '_LODn' outputs whose n is past the current schedule length are
    deleted."""
    n_lods = len(LOD_SCHEDULE)
    gp_obj = bpy.data.objects.get(gp_name)
    if gp_obj is None:
        raise RuntimeError(f"GP object '{gp_name}' not found.")
    for i, (vmul, ring) in enumerate(LOD_SCHEDULE):
        verts = max(3, int(round(total_verts * vmul)))
        # Quadratic ramp: 1.0x at LOD0 -> lod_size_expansion at the last LOD.
        # With 4 LODs and expansion=2.0 this gives 1.0, 1.11, 1.44, 2.0.
        t = (i / (n_lods - 1)) if n_lods > 1 else 0.0
        expansion = 1.0 + (lod_size_expansion - 1.0) * (t * t)
        eff_tube_scale = tube_scale * expansion
        run_conversion(gp_obj, verts, curve_importance,
                       eff_tube_scale, ring,
                       thickness_delta_importance,
                       output_suffix=f"_LOD{i}")
        yield i + 1, n_lods, f"LOD{i}: {verts} spine x ring {ring}"

    # Prune any LODs beyond the current schedule length.
    prefix = gp_obj.name + "_LOD"
    for o in list(_lod_outputs_for(gp_obj)):
        suffix = o.name[len(prefix):]
        try:
            idx = int(suffix)
        except ValueError:
            continue
        if idx >= n_lods:
            mesh = o.data
            bpy.data.objects.remove(o, do_unlink=True)
            if mesh is not None and mesh.users == 0:
                bpy.data.meshes.remove(mesh)


class OBJECT_OT_gp_to_mesh_lod_bake_set(bpy.types.Operator):
    """Bake every LOD level from LOD_SCHEDULE one tick at a time so the UI
    stays responsive. Press ESC to cancel."""
    bl_idname = "object.gp_to_mesh_lod_bake_set"
    bl_label = "Bake LOD Set"
    bl_options = {'REGISTER'}

    total_verts: bpy.props.IntProperty(name="Total Stroke Verts (LOD0)", default=1000, min=2, soft_max=200000)
    curve_importance: bpy.props.FloatProperty(name="Curve Importance", default=5.0, min=0.0, soft_max=50.0)
    thickness_delta_importance: bpy.props.FloatProperty(name="Thickness Delta Importance", default=10.0, min=0.0, soft_max=200.0)
    tube_scale: bpy.props.FloatProperty(name="Tube Scale", default=1.0, min=0.0, soft_max=10.0)
    lod_size_expansion: bpy.props.FloatProperty(name="Min LOD Size Multiplier", default=2.0, min=1.0, soft_max=8.0)

    _timer = None
    _iter = None
    _gp_name = ""

    @classmethod
    def poll(cls, context):
        return _is_gp(context.active_object) and not _BAKE_PROGRESS["active"]

    def invoke(self, context, event):
        s = context.active_object.gp_lod_settings
        self.total_verts                = s.total_verts
        self.curve_importance           = s.curve_importance
        self.thickness_delta_importance = s.thickness_delta_importance
        self.tube_scale                 = s.tube_scale
        self.lod_size_expansion         = s.lod_size_expansion
        self._gp_name = context.active_object.name

        try:
            self._iter = _bake_set_iter(
                self._gp_name, self.total_verts, self.curve_importance,
                self.thickness_delta_importance, self.tube_scale,
                self.lod_size_expansion,
            )
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}

        _BAKE_PROGRESS.update(active=True, step=0,
                              total=len(LOD_SCHEDULE), msg="Starting...")
        wm = context.window_manager
        wm.progress_begin(0, len(LOD_SCHEDULE))
        self._timer = wm.event_timer_add(0.05, window=context.window)
        wm.modal_handler_add(self)
        _redraw_lod_panel(context)
        return {'RUNNING_MODAL'}

    def modal(self, context, event):
        if event.type == 'ESC':
            self.report({'WARNING'}, "Bake LOD Set cancelled (partial output kept).")
            self._cleanup(context)
            return {'CANCELLED'}
        if event.type != 'TIMER':
            return {'PASS_THROUGH'}
        try:
            step, total, msg = next(self._iter)
        except StopIteration:
            gp_obj = bpy.data.objects.get(self._gp_name)
            if gp_obj is not None:
                s = gp_obj.gp_lod_settings
                s.total_verts                = self.total_verts
                s.curve_importance           = self.curve_importance
                s.thickness_delta_importance = self.thickness_delta_importance
                s.tube_scale                 = self.tube_scale
                s.lod_size_expansion         = self.lod_size_expansion
            self.report({'INFO'}, f"Baked {len(LOD_SCHEDULE)} LODs")
            self._cleanup(context)
            return {'FINISHED'}
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            self._cleanup(context)
            return {'CANCELLED'}

        _BAKE_PROGRESS.update(step=step, total=total, msg=msg)
        wm = context.window_manager
        wm.progress_update(step)
        context.workspace.status_text_set(f"GP -> LOD: {msg} ({step}/{total})")
        _redraw_lod_panel(context)
        return {'RUNNING_MODAL'}

    def _cleanup(self, context):
        wm = context.window_manager
        if self._timer is not None:
            wm.event_timer_remove(self._timer)
            self._timer = None
        try:
            wm.progress_end()
        except Exception:
            pass
        context.workspace.status_text_set(None)
        _BAKE_PROGRESS.update(active=False, step=0, total=0, msg="")
        self._iter = None
        _redraw_lod_panel(context)


def _redraw_lod_panel(context):
    """Force a viewport redraw so the progress bar updates between timer ticks."""
    for area in context.screen.areas:
        if area.type == 'VIEW_3D':
            area.tag_redraw()


class OBJECT_OT_gp_to_mesh_lod_recommend(bpy.types.Operator):
    """Inspect the active GP object and set Total Stroke Verts to a recommended
    value based on source point count and curvature density."""
    bl_idname = "object.gp_to_mesh_lod_recommend"
    bl_label = "Recommend Max Verts"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return _is_gp(context.active_object) and not _BAKE_PROGRESS["active"]

    def execute(self, context):
        gp_obj = context.active_object
        try:
            src, total_turn, tpp, rec = _calc_recommendations(gp_obj)
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}
        if src == 0:
            self.report({'ERROR'}, "No usable strokes found.")
            return {'CANCELLED'}
        gp_obj.gp_lod_settings.total_verts = rec
        self.report(
            {'INFO'},
            f"Source: {src} pts, total turn {total_turn:.2f}rad "
            f"({tpp:.3f} rad/pt) -> recommended {rec} spine verts.",
        )
        print(f"[gp_to_mesh_lod] recommend: source={src} pts, "
              f"total_turn={total_turn:.3f}rad, per_point={tpp:.4f}rad "
              f"-> set total_verts={rec}")
        return {'FINISHED'}


class OBJECT_OT_gp_to_mesh_lod_clear(bpy.types.Operator):
    """Delete every <gp>_LOD* mesh for the active GP object."""
    bl_idname = "object.gp_to_mesh_lod_clear"
    bl_label = "Clear All LODs"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return _is_gp(context.active_object) and bool(_lod_outputs_for(context.active_object))

    def execute(self, context):
        _delete_lod_outputs(context.active_object)
        return {'FINISHED'}


class VIEW3D_PT_gp_to_mesh_lod(bpy.types.Panel):
    bl_idname = "VIEW3D_PT_gp_to_mesh_lod"
    bl_label = "GP -> Mesh LOD"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "GP LOD"

    @classmethod
    def poll(cls, context):
        return _is_gp(context.active_object)

    def draw(self, context):
        layout = self.layout
        obj = context.active_object
        s = obj.gp_lod_settings

        col = layout.column(align=True)
        row_v = col.row(align=True)
        row_v.prop(s, "total_verts")
        row_v.operator(OBJECT_OT_gp_to_mesh_lod_recommend.bl_idname,
                       text="", icon='SHADERFX')
        col.prop(s, "curve_importance")
        col.prop(s, "thickness_delta_importance")
        col.separator()
        col.prop(s, "tube_scale")
        col.prop(s, "ring_verts")
        col.prop(s, "lod_size_expansion")

        row = layout.row(align=True)
        op = row.operator(OBJECT_OT_gp_to_mesh_lod_bake.bl_idname, text="Bake LOD0", icon='MOD_REMESH')
        op.total_verts                = s.total_verts
        op.curve_importance           = s.curve_importance
        op.thickness_delta_importance = s.thickness_delta_importance
        op.tube_scale                 = s.tube_scale
        op.ring_verts                 = s.ring_verts
        op2 = row.operator(OBJECT_OT_gp_to_mesh_lod_bake_set.bl_idname, text="Bake LOD Set", icon='IMPORT')
        op2.total_verts                = s.total_verts
        op2.curve_importance           = s.curve_importance
        op2.thickness_delta_importance = s.thickness_delta_importance
        op2.tube_scale                 = s.tube_scale
        op2.lod_size_expansion         = s.lod_size_expansion
        row.operator(OBJECT_OT_gp_to_mesh_lod_clear.bl_idname, text="", icon='TRASH')

        if _BAKE_PROGRESS["active"]:
            box = layout.box()
            step  = _BAKE_PROGRESS["step"]
            total = max(1, _BAKE_PROGRESS["total"])
            ratio = step / total
            BAR_W = 22
            fill  = int(round(ratio * BAR_W))
            bar   = "[" + ("#" * fill) + ("-" * (BAR_W - fill)) + "]"
            box.label(text=f"Baking {step}/{total}", icon='TIME')
            box.label(text=bar)
            if _BAKE_PROGRESS["msg"]:
                box.label(text=_BAKE_PROGRESS["msg"])
            box.label(text="(ESC to cancel)")

        outs = sorted(_lod_outputs_for(obj), key=lambda o: o.name)
        if outs:
            box = layout.box()
            box.label(text="Outputs:", icon='OUTLINER_OB_MESH')
            for o in outs:
                box.label(text=f"{o.name}  ({len(o.data.vertices)} verts)")


# --------------------------------------------------------------------------- #
# Registration                                                                #
# --------------------------------------------------------------------------- #
classes = (
    GPLODSettings,
    OBJECT_OT_gp_to_mesh_lod_bake,
    OBJECT_OT_gp_to_mesh_lod_bake_set,
    OBJECT_OT_gp_to_mesh_lod_recommend,
    OBJECT_OT_gp_to_mesh_lod_clear,
    VIEW3D_PT_gp_to_mesh_lod,
)


def register():
    for c in classes:
        bpy.utils.register_class(c)
    bpy.types.Object.gp_lod_settings = bpy.props.PointerProperty(type=GPLODSettings)


def unregister():
    if hasattr(bpy.types.Object, "gp_lod_settings"):
        del bpy.types.Object.gp_lod_settings
    for c in reversed(classes):
        try:
            bpy.utils.unregister_class(c)
        except RuntimeError:
            pass


if __name__ == "__main__":
    try:
        unregister()
    except Exception:
        pass
    register()
