# BE A BIRD — Quill → Grease Pencil → LOD Mesh Pipeline

Context for working on this project's Blender asset pipeline. The game is a Unity
project ("BE A BIRD"); environment art is painted in **Quill** (VR), imported into
Blender as **Grease Pencil v3**, split/arranged into objects, converted to **LOD
mesh sets** (LOD0–LOD3) by a custom script, and exported as **FBX for Unity**.

## Key files (this directory = `Assets/.BlenderFiles/`)

| File | What it is |
|---|---|
| `gp_to_mesh_lod.py` | **The pipeline.** Single-file Blender add-on/script, ~2500 lines. `EDIT_TAG` comment at the top is bumped on every edit; the panel shows `build #N @time` so you can verify a reload took. |
| `Canyon1.blend` | Canyon scene. 91 GP objects with baked LOD sets. Temple @60% verts, everything else @80%. |
| `Canyon1_pre-LOD-backup.blend` | Untouched rollback from before any LOD work. Do not modify. |
| `BlenderAssets/GreasePencilToLODs.blend` | City scene (`isaac_city_12212025`). Contains `lines`, `bird`, `transformer01–05`, `domes`. |
| `QuillSources/isaac_canyon_03092026/` | Copy of the canyon Quill source (Quill.json + Quill.qbin). Blend references point here via relative path `//QuillSources/isaac_canyon_03092026`. |
| `%APPDATA%/Blender Foundation/Blender/5.0/scripts/addons/GreasePencilLODHelpers.py` | **Legacy** geometry-nodes pipeline (deprecated). Only its Unity export path convention was kept. |

