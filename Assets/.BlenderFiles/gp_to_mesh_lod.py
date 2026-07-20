# ============================================================
# EDIT_TAG: 6859
# ^ bumped on every edit so you can verify a reload picked up the latest code.
# ============================================================
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
import os
import time
from mathutils import Matrix, Vector, kdtree as mu_kdtree

# Optional Numba JIT. If installed (`pip install numba` into Blender's Python),
# the `@njit`-decorated functions compile to native machine code on first call.
# If not installed, the decorator becomes a no-op and the functions run as
# pure Python with the same numpy ops.
try:
    from numba import njit, prange
    HAVE_NUMBA = True
except Exception:
    HAVE_NUMBA = False
    def njit(*args, **kwargs):
        if len(args) == 1 and callable(args[0]):
            return args[0]
        def deco(f):
            return f
        return deco
    def prange(*args, **kwargs):
        return range(*args, **kwargs)

# Captured fresh every time this module loads. Used by the panel to show a
# reload indicator so we can verify Blender actually picked up new code.
_LOAD_TIME = time.strftime("%H:%M:%S")
_LOAD_COUNT = 0  # populated in register()

COLOR_ATTR_NAME = "Col"
LOD_MAT_NAME    = "GP_LOD_VertexColor"


def _ensure_lod_material():
    """Get or create a shared material that pipes the 'Col' attribute into the
    Principled BSDF's base color. One material is reused across every LOD
    output so users can edit it once and have the change apply everywhere."""
    mat = bpy.data.materials.get(LOD_MAT_NAME)
    if mat is not None:
        return mat
    mat = bpy.data.materials.new(LOD_MAT_NAME)
    mat.use_nodes = True
    nt = mat.node_tree
    # Wipe the default Principled+Output and rebuild so the attribute node
    # is wired into the BSDF in a known state.
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new('ShaderNodeOutputMaterial')
    out.location = (300, 0)
    bsdf = nt.nodes.new('ShaderNodeBsdfPrincipled')
    bsdf.location = (0, 0)
    # Roughen it a bit so the color reads better in default lighting.
    try:
        bsdf.inputs['Roughness'].default_value = 0.9
    except Exception:
        pass
    attr = nt.nodes.new('ShaderNodeAttribute')
    attr.location = (-300, 0)
    attr.attribute_name = COLOR_ATTR_NAME
    nt.links.new(attr.outputs['Color'], bsdf.inputs['Base Color'])
    nt.links.new(bsdf.outputs['BSDF'],  out.inputs['Surface'])
    return mat

# Ring verts per LOD. Length of this list = number of LOD levels to bake.
# Per-LOD vertex count is now driven by the "Min LOD Vert Ratio" setting:
# LOD0 = 1.0x of Total Stroke Verts, last LOD = ratio, geometric in between.
LOD_RING_SCHEDULE = [6, 4, 2, 2]


def _lod_vert_multiplier(lod_index, n_lods, min_ratio):
    """Geometric ramp from 1.0 at LOD0 down to `min_ratio` at the last LOD:
    mult = min_ratio ** (i / (n_lods - 1)), so each LOD is a constant
    fraction of the previous one. Examples for 4 LODs:
      ratio 0.01 -> 100% / 21.5% / 4.6% / 1%
      ratio 0.10 -> 100% / 46.4% / 21.5% / 10%"""
    if n_lods <= 1 or min_ratio >= 1.0:
        return 1.0
    t = lod_index / (n_lods - 1)
    return float(max(min_ratio, 1e-4) ** t)

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
    all_qnorm  = _bulk(QUILL_NORMAL_ATTR, 3, "vector")

    # Per-curve Quill brush type (stamped by Fetch Quill Orientation).
    brush_arr = None
    try:
        bdata = attrs[QUILL_BRUSH_ATTR].data
        brush_arr = np.empty(len(bdata), dtype=np.int32)
        bdata.foreach_get("value", brush_arr)
    except (KeyError, AttributeError, TypeError):
        brush_arr = None

    cursor = 0
    for s_i, s in enumerate(drawing.strokes):
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

        qnorm = None
        if all_qnorm is not None:
            qnorm = all_qnorm[cursor:cursor + n].astype(np.float64)
            # All-zero normals mark strokes with no Quill match (added after
            # import) - treat as "no data" so they use parallel transport.
            if float(np.abs(qnorm).max()) < 1e-6:
                qnorm = None
        brush = int(brush_arr[s_i]) if brush_arr is not None and s_i < len(brush_arr) else 0

        out.append({"pos": pos, "col": col, "thick": thick,
                    "qnorm": qnorm, "brush": brush})
        cursor += n

    return out


def gather_stroke_data_world(obj):
    """Returns (raw, stats). raw is a list of dicts with positions IN GP LOCAL
    SPACE (despite the historical function name), per-point colors, and per-
    point thicknesses. We bake in local space so the output mesh inherits the
    GP's transform via parenting without a parent_inverse round-trip - that
    inversion was introducing sub-pixel offsets visible to the user."""
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

    # Positions are already in GP local space (point.co / point.position is
    # stored in the GP object's coordinate system) - no transform needed.
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
    turn_total = float(turning.sum()) if turning.size else 0.0
    # Thickness total-variation on a lightly smoothed profile, so per-point
    # Quill jitter doesn't masquerade as real bulges/tapers.
    if len(thick) >= 3:
        t_s = thick.astype(np.float64).copy()
        t_s[1:-1] = 0.25 * (thick[:-2] + 2.0 * thick[1:-1] + thick[2:])
        tv_thick = float(np.abs(np.diff(t_s)).sum())
    else:
        tv_thick = float(seg_dthick.sum()) if seg_dthick.size else 0.0
    return cum_w, cum_w[-1], arc_len, turn_total, tv_thick


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


