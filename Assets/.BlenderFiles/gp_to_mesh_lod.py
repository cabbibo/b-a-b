# ============================================================
# EDIT_TAG: 6845
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
import time
from mathutils import Matrix

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
    """Quadratic ramp from 1.0 at LOD0 to `min_ratio` at the last LOD,
    matching the same t^2 shape used by the tube_scale expansion.
    Front-loaded: small drop early, large drop late."""
    if n_lods <= 1:
        return 1.0
    if min_ratio >= 1.0:
        return 1.0
    t = lod_index / (n_lods - 1)
    return 1.0 - (1.0 - min_ratio) * (t * t)

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


def build_tube_batch(spine_batch, radii_batch, R, thickness_ratio=1.0, up_axis='Z'):
    """Batched tube builder. spine_batch (N,S,3) and radii_batch (N,S).
    Returns:
      verts_flat: (N*S*R, 3)
      quads:      (N*Q_per, 4)  where Q_per = (S-1)*R for tubes, (S-1) for ribbons
      caps:       (N*2, R) for tubes, None for ribbons
    Vert k for stroke s, ring step r is at index s*S*R + k*R + r."""
    N, S, _ = spine_batch.shape
    _, normals, binormals = _frames_along_batch(spine_batch, up_axis=up_axis)

    angles = np.linspace(0.0, 2.0 * np.pi, R, endpoint=False)
    cos_a = np.cos(angles)
    sin_a = np.sin(angles) * float(thickness_ratio)

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


def _prepare_stroke_data(gp_obj, curve_importance, thickness_delta_importance):
    """Read strokes, apply zero-thickness fallback, and compute per-stroke
    weights/scores. Returns (enriched, stats). Independent of total_verts /
    ring_verts / tube_scale / thickness_ratio / up_axis, so its output can be
    cached across multiple LODs of the same GP."""
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

    # Enrich each stroke with cum_w, arc_len, score.
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

    elapsed = time.time() - t0
    print(f"[gp_to_mesh_lod] prepare '{gp_obj.name}': "
          f"{stats['strokes_total']} raw -> {len(enriched)} enriched in {elapsed:.2f}s")
    return enriched, stats