**TODO carried over:** the city blend's quill refs still point at
`C:\Users\Isaac Cohen\Downloads\isaac_city_12212025\` — copy into `QuillSources/`
and repoint (same as was done for canyon) next time that file is open.

## Running the pipeline

- Blender 5.0, GP v3 (`GREASEPENCIL` objects). The script registers a panel:
  **3D View sidebar (N) → "GP LOD" tab**.
- The city blend embeds the script as a text block; Canyon1.blend does too. After
  a Blender restart, run the text block (or `exec(open(path).read())` with
  `__name__='__main__'`) once to register.
- MCP: the ahujasid **blender-mcp** addon connects on port 9876 (BlenderMCP
  sidebar tab → Connect). Reload pattern used from MCP:
  ```python
  src = open(r"...\gp_to_mesh_lod.py", encoding="utf-8").read()
  exec(compile(src, "gp_to_mesh_lod.py", 'exec'), {'__name__': '__main__'})
  ```
- For fast scripted bakes, bypass the modal operator:
  ```python
  G = bpy.types.OBJECT_OT_gp_to_mesh_lod_bake_set.invoke.__globals__
  for step, total, msg in G["_bake_set_iter"]([obj_names]): pass
  ```
  Capture stdout (`io.StringIO`) when batching — per-stroke prints are verbose.

## Pipeline concepts (as implemented)

- **Per-object settings** (`obj.gp_lod_settings`): the panel edits the ACTIVE
  object only. Batch-copy via "Override Settings (Active → Others)" button or by
  script. This has bitten the user before — always confirm which object holds a
  setting.
- **LOD0 never drops a stroke** (`keep_all`); budget squeeze gives every stroke a
  3-sample base then distributes the rest by *shape need* (curvature + thickness
  variation within a per-LOD detail angle). Straight constant strokes stay at 3.
- **Lower LODs** drop strokes by score (arc_len × mean thickness), keep survivors
  fully shaped, and **absorb** dropped strokes into the nearest survivor's radius
  (KD-tree; capped at `absorb_max_scale`× local radius).
- **Vert ratios are geometric**: `min_lod_ratio=0.01` → 100/21.5/4.6/1 % of the
  LOD0 spine budget. Ring schedule 6/4/2/2, floored by `min_ring_verts`.
- **Anti-aliased thickness**: resampling takes arc-length cell *averages*, not
  point samples — this is what prevents "tent" artifacts on fat-bulge strokes.
- **Silhouette compensation**: Cauchy mean-width ring compensation (×π/(R·sin(π/R)))
  plus local deviation re-inflation where the spine was decimated; deviation ramps
  in with LOD depth (zero at LOD0 so strokes never occlude neighbors).
- **Brick detection** exists (`brick_detect_angle`, straight + constant-thickness
  strokes → 8-vert square boxes) but is **off by default (0)** per user preference.

## Quill orientation (the flat-brush look)

Quill stores per-vertex normals + per-stroke brush type (ribbon/cylinder/ellipse/
cube); the GP importer discards them. The pipeline recovers them:

- **"Fetch Quill Orientation"** button / `_relink_quill_orientation(obj)` reads the
  Quill source via `obj.quill.scene_path` + `layer_path` and stamps GP attributes
  `quill_normal` (POINT) + `quill_brush` (CURVE). **These persist in the .blend**
  — baking never needs the Quill files afterward.
- Matching is two-stage: exact (count+endpoints) then **transform-invariant**
  (count, arc length, segment proportions, thickness values) verified by a Kabsch
  rigid fit whose rotation carries the normals into the stroke's current pose.
  This survives the user's workflow of splitting one giant layer into objects and
  moving strokes in edit mode (which bakes transforms into points).
- Bake profiles by brush: ribbon → flat R=2 strip (all LODs), ellipse → 0.3 aspect,
  cube → square R=4, cylinder → round. Unmatched strokes (all-zero normals) fall
  back to parallel transport with `up_axis`.
- Canyon match rate ~90–96%. Known low matches (heavily reshaped): `Big Rocks`
  (~1%), `Maze` (~6%), `temple__Rubble` + `Rubble5/6/7` (2–54%). This is fine.
- **Importer bug**: stored `layer_path` accumulates one `/Root` per preceding
  sibling — never trust it as an actual path; the code searches by name and falls
  back to all paint layers.
- **Advice for future splitting**: move pieces by OBJECT transform, not edit-mode
  point moves — then positions stay identical to the source and matching is exact.

## Unity export

- **"Export LOD Sets (Unity)"** button / `_export_lod_set_fbx(gp, dir)`.
- Path convention (from the legacy addon):
  `<Assets>/Resources/ISLANDS/<BlendName>/Models/<object>.fbx`.
- FBX structure: root node named exactly like the object, carrying its **world
  transform**; `<name>_LOD0..3` as identity-local children → Unity auto-creates
  the LODGroup, the import lands at the authored world position, and the prefab
  is still freely duplicatable/movable.
- FBX settings: `use_selection`, EMPTY+MESH, `bake_space_transform=True`,
  `add_leaf_bones=False`. Art is in vertex colors (`Col` attribute; material
  `GP_LOD_VertexColor`).

## Current recipe values (canyon, July 2026)

Temple hierarchy: verts 60%; everything else (Nature/Monuments/Objects/Temple
Grass/Rocks/Maze): verts 80%. Shared: curve_importance 20, thickness_delta 10,
ring 6, **min_ring_verts 5**, lod_size_expansion 1, min_lod_ratio 0.01,
thickness_ratio 1, detail angles 8→45, absorb on ×3, silhouette comp 1,
brick detect 0, use_quill_orientation on.

## Gotchas

- Baking overwrites `<name>_LOD0..3` mesh objects in place (parenting preserved).
- The bake-set modal drains ~120ms/tick — if bakes ever feel minutes-long again,
  someone probably reverted that (`OBJECT_OT_gp_to_mesh_lod_bake_set.modal`).
- GP object names must match `<gp>_LOD<i>` children; renaming a GP orphans them.
- `bird` (city file) has no Quill provenance and its strokes match no known layer
  (different source/version) — parallel transport only, that's expected.
- Always `py_compile` the script and check the panel `build #` after reloads.
- User keeps `*_pre-LOD-backup.blend` rollbacks — make one before batch surgery
  on a file that doesn't have one (copy the on-disk file BEFORE saving).