def _classify_brick(thick, turn_total, max_turn_rad):
    """A stroke is 'brick-like' when it is nearly straight AND its body
    thickness is nearly constant (Quill pressure tapers at the very ends are
    ignored by sampling the middle 50%). Brick strokes bake as 2-spine-sample
    square-profile boxes (8 verts, 6 quads) with flat ends at every LOD.
    Returns (is_brick, body_thickness)."""
    if max_turn_rad <= 0.0 or turn_total > max_turn_rad:
        return False, 0.0
    n = len(thick)
    lo = n // 4
    hi = max(lo + 1, (3 * n) // 4)
    body = thick[lo:hi]
    med = float(np.median(body))
    if med <= 1e-9:
        return False, 0.0
    spread = float(np.percentile(body, 90) - np.percentile(body, 10))
    if spread / med > 0.35:
        return False, 0.0
    return True, med


def _mean_width_comp(R):
    """Radius multiplier so an R-gon tube reads as wide as the round source
    stroke. Cauchy's formula: mean silhouette width of a convex shape =
    perimeter / pi. Circle of radius r: 2r. Inscribed R-gon: 2Rr*sin(pi/R)/pi.
    Matching them gives pi / (R * sin(pi/R)) -> x1.047 for R=6, x1.111 for
    R=4. A 2-vert ribbon degenerates to a segment (mean width 4r/pi) -> pi/2."""
    if R >= 3:
        return float(np.pi / (R * np.sin(np.pi / R)))
    return float(np.pi / 2.0)


def _resample_deviation(orig, new_pos, idx):
    """Per-spine-sample silhouette loss from resampling: for each gap between
    consecutive resampled points, the max distance from the skipped source
    points to the resampled segment. Added back into the local tube radius so
    the silhouette re-inflates exactly where corners were cut off, instead of
    uniformly fattening the whole stroke. Returns (S,) array."""
    S = len(new_pos)
    dev = np.zeros(S)
    for j in range(S - 1):
        lo, hi = int(idx[j]) + 1, int(idx[j + 1]) + 1
        if hi <= lo:
            continue
        P = orig[lo:hi]
        A, B = new_pos[j], new_pos[j + 1]
        AB = B - A
        L2 = float(AB @ AB)
        if L2 < 1e-18:
            dists = np.linalg.norm(P - A, axis=1)
        else:
            tt = np.clip(((P - A) @ AB) / L2, 0.0, 1.0)
            dists = np.linalg.norm(P - (A + tt[:, None] * AB), axis=1)
        m = float(dists.max())
        if m > dev[j]:
            dev[j] = m
        if m > dev[j + 1]:
            dev[j + 1] = m
    return dev


# --------------------------------------------------------------------------- #
# Quill orientation recovery                                                  #
# --------------------------------------------------------------------------- #
# The Quill file format stores per-vertex normals (ribbon orientation) and a
# per-stroke brush type, both of which the GP importer discards. When a GP
# object still has its Quill provenance (obj.quill.scene_path / layer_path,
# written by the importer), we can read them straight back out of the .qbin
# and stamp them onto the GP drawing as attributes. The tube builder then
# constructs cross-sections in the painted orientation (flat ribbons that
# twist like the original) instead of guessing with parallel transport.
QUILL_NORMAL_ATTR = "quill_normal"
QUILL_BRUSH_ATTR  = "quill_brush"

QUILL_BRUSH_RIBBON   = 1
QUILL_BRUSH_CYLINDER = 2
QUILL_BRUSH_ELLIPSE  = 3
QUILL_BRUSH_CUBE     = 4


def _quill_find_layer(node, parts):
    """Walk the raw Quill.json layer tree by name parts."""
    if not parts:
        return node
    if node.get("Type") != "Group":
        return None
    for child in node.get("Implementation", {}).get("Children", []):
        if child.get("Name") == parts[0]:
            return _quill_find_layer(child, parts[1:])
    return None


def _quill_paint_layers_by_name(node, name, found=None):
    """Collect every Paint layer with the given name anywhere in the tree.
    Needed because the importer's stored layer_path is unreliable (it
    accumulates one duplicate parent name per preceding sibling)."""
    if found is None:
        found = []
    if node.get("Type") == "Paint" and node.get("Name") == name:
        found.append(node)
    if node.get("Type") == "Group":
        for child in node.get("Implementation", {}).get("Children", []):
            _quill_paint_layers_by_name(child, name, found)
    return found


def _quill_read_drawing_strokes(qbin_path, layer):
    """Read every stroke of a Quill paint layer's first drawing. Seeks
    straight to the drawing's byte range, so only that layer is parsed (the
    qbin can be hundreds of MB). Returns a list of dicts
    {brush, pos (N,3), normal (N,3), width (N,)}."""
    import struct as _struct
    drawings = layer.get("Implementation", {}).get("Drawings", [])
    if not drawings:
        raise RuntimeError(f"layer {layer.get('Name')!r} has no drawings")

    # Stroke record layout (see the Quill add-on's model/paint.py):
    # header = id u32, unknown u32, bbox 6*f32, brush i16, bool u8, pad u8,
    # vertex count u32 (40 bytes). Then count vertices of 14 f32 each:
    # position 3, normal 3, tangent 3, color 3, opacity 1, width 1.
    strokes = []
    with open(qbin_path, "rb") as qbin:
        qbin.seek(int(drawings[0]["DataFileOffset"], 16))
        (stroke_count,) = _struct.unpack("<I", qbin.read(4))
        for _ in range(stroke_count):
            head = qbin.read(40)
            brush = _struct.unpack_from("<h", head, 32)[0]
            (n_verts,) = _struct.unpack_from("<I", head, 36)
            raw = np.frombuffer(qbin.read(n_verts * 56), dtype="<f4").reshape(n_verts, 14)
            strokes.append({
                "brush":  int(brush),
                "pos":    raw[:, 0:3].astype(np.float64),
                "normal": raw[:, 3:6].astype(np.float64),
                "width":  raw[:, 13].astype(np.float64),
            })
    return strokes


def _quill_read_layer_strokes(scene_dir, layer_path):
    """Resolve a Quill paint layer and read its strokes. Tries the stored
    layer_path first; falls back to searching the tree by the path's last
    component (the layer name), returning candidates for structure matching.
    Returns a list of stroke-list candidates (usually length 1)."""
    import json as _json
    scene_json = os.path.join(scene_dir, "Quill.json")
    qbin_path  = os.path.join(scene_dir, "Quill.qbin")
    if not (os.path.exists(scene_json) and os.path.exists(qbin_path)):
        raise RuntimeError(f"Quill scene not found at {scene_dir!r}")
    with open(scene_json, "r", encoding="utf8") as f:
        root = _json.load(f)["Sequence"]["RootLayer"]

    parts = [p for p in layer_path.split("/") if p]
    name = parts[-1] if parts else ""

    # Exact path walk (skip the root layer itself).
    layer = _quill_find_layer(root, parts[1:])
    if layer is not None and layer.get("Type") == "Paint":
        candidates = [layer]
    else:
        candidates = _quill_paint_layers_by_name(root, name)
    if not candidates:
        raise RuntimeError(f"no Paint layer named {name!r} found in Quill.json")
    return [_quill_read_drawing_strokes(qbin_path, c) for c in candidates]


def _fetch_quill_orientation(gp_obj):
    """Recover normals + brush types from the Quill source and store them as
    attributes on the GP drawing. Returns the number of points stamped."""
    q = getattr(gp_obj, "quill", None)
    if q is None or not q.scene_path or not q.layer_path:
        raise RuntimeError("no Quill provenance (scene_path/layer_path) on this object")
    if gp_obj.type != 'GREASEPENCIL':
        raise RuntimeError("only v3 GREASEPENCIL objects are supported")

    candidates = _quill_read_layer_strokes(bpy.path.abspath(q.scene_path), q.layer_path)

    drawings = []
    for lay in gp_obj.data.layers:
        fr = lay.current_frame()
        if fr is not None:
            drawings.append(fr.drawing)
    if len(drawings) != 1:
        raise RuntimeError(f"expected exactly 1 GP drawing, found {len(drawings)}")
    drawing = drawings[0]
    gp_strokes = drawing.strokes

    # GP point positions (importer wrote Quill positions verbatim).
    pos_attr = drawing.attributes["position"].data
    gp_flat = np.empty(len(pos_attr) * 3, dtype=np.float32)
    pos_attr.foreach_get("vector", gp_flat)
    gp_pts = gp_flat.reshape(-1, 3)

    # Per-stroke matching by point count + endpoint positions. This survives
    # GPs that were edited after import: strokes still present in the Quill
    # source get their painted orientation; strokes added in Blender get zero
    # normals, which the reader treats as "no data" (parallel transport).
    def _skey(n, p0, p1):
        return (n, tuple(np.round(p0, 4)), tuple(np.round(p1, 4)))

    gp_keys = []
    pts_cursor = 0
    for gs in gp_strokes:
        n = len(gs.points)
        gp_keys.append(_skey(n, gp_pts[pts_cursor], gp_pts[pts_cursor + n - 1]))
        pts_cursor += n

    best = None  # (n_matched, matches, candidate strokes)
    for cand in candidates:
        lut = {}
        for qi, qs in enumerate(cand):
            lut.setdefault(
                _skey(len(qs["pos"]), qs["pos"][0], qs["pos"][-1]), []).append(qi)
        matches = []
        n_matched = 0
        for key in gp_keys:
            qis = lut.get(key)
            if qis:
                matches.append(qis.pop(0))
                n_matched += 1
            else:
                matches.append(-1)
        if best is None or n_matched > best[0]:
            best = (n_matched, matches, cand)
    n_matched, matches, q_cand = best
    if n_matched == 0:
        raise RuntimeError(
            f"no strokes matched any Quill layer candidate "
            f"({len(candidates)} candidate(s) named alike)")

    normals_list = []
    brushes = np.zeros(len(gp_strokes), dtype=np.int32)
    for gi, (gs, qi) in enumerate(zip(gp_strokes, matches)):
        if qi >= 0:
            normals_list.append(q_cand[qi]["normal"])
            brushes[gi] = q_cand[qi]["brush"]
        else:
            normals_list.append(np.zeros((len(gs.points), 3)))
    normals_flat = np.concatenate(normals_list).astype(np.float32)
    total = int(sum(len(q_cand[qi]["pos"]) for qi in matches if qi >= 0))
    if n_matched < len(gp_strokes):
        print(f"[gp_to_mesh_lod] {gp_obj.name}: {len(gp_strokes) - n_matched} "
              f"stroke(s) not in the Quill source (added after import?) - "
              f"they'll use parallel-transport orientation.")

    attrs = drawing.attributes
    for name in (QUILL_NORMAL_ATTR, QUILL_BRUSH_ATTR):
        if name in attrs:
            attrs.remove(attrs[name])
    na = attrs.new(QUILL_NORMAL_ATTR, 'FLOAT_VECTOR', 'POINT')
    na.data.foreach_set("vector", normals_flat.ravel())
    ba = attrs.new(QUILL_BRUSH_ATTR, 'INT', 'CURVE')
    ba.data.foreach_set("value", brushes)

    from collections import Counter
    hist = Counter(int(b) for b in brushes if b > 0)
    names = {1: "ribbon", 2: "cylinder", 3: "ellipse", 4: "cube"}
    hist_s = ", ".join(f"{names.get(k, k)}={v}" for k, v in sorted(hist.items()))
    print(f"[gp_to_mesh_lod] {gp_obj.name}: quill orientation stored for "
          f"{total} points across {n_matched}/{len(gp_strokes)} strokes ({hist_s})")
    return total


# --- Quill relink: matching that survives edit-mode moves ----------------- #
# Separating GP strokes into new objects keeps the quill pointer, but moving
# strokes in edit mode bakes the transform into the point coordinates, so
# position-based matching dies. These helpers match strokes by properties an
# edit-mode move CANNOT change - point count, arc length, segment-length
# proportions, thickness values - then recover each stroke's rotation with a
# rigid (Kabsch) fit so the painted normals can be carried into its current
# pose.

_QUILL_CACHE = {}  # scene_dir -> {root, qbin, strokes{offset}, luts{key}}


def _quill_json_root(scene_dir):
    """Cached parse of Quill.json. Returns (root layer dict, qbin path)."""
    entry = _QUILL_CACHE.setdefault(scene_dir, {})
    if "root" not in entry:
        import json as _json
        scene_json = os.path.join(scene_dir, "Quill.json")
        qbin_path = os.path.join(scene_dir, "Quill.qbin")
        if not (os.path.exists(scene_json) and os.path.exists(qbin_path)):
            raise RuntimeError(f"Quill scene not found at {scene_dir!r}")
        with open(scene_json, "r", encoding="utf8") as f:
            entry["root"] = _json.load(f)["Sequence"]["RootLayer"]
        entry["qbin"] = qbin_path
    return entry["root"], entry["qbin"]


def _quill_layer_strokes_cached(scene_dir, layer):
    """Cached read of a paint layer's first-drawing strokes."""
    entry = _QUILL_CACHE.setdefault(scene_dir, {})
    drawings = layer.get("Implementation", {}).get("Drawings", [])
    if not drawings:
        raise RuntimeError(f"layer {layer.get('Name')!r} has no drawings")
    off = drawings[0]["DataFileOffset"]
    strokes_by_off = entry.setdefault("strokes", {})
    if off not in strokes_by_off:
        strokes_by_off[off] = _quill_read_drawing_strokes(entry["qbin"], layer)
    return strokes_by_off[off]


def _quill_all_paint_layers(node, found=None):
    if found is None:
        found = []
    if node.get("Type") == "Paint":
        found.append(node)
    if node.get("Type") == "Group":
        for child in node.get("Implementation", {}).get("Children", []):
            _quill_all_paint_layers(child, found)
    return found


def _stroke_fingerprint(pos, thick):
    """Rigid-transform-invariant fingerprint: (point count, segment-length
    proportions, quantized arc length, quantized first thickness). Thickness
    values are untouched by edit-mode moves, so they discriminate strongly.
    Returns (key, arcQ, thickQ) or (None, 0, 0) for degenerate strokes;
    query neighboring arcQ/thickQ grid cells to absorb rounding at
    quantization boundaries."""
    n = len(pos)
    if n < 2:
        return None, 0, 0
    seg = np.linalg.norm(np.diff(pos, axis=0), axis=1)
    arc = float(seg.sum())
    if arc <= 1e-9:
        return None, 0, 0
    arcQ = int(round(arc * 100.0))
    thickQ = int(round(float(thick[0]) * 1000.0))
    profile = tuple(np.round(seg / arc, 2))
    return (n, profile), arcQ, thickQ


def _rigid_rotation(src, dst):
    """Best rotation mapping src points onto dst (Kabsch), centering first.
    See _rigid_rotation_centered for the fast path."""
    return _rigid_rotation_centered(src - src.mean(0), dst - dst.mean(0))


def _rigid_rotation_centered(A, B):
    """Best rotation mapping centered A points onto centered B (Kabsch).
    Falls back to a shortest-arc axis alignment for (near-)collinear strokes,
    where the roll around the stroke axis is unobservable."""
    U, S, Vt = np.linalg.svd(A.T @ B)
    if len(S) > 1 and S[1] > 1e-8:
        d = np.sign(np.linalg.det(Vt.T @ U.T))
        return Vt.T @ np.diag([1.0, 1.0, d]) @ U.T
    u = A[-1] - A[0]
    v = B[-1] - B[0]
    un, vn = np.linalg.norm(u), np.linalg.norm(v)
    if un < 1e-12 or vn < 1e-12:
        return np.eye(3)
    u, v = u / un, v / vn
    c = float(np.clip(u @ v, -1.0, 1.0))
    axis = np.cross(u, v)
    s = float(np.linalg.norm(axis))
    if s < 1e-12:
        if c > 0:
            return np.eye(3)
        p = np.cross(u, np.array([1.0, 0.0, 0.0]))
        if np.linalg.norm(p) < 1e-6:
            p = np.cross(u, np.array([0.0, 1.0, 0.0]))
        p /= np.linalg.norm(p)
        return 2.0 * np.outer(p, p) - np.eye(3)
    axis /= s
    K = np.array([[0.0, -axis[2], axis[1]],
                  [axis[2], 0.0, -axis[0]],
                  [-axis[1], axis[0], 0.0]])
    return np.eye(3) + s * K + (1.0 - c) * (K @ K)


def _quill_match_pool(scene_dir, layer_path):
    """Candidate stroke pool + lookup tables for one GP object's provenance.
    Cached per (scene, candidate layer set) so batch relinks don't rebuild."""
    root, _qbin = _quill_json_root(scene_dir)
    parts = [p for p in layer_path.split("/") if p]
    name = parts[-1] if parts else ""
    layer = _quill_find_layer(root, parts[1:])
    cand_layers = []
    if layer is not None and layer.get("Type") == "Paint":
        cand_layers.append(layer)
    for l in _quill_paint_layers_by_name(root, name):
        if l not in cand_layers:
            cand_layers.append(l)
    if not cand_layers:
        cand_layers = _quill_all_paint_layers(root)
    if not cand_layers:
        raise RuntimeError("no Paint layers found in Quill.json")

    cache_key = tuple(sorted(
        l.get("Implementation", {}).get("Drawings", [{}])[0].get("DataFileOffset", "?")
        for l in cand_layers))
    entry = _QUILL_CACHE.setdefault(scene_dir, {})
    luts = entry.setdefault("luts", {})
    if cache_key in luts:
        return luts[cache_key]

    pool = []
    for lay in cand_layers:
        try:
            pool.extend(_quill_layer_strokes_cached(scene_dir, lay))
        except RuntimeError:
            pass
    if not pool:
        raise RuntimeError("no strokes readable from candidate Quill layers")

    exact_lut = {}
    fp_lut = {}
    for qi, s in enumerate(pool):
        p = s["pos"]
        if len(p) < 2:
            continue
        # Pre-centered positions for the Kabsch hot loop.
        s["pos_c"] = p - p.mean(0)
        ekey = (len(p), tuple(np.round(p[0].astype(np.float32), 4)),
                tuple(np.round(p[-1].astype(np.float32), 4)))
        exact_lut.setdefault(ekey, []).append(qi)
        fp, arcQ, thickQ = _stroke_fingerprint(p, s["width"])
        if fp:
            fp_lut.setdefault(fp + (arcQ, thickQ), []).append(qi)
    result = (pool, exact_lut, fp_lut)
    luts[cache_key] = result
    return result


def _relink_quill_orientation(gp_obj):
    """Recover per-point normals + brush types from the Quill source for
    every drawing of a GP object, surviving separation, reordering, and
    edit-mode moves. Two-stage matching per stroke:
      1. exact  - point count + endpoint positions (unmoved strokes),
      2. rigid  - invariant fingerprint (count/arc/proportions), thickness
                  verification, then a Kabsch fit; accepted only when the
                  residual says the stroke really is a rigidly-moved copy.
    The fitted rotation carries the painted normals into the stroke's
    current pose. Unmatched strokes get zero normals (the reader treats
    those as 'no data' and uses parallel transport).
    Returns (points stamped, strokes matched, strokes total)."""
    q = getattr(gp_obj, "quill", None)
    if q is None or not q.scene_path or not q.layer_path:
        raise RuntimeError("no Quill provenance (scene_path/layer_path) on this object")
    if gp_obj.type != 'GREASEPENCIL':
        raise RuntimeError("only v3 GREASEPENCIL objects are supported")
    scene_dir = bpy.path.abspath(q.scene_path)
    pool, exact_lut, fp_lut = _quill_match_pool(scene_dir, q.layer_path)

    total_pts = total_matched = total_strokes = 0
    for lay in gp_obj.data.layers:
        fr = lay.current_frame()
        if fr is None:
            continue
        drawing = fr.drawing
        strokes = drawing.strokes
        counts = [len(s.points) for s in strokes]
        n_pts_total = int(sum(counts))
        if n_pts_total == 0:
            continue
        pos_attr = drawing.attributes["position"].data
        flat = np.empty(len(pos_attr) * 3, dtype=np.float32)
        pos_attr.foreach_get("vector", flat)
        pts = flat.reshape(-1, 3).astype(np.float64)
        try:
            rad_attr = drawing.attributes["radius"].data
            radii = np.empty(len(rad_attr), dtype=np.float32)
            rad_attr.foreach_get("value", radii)
        except (KeyError, AttributeError):
            radii = np.zeros(len(pts), dtype=np.float32)

        offs = np.concatenate([[0], np.cumsum(counts)]).astype(int)
        normals_out = np.zeros((n_pts_total, 3), dtype=np.float32)
        brush_out = np.zeros(len(strokes), dtype=np.int32)
        for i, n in enumerate(counts):
            total_strokes += 1
            if n < 2:
                continue
            gp_p = pts[offs[i]:offs[i + 1]]
            gp_t = radii[offs[i]:offs[i + 1]].astype(np.float64)
            hit = None
            R = None
            ekey = (n, tuple(np.round(gp_p[0].astype(np.float32), 4)),
                    tuple(np.round(gp_p[-1].astype(np.float32), 4)))
            hits = exact_lut.get(ekey)
            if hits:
                hit = hits[0]
            if hit is None:
                fp, arcQ, thickQ = _stroke_fingerprint(gp_p, gp_t)
                if fp:
                    arc = arcQ / 100.0
                    tol = max(0.01, 0.005 * arc)
                    B = gp_p - gp_p.mean(0)
                    best_qi, best_R, best_resid = None, None, np.inf
                    for da in (0, -1, 1):
                        for dt in (0, -1, 1):
                            for qi in fp_lut.get(fp + (arcQ + da, thickQ + dt), [])[:60]:
                                qs = pool[qi]
                                # Fast reject on thickness (raw compare - the
                                # values are byte-identical after edit moves).
                                dw = qs["width"] - gp_t
                                if abs(float(dw.max())) > 2e-3 or abs(float(dw.min())) > 2e-3:
                                    continue
                                A = qs["pos_c"]
                                Rc = _rigid_rotation_centered(A, B)
                                resid = float(np.abs(B - A @ Rc.T).max())
                                if resid < best_resid:
                                    best_qi, best_R, best_resid = qi, Rc, resid
                                    if resid < tol:
                                        break  # duplicates are common; first fit wins
                            if best_qi is not None and best_resid < tol:
                                break
                        if best_qi is not None and best_resid < tol:
                            break
                    if best_qi is not None and best_resid < tol:
                        hit, R = best_qi, best_R
            if hit is None:
                continue
            total_matched += 1
            total_pts += n
            qn = pool[hit]["normal"]
            if R is not None:
                qn = qn @ R.T
            normals_out[offs[i]:offs[i + 1]] = qn.astype(np.float32)
            brush_out[i] = pool[hit]["brush"]

        attrs = drawing.attributes
        for aname in (QUILL_NORMAL_ATTR, QUILL_BRUSH_ATTR):
            if aname in attrs:
                attrs.remove(attrs[aname])
        na = attrs.new(QUILL_NORMAL_ATTR, 'FLOAT_VECTOR', 'POINT')
        na.data.foreach_set("vector", normals_out.ravel())
        ba = attrs.new(QUILL_BRUSH_ATTR, 'INT', 'CURVE')
        ba.data.foreach_set("value", brush_out)

    print(f"[gp_to_mesh_lod] {gp_obj.name}: quill relink matched "
          f"{total_matched}/{total_strokes} strokes ({total_pts} points)")
    if total_matched == 0:
        raise RuntimeError(
            f"no strokes matched the Quill source ({total_strokes} checked)")
    return total_pts


def _stroke_kind(d, use_quill):
    """Cross-section kind for batching: the brick detector wins, then the
    Quill brush type when orientation data has been fetched."""
    if d.get("is_brick"):
        return 'BRICK'
    if not use_quill or d.get("qnorm") is None:
        return 'ROUND'
    b = d.get("brush", 0)
    if b == QUILL_BRUSH_RIBBON:
        return 'RIBBON'
    if b == QUILL_BRUSH_ELLIPSE:
        return 'ELLIPSE'
    if b == QUILL_BRUSH_CUBE:
        return 'CUBE'
    return 'CYL'  # cylinder / unknown: round profile, oriented frames


def _thickness_cell_average(pos, thick, idx, t):
    """Anti-aliased thickness resampling. Each output sample takes the
    arc-length-weighted AVERAGE of the source thickness over its own cell
    (midpoint-to-midpoint between samples) instead of point-sampling.
    Point-sampling a narrow fat bump onto sparse samples spreads its peak
    across the whole span via lerp - the 'tent' artifact; cell-averaging is
    the correct low-pass for downsampling, so a bump contributes only its
    average over the cell no matter how few samples the stroke gets."""
    seg = np.linalg.norm(np.diff(pos, axis=0), axis=1)
    arc = np.concatenate([[0.0], np.cumsum(seg)])
    total_len = float(arc[-1])
    if total_len <= 1e-12:
        return interp_along(thick, idx, t)
    # Cumulative integral of thickness along arc (trapezoid).
    seg_avg = 0.5 * (thick[:-1] + thick[1:])
    cumint = np.concatenate([[0.0], np.cumsum(seg_avg * seg)])
    s_arc = arc[idx] + t * (arc[idx + 1] - arc[idx])
    bounds = np.empty(len(s_arc) + 1)
    bounds[0] = 0.0
    bounds[-1] = total_len
    bounds[1:-1] = 0.5 * (s_arc[:-1] + s_arc[1:])
    Ib = np.interp(bounds, arc, cumint)
    widths = np.maximum(np.diff(bounds), 1e-12)
    return np.diff(Ib) / widths


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
def _frames_along(spine, up_axis='Z'):
    """Parallel-transport frames along an open polyline.
    `up_axis` ('X'|'Y'|'Z') picks the GP-local axis used as the initial frame
    reference. Pick the axis that's the *normal* of your drawing plane: Z for
    top-view (XY) drawings, Y for front-view (XZ), X for side-view (YZ).
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

    # Initial normal: perpendicular to t0, biased by up_axis selection.
    AXIS_VECS = {'X': np.array([1.0, 0.0, 0.0]),
                 'Y': np.array([0.0, 1.0, 0.0]),
                 'Z': np.array([0.0, 0.0, 1.0])}
    AXIS_FALLBACK = {'X': 'Y', 'Y': 'Z', 'Z': 'X'}
    primary = AXIS_VECS.get(up_axis, AXIS_VECS['Z'])
    fallback = AXIS_VECS[AXIS_FALLBACK.get(up_axis, 'X')]
    t0 = tangents[0]
    # If tangent is too parallel to primary, fall back to a different axis.
    ref = primary if abs(float(np.dot(t0, primary))) < 0.9 else fallback
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


def _curvature_radius_cap(spine, safety=0.9):
    """Return per-spine-sample maximum radius before the tube self-intersects
    on the inside of a bend. Critical radius at vertex i with adjacent-segment
    lengths L1,L2 and turn angle theta is r_crit = (L1+L2)/2 / (2 * tan(theta/2)).
    Straight regions get +inf (no cap)."""
    S = len(spine)
    max_r = np.full(S, np.inf)
    if S < 3:
        return max_r
    deltas = np.diff(spine, axis=0)
    seg_len = np.linalg.norm(deltas, axis=1)
    tangs = deltas / np.maximum(seg_len[:, None], 1e-12)
    dots = np.clip((tangs[:-1] * tangs[1:]).sum(axis=1), -1.0, 1.0)
    turn = np.arccos(dots)  # (S-2,)
    half = np.clip(turn / 2.0, 0.0, np.pi / 2 - 1e-4)
    tan_half = np.tan(half)
    avg_seg = 0.5 * (seg_len[:-1] + seg_len[1:])
    crit = np.where(tan_half > 1e-6, avg_seg / (2.0 * tan_half), np.inf)
    max_r[1:-1] = crit * safety
    return max_r


AXIS_VECS = {'X': np.array([1.0, 0.0, 0.0]),
             'Y': np.array([0.0, 1.0, 0.0]),
             'Z': np.array([0.0, 0.0, 1.0])}
AXIS_FALLBACK = {'X': 'Y', 'Y': 'Z', 'Z': 'X'}


@njit(cache=True, parallel=True)
def _parallel_transport_njit(tangents, n0):
    """Per-stroke parallel transport, parallelized over the stroke axis with
    Numba's prange. tangents: (N, S, 3), n0: (N, 3). Returns normals (N, S, 3).

    Scalar math throughout - no numpy ops inside the loops - so the JIT can
    inline aggressively. Without Numba this is a no-op decorator and the
    function runs as pure Python (very slow); use the numpy batched path
    instead in that case."""
    N, S, _ = tangents.shape
    normals = np.empty((N, S, 3))
    for k in prange(N):
        normals[k, 0, 0] = n0[k, 0]
        normals[k, 0, 1] = n0[k, 1]
        normals[k, 0, 2] = n0[k, 2]
        for i in range(1, S):
            tpx = tangents[k, i - 1, 0]
            tpy = tangents[k, i - 1, 1]
            tpz = tangents[k, i - 1, 2]
            tcx = tangents[k, i, 0]
            tcy = tangents[k, i, 1]
            tcz = tangents[k, i, 2]
            ax = tpy * tcz - tpz * tcy
            ay = tpz * tcx - tpx * tcz
            az = tpx * tcy - tpy * tcx
            sin_a = (ax * ax + ay * ay + az * az) ** 0.5
            cos_a = tpx * tcx + tpy * tcy + tpz * tcz
            if cos_a > 1.0:
                cos_a = 1.0
            elif cos_a < -1.0:
                cos_a = -1.0
            npx = normals[k, i - 1, 0]
            npy = normals[k, i - 1, 1]
            npz = normals[k, i - 1, 2]
            if sin_a < 1e-8:
                nnx = npx; nny = npy; nnz = npz
            else:
                ax /= sin_a; ay /= sin_a; az /= sin_a
                cnx = ay * npz - az * npy
                cny = az * npx - ax * npz
                cnz = ax * npy - ay * npx
                dot_an = ax * npx + ay * npy + az * npz
                omc = 1.0 - cos_a
                nnx = npx * cos_a + cnx * sin_a + ax * dot_an * omc
                nny = npy * cos_a + cny * sin_a + ay * dot_an * omc
                nnz = npz * cos_a + cnz * sin_a + az * dot_an * omc
            # Re-project onto plane perpendicular to t_cur.
            proj = nnx * tcx + nny * tcy + nnz * tcz
            nnx -= tcx * proj
            nny -= tcy * proj
            nnz -= tcz * proj
            nrm = (nnx * nnx + nny * nny + nnz * nnz) ** 0.5
            if nrm > 1e-12:
                normals[k, i, 0] = nnx / nrm
                normals[k, i, 1] = nny / nrm
                normals[k, i, 2] = nnz / nrm
            else:
                normals[k, i, 0] = npx
                normals[k, i, 1] = npy
                normals[k, i, 2] = npz
    return normals


def _frames_along_batch(spine_batch, up_axis='Z'):
    """Batched parallel-transport frames for N strokes that all have the same
    S spine samples. spine_batch is (N, S, 3). Returns (tangents, normals,
    binormals) each shape (N, S, 3).

    The parallel transport step is sequential along S but vectorized across
    N strokes, so the Python-level loop runs O(S) times instead of O(N*S)."""
    N, S, _ = spine_batch.shape

    deltas = np.diff(spine_batch, axis=1)                            # (N, S-1, 3)
    seg_len = np.linalg.norm(deltas, axis=-1, keepdims=True)         # (N, S-1, 1)
    seg_t = deltas / np.maximum(seg_len, 1e-12)                      # (N, S-1, 3)

    tangents = np.empty_like(spine_batch)
    tangents[:, 0]  = seg_t[:, 0]
    tangents[:, -1] = seg_t[:, -1]
    if S > 2:
        avg = seg_t[:, :-1] + seg_t[:, 1:]                           # (N, S-2, 3)
        tangents[:, 1:-1] = avg / np.maximum(np.linalg.norm(avg, axis=-1, keepdims=True), 1e-12)

    primary  = AXIS_VECS.get(up_axis, AXIS_VECS['Z'])
    fallback = AXIS_VECS[AXIS_FALLBACK.get(up_axis, 'X')]

    t0 = tangents[:, 0]                                              # (N, 3)
    dot_primary = np.abs(t0 @ primary)                               # (N,)
    refs = np.where((dot_primary < 0.9)[:, None],
                    primary[None, :], fallback[None, :])             # (N, 3)
    n0 = np.cross(t0, refs)
    n0 /= np.maximum(np.linalg.norm(n0, axis=-1, keepdims=True), 1e-12)

    if HAVE_NUMBA:
        # Numba path: scalar parallel transport, prange across strokes.
        normals = _parallel_transport_njit(
            np.ascontiguousarray(tangents),
            np.ascontiguousarray(n0))
    else:
        # Pure-numpy path: vectorized across strokes, sequential along S.
        normals = np.empty_like(spine_batch)
        normals[:, 0] = n0
        for i in range(1, S):
            t_prev = tangents[:, i - 1]
            t_cur  = tangents[:, i]
            axis   = np.cross(t_prev, t_cur)
            sin_a  = np.linalg.norm(axis, axis=-1, keepdims=True)
            cos_a  = np.clip(np.sum(t_prev * t_cur, axis=-1, keepdims=True), -1.0, 1.0)
            n_prev = normals[:, i - 1]
            axis_unit = axis / np.maximum(sin_a, 1e-12)
            rod = (n_prev * cos_a
                   + np.cross(axis_unit, n_prev) * sin_a
                   + axis_unit * np.sum(axis_unit * n_prev, axis=-1, keepdims=True) * (1.0 - cos_a))
            small = (sin_a.squeeze(-1) < 1e-8)[:, None]
            n_new = np.where(small, n_prev, rod)
            proj = np.sum(n_new * t_cur, axis=-1, keepdims=True)
            n_new = n_new - t_cur * proj
            nrm = np.linalg.norm(n_new, axis=-1, keepdims=True)
            n_normed = n_new / np.maximum(nrm, 1e-12)
            normals[:, i] = np.where((nrm > 1e-12), n_normed, n_prev)

    binormals = np.cross(tangents, normals)
    return tangents, normals, binormals


def build_tube_batch(spine_batch, radii_batch, R, thickness_ratio=1.0, up_axis='Z',
                     profile='ROUND', qnorm_batch=None):
    """Batched tube builder. spine_batch (N,S,3) and radii_batch (N,S).
    profile 'ROUND' places R ring verts on a circle; 'SQUARE' places 4 corner
    verts at 45-degree offsets scaled by sqrt(2), so the flat faces sit at the
    stroke's half-width - a box whose width matches the round tube's diameter,
    aligned with the drawing plane via up_axis.
    qnorm_batch (N,S,3), when given, holds per-sample Quill normals: frames
    are built in the painted orientation (width direction = normal x tangent,
    matching the Quill importer's compute_basis) instead of parallel
    transport, so ribbons stay flat and twist exactly like the source.
    Returns:
      verts_flat: (N*S*R, 3)
      quads:      (N*Q_per, 4)  where Q_per = (S-1)*R for tubes, (S-1) for ribbons
      caps:       (N*2, R) for tubes, None for ribbons
    Vert k for stroke s, ring step r is at index s*S*R + k*R + r."""
    N, S, _ = spine_batch.shape
    if qnorm_batch is not None:
        # Tangents (same averaging as _frames_along_batch).
        deltas = np.diff(spine_batch, axis=1)
        seg_t = deltas / np.maximum(np.linalg.norm(deltas, axis=-1, keepdims=True), 1e-12)
        tangents = np.empty_like(spine_batch)
        tangents[:, 0]  = seg_t[:, 0]
        tangents[:, -1] = seg_t[:, -1]
        if S > 2:
            avg = seg_t[:, :-1] + seg_t[:, 1:]
            tangents[:, 1:-1] = avg / np.maximum(
                np.linalg.norm(avg, axis=-1, keepdims=True), 1e-12)
        # Quill basis: width direction = stored normal x tangent.
        wdir = np.cross(qnorm_batch, tangents)
        wnrm = np.linalg.norm(wdir, axis=-1, keepdims=True)
        # Degenerate (normal parallel to tangent): any perpendicular will do.
        alt = np.cross(np.broadcast_to(np.array([0.0, 0.0, 1.0]), tangents.shape), tangents)
        alt_nrm = np.linalg.norm(alt, axis=-1, keepdims=True)
        alt2 = np.cross(np.broadcast_to(np.array([1.0, 0.0, 0.0]), tangents.shape), tangents)
        alt2 = alt2 / np.maximum(np.linalg.norm(alt2, axis=-1, keepdims=True), 1e-12)
        alt = np.where(alt_nrm > 1e-6, alt / np.maximum(alt_nrm, 1e-12), alt2)
        normals = np.where(wnrm > 1e-8, wdir / np.maximum(wnrm, 1e-12), alt)
        binormals = np.cross(tangents, normals)
        binormals = binormals / np.maximum(
            np.linalg.norm(binormals, axis=-1, keepdims=True), 1e-12)
    else:
        _, normals, binormals = _frames_along_batch(spine_batch, up_axis=up_axis)

    if profile == 'SQUARE':
        angles = np.pi / 4.0 + np.linspace(0.0, 2.0 * np.pi, R, endpoint=False)
        rad_mult = np.sqrt(2.0)
    else:
        angles = np.linspace(0.0, 2.0 * np.pi, R, endpoint=False)
        rad_mult = 1.0
    cos_a = np.cos(angles) * rad_mult
    sin_a = np.sin(angles) * rad_mult * float(thickness_ratio)

    ring = (cos_a[None, None, :, None] * normals[:, :, None, :]
            + sin_a[None, None, :, None] * binormals[:, :, None, :])
    verts = spine_batch[:, :, None, :] + radii_batch[:, :, None, None] * ring
    verts_flat = verts.reshape(N * S * R, 3)

    stroke_off = (np.arange(N) * (S * R)).astype(np.int64)           # (N,)

    if R < 3:
        s_idx = np.arange(S - 1)
        v00 = s_idx * R
        v01 = s_idx * R + 1
        v11 = (s_idx + 1) * R + 1
        v10 = (s_idx + 1) * R
        quads_local = np.stack([v00, v01, v11, v10], axis=1).astype(np.int64)
        quads = (quads_local[None, :, :] + stroke_off[:, None, None]).reshape(-1, 4)
        return verts_flat, quads, None

    s_idx = np.repeat(np.arange(S - 1), R)
    r_idx = np.tile(np.arange(R), S - 1)
    r2 = (r_idx + 1) % R
    v00 = s_idx * R + r_idx
    v01 = s_idx * R + r2
    v11 = (s_idx + 1) * R + r2
    v10 = (s_idx + 1) * R + r_idx
    quads_local = np.stack([v00, v01, v11, v10], axis=1).astype(np.int64)
    quads = (quads_local[None, :, :] + stroke_off[:, None, None]).reshape(-1, 4)

    start_cap_local = np.arange(R - 1, -1, -1, dtype=np.int64)
    end_cap_local   = np.arange((S - 1) * R, S * R, dtype=np.int64)
    starts = start_cap_local[None, :] + stroke_off[:, None]           # (N, R)
    ends   = end_cap_local[None, :]   + stroke_off[:, None]           # (N, R)
    caps = np.empty((N * 2, R), dtype=np.int64)
    caps[0::2] = starts
    caps[1::2] = ends
    return verts_flat, quads, caps


def build_tube(spine, radii, R, thickness_ratio=1.0, up_axis='Z'):
    """Return (verts (S*R,3), quads, start_cap, end_cap).
    R == 2 -> ribbon mode (single quad per segment, no caps).
    R >= 3 -> tube mode (ring of quads + n-gon end caps).
    thickness_ratio scales the binormal axis only - 1.0 is a perfect circle,
    0.0 flattens the cross-section to a ribbon, 0.1 makes a thin pancake.
    up_axis picks the local-space axis used as the frame's reference - lets
    the ribbon's flat side align with the drawing plane normal."""
    S = len(spine)
    _, normals, binormals = _frames_along(spine, up_axis=up_axis)

    angles = np.linspace(0.0, 2.0 * np.pi, R, endpoint=False)
    cos_a = np.cos(angles)
    sin_a = np.sin(angles) * float(thickness_ratio)

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
CHUNK_SIZE = 20  # strokes per progress yield in run_conversion_iter


def run_conversion(*args, **kwargs):
    """Non-streaming wrapper. Consumes the iter to completion. Use for the
    single Bake LOD0 operator where intra-LOD progress doesn't matter."""
    for _ in run_conversion_iter(*args, **kwargs):
        pass


def _prepare_stroke_data(gp_obj, curve_importance, thickness_delta_importance,
                         brick_detect_angle=10.0):
    """Read strokes, apply zero-thickness fallback, and compute per-stroke
    weights/scores. Also classifies near-straight constant-thickness strokes
    as 'bricks' (see _classify_brick). Returns (enriched, stats). Independent
    of total_verts / ring_verts / tube_scale / thickness_ratio / up_axis, so
    its output can be cached across multiple LODs of the same GP."""
    t0 = time.time()
    raw, stats = gather_stroke_data_world(gp_obj)
    if not raw:
        raise RuntimeError("No usable strokes on active object.")

    # Zero-thickness fallback.
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

    # Enrich each stroke with cum_w, arc_len, score, brick classification.
    brick_max_turn_rad = np.deg2rad(max(0.0, float(brick_detect_angle)))
    enriched = []
    zero_weight = 0
    for d in raw:
        pos = d["pos"]
        cum_w, tot_w, arc_len, turn_total, tv_thick = stroke_weights(
            pos, d["thick"], curve_importance, thickness_delta_importance)
        if tot_w <= 0 or arc_len <= 0:
            zero_weight += 1
            continue
        mean_thick = float(np.mean(d["thick"])) if d["thick"].size else 0.0
        peak_thick = float(np.max(d["thick"])) if d["thick"].size else 0.0
        score = arc_len * max(mean_thick, 1e-9)
        is_brick, body_thick = _classify_brick(d["thick"], turn_total, brick_max_turn_rad)
        d.update({"cum_w": cum_w, "tot_w": tot_w, "arc_len": arc_len,
                  "mean_thick": mean_thick, "peak_thick": peak_thick,
                  "score": score,
                  "turn_total": turn_total, "tv_thick": tv_thick,
                  "is_brick": is_brick, "body_thick": body_thick})
        enriched.append(d)
    stats["zero_weight"] = zero_weight
    if not enriched:
        raise RuntimeError("All strokes had zero weight.")
    n_bricks = sum(1 for d in enriched if d["is_brick"])
    stats["bricks"] = n_bricks

    elapsed = time.time() - t0
    print(f"[gp_to_mesh_lod] prepare '{gp_obj.name}': "
          f"{stats['strokes_total']} raw -> {len(enriched)} enriched "
          f"({n_bricks} brick) in {elapsed:.2f}s")
    return enriched, stats


def run_conversion_iter(gp_obj, total_verts, curve_importance, tube_scale, ring_verts,
                        thickness_delta_importance, output_suffix="_LOD0",
                        thickness_ratio=1.0, up_axis='Z',
                        force_hard_ends=False,
                        silhouette_comp=1.0, brick_detect_angle=10.0,
                        detail_angle=8.0,
                        absorb_dropped=True, absorb_max_scale=3.0,
                        keep_all=False, use_quill_orient=True,
                        cached_enriched=None, cached_stats=None):
    """Generator yielding (step, total, message) where total == 100. Step
    advances ~5 times during setup, then once per CHUNK_SIZE strokes during
    tube construction, then a couple more times during mesh write.

    If cached_enriched / cached_stats are provided, the read + thickness
    fallback + enrichment stages are skipped - the caller is expected to have
    run _prepare_stroke_data already. This is the 4x-saver for LOD set bakes."""
    R = max(2, int(ring_verts))
    spine_budget = max(3, int(total_verts))
    print(f"[gp_to_mesh_lod] {gp_obj.name}{output_suffix}: "
          f"ring_verts requested={ring_verts}, effective R={R}, "
          f"force_hard_ends={force_hard_ends}")

    if cached_enriched is not None:
        # Fast path - reuse pre-computed data.
        enriched = cached_enriched
        stats = cached_stats if cached_stats is not None else {}
        yield 6, 100, "Using cached enrichment"
    else:
        yield 0, 100, "Reading strokes"
        enriched, stats = _prepare_stroke_data(
            gp_obj, curve_importance, thickness_delta_importance,
            brick_detect_angle)
        yield 6, 100, "Enriched"

    # --- Keep/drop + spine allocation (need-based) ------------------------
    # Each stroke's "need" is the sample count required to represent its
    # curvature within this LOD's detail angle: a straight brick needs 2, a
    # stroke that turns 360 degrees at a 45-degree error budget needs ~10.
    # Strokes are kept in score order (arc_len * thickness = silhouette
    # importance) and each kept stroke gets its full need up front - so big
    # silhouette-defining strokes stay well-shaped at low LODs while small
    # strokes drop out entirely, instead of every stroke degrading into a
    # 3-sample diamond together. Leftover budget then tops kept strokes back
    # up toward their source point counts (by arc length), which is what
    # keeps LOD0 near-exact.
    theta = np.deg2rad(max(0.5, float(detail_angle)))
    # Thickness tolerance rides the same LOD ramp: at 8 deg detail angle a
    # stroke gets a sample per ~10% thickness swing, at 45 deg per ~50%.
    # Without this, a long straight stroke with a fat bulge gets 3 samples
    # and the bulge lerps across the whole strip - the giant-tent artifact.
    rel_tol = min(0.5, max(0.1, float(detail_angle) / 90.0))
    n_enriched = len(enriched)
    needs = np.empty(n_enriched, dtype=np.int64)
    for i, d in enumerate(enriched):
        if d.get("is_brick"):
            needs[i] = 2
        else:
            need = 2 + int(np.ceil(d["turn_total"] / theta))
            peak = d.get("peak_thick", 0.0)
            tv = d.get("tv_thick", 0.0)
            if peak > 1e-9 and tv > 0.0:
                need += int(np.ceil(tv / (peak * rel_tol)))
            needs[i] = max(3, min(need, len(d["pos"])))
    scores = np.array([d["score"] for d in enriched])
    if keep_all:
        # LOD0 contract: NOTHING is ever dropped. Every stroke is kept; if
        # the budget can't cover every stroke's need, allocations shrink
        # proportionally below (strokes get simpler, never deleted).
        keep_set = set(range(n_enriched))
        used = int(needs.sum())
    else:
        keep_set = set()
        used = 0
        for i in np.argsort(-scores):
            cost = int(needs[i])
            if used + cost > spine_budget:
                continue
            keep_set.add(int(i))
            used += cost
        if not keep_set:  # budget below the cheapest stroke - keep the best one
            best = int(np.argmax(scores))
            keep_set.add(best)
            used = int(needs[best])
    dropped = n_enriched - len(keep_set)
    stats["dropped_by_budget"] = dropped
    stats["kept"] = len(keep_set)

    print(f"[gp_to_mesh_lod] {output_suffix}: "
          f"{stats.get('strokes_total', '?')} found  "
          f"- {stats.get('strokes_short', '?')} short  "
          f"- {stats.get('zero_weight', '?')} zero-weight  "
          f"- {dropped} dropped by budget  "
          f"= {len(keep_set)} kept (detail angle {float(detail_angle):.1f} deg).")

    dropped_strokes = [d for i, d in enumerate(enriched) if i not in keep_set]
    spine_alloc = [int(needs[i]) for i in range(n_enriched) if i in keep_set]
    enriched = [d for i, d in enumerate(enriched) if i in keep_set]
    n_bricks = sum(1 for d in enriched if d.get("is_brick"))
    if n_bricks:
        print(f"[gp_to_mesh_lod] {output_suffix}: {n_bricks} brick stroke(s) "
              f"at 2 spine samples each.")

    # Budget overspend (keep_all path): every stroke stays. Two-phase squeeze:
    # every round stroke gets a 3-sample base, then the remaining budget goes
    # preferentially to strokes whose SHAPE needs it (weights = need - base).
    # Straight constant-width strokes stay at 3 - they're already exact - so
    # their verts flow to bulged/tapered/curvy strokes, which keeps the tent
    # artifact away even under a tight budget. Anything left after shape
    # needs are met falls through to the arc-length top-up below.
    if used > spine_budget:
        round_idx = [k for k, d in enumerate(enriched) if not d.get("is_brick")]
        round_budget = max(0, spine_budget - 2 * (len(enriched) - len(round_idx)))
        base = [min(3, len(enriched[k]["pos"])) for k in round_idx]
        want = [max(0, spine_alloc[k] - b) for k, b in zip(round_idx, base)]
        rem = round_budget - sum(base)
        if rem > 0 and sum(want) > 0:
            grant = largest_remainder(want, rem, min_each=0)
        else:
            grant = [0] * len(round_idx)
        for k_r, k in enumerate(round_idx):
            spine_alloc[k] = min(base[k_r] + min(int(grant[k_r]), want[k_r]),
                                 len(enriched[k]["pos"]))
        used = sum(spine_alloc)
        print(f"[gp_to_mesh_lod] {output_suffix}: keep-all budget squeeze - "
              f"3-sample base + shape-need priority ({used} spine verts total).")

    # Top up with leftover budget, proportional to arc length, capped at each
    # round stroke's source point count (bricks stay at 2). When alloc ==
    # source the build loop uses the source points directly, so at LOD0 with
    # a generous budget the tube spine is exactly the GP stroke.
    leftover = spine_budget - used
    if leftover > 0 and enriched:
        room = np.array([0 if d.get("is_brick") else max(0, len(d["pos"]) - a)
                         for d, a in zip(enriched, spine_alloc)], dtype=np.int64)
        grant_total = min(leftover, int(room.sum()))
        if grant_total > 0:
            w = np.array([d["arc_len"] if r > 0 else 0.0
                          for d, r in zip(enriched, room)])
            ws = w.sum() or 1.0
            grant = np.minimum(np.floor(grant_total * w / ws).astype(np.int64), room)
            rem = grant_total - int(grant.sum())
            if rem > 0:
                for k in np.argsort(-w):
                    if rem <= 0:
                        break
                    give = min(int(room[k] - grant[k]), rem)
                    grant[k] += give
                    rem -= give
            for k in range(len(enriched)):
                spine_alloc[k] += int(grant[k])

    # --- Absorb dropped strokes into their nearest survivor ---------------
    # A dropped stroke shouldn't just vanish: if it ran alongside a kept
    # stroke (e.g. 40 thin strokes bundling into one ring), the survivor's
    # tube inflates locally to cover the dropped stroke's footprint
    # (distance to the survivor's spine + the dropped stroke's own radius).
    # Capped at absorb_max_scale x the survivor's local radius, so strokes
    # too far away or too big to credibly merge are simply dropped instead
    # of ballooning the survivor.
    absorb_r_list = None
    if (absorb_dropped and dropped_strokes and enriched
            and float(absorb_max_scale) > 1.0):
        ts = float(tube_scale)
        stride = 2
        entries = []  # flat kd index -> (kept stroke k, source point i)
        for k, d in enumerate(enriched):
            for i in range(0, len(d["pos"]), stride):
                entries.append((k, i))
        kd = mu_kdtree.KDTree(len(entries))
        for e_idx, (k, i) in enumerate(entries):
            kd.insert(Vector(enriched[k]["pos"][i]), e_idx)
        kd.balance()

        absorb_r_list = [np.zeros(len(d["pos"])) for d in enriched]
        n_absorbed = 0
        for dd in dropped_strokes:
            for p, r in zip(dd["pos"][::stride], dd["thick"][::stride]):
                _co, e_idx, dist = kd.find(Vector(p))
                if e_idx is None:
                    continue
                k, i = entries[e_idx]
                base_r = float(enriched[k]["thick"][i]) * ts
                required = float(dist) + float(r) * ts
                if required <= base_r:
                    continue  # already covered by the survivor
                if base_r <= 0.0 or required > base_r * float(absorb_max_scale):
                    continue  # too far / too big to credibly merge
                if required > absorb_r_list[k][i]:
                    absorb_r_list[k][i] = required
                    n_absorbed += 1
        # Dilate one sample each way so linear resampling can't halve an
        # isolated inflation spike into invisibility.
        for a in absorb_r_list:
            if a.any():
                a[1:]  = np.maximum(a[1:],  a[:-1])
                a[:-1] = np.maximum(a[:-1], a[1:])
        if n_absorbed:
            print(f"[gp_to_mesh_lod] {output_suffix}: absorbed silhouette of "
                  f"{len(dropped_strokes)} dropped stroke(s) into survivors "
                  f"({n_absorbed} inflation points).")

    yield 8, 100, "Allocated; finding/creating output mesh"

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

    if out_obj.parent != gp_obj:
        out_obj.parent = gp_obj
    # Always reset parent_inverse to identity - verts are in GP local space,
    # so the LOD should simply inherit parent's world transform. Forcing this
    # every bake also migrates LODs originally baked with the old world-space
    # code (which had matrix_parent_inverse = gp.matrix_world.inverted()).
    out_obj.matrix_parent_inverse = Matrix.Identity(4)
    out_obj.matrix_local = Matrix.Identity(4)

    # Tube-building - batched. Group strokes by output spine count, then run
    # the frame computation + ring placement + face indexing as a single
    # vectorized op per group. For N strokes with S spine samples each, the
    # parallel transport loop is now O(S) Python iterations instead of O(N*S).
    all_verts  = []
    all_quads  = []
    all_caps   = []   # 2D arrays (n_caps_group, R)
    all_colors = []
    vbase = 0
    n_strokes = len(enriched)
    BUILD_LO, BUILD_HI = 10, 90
    t_build = time.time()

    # Group stroke indices by (allocated spine count, cross-section kind).
    # Kind comes from the brick detector and - when Fetch Quill Orientation
    # has stamped the data - the Quill brush type: ribbons bake as flat
    # oriented quad strips (R=2 at every LOD, like the Quill mesh importer),
    # cubes as square tubes, ellipses as flattened tubes, cylinders as round
    # tubes with painted orientation.
    groups = {}
    for j in range(n_strokes):
        key = (int(spine_alloc[j]), _stroke_kind(enriched[j], use_quill_orient))
        groups.setdefault(key, []).append(j)

    kind_counts = {}
    for (_sg, kind), idxs in groups.items():
        kind_counts[kind] = kind_counts.get(kind, 0) + len(idxs)
    print(f"[gp_to_mesh_lod] {output_suffix}: cross-sections {kind_counts}")

    processed = 0
    group_order = sorted(groups.keys())
    for (S_group, kind) in group_order:
        stroke_indices = groups[(S_group, kind)]
        N = len(stroke_indices)
        grp_is_brick = (kind == 'BRICK')
        if kind in ('BRICK', 'CUBE'):
            R_grp, profile = 4, 'SQUARE'
        elif kind == 'RIBBON':
            R_grp, profile = 2, 'ROUND'  # R=2 -> flat quad strip, no caps
        else:
            R_grp, profile = R, 'ROUND'
        # Ellipse brush: flattened cross-section (matches the Quill mesh
        # importer's aspect for the ellipse brush).
        aspect = thickness_ratio * (0.3 if kind == 'ELLIPSE' else 1.0)
        # Mean-width compensation (Cauchy) only applies to round-ish
        # profiles; ribbons/squares are exact by construction.
        width_comp = _mean_width_comp(R_grp) if kind in ('ROUND', 'CYL', 'ELLIPSE') else 1.0
        grp_use_qn = kind in ('RIBBON', 'ELLIPSE', 'CUBE', 'CYL')

        # Build per-group batch arrays (this loop is the remaining per-stroke
        # Python cost - resampling - but it's much lighter than build_tube was).
        spine_batch  = np.empty((N, S_group, 3), dtype=np.float64)
        thick_batch  = np.empty((N, S_group),    dtype=np.float64)
        colors_batch = np.empty((N, S_group, 4), dtype=np.float64)
        dev_batch    = np.zeros((N, S_group),    dtype=np.float64)
        absorb_batch = np.zeros((N, S_group),    dtype=np.float64)
        qnorm_batch  = np.zeros((N, S_group, 3), dtype=np.float64) if grp_use_qn else None
        for k, j in enumerate(stroke_indices):
            d = enriched[j]
            n_spine = spine_alloc[j]
            a_src = absorb_r_list[j] if absorb_r_list is not None else None
            has_absorb = a_src is not None and a_src.any()
            qn_src = d.get("qnorm") if grp_use_qn else None
            if grp_is_brick:
                # Straight stroke: endpoints only, constant body thickness.
                spine_batch[k, 0]  = d["pos"][0]
                spine_batch[k, -1] = d["pos"][-1]
                thick_batch[k]     = d["body_thick"]
                colors_batch[k, 0]  = d["col"][0]
                colors_batch[k, -1] = d["col"][-1]
                if has_absorb:
                    absorb_batch[k] = float(a_src.max())
            elif n_spine >= len(d["pos"]):
                spine_batch[k]  = d["pos"]
                thick_batch[k]  = d["thick"]
                colors_batch[k] = d["col"]
                if has_absorb:
                    absorb_batch[k] = a_src
                if qn_src is not None:
                    qnorm_batch[k] = qn_src
            else:
                idx, t = compute_resample_keys(d["cum_w"], n_spine)
                spine_batch[k]  = interp_along(d["pos"],   idx, t)
                thick_batch[k]  = _thickness_cell_average(d["pos"], d["thick"], idx, t)
                colors_batch[k] = interp_along(d["col"],   idx, t)
                if silhouette_comp > 0.0:
                    dev_batch[k] = _resample_deviation(d["pos"], spine_batch[k], idx)
                if has_absorb:
                    absorb_batch[k] = interp_along(a_src, idx, t)
                if qn_src is not None:
                    qnorm_batch[k] = interp_along(qn_src, idx, t)
        if qnorm_batch is not None:
            # Renormalize lerped normals (unit in the source data).
            qn_len = np.linalg.norm(qnorm_batch, axis=-1, keepdims=True)
            qnorm_batch = qnorm_batch / np.maximum(qn_len, 1e-12)

        if grp_is_brick:
            radii_batch = thick_batch * float(tube_scale)
        else:
            # Width compensation keeps the R-gon reading as wide as the source
            # stroke; deviation compensation re-inflates the silhouette
            # exactly where spine decimation cut corners off. The dev term is
            # capped at 2x the stroke's true radius so a heavily-squeezed
            # stroke can never balloon far past its painted width and occlude
            # its neighbors.
            base_radii = thick_batch * float(tube_scale) * width_comp
            dev_add = np.minimum(dev_batch * float(silhouette_comp),
                                 base_radii * 2.0)
            radii_batch = base_radii + dev_add
        # Absorption: survivor tubes locally inflate to cover the footprint
        # of dropped neighbors (absorb_batch already holds world radii).
        if absorb_r_list is not None:
            radii_batch = np.maximum(radii_batch, absorb_batch)

        # Hard flat ends - always for bricks, opt-in for round strokes:
        # (1) clamp the end-sample radius up to its neighbor so the tube
        # doesn't taper to a point, then (2) extrude the first and last spine
        # samples outward along the local tangent by half a radius. The
        # extrusion matches how Blender's GP draws a flat cap - the visible
        # square extends past the actual data endpoint by r/2.
        if (grp_is_brick or force_hard_ends) and S_group >= 2:
            radii_batch[:, 0]  = np.maximum(radii_batch[:, 0],  radii_batch[:, 1])
            radii_batch[:, -1] = np.maximum(radii_batch[:, -1], radii_batch[:, -2])

            # Start tangent points from spine[0] -> spine[1]; we move spine[0]
            # backward (negative tangent) so the tube extends past the data.
            d_start = spine_batch[:, 1] - spine_batch[:, 0]
            t_start = d_start / np.maximum(
                np.linalg.norm(d_start, axis=-1, keepdims=True), 1e-12)
            spine_batch[:, 0] = (spine_batch[:, 0]
                                 - t_start * (radii_batch[:, 0:1] * 0.5))

            # End tangent points from spine[-2] -> spine[-1]; spine[-1] moves
            # forward (positive tangent) to extend past the data.
            d_end = spine_batch[:, -1] - spine_batch[:, -2]
            t_end = d_end / np.maximum(
                np.linalg.norm(d_end, axis=-1, keepdims=True), 1e-12)
            spine_batch[:, -1] = (spine_batch[:, -1]
                                  + t_end * (radii_batch[:, -1:] * 0.5))

        verts_grp, quads_grp, caps_grp = build_tube_batch(
            spine_batch, radii_batch, R_grp,
            thickness_ratio=aspect,
            up_axis=up_axis,
            profile=profile,
            qnorm_batch=qnorm_batch)

        all_verts.append(verts_grp)
        # Colors: spine-sample colors repeated R_grp times each.
        all_colors.append(np.repeat(colors_batch.reshape(N * S_group, 4), R_grp, axis=0))
        all_quads.append(quads_grp + vbase)
        if caps_grp is not None:
            all_caps.append(caps_grp + vbase)
        vbase += verts_grp.shape[0]

        processed += N
        prog = BUILD_LO + int((BUILD_HI - BUILD_LO) * processed / max(1, n_strokes))
        yield prog, 100, f"{kind.lower()}s {processed}/{n_strokes} (group S={S_group}, N={N})"
    t_build = time.time() - t_build

    yield 92, 100, "Writing mesh"
    t_w = time.time()
    verts_concat  = np.concatenate(all_verts,  axis=0)
    colors_concat = np.concatenate(all_colors, axis=0)
    quads_concat  = np.concatenate(all_quads,  axis=0) if all_quads else np.zeros((0, 4), dtype=np.int64)
    # Caps are 2D arrays (n_caps_group, R_grp) per group. Widths can differ
    # between groups (4 for bricks vs R for round tubes), so they stay a list
    # instead of one concatenated array.
    n_caps    = sum(c.shape[0] for c in all_caps)
    cap_loops = sum(c.size     for c in all_caps)

    n_verts  = verts_concat.shape[0]
    n_quads  = quads_concat.shape[0]
    n_polys  = n_quads + n_caps
    n_loops  = n_quads * 4 + cap_loops

    fast_path_ok = False
    try:
        if n_verts and n_polys:
            # Build flat loop indices: quads first (groups of 4), then caps
            # (each group contributes rows of its own width).
            loop_idx = np.empty(n_loops, dtype=np.int32)
            loop_idx[:n_quads * 4] = quads_concat.ravel()
            poly_sizes = np.empty(n_polys, dtype=np.int32)
            poly_sizes[:n_quads] = 4
            cur_loop = n_quads * 4
            cur_poly = n_quads
            for c in all_caps:
                loop_idx[cur_loop:cur_loop + c.size] = c.ravel()
                poly_sizes[cur_poly:cur_poly + c.shape[0]] = c.shape[1]
                cur_loop += c.size
                cur_poly += c.shape[0]
            poly_starts = np.zeros(n_polys, dtype=np.int32)
            poly_starts[1:] = np.cumsum(poly_sizes[:-1])

            me.vertices.add(n_verts)
            me.vertices.foreach_set("co", verts_concat.astype(np.float32).ravel())
            me.loops.add(n_loops)
            me.loops.foreach_set("vertex_index", loop_idx)
            me.polygons.add(n_polys)
            me.polygons.foreach_set("loop_start", poly_starts)
            me.polygons.foreach_set("loop_total", poly_sizes)
            me.update()
            me.validate(verbose=False)
            fast_path_ok = True
    except Exception as e:
        print(f"[gp_to_mesh_lod] fast mesh write failed ({e!r}); falling back to from_pydata")
        me.clear_geometry()

    if not fast_path_ok:
        # Slow fallback.
        all_faces = quads_concat.tolist()
        for c in all_caps:
            all_faces.extend(c.tolist())  # each cap row is one n-gon face
        me.from_pydata(verts_concat.tolist(), [], all_faces)
        me.update()
    t_mesh = time.time() - t_w

    t_w = time.time()
    if len(me.polygons):
        smooth_flags = np.ones(len(me.polygons), dtype=bool)
        me.polygons.foreach_set("use_smooth", smooth_flags)
        me.update()
    t_smooth = time.time() - t_w

    yield 96, 100, "Vertex colors"
    t_w = time.time()
    ca = me.color_attributes.new(name=COLOR_ATTR_NAME, type='FLOAT_COLOR', domain='POINT')
    ca.data.foreach_set("color", colors_concat.astype(np.float32).ravel())

    # Assign the shared vertex-color material if the mesh has no slot yet.
    # Existing material slots are left alone so user customizations survive
    # rebakes. If the user wants the default back, clear the slots manually
    # and re-bake.
    if len(me.materials) == 0:
        me.materials.append(_ensure_lod_material())
    t_color = time.time() - t_w

    fast = "fast" if fast_path_ok else "fallback"
    jit  = "numba" if HAVE_NUMBA else "numpy"
    print(f"[gp_to_mesh_lod] '{out_obj.name}': "
          f"{n_verts} mesh verts "
          f"({sum(spine_alloc)} spine x {R} ring), "
          f"{len(enriched)} strokes kept"
          + (f" ({dropped} dropped)" if dropped else "")
          + f"  | timings: build[{jit}]={t_build:.2f}s mesh[{fast}]={t_mesh:.2f}s "
            f"smooth={t_smooth:.2f}s color={t_color:.2f}s")
    yield 100, 100, "Done"


# --------------------------------------------------------------------------- #
# Blender types                                                               #
# --------------------------------------------------------------------------- #
def _is_gp(obj):
    return obj is not None and obj.type in {'GPENCIL', 'GREASEPENCIL'}


def _lod_outputs_for(gp_obj):
    """All existing mesh outputs whose name matches '<gp>_LOD*'."""
    prefix = gp_obj.name + "_LOD"
    return [o for o in bpy.data.objects if o.name.startswith(prefix) and o.type == 'MESH']


def _resolve_gp(obj):
    """If `obj` is a GP, return it. If `obj` is a mesh that is a child of a GP
    (i.e. one of our baked LOD outputs), return that parent GP. Else None."""
    if obj is None:
        return None
    if _is_gp(obj):
        return obj
    if obj.parent is not None and _is_gp(obj.parent):
        return obj.parent
    return None


def _active_gp(context):
    """Resolve the 'current' GP for panel display: active if GP, else parent
    if active is one of its LOD meshes."""
    return _resolve_gp(context.active_object)


def _get_selected_objects_robust(context):
    """Some panel contexts report context.selected_objects as empty even when
    the 3D viewport has a multi-selection. This iterates the view layer and
    asks each object .select_get() directly, then falls back to
    context.selected_objects."""
    sel = []
    vl = getattr(context, 'view_layer', None)
    if vl is not None:
        for o in vl.objects:
            try:
                if o.select_get():
                    sel.append(o)
            except (RuntimeError, AttributeError):
                pass
    if not sel:
        try:
            sel = list(context.selected_objects)
        except Exception:
            sel = []
    return sel


def _selected_gp_objects(context):
    """Every GP object 'represented' by the current selection. Selected GPs
    count directly; selected LOD meshes resolve to their parent GP. Falls
    back to the active object's resolved GP if nothing else qualifies."""
    seen = set()
    out = []
    for o in _get_selected_objects_robust(context):
        gp = _resolve_gp(o)
        if gp is not None and gp.name not in seen:
            out.append(gp)
            seen.add(gp.name)
    if not out:
        gp = _active_gp(context)
        if gp is not None:
            out.append(gp)
    return out


def _count_source_points(gp_obj):
    """Sum of points across every stroke on the active frame of every layer.
    Cheap (no array allocation); used to convert verts_percentage -> total_verts."""
    if gp_obj is None:
        return 0
    n = 0
    if gp_obj.type == 'GPENCIL':
        for layer in gp_obj.data.layers:
            frame = layer.active_frame
            if frame is None:
                continue
            for s in frame.strokes:
                n += len(s.points)
    elif gp_obj.type == 'GREASEPENCIL':
        for layer in gp_obj.data.layers:
            frame = layer.current_frame()
            if frame is None:
                continue
            for s in frame.drawing.strokes:
                n += len(s.points)
    return n


def _verts_from_percentage(gp_obj, pct):
    """Convert verts_percentage -> absolute total spine verts for this object."""
    src = _count_source_points(gp_obj)
    return max(3, int(round(src * (pct / 100.0))))


# Settings defaults — used to detect "uncustomized" GP objects so we can
# auto-inherit the active object's settings on bake. Must mirror the
# defaults declared on GPLODSettings below.
SETTINGS_DEFAULTS = {
    "verts_percentage":           100.0,
    "curve_importance":             5.0,
    "thickness_delta_importance":  10.0,
    "tube_scale":                   1.0,
    "ring_verts":                   6,
    "lod_size_expansion":           1.0,
    "min_lod_ratio":                0.10,
    "thickness_ratio":              1.0,
    "cross_section_up_axis":        'Z',
    "force_hard_ends":              False,
    "min_ring_verts":               2,
    "brick_detect_angle":           0.0,
    "silhouette_compensation":      1.0,
    "detail_angle_lod0":            8.0,
    "detail_angle_min":            45.0,
    "absorb_dropped":              True,
    "absorb_max_scale":             3.0,
    "use_quill_orientation":       True,
}


def _settings_are_default(s):
    for k, v in SETTINGS_DEFAULTS.items():
        if getattr(s, k) != v:
            return False
    return True


def _copy_settings(src, dst):
    for k in SETTINGS_DEFAULTS:
        setattr(dst, k, getattr(src, k))


def _auto_inherit_from_active(context):
    """For every selected GP whose settings are still at all defaults, copy
    the active GP's settings onto it. Returns the count of updated objects.
    'active' here means the GP resolved from the active object — so even if
    the user has an LOD mesh active, its parent GP is the source."""
    active = _active_gp(context)
    if active is None:
        return 0
    src = active.gp_lod_settings
    count = 0
    for gp in _selected_gp_objects(context):
        if gp == active:
            continue
        if _settings_are_default(gp.gp_lod_settings):
            _copy_settings(src, gp.gp_lod_settings)
            count += 1
    return count


def _delete_lod_outputs(gp_obj):
    for o in _lod_outputs_for(gp_obj):
        mesh = o.data
        bpy.data.objects.remove(o, do_unlink=True)
        if mesh is not None and mesh.users == 0:
            bpy.data.meshes.remove(mesh)


def _max_lod_in_selection(context):
    """Highest LOD index baked under any currently-selected GP object, or -1
    if no LODs are present."""
    max_idx = -1
    for gp in _selected_gp_objects(context):
        prefix = gp.name + "_LOD"
        for o in _lod_outputs_for(gp):
            try:
                idx = int(o.name[len(prefix):])
            except ValueError:
                continue
            if idx > max_idx:
                max_idx = idx
    return max_idx


def _set_hidden(obj, hidden):
    """Hide via view-layer toggle; fall back to global hide if there's no
    active view layer (e.g. during background ops)."""
    try:
        obj.hide_set(hidden)
    except RuntimeError:
        obj.hide_viewport = hidden


GP_DIMMED_OPACITY = 0.1
GP_FULL_OPACITY   = 1.0


def _set_gp_layers_opacity(gp, opacity):
    """Apply a uniform opacity to every layer of a GP object (both classic
    GPENCIL and v3 GREASEPENCIL). Silently no-ops if the type is wrong or
    the layers can't be written."""
    if not _is_gp(gp):
        return
    try:
        for layer in gp.data.layers:
            try:
                layer.opacity = opacity
            except Exception:
                pass
    except Exception:
        pass


def _apply_preview_lod(context, target):
    """Toggle LOD mesh visibility and dim/restore GP layer opacity.

    target < 0  -> every LOD hidden, GP layer opacity restored to 1.0.
    target >= 0 -> only that LOD level visible, GP layer opacity dimmed to 0.1
                   so the LOD shows through but the GP is still selectable."""
    for gp in _selected_gp_objects(context):
        _set_gp_layers_opacity(gp, GP_FULL_OPACITY if target < 0 else GP_DIMMED_OPACITY)
        prefix = gp.name + "_LOD"
        for o in _lod_outputs_for(gp):
            try:
                idx = int(o.name[len(prefix):])
            except ValueError:
                continue
            visible = (target >= 0) and (idx == target)
            o.hide_viewport = not visible


class GPLODSceneSettings(bpy.types.PropertyGroup):
    # Plain int storage - no update callback. The preview operator below
    # is what actually applies visibility changes. Letting the property carry
    # an update callback was causing the slider to fight with the viewport.
    preview_lod: bpy.props.IntProperty(
        name="Preview LOD",
        default=-1, min=-1, max=max(0, len(LOD_RING_SCHEDULE) - 1),
        description="Current preview state. Changed via the Preview buttons in the panel.",
    )


class GPLODSettings(bpy.types.PropertyGroup):
    verts_percentage: bpy.props.FloatProperty(
        name="Verts %",
        default=100.0, min=1.0, soft_max=200.0, max=1000.0,
        subtype='PERCENTAGE',
        description="LOD0 spine vert count as a percentage of the GP object's source point count. 100 = 1:1 with source; 50 = half.",
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
        description="Cross-section verts for the single Bake. 2 = flat ribbon (no caps). The LOD Set ignores this and follows LOD_RING_SCHEDULE.",
    )
    lod_size_expansion: bpy.props.FloatProperty(
        name="Min LOD Size Multiplier",
        default=1.0, min=1.0, soft_max=8.0,
        description="Extra artistic radius multiplier ramped in at low LODs (1 = off). Silhouette Compensation now handles measured geometry loss automatically; use this only for additional stylistic chunkiness.",
    )
    brick_detect_angle: bpy.props.FloatProperty(
        name="Brick Detect Angle",
        default=0.0, min=0.0, soft_max=45.0, max=180.0,
        description="Strokes whose total turning is under this many degrees (and whose body thickness is near-constant) bake as 8-vert square-profile 'bricks' with flat ends at every LOD. 0 (default) disables detection; ~10 is a good starting value when enabling.",
    )
    silhouette_compensation: bpy.props.FloatProperty(
        name="Silhouette Compensation",
        default=1.0, min=0.0, soft_max=2.0,
        description="Re-inflate tube radius where spine decimation cut geometry away, by the measured deviation from the source stroke. 1.0 = exact compensation, 0 = off. Applies on top of the automatic ring-count width matching.",
    )
    detail_angle_lod0: bpy.props.FloatProperty(
        name="LOD0 Detail Angle",
        default=8.0, min=0.5, soft_max=45.0, max=90.0,
        description="Angular error budget per spine segment at LOD0. Each stroke gets enough samples to represent its total curvature within this angle; important strokes are kept fully-shaped while small strokes are dropped when the vert budget runs out.",
    )
    detail_angle_min: bpy.props.FloatProperty(
        name="Min LOD Detail Angle",
        default=45.0, min=1.0, soft_max=90.0, max=180.0,
        description="Angular error budget at the lowest LOD (intermediate LODs interpolate). Larger = big strokes simplify harder; smaller = big strokes hold their shape longer while more small strokes are dropped instead.",
    )
    absorb_dropped: bpy.props.BoolProperty(
        name="Absorb Dropped Strokes",
        default=True,
        description="When a stroke is dropped at a low LOD, locally inflate the nearest surviving stroke's tube to cover its footprint - bundles of parallel strokes collapse into fewer, fatter tubes instead of leaving holes in the silhouette.",
    )
    absorb_max_scale: bpy.props.FloatProperty(
        name="Max Absorb Scale",
        default=3.0, min=1.0, soft_max=10.0,
        description="Cap on absorption inflation, as a multiple of the survivor's own radius. Dropped strokes needing more than this to be covered are simply dropped.",
    )
    use_quill_orientation: bpy.props.BoolProperty(
        name="Use Quill Orientation",
        default=True,
        description="When quill_normal/quill_brush attributes are present (run Fetch Quill Orientation first), build cross-sections in the painted orientation: ribbons bake as flat twisting quad strips, ellipses flattened, cubes square, cylinders round.",
    )
    min_lod_ratio: bpy.props.FloatProperty(
        name="Min LOD Vert Ratio",
        default=0.10, min=0.001, max=1.0, soft_min=0.005, soft_max=0.5,
        description="Spine vert count at the lowest LOD = (Total Stroke Verts) x (this ratio). Intermediate LODs interpolate geometrically: 0.01 gives 100% / 21.5% / 4.6% / 1% across 4 LODs.",
    )
    thickness_ratio: bpy.props.FloatProperty(
        name="Thickness Ratio",
        default=1.0, min=0.0, soft_max=2.0, max=5.0,
        description="Cross-section depth. 1.0 = perfect circle, 0.0 = totally flat ribbon, 0.1 = very thin pancake.",
    )
    cross_section_up_axis: bpy.props.EnumProperty(
        name="Up Axis",
        items=[
            ('X', "Local X", "Use GP-local X as the cross-section reference (drawings on YZ plane)"),
            ('Y', "Local Y", "Use GP-local Y as the cross-section reference (drawings on XZ plane / front view)"),
            ('Z', "Local Z", "Use GP-local Z as the cross-section reference (drawings on XY plane / top view)"),
        ],
        default='Z',
        description="Pick the GP-local axis that's perpendicular to your drawing plane. The ribbon's flat side will align with the other two axes.",
    )
    force_hard_ends: bpy.props.BoolProperty(
        name="Force Hard Ends",
        default=False,
        description="Clamp endpoint radii up to neighbor + extrude the spine ends outward by half a radius along the tangent, so a flat-capped GP stroke renders as a hard square tube end (no taper, cap extends past the data endpoint to match GP's flat-cap rendering).",
    )
    min_ring_verts: bpy.props.IntProperty(
        name="Min Ring Verts",
        default=2, min=2, soft_max=12, max=32,
        description="Floor the ring vert count for every LOD level. With default 2 the LOD_RING_SCHEDULE is used as-is; raise to 3 to force triangular cross-section even at the lowest LOD instead of falling to a flat ribbon.",
    )


class OBJECT_OT_gp_to_mesh_lod_bake(bpy.types.Operator):
    """Bake LOD0 for every selected GP object using each one's OWN stored
    settings. Use the Override Settings button to mirror the active object's
    settings onto the rest of the selection beforehand."""
    bl_idname = "object.gp_to_mesh_lod_bake"
    bl_label = "Bake LOD0"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return bool(_selected_gp_objects(context))

    def execute(self, context):
        gps = _selected_gp_objects(context)
        if not gps:
            self.report({'ERROR'}, "No GP objects selected.")
            return {'CANCELLED'}
        inherited = _auto_inherit_from_active(context)
        if inherited:
            self.report({'INFO'},
                        f"Inherited active settings on {inherited} default GP(s).")
        for gp_obj in gps:
            s = gp_obj.gp_lod_settings
            total_verts = _verts_from_percentage(gp_obj, s.verts_percentage)
            # Apply min_ring_verts here too so single Bake LOD0 honors the floor.
            eff_ring = max(int(s.ring_verts), int(s.min_ring_verts))
            try:
                run_conversion(gp_obj, total_verts, s.curve_importance,
                               s.tube_scale, eff_ring,
                               s.thickness_delta_importance,
                               output_suffix="_LOD0",
                               thickness_ratio=s.thickness_ratio,
                               up_axis=s.cross_section_up_axis,
                               force_hard_ends=s.force_hard_ends,
                               silhouette_comp=0.0,  # LOD0: true widths only
                               brick_detect_angle=s.brick_detect_angle,
                               detail_angle=s.detail_angle_lod0,
                               absorb_dropped=s.absorb_dropped,
                               absorb_max_scale=s.absorb_max_scale,
                               keep_all=True,
                               use_quill_orient=s.use_quill_orientation)
            except RuntimeError as e:
                self.report({'ERROR'}, f"{gp_obj.name}: {e}")
                return {'CANCELLED'}
        return {'FINISHED'}


def _bake_set_iter(gp_names):
    """Generator: bakes the full LOD set for every GP object in `gp_names`,
    using each one's OWN stored gp_lod_settings. Yields (composite_step,
    composite_total, message) frequently enough that a modal driver can keep
    its progress bar moving."""
    n_lods = len(LOD_RING_SCHEDULE)
    n_obj  = len(gp_names)
    composite_total = max(1, n_obj * n_lods * 100)

    for obj_idx, gp_name in enumerate(gp_names):
        gp_obj = bpy.data.objects.get(gp_name)
        if gp_obj is None or not _is_gp(gp_obj):
            continue
        t_obj = time.time()
        s = gp_obj.gp_lod_settings
        total_verts = _verts_from_percentage(gp_obj, s.verts_percentage)

        # Diagnostic: report what we actually read off this GP. If
        # min_ring_verts here disagrees with what's shown in the panel, the
        # panel is editing a different object's settings.
        print(f"[gp_to_mesh_lod] bake-set on '{gp_obj.name}': "
              f"verts_pct={s.verts_percentage}  ring_verts={s.ring_verts}  "
              f"min_ring_verts={s.min_ring_verts}  "
              f"force_hard_ends={s.force_hard_ends}")

        # Read + enrich ONCE per GP, reuse across all LODs (4x read savings).
        try:
            cached_enriched, cached_stats = _prepare_stroke_data(
                gp_obj, s.curve_importance, s.thickness_delta_importance,
                s.brick_detect_angle)
        except RuntimeError as e:
            print(f"[gp_to_mesh_lod] skip {gp_obj.name}: {e}")
            continue

        for i, ring in enumerate(LOD_RING_SCHEDULE):
            vmul = _lod_vert_multiplier(i, n_lods, s.min_lod_ratio)
            verts = max(3, int(round(total_verts * vmul)))
            t = (i / (n_lods - 1)) if n_lods > 1 else 0.0
            expansion = 1.0 + (s.lod_size_expansion - 1.0) * (t * t)
            eff_tube_scale = s.tube_scale * expansion
            # Detail angle ramps from the LOD0 value to the min-LOD value
            # with the same t^2 shape as the other per-LOD ramps.
            detail_angle_i = (s.detail_angle_lod0
                              + (s.detail_angle_min - s.detail_angle_lod0) * (t * t))
            # Deviation compensation ramps in with LOD depth: at LOD0 every
            # stroke still exists, so inflating a resampled stroke only
            # occludes its (still present) neighbors - strokes must render at
            # true width. At low LODs neighbors are gone and inflation fills
            # the gaps they left.
            sil_comp_i = s.silhouette_compensation * t
            # Per-object floor on ring verts (clamps low LODs above their schedule default).
            eff_ring = max(int(ring), int(s.min_ring_verts))
            base = (obj_idx * n_lods + i) * 100
            for inner_step, _t, inner_msg in run_conversion_iter(
                gp_obj, verts, s.curve_importance, eff_tube_scale, eff_ring,
                s.thickness_delta_importance, output_suffix=f"_LOD{i}",
                thickness_ratio=s.thickness_ratio,
                up_axis=s.cross_section_up_axis,
                force_hard_ends=s.force_hard_ends,
                silhouette_comp=sil_comp_i,
                brick_detect_angle=s.brick_detect_angle,
                detail_angle=detail_angle_i,
                absorb_dropped=s.absorb_dropped,
                absorb_max_scale=s.absorb_max_scale,
                keep_all=(i == 0),  # LOD0 never drops a stroke
                use_quill_orient=s.use_quill_orientation,
                cached_enriched=cached_enriched,
                cached_stats=cached_stats,
            ):
                yield base + inner_step, composite_total, \
                    f"[{gp_obj.name}] LOD{i}: {inner_msg}"

        # Prune any LODs beyond the current schedule length on this object.
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

        print(f"[gp_to_mesh_lod] '{gp_obj.name}': full LOD set baked in "
              f"{time.time() - t_obj:.2f}s")


class OBJECT_OT_gp_to_mesh_lod_bake_set(bpy.types.Operator):
    """Bake every LOD level from LOD_RING_SCHEDULE one tick at a time so the
    UI stays responsive. Press ESC to cancel."""
    bl_idname = "object.gp_to_mesh_lod_bake_set"
    bl_label = "Bake LOD Set"
    bl_options = {'REGISTER'}

    _timer = None
    _iter = None
    _gp_names = []

    @classmethod
    def poll(cls, context):
        return bool(_selected_gp_objects(context)) and not _BAKE_PROGRESS["active"]

    def invoke(self, context, event):
        gps = _selected_gp_objects(context)
        if not gps:
            self.report({'ERROR'}, "No GP objects selected.")
            return {'CANCELLED'}
        inherited = _auto_inherit_from_active(context)
        if inherited:
            self.report({'INFO'},
                        f"Inherited active settings on {inherited} default GP(s).")
        self._gp_names = [o.name for o in gps]

        try:
            self._iter = _bake_set_iter(self._gp_names)
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}

        composite_total = len(self._gp_names) * len(LOD_RING_SCHEDULE) * 100
        _BAKE_PROGRESS.update(active=True, step=0,
                              total=composite_total, msg="Starting...")
        wm = context.window_manager
        wm.progress_begin(0, composite_total)
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
        # Drain the generator for up to ~120ms per timer tick. One-step-per-
        # tick (the old behavior) capped the bake at 20 steps/second, which
        # turned seconds of compute into minutes of idle waiting between
        # ticks. Draining keeps the bake compute-bound while the UI still
        # redraws between ticks and ESC stays responsive.
        deadline = time.time() + 0.12
        step, total, msg = 0, 100, ""
        try:
            while True:
                step, total, msg = next(self._iter)
                if time.time() >= deadline:
                    break
        except StopIteration:
            self.report({'INFO'},
                        f"Baked {len(LOD_RING_SCHEDULE)} LODs x "
                        f"{len(self._gp_names)} GP object(s).")
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


class OBJECT_OT_gp_to_mesh_lod_show_all(bpy.types.Operator):
    """Un-hide every Grease Pencil and every <name>_LOD* mesh in the scene.
    Works regardless of selection or active state - by the time you click
    this you've probably lost both."""
    bl_idname = "object.gp_to_mesh_lod_show_all"
    bl_label = "Show All"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return True

    def execute(self, context):
        def _force_visible(obj):
            for attr in ("hide_viewport", "hide_render", "hide_select"):
                try:
                    setattr(obj, attr, False)
                except Exception:
                    pass
            try:
                obj.hide_set(False, view_layer=context.view_layer)
            except Exception:
                try:
                    obj.hide_set(False)
                except Exception:
                    pass

        def _walk_layer_colls(lc):
            try:
                if lc.exclude:
                    lc.exclude = False
            except Exception:
                pass
            try:
                if lc.hide_viewport:
                    lc.hide_viewport = False
            except Exception:
                pass
            try:
                if lc.collection is not None and lc.collection.hide_viewport:
                    lc.collection.hide_viewport = False
            except Exception:
                pass
            for child in lc.children:
                _walk_layer_colls(child)

        n_obj = 0
        print("[gp_to_mesh_lod] Show All diagnostic:")
        for obj in context.scene.objects:
            if not (_is_gp(obj) or (obj.type == 'MESH' and '_LOD' in obj.name)):
                continue
            colls = [c.name for c in obj.users_collection]
            print(f"  before {obj.name}: hide_v={obj.hide_viewport} "
                  f"hide_get={obj.hide_get()} hide_sel={obj.hide_select} "
                  f"visible={obj.visible_get()} colls={colls}")
            _force_visible(obj)
            n_obj += 1

        # Walk EVERY layer collection in EVERY view layer of this scene and
        # clear exclude/hide flags so nothing further up the tree is suppressing visibility.
        for vl in context.scene.view_layers:
            _walk_layer_colls(vl.layer_collection)

        # Also walk every collection datablock and clear its global hide_viewport.
        for c in bpy.data.collections:
            try:
                if c.hide_viewport:
                    c.hide_viewport = False
            except Exception:
                pass

        # Restore GP layer opacities to 1.0 (preview may have dimmed them).
        for obj in context.scene.objects:
            if _is_gp(obj):
                _set_gp_layers_opacity(obj, GP_FULL_OPACITY)

        # Re-check and log after-state for the same objects.
        print("[gp_to_mesh_lod] Show All - after state:")
        for obj in context.scene.objects:
            if not (_is_gp(obj) or (obj.type == 'MESH' and '_LOD' in obj.name)):
                continue
            print(f"  after  {obj.name}: hide_v={obj.hide_viewport} "
                  f"hide_get={obj.hide_get()} visible={obj.visible_get()}")

        try:
            context.scene.gp_lod_scene.preview_lod = -1
        except Exception:
            pass

        self.report({'INFO'},
                    f"Show All: forced visibility on {n_obj} obj(s); "
                    f"see system console for per-object diagnostic.")
        return {'FINISHED'}


class OBJECT_OT_gp_to_mesh_lod_preview(bpy.types.Operator):
    """Show only the chosen LOD level (or the GP itself if -1). Hides the GP
    via hide_viewport when previewing an LOD so other interactions still work."""
    bl_idname = "object.gp_to_mesh_lod_preview"
    bl_label = "Preview LOD"
    bl_options = {'REGISTER', 'UNDO'}

    lod: bpy.props.IntProperty(default=-1)

    def execute(self, context):
        _apply_preview_lod(context, int(self.lod))
        # Stash the current selection on the scene so the panel can show
        # which button is depressed. No update callback on the prop, so this
        # is a plain write.
        context.scene.gp_lod_scene.preview_lod = int(self.lod)
        return {'FINISHED'}


class OBJECT_OT_gp_to_mesh_lod_override(bpy.types.Operator):
    """Copy the active GP object's settings onto every OTHER selected GP."""
    bl_idname = "object.gp_to_mesh_lod_override"
    bl_label = "Override Settings (Active -> Others)"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        active_gp = _active_gp(context)
        if active_gp is None:
            return False
        return sum(1 for o in _selected_gp_objects(context) if o != active_gp) >= 1

    def execute(self, context):
        active = _active_gp(context)
        if active is None:
            self.report({'ERROR'}, "No Grease Pencil object resolvable from active.")
            return {'CANCELLED'}
        src = active.gp_lod_settings
        n = 0
        for gp in _selected_gp_objects(context):
            if gp == active:
                continue
            # Copies every key in SETTINGS_DEFAULTS, so new settings are
            # automatically included instead of silently drifting.
            _copy_settings(src, gp.gp_lod_settings)
            n += 1
        self.report({'INFO'}, f"Copied '{active.name}' settings to {n} other GP(s).")
        return {'FINISHED'}


class OBJECT_OT_gp_quill_orient(bpy.types.Operator):
    """Recover per-point normals and brush types from the original Quill
    scene (via each object's import provenance) and store them as GP
    attributes. Bakes then build cross-sections in the painted orientation -
    flat ribbons that twist exactly like the Quill source."""
    bl_idname = "object.gp_quill_fetch_orientation"
    bl_label = "Fetch Quill Orientation"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return bool(_selected_gp_objects(context))

    def execute(self, context):
        n_ok = 0
        for gp in _selected_gp_objects(context):
            try:
                n_pts = _relink_quill_orientation(gp)
            except (RuntimeError, OSError, KeyError, ValueError) as e:
                self.report({'WARNING'}, f"{gp.name}: {e}")
                continue
            n_ok += 1
            self.report({'INFO'}, f"{gp.name}: stored orientation for {n_pts} points.")
        return {'FINISHED'} if n_ok else {'CANCELLED'}


def _unity_export_dir():
    """Match the legacy GP LOD Helpers convention:
    <parent of blend dir>/Resources/ISLANDS/<BlendName>/Models/"""
    blend_path = bpy.data.filepath
    if not blend_path:
        raise RuntimeError("save the .blend first (export path derives from it)")
    blend_dir = os.path.dirname(blend_path)
    blend_name = os.path.splitext(os.path.basename(blend_path))[0]
    d = os.path.join(os.path.dirname(blend_dir), "Resources", "ISLANDS",
                     blend_name, "Models")
    os.makedirs(d, exist_ok=True)
    return d


def _export_lod_set_fbx(gp_obj, export_dir):
    """Export one GP object's LOD set as <export_dir>/<name>.fbx.
    Structure: a root node named exactly <name> carrying the GP object's
    world transform, with <name>_LOD0..N as identity-local children - Unity
    builds a LODGroup automatically from the _LODn naming, the import lands
    at the authored world position, and the prefab can still be duplicated
    and placed anywhere (the placement lives on the root, not in the verts).
    Restores parenting/visibility afterward. Returns the file path."""
    name = gp_obj.name
    lods = sorted(_lod_outputs_for(gp_obj), key=lambda o: o.name)
    if not lods:
        return None

    # Free the name so the FBX root node is named exactly <name>.
    gp_obj.name = name + ".__gp_tmp"
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    root.matrix_world = gp_obj.matrix_world.copy()

    restore = []
    try:
        for o in lods:
            restore.append((o, o.parent, o.hide_viewport))
            o.hide_viewport = False
            try:
                o.hide_set(False)
            except RuntimeError:
                pass
            o.parent = root
            o.matrix_parent_inverse = Matrix.Identity(4)
            o.matrix_local = Matrix.Identity(4)

        for ob in bpy.context.view_layer.objects:
            try:
                ob.select_set(False)
            except RuntimeError:
                pass
        root.select_set(True)
        for o in lods:
            o.select_set(True)
        bpy.context.view_layer.objects.active = root

        path = os.path.join(export_dir, f"{name}.fbx")
        bpy.ops.export_scene.fbx(
            filepath=path,
            use_selection=True,
            object_types={'EMPTY', 'MESH'},
            use_mesh_modifiers=True,
            add_leaf_bones=False,
            bake_space_transform=True,
            path_mode='AUTO',
        )
        return path
    finally:
        for o, par, hv in restore:
            o.parent = par
            o.matrix_parent_inverse = Matrix.Identity(4)
            o.matrix_local = Matrix.Identity(4)
            o.hide_viewport = hv
        bpy.data.objects.remove(root, do_unlink=True)
        gp_obj.name = name


class OBJECT_OT_gp_lod_export_unity(bpy.types.Operator):
    """Export each selected GP object's LOD set as a Unity-ready FBX
    (root at world transform + _LOD0..N children -> automatic LODGroup)"""
    bl_idname = "object.gp_lod_export_unity"
    bl_label = "Export LOD Sets (Unity)"
    bl_options = {'REGISTER'}

    @classmethod
    def poll(cls, context):
        return any(_lod_outputs_for(o) for o in _selected_gp_objects(context))

    def execute(self, context):
        try:
            export_dir = _unity_export_dir()
        except RuntimeError as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}
        n = 0
        for gp in _selected_gp_objects(context):
            path = _export_lod_set_fbx(gp, export_dir)
            if path:
                n += 1
        self.report({'INFO'}, f"Exported {n} LOD set(s) to {export_dir}")
        return {'FINISHED'}