def run_conversion_iter(gp_obj, total_verts, curve_importance, tube_scale, ring_verts,
                        thickness_delta_importance, output_suffix="_LOD0",
                        thickness_ratio=1.0, up_axis='Z',
                        force_hard_ends=False,
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
            gp_obj, curve_importance, thickness_delta_importance)
        yield 6, 100, "Enriched"

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

    print(f"[gp_to_mesh_lod] {output_suffix}: "
          f"{stats.get('strokes_total', '?')} found  "
          f"- {stats.get('strokes_short', '?')} short  "
          f"- {stats.get('zero_weight', '?')} zero-weight  "
          f"- {dropped} dropped by budget  "
          f"= {len(keep_set)} kept.")

    enriched = [d for i, d in enumerate(enriched) if i in keep_set]

    # Per-stroke allocation is by arc length ONLY. The importance values
    # (curvature, thickness-delta) drive *where* samples land within each
    # stroke via cum_w, but don't steal budget from neighbors.
    spine_alloc = largest_remainder([d["arc_len"] for d in enriched], spine_budget, min_each=3)

    # Hard cap: never allocate more spine samples than the GP stroke's source
    # point count. When alloc == source, the loop below uses the source points
    # directly with no resampling, so the tube spine is exactly the GP stroke.
    capped = 0
    for i_a, d in enumerate(enriched):
        src_n = len(d["pos"])
        if spine_alloc[i_a] > src_n:
            spine_alloc[i_a] = src_n
            capped += 1
        if spine_alloc[i_a] < 2:
            spine_alloc[i_a] = max(2, min(src_n, 2))
    if capped:
        print(f"[gp_to_mesh_lod] {capped} stroke(s) capped to source point count.")

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

    # Group stroke indices by their allocated spine count.
    groups = {}
    for j in range(n_strokes):
        groups.setdefault(int(spine_alloc[j]), []).append(j)

    processed = 0
    group_order = sorted(groups.keys())
    for S_group in group_order:
        stroke_indices = groups[S_group]
        N = len(stroke_indices)

        # Build per-group batch arrays (this loop is the remaining per-stroke
        # Python cost - resampling - but it's much lighter than build_tube was).
        spine_batch  = np.empty((N, S_group, 3), dtype=np.float64)
        thick_batch  = np.empty((N, S_group),    dtype=np.float64)
        colors_batch = np.empty((N, S_group, 4), dtype=np.float64)
        for k, j in enumerate(stroke_indices):
            d = enriched[j]
            n_spine = spine_alloc[j]
            if n_spine >= len(d["pos"]):
                spine_batch[k]  = d["pos"]
                thick_batch[k]  = d["thick"]
                colors_batch[k] = d["col"]
            else:
                idx, t = compute_resample_keys(d["cum_w"], n_spine)
                spine_batch[k]  = interp_along(d["pos"],   idx, t)
                thick_batch[k]  = interp_along(d["thick"], idx, t)
                colors_batch[k] = interp_along(d["col"],   idx, t)

        radii_batch = thick_batch * float(tube_scale)

        # Force hard ends: (1) clamp the end-sample radius up to its neighbor
        # so the tube doesn't taper to a point, then (2) extrude the first
        # and last spine samples outward along the local tangent by half a
        # radius. The extrusion matches how Blender's GP draws a flat cap -
        # the visible square extends past the actual data endpoint by r/2.
        if force_hard_ends and S_group >= 2:
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
            spine_batch, radii_batch, R,
            thickness_ratio=thickness_ratio,
            up_axis=up_axis)

        all_verts.append(verts_grp)
        # Colors: spine-sample colors repeated R times each.
        all_colors.append(np.repeat(colors_batch.reshape(N * S_group, 4), R, axis=0))
        all_quads.append(quads_grp + vbase)
        if caps_grp is not None:
            all_caps.append(caps_grp + vbase)
        vbase += verts_grp.shape[0]

        processed += N
        prog = BUILD_LO + int((BUILD_HI - BUILD_LO) * processed / max(1, n_strokes))
        yield prog, 100, f"Tubes {processed}/{n_strokes} (group S={S_group}, N={N})"
    t_build = time.time() - t_build

    yield 92, 100, "Writing mesh"
    t_w = time.time()
    verts_concat  = np.concatenate(all_verts,  axis=0)
    colors_concat = np.concatenate(all_colors, axis=0)
    quads_concat  = np.concatenate(all_quads,  axis=0) if all_quads else np.zeros((0, 4), dtype=np.int64)
    # Caps are 2D arrays (n_caps_group, R) per group; concatenate along axis 0.
    caps_concat = np.concatenate(all_caps, axis=0) if all_caps else np.zeros((0, R), dtype=np.int64)
    n_caps = caps_concat.shape[0]

    n_verts  = verts_concat.shape[0]
    n_quads  = quads_concat.shape[0]
    n_polys  = n_quads + n_caps
    n_loops  = n_quads * 4 + n_caps * R

    fast_path_ok = False
    try:
        if n_verts and n_polys:
            # Build flat loop indices: quads first (groups of 4), then caps (groups of R).
            loop_idx = np.empty(n_loops, dtype=np.int32)
            loop_idx[:n_quads * 4] = quads_concat.ravel()
            if n_caps:
                loop_idx[n_quads * 4:] = caps_concat.ravel()
            poly_sizes = np.empty(n_polys, dtype=np.int32)
            poly_sizes[:n_quads] = 4
            poly_sizes[n_quads:] = R
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
        all_faces.extend(c.tolist() for c in all_caps)
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
    "lod_size_expansion":           2.0,
    "min_lod_ratio":                0.10,
    "thickness_ratio":              1.0,
    "cross_section_up_axis":        'Z',
    "force_hard_ends":              False,
    "min_ring_verts":               2,
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
        default=2.0, min=1.0, soft_max=8.0,
        description="Tube radius is scaled from 1x at LOD0 up to this value at the lowest LOD, to mask visual gaps when verts are sparse.",
    )
    min_lod_ratio: bpy.props.FloatProperty(
        name="Min LOD Vert Ratio",
        default=0.10, min=0.01, max=1.0, soft_min=0.02, soft_max=0.5,
        description="Spine vert count at the lowest LOD = (Total Stroke Verts) x (this ratio). Intermediate LODs interpolate geometrically.",
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
                               force_hard_ends=s.force_hard_ends)
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
                gp_obj, s.curve_importance, s.thickness_delta_importance)
        except RuntimeError as e:
            print(f"[gp_to_mesh_lod] skip {gp_obj.name}: {e}")
            continue

        for i, ring in enumerate(LOD_RING_SCHEDULE):
            vmul = _lod_vert_multiplier(i, n_lods, s.min_lod_ratio)
            verts = max(3, int(round(total_verts * vmul)))
            t = (i / (n_lods - 1)) if n_lods > 1 else 0.0
            expansion = 1.0 + (s.lod_size_expansion - 1.0) * (t * t)
            eff_tube_scale = s.tube_scale * expansion
            # Per-object floor on ring verts (clamps low LODs above their schedule default).
            eff_ring = max(int(ring), int(s.min_ring_verts))
            base = (obj_idx * n_lods + i) * 100
            for inner_step, _t, inner_msg in run_conversion_iter(
                gp_obj, verts, s.curve_importance, eff_tube_scale, eff_ring,
                s.thickness_delta_importance, output_suffix=f"_LOD{i}",
                thickness_ratio=s.thickness_ratio,
                up_axis=s.cross_section_up_axis,
                force_hard_ends=s.force_hard_ends,
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
        try:
            step, total, msg = next(self._iter)
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
            d = gp.gp_lod_settings
            d.verts_percentage           = src.verts_percentage
            d.curve_importance           = src.curve_importance
            d.thickness_delta_importance = src.thickness_delta_importance
            d.tube_scale                 = src.tube_scale
            d.ring_verts                 = src.ring_verts
            d.lod_size_expansion         = src.lod_size_expansion
            d.min_lod_ratio              = src.min_lod_ratio
            d.thickness_ratio            = src.thickness_ratio
            d.cross_section_up_axis      = src.cross_section_up_axis
            d.force_hard_ends            = src.force_hard_ends
            d.min_ring_verts             = src.min_ring_verts
            n += 1
        self.report({'INFO'}, f"Copied '{active.name}' settings to {n} other GP(s).")
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


if __name__ == "__main__":
    try:
        unregister()
    except Exception:
        pass
    register()