class OBJECT_OT_gp_to_mesh_lod_clear(bpy.types.Operator):
    """Delete every <gp>_LOD* mesh under each selected GP object."""
    bl_idname = "object.gp_to_mesh_lod_clear"
    bl_label = "Clear All LODs"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return any(_lod_outputs_for(o) for o in _selected_gp_objects(context))

    def execute(self, context):
        for gp in _selected_gp_objects(context):
            _delete_lod_outputs(gp)
        return {'FINISHED'}


class VIEW3D_PT_gp_to_mesh_lod(bpy.types.Panel):
    bl_idname = "VIEW3D_PT_gp_to_mesh_lod"
    bl_label = "GP -> Mesh LOD"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "GP LOD"

    @classmethod
    def poll(cls, context):
        # Always show; the draw method handles the "no GP resolved" case.
        # Panel poll must NOT depend on the active object being visible/selected,
        # since the LOD preview slider hides the GP and that could otherwise
        # make the panel itself disappear.
        return True

    def draw(self, context):
        layout = self.layout
        layout.label(text=f"build #{_LOAD_COUNT}  @{_LOAD_TIME}", icon='FILE_SCRIPT')

        # Show All FIRST so it's always reachable even if everything else
        # below early-returns due to no active GP or other context issues.
        layout.operator(OBJECT_OT_gp_to_mesh_lod_show_all.bl_idname,
                        text="Show All (Restore Visibility)",
                        icon='HIDE_OFF')

        # --- DEBUG: confirm robust selection works for multi-select.
        robust_sel = _get_selected_objects_robust(context)
        ctx_sel    = list(getattr(context, 'selected_objects', []))
        dbg = layout.box()
        dbg.label(text=f"DEBUG ctx.selected={len(ctx_sel)}  robust={len(robust_sel)}",
                  icon='INFO')
        for o in robust_sel[:6]:
            gp_resolved = _resolve_gp(o)
            tag = gp_resolved.name if gp_resolved else "no-gp"
            dbg.label(text=f"  {o.name} ({o.type}) -> {tag}")
        active = context.active_object
        dbg.label(text=f"active = {active.name if active else 'None'}"
                       f" ({active.type if active else '-'})")
        # --- end debug

        obj = _active_gp(context)
        if obj is None:
            layout.separator()
            layout.label(text="Select a Grease Pencil object (or one of its baked",
                         icon='INFO')
            layout.label(text="LOD meshes) to use this tool.")
            # Offer a reset in case visibility is stuck after previous previews.
            row = layout.row()
            op = row.operator(OBJECT_OT_gp_to_mesh_lod_preview.bl_idname,
                              text="Reset Preview (show selected GPs)")
            op.lod = -1
            return
        s = obj.gp_lod_settings
        layout.label(text=f"Editing: {obj.name}", icon='GREASEPENCIL')

        gps = _selected_gp_objects(context)

        col = layout.column(align=True)
        col.prop(s, "verts_percentage")
        col.prop(s, "curve_importance")
        col.prop(s, "thickness_delta_importance")
        col.separator()
        col.prop(s, "tube_scale")
        col.prop(s, "thickness_ratio")
        col.prop(s, "cross_section_up_axis")
        col.prop(s, "force_hard_ends")
        col.prop(s, "ring_verts")
        col.prop(s, "min_ring_verts")
        col.prop(s, "lod_size_expansion")
        col.prop(s, "min_lod_ratio")
        col.separator()
        col.prop(s, "brick_detect_angle")
        col.prop(s, "silhouette_compensation")
        col.prop(s, "detail_angle_lod0")
        col.prop(s, "detail_angle_min")
        col.prop(s, "absorb_dropped")
        col.prop(s, "absorb_max_scale")
        col.separator()
        col.prop(s, "use_quill_orientation")
        col.operator(OBJECT_OT_gp_quill_orient.bl_idname, icon='IMPORT')
        col.operator(OBJECT_OT_gp_lod_export_unity.bl_idname, icon='EXPORT')

        # Selection summary — show source point counts so the percentage makes sense.
        if gps:
            box = layout.box()
            box.label(text=f"Will bake {len(gps)} GP object(s):", icon='RESTRICT_SELECT_OFF')
            for g in gps[:6]:
                src = _count_source_points(g)
                est = _verts_from_percentage(g, s.verts_percentage)
                box.label(text=f"  {g.name}: {src} src -> {est} LOD0 spine")
            if len(gps) > 6:
                box.label(text=f"  ... and {len(gps) - 6} more")

        row = layout.row(align=True)
        row.operator(OBJECT_OT_gp_to_mesh_lod_bake.bl_idname, text="Bake LOD0", icon='MOD_REMESH')
        row.operator(OBJECT_OT_gp_to_mesh_lod_bake_set.bl_idname, text="Bake LOD Set", icon='IMPORT')
        row.operator(OBJECT_OT_gp_to_mesh_lod_clear.bl_idname, text="", icon='TRASH')

        # "Override Settings": only shown when 2+ GPs are selected so it has
        # work to do. Copies the active GP's settings onto the rest.
        if len(gps) >= 2:
            layout.operator(OBJECT_OT_gp_to_mesh_lod_override.bl_idname,
                            text=f"Override Settings (Active -> {len(gps)-1} other)",
                            icon='DUPLICATE')

        if _BAKE_PROGRESS["active"]:
            box = layout.box()
            step  = _BAKE_PROGRESS["step"]
            total = max(1, _BAKE_PROGRESS["total"])
            ratio = max(0.0, min(1.0, step / total))
            BAR_W = 32
            fill  = int(round(ratio * BAR_W))
            bar   = "[" + ("#" * fill) + ("-" * (BAR_W - fill)) + "]"
            box.label(text=f"Baking {int(ratio * 100)}%", icon='TIME')
            box.label(text=bar)
            if _BAKE_PROGRESS["msg"]:
                box.label(text=_BAKE_PROGRESS["msg"])
            box.label(text="(ESC to cancel)")

        # LOD outputs across the entire selection.
        all_outs = []
        for g in gps:
            all_outs.extend(_lod_outputs_for(g))
        all_outs.sort(key=lambda o: o.name)
        if all_outs:
            scene_s = context.scene.gp_lod_scene
            max_idx = _max_lod_in_selection(context)
            box = layout.box()
            box.label(text="Outputs:", icon='OUTLINER_OB_MESH')
            if max_idx >= 0:
                current = scene_s.preview_lod
                row = box.row(align=True)
                op = row.operator(OBJECT_OT_gp_to_mesh_lod_preview.bl_idname,
                                  text="GP Only", depress=(current < 0))
                op.lod = -1
                for i in range(max_idx + 1):
                    op = row.operator(OBJECT_OT_gp_to_mesh_lod_preview.bl_idname,
                                      text=f"LOD{i}", depress=(current == i))
                    op.lod = i
            for o in all_outs[:12]:
                box.label(text=f"{o.name}  ({len(o.data.vertices)} verts)")
            if len(all_outs) > 12:
                box.label(text=f"... and {len(all_outs) - 12} more")

        # Show All is rendered near the top of draw() instead - this used to
        # be here but moved up so it's reachable even if draw() early-returns.


# --------------------------------------------------------------------------- #
# Registration                                                                #
# --------------------------------------------------------------------------- #
classes = (
    GPLODSettings,
    GPLODSceneSettings,
    OBJECT_OT_gp_to_mesh_lod_bake,
    OBJECT_OT_gp_to_mesh_lod_bake_set,
    OBJECT_OT_gp_to_mesh_lod_show_all,
    OBJECT_OT_gp_to_mesh_lod_preview,
    OBJECT_OT_gp_to_mesh_lod_override,
    OBJECT_OT_gp_quill_orient,
    OBJECT_OT_gp_lod_export_unity,
    OBJECT_OT_gp_to_mesh_lod_clear,
    VIEW3D_PT_gp_to_mesh_lod,
)


def register():
    global _LOAD_COUNT
    try:
        ns = bpy.app.driver_namespace
        ns["_gp_lod_load_count"] = ns.get("_gp_lod_load_count", 0) + 1
        _LOAD_COUNT = ns["_gp_lod_load_count"]
    except Exception:
        _LOAD_COUNT += 1

    for c in classes:
        bpy.utils.register_class(c)
    bpy.types.Object.gp_lod_settings = bpy.props.PointerProperty(type=GPLODSettings)
    bpy.types.Scene.gp_lod_scene = bpy.props.PointerProperty(type=GPLODSceneSettings)
    print(f"[gp_to_mesh_lod] registered  build #{_LOAD_COUNT}  @{_LOAD_TIME}")


def unregister():
    if hasattr(bpy.types.Object, "gp_lod_settings"):
        del bpy.types.Object.gp_lod_settings
    if hasattr(bpy.types.Scene, "gp_lod_scene"):
        del bpy.types.Scene.gp_lod_scene
    for c in reversed(classes):
        try:
            bpy.utils.unregister_class(c)
        except RuntimeError:
            pass


# Register unconditionally: covers Run Script in the text editor (__main__),
# exec() reloads from MCP, AND auto-run as an embedded text module on file
# open (use_module=True, where __name__ is the text block name).
try:
    unregister()
except Exception:
    pass
register()
