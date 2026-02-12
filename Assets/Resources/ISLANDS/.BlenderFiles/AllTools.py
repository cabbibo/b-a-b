# Blender Add-on: Wren Tools
# 1) Decimate selected meshes
# 2) Export selected objects as FBX
#
# Install: Blender > Edit > Preferences > Add-ons > Install... (pick this .py) > Enable
# Use: 3D View > Sidebar (N) > "Wren Tools" tab

bl_info = {
    "name": "Wren Tools",
    "author": "Isaac Cohen",
    "version": (1, 0, 0),
    "blender": (3, 6, 0),
    "location": "View3D > Sidebar > Wren Tools",
    "category": "Object",
}




# Tools:
# - Vertex Color Material
# - Unity Camera Navigation
# - Decimate
# - Export FBX

import bpy
import os
import math
from bpy.props import (
    FloatProperty,
    BoolProperty,
    StringProperty,
    EnumProperty,
    IntProperty,
)
from mathutils import Vector
import bpy.utils


def ensure_dir(path: str):
    """Create directory if it doesn't exist. Raises exception on failure."""
    if not path:
        return
    os.makedirs(path, exist_ok=True)


def get_export_dir(context) -> str:
    # Allow // relative paths
    raw = context.scene.poptools_export_dir
    if not raw:
        return ""
    return bpy.path.abspath(raw)


def selected_objects(context):
    return list(context.selected_objects or [])


class POPTOOLS_OT_decimate_selected(bpy.types.Operator):
    bl_idname = "poptools.decimate_selected"
    bl_label = "Decimate Selected"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        ratio = context.scene.poptools_decimate_ratio
        apply_mod = context.scene.poptools_decimate_apply
        only_active = context.scene.poptools_decimate_only_active

        if ratio <= 0.0 or ratio > 1.0:
            self.report({"ERROR"}, "Decimate ratio must be in (0, 1].")
            return {"CANCELLED"}

        objs = selected_objects(context)
        if not objs:
            self.report({"WARNING"}, "No selected objects.")
            return {"CANCELLED"}

        if only_active and context.view_layer.objects.active is not None:
            objs = [context.view_layer.objects.active]

        count = 0
        for obj in objs:
            if obj is None or obj.type != "MESH":
                continue

            mod = obj.modifiers.new(name="PopTools_Decimate", type="DECIMATE")
            mod.decimate_type = "COLLAPSE"
            mod.ratio = ratio
            mod.use_collapse_triangulate = True

            if apply_mod:
                # Need active object for modifier_apply
                context.view_layer.objects.active = obj
                try:
                    bpy.ops.object.modifier_apply(modifier=mod.name)
                except Exception as e:
                    self.report({"WARNING"}, f"Could not apply modifier on {obj.name}: {e}")

            count += 1

        self.report({"INFO"}, f"Decimated {count} mesh object(s).")
        return {"FINISHED"}


class POPTOOLS_OT_export_lod_full_fbx(bpy.types.Operator):
    bl_idname = "poptools.export_lod_full_fbx"
    bl_label = "Export Full LOD as FBX"
    bl_options = {"REGISTER"}

    def execute(self, context):
        # Use the LODify FBX export path if set, otherwise fall back to poptools_export_dir
        export_path = context.scene.poptools_lodify_fbx_export_path
        if export_path:
            # The path already includes the model folder, so use it as the model_folder
            export_dir = os.path.dirname(export_path)  # Parent of model folder (Models/)
        else:
            export_dir = get_export_dir(context)
            if not export_dir:
                self.report({"ERROR"}, "Set an Export Path first.")
                return {"CANCELLED"}

        ensure_dir(export_dir)

        # Find LOD parent
        active = context.view_layer.objects.active
        if not active:
            self.report({"WARNING"}, "No active object selected.")
            return {"CANCELLED"}

        lod_parent = None
        if active.name.endswith("_LODs"):
            lod_parent = active
        else:
            # Check if active is a child of a LOD parent
            if active.parent and active.parent.name.endswith("_LODs"):
                lod_parent = active.parent
            else:
                # Check selected objects
                for obj in context.selected_objects:
                    if obj.name.endswith("_LODs"):
                        lod_parent = obj
                        break
                    elif obj.parent and obj.parent.name.endswith("_LODs"):
                        lod_parent = obj.parent
                        break

        if not lod_parent:
            self.report({"WARNING"}, "No LOD parent found. Select a LOD group or LOD object.")
            return {"CANCELLED"}

        model_name = lod_parent.name.replace("_LODs", "")

        # Get ALL descendants of the Full object recursively - this includes the entire hierarchy
        def get_all_descendants(obj):
            """Recursively get all descendants of an object, including nested children."""
            all_descendants = []
            
            def collect_children(parent):
                for child in parent.children:
                    if child not in all_descendants:
                        all_descendants.append(child)
                        collect_children(child)  # Recursively collect children of children
            
            collect_children(obj)
            return all_descendants
        
        # Get all children of the LOD parent
        descendants = get_all_descendants(lod_parent)
        all_children = descendants
        
        # Find the Full object
        full_obj = None
        full_name = f"{model_name}_Full"
        for child in all_children:
            if child.name == full_name:
                full_obj = child
                break
        
        # Separate objects into two groups:
        # 1. Full object only
        # 2. LOD parent + all other children (excluding Full)
        full_objects = [full_obj] if full_obj else []
        lod_objects = [lod_parent] + [obj for obj in all_children if obj != full_obj]
        
        # Remove duplicates while preserving order
        def deduplicate_objects(obj_list):
            seen = set()
            unique = []
            for obj in obj_list:
                if obj:
                    obj_id = id(obj)
                    if obj_id not in seen:
                        seen.add(obj_id)
                        unique.append(obj)
            return unique
        
        full_objects = deduplicate_objects(full_objects)
        lod_objects = deduplicate_objects(lod_objects)
        
        # Debug: report what we found
        self.report({"INFO"}, f"Found Full object: {len(full_objects)}, LOD hierarchy: {len(lod_objects)} objects")

        if not full_objects and not lod_objects:
            self.report({"WARNING"}, "No objects to export.")
            return {"CANCELLED"}

        # Determine model folder from export path or fallback
        export_path = context.scene.poptools_lodify_fbx_export_path
        if export_path:
            # Use the explicit export path (already includes model name)
            model_folder = export_path
        else:
            # Fallback to export_dir + model_name
            model_folder = os.path.join(export_dir, model_name)
        if os.path.exists(model_folder):
            import shutil
            try:
                shutil.rmtree(model_folder)
                self.report({"INFO"}, f"Deleted existing folder: {model_folder}")
            except Exception as e:
                self.report({"WARNING"}, f"Could not delete existing folder: {e}")
        ensure_dir(model_folder)

        # Save original selection
        view_layer = context.view_layer
        orig_active = view_layer.objects.active
        orig_sel = list(context.selected_objects)

        def restore_selection():
            for obj in context.selected_objects:
                obj.select_set(False)
            for o in orig_sel:
                if o and o.name in bpy.data.objects:
                    o.select_set(True)
            if orig_active and orig_active.name in bpy.data.objects:
                view_layer.objects.active = orig_active

        def prepare_objects_for_export(objects_list):
            """Make objects visible and ensure they're in the view layer."""
            for obj in objects_list:
                if obj and obj.name in bpy.data.objects:
                    if obj.name not in view_layer.objects:
                        view_layer.objects.link(obj)
                    obj.hide_viewport = False
                    obj.hide_render = False
                    obj.update_tag()
            view_layer.update()

        def select_objects_for_export(objects_list, root_obj):
            """Select objects and set root as active."""
            for obj in context.selected_objects:
                obj.select_set(False)
            
            selected_count = 0
            if root_obj and root_obj.name in bpy.data.objects:
                root_obj.select_set(True)
                selected_count += 1
            
            for obj in objects_list:
                if obj and obj.name in bpy.data.objects and obj != root_obj:
                    obj.select_set(True)
                    selected_count += 1
            
            if root_obj:
                view_layer.objects.active = root_obj
            
            return selected_count

        def export_fbx(objects_list, root_obj, filepath, description):
            """Export selected objects as FBX."""
            prepare_objects_for_export(objects_list)
            selected_count = select_objects_for_export(objects_list, root_obj)
            
            if selected_count == 0:
                return False
            
            bake_space = context.scene.poptools_export_bake_space_transform
            add_leaf_bones = context.scene.poptools_export_add_leaf_bones
            
            bpy.ops.export_scene.fbx(
                filepath=filepath,
                use_selection=True,
                use_active_collection=False,
                apply_unit_scale=True,
                bake_space_transform=bake_space,
                object_types={"MESH", "ARMATURE", "EMPTY", "LIGHT", "CAMERA", "OTHER"},
                add_leaf_bones=add_leaf_bones,
                bake_anim=False,
                use_armature_deform_only=False,
                mesh_smooth_type='OFF',
                path_mode='AUTO',
                axis_forward='Y',  # Blender's Y becomes forward (Z in target system)
                axis_up='Z',        # Blender's Z becomes up (Y in target system)
            )
            
            self.report({"INFO"}, f"Exported {description}: {selected_count} objects to {filepath}")
            return True

        try:
            # Ensure we're in object mode
            if context.active_object and context.active_object.mode != 'OBJECT':
                bpy.ops.object.mode_set(mode='OBJECT')

            exported_count = 0

            # Export 1: Full object only
            if full_objects:
                full_filepath = os.path.join(model_folder, f"{model_name}_Full.fbx")
                if export_fbx(full_objects, full_objects[0], full_filepath, "Full object"):
                    exported_count += 1

            # Export 2: LOD parent + all other children (excluding Full)
            if lod_objects:
                lod_filepath = os.path.join(model_folder, f"{model_name}.fbx")
                if export_fbx(lod_objects, lod_parent, lod_filepath, "LOD hierarchy"):
                    exported_count += 1

            restore_selection()
            
            if exported_count > 0:
                self.report({"INFO"}, f"Exported {exported_count} FBX file(s) to: {model_folder}")
            else:
                self.report({"WARNING"}, "No files were exported.")
            
            return {"FINISHED"}

        except Exception as e:
            restore_selection()
            self.report({"ERROR"}, f"FBX export failed: {e}")
            return {"CANCELLED"}


class POPTOOLS_OT_export_collection_lods(bpy.types.Operator):
    bl_idname = "poptools.export_collection_lods"
    bl_label = "Export Collection LODs"
    bl_options = {"REGISTER"}

    def execute(self, context):
        # Check if a collection is selected - try multiple methods
        collection = None
        collection_name = None
        
        # Method 1: Check active layer collection
        active_layer_collection = context.view_layer.active_layer_collection
        if active_layer_collection:
            collection = active_layer_collection.collection
            collection_name = collection.name
        
        # Method 2: Check context.collection (active collection)
        if not collection:
            collection = context.collection
            if collection and collection.name != "Scene Collection":
                collection_name = collection.name
        
        # Method 3: Check if selected objects belong to a collection
        if not collection and context.selected_objects:
            # Find which collection the selected objects belong to
            for obj in context.selected_objects:
                for coll in bpy.data.collections:
                    if obj.name in coll.objects:
                        collection = coll
                        collection_name = coll.name
                        break
                if collection:
                    break
        
        if not collection or not collection_name:
            self.report({"ERROR"}, "No collection selected. Select a collection in the outliner or select objects in a collection.")
            return {"CANCELLED"}
        
        # Get current file path
        filepath = bpy.data.filepath
        if not filepath:
            self.report({"ERROR"}, "File must be saved before exporting.")
            return {"CANCELLED"}
        
        # Navigate directory structure
        # Current file: ISLANDS/.BlendFiles/full.blend
        # Target: ISLANDS/Desert/Models/
        file_dir = os.path.dirname(filepath)
        parent_dir = os.path.dirname(file_dir)  # Go up one directory
        collection_dir = os.path.join(parent_dir, collection_name)  # Down into collection folder
        models_dir = os.path.join(collection_dir, "Models")  # Down into Models folder
        
        # Check if collection folder exists
        if not os.path.exists(collection_dir):
            self.report({"ERROR"}, f"Collection folder does not exist: {collection_dir}")
            return {"CANCELLED"}
        
        # Create Models folder if it doesn't exist
        try:
            ensure_dir(models_dir)
        except (PermissionError, OSError) as e:
            self.report({"ERROR"}, f"Cannot create Models folder: {e}")
            return {"CANCELLED"}
        except Exception as e:
            self.report({"ERROR"}, f"Models folder error: {e}")
            return {"CANCELLED"}
        
        # Find all LODified objects in the collection (objects ending with "_LODs")
        lod_parents = []
        for obj in collection.all_objects:
            if obj.name.endswith("_LODs"):
                lod_parents.append(obj)
        
        if not lod_parents:
            self.report({"WARNING"}, f"No LODified objects found in collection '{collection_name}'.")
            return {"CANCELLED"}
        
        # Save original selection
        view_layer = context.view_layer
        orig_active = view_layer.objects.active
        orig_sel = list(context.selected_objects)
        
        def restore_selection():
            for obj in context.selected_objects:
                obj.select_set(False)
            for o in orig_sel:
                if o and o.name in bpy.data.objects:
                    o.select_set(True)
            if orig_active and orig_active.name in bpy.data.objects:
                view_layer.objects.active = orig_active
        
        def get_all_descendants(obj):
            """Recursively get all descendants of an object."""
            all_descendants = []
            def collect_children(parent):
                for child in parent.children:
                    if child not in all_descendants:
                        all_descendants.append(child)
                        collect_children(child)
            collect_children(obj)
            return all_descendants
        
        def prepare_objects_for_export(objects_list):
            """Make objects visible and ensure they're in the view layer."""
            for obj in objects_list:
                if obj and obj.name in bpy.data.objects:
                    if obj.name not in view_layer.objects:
                        view_layer.objects.link(obj)
                    obj.hide_viewport = False
                    obj.hide_render = False
                    obj.update_tag()
            view_layer.update()
        
        def select_objects_for_export(objects_list, root_obj):
            """Select objects and set root as active."""
            for obj in context.selected_objects:
                obj.select_set(False)
            
            selected_count = 0
            if root_obj and root_obj.name in bpy.data.objects:
                root_obj.select_set(True)
                selected_count += 1
            
            for obj in objects_list:
                if obj and obj.name in bpy.data.objects and obj != root_obj:
                    obj.select_set(True)
                    selected_count += 1
            
            if root_obj:
                view_layer.objects.active = root_obj
            
            return selected_count
        
        def export_fbx_with_hierarchy(lod_parent, export_folder):
            """Export a single LOD parent hierarchy to a folder."""
            model_name = lod_parent.name.replace("_LODs", "")
            
            # Get all descendants
            descendants = get_all_descendants(lod_parent)
            all_children = descendants
            
            # Find the Full object
            full_obj = None
            full_name = f"{model_name}_Full"
            for child in all_children:
                if child.name == full_name:
                    full_obj = child
                    break
            
            # Separate objects into two groups
            full_objects = [full_obj] if full_obj else []
            lod_objects = [lod_parent] + [obj for obj in all_children if obj != full_obj]
            
            # Remove duplicates
            def deduplicate_objects(obj_list):
                seen = set()
                unique = []
                for obj in obj_list:
                    if obj:
                        obj_id = id(obj)
                        if obj_id not in seen:
                            seen.add(obj_id)
                            unique.append(obj)
                return unique
            
            full_objects = deduplicate_objects(full_objects)
            lod_objects = deduplicate_objects(lod_objects)
            
            if not full_objects and not lod_objects:
                return False
            
            # Ensure we're in object mode
            if context.active_object and context.active_object.mode != 'OBJECT':
                bpy.ops.object.mode_set(mode='OBJECT')
            
            exported_count = 0
            
            # Export 1: Full object only
            if full_objects:
                full_filepath = os.path.join(export_folder, f"{model_name}_Full.fbx")
                prepare_objects_for_export(full_objects)
                selected_count = select_objects_for_export(full_objects, full_objects[0])
                
                if selected_count > 0:
                    bake_space = context.scene.poptools_export_bake_space_transform
                    add_leaf_bones = context.scene.poptools_export_add_leaf_bones
                    
                    bpy.ops.export_scene.fbx(
                        filepath=full_filepath,
                        use_selection=True,
                        use_active_collection=False,
                        apply_unit_scale=True,
                        bake_space_transform=bake_space,
                        object_types={"MESH", "ARMATURE", "EMPTY", "LIGHT", "CAMERA", "OTHER"},
                        add_leaf_bones=add_leaf_bones,
                        bake_anim=False,
                        use_armature_deform_only=False,
                        mesh_smooth_type='OFF',
                        path_mode='AUTO',
                        axis_forward='Y',
                        axis_up='Z',
                    )
                    exported_count += 1
            
            # Export 2: LOD parent + all other children (excluding Full)
            if lod_objects:
                lod_filepath = os.path.join(export_folder, f"{model_name}.fbx")
                prepare_objects_for_export(lod_objects)
                selected_count = select_objects_for_export(lod_objects, lod_parent)
                
                if selected_count > 0:
                    bake_space = context.scene.poptools_export_bake_space_transform
                    add_leaf_bones = context.scene.poptools_export_add_leaf_bones
                    
                    bpy.ops.export_scene.fbx(
                        filepath=lod_filepath,
                        use_selection=True,
                        use_active_collection=False,
                        apply_unit_scale=True,
                        bake_space_transform=bake_space,
                        object_types={"MESH", "ARMATURE", "EMPTY", "LIGHT", "CAMERA", "OTHER"},
                        add_leaf_bones=add_leaf_bones,
                        bake_anim=False,
                        use_armature_deform_only=False,
                        mesh_smooth_type='OFF',
                        path_mode='AUTO',
                        axis_forward='Y',
                        axis_up='Z',
                    )
                    exported_count += 1
            
            return exported_count > 0
        
        try:
            # Export each LOD parent individually
            exported_count = 0
            for lod_parent in lod_parents:
                model_name = lod_parent.name.replace("_LODs", "")
                model_folder = os.path.join(models_dir, model_name)
                
                # Delete folder if it exists to ensure clean export
                if os.path.exists(model_folder):
                    try:
                        import shutil
                        shutil.rmtree(model_folder)
                    except Exception as e:
                        self.report({"WARNING"}, f"Could not delete existing folder {model_folder}: {e}")
                
                try:
                    ensure_dir(model_folder)
                except Exception as e:
                    self.report({"WARNING"}, f"Could not create folder {model_folder}: {e}. Skipping.")
                    continue
                
                if export_fbx_with_hierarchy(lod_parent, model_folder):
                    exported_count += 1
                    self.report({"INFO"}, f"Exported {model_name} to {model_folder}")
            
            restore_selection()
            
            if exported_count > 0:
                self.report({"INFO"}, f"Exported {exported_count} LOD object(s) to {models_dir}")
            else:
                self.report({"WARNING"}, "No objects were exported.")
            
            return {"FINISHED"}
        
        except Exception as e:
            restore_selection()
            self.report({"ERROR"}, f"Export failed: {e}")
            return {"CANCELLED"}


class POPTOOLS_OT_export_selected_fbx(bpy.types.Operator):
    bl_idname = "poptools.export_selected_fbx"
    bl_label = "Export Selected as FBX"
    bl_options = {"REGISTER"}

    def execute(self, context):
        export_dir = get_export_dir(context)
        if not export_dir:
            self.report({"ERROR"}, "Set an Export Directory first.")
            return {"CANCELLED"}

        ensure_dir(export_dir)

        export_mode = context.scene.poptools_export_mode
        bake_space = context.scene.poptools_export_bake_space_transform
        add_leaf_bones = context.scene.poptools_export_add_leaf_bones
        use_active_collection = context.scene.poptools_export_use_active_collection

        objs = selected_objects(context)
        if not objs:
            self.report({"WARNING"}, "No selected objects.")
            return {"CANCELLED"}

        # Optionally restrict to active collection objects (still must be selected)
        if use_active_collection:
            col = context.collection
            allowed = set(col.all_objects)
            objs = [o for o in objs if o in allowed]
            if not objs:
                self.report({"WARNING"}, "No selected objects in the active collection.")
                return {"CANCELLED"}

        view_layer = context.view_layer
        orig_active = view_layer.objects.active
        orig_sel = list(context.selected_objects)

        def restore_selection():
            bpy.ops.object.select_all(action="DESELECT")
            for o in orig_sel:
                if o and o.name in bpy.data.objects:
                    o.select_set(True)
            view_layer.objects.active = orig_active

        exported = 0
        try:
            if export_mode == "SINGLE_FILE":
                filename = context.scene.poptools_export_filename.strip() or "export.fbx"
                if not filename.lower().endswith(".fbx"):
                    filename += ".fbx"
                filepath = os.path.join(export_dir, filename)

                bpy.ops.export_scene.fbx(
                    filepath=filepath,
                    use_selection=True,
                    apply_unit_scale=True,
                    bake_space_transform=bake_space,
                    object_types={"MESH", "ARMATURE", "EMPTY"},
                    add_leaf_bones=add_leaf_bones,
                    bake_anim=False,
                )
                exported = 1
            else:
                # One FBX per selected object
                for obj in objs:
                    bpy.ops.object.select_all(action="DESELECT")
                    obj.select_set(True)
                    view_layer.objects.active = obj

                    name = bpy.path.clean_name(obj.name) or "object"
                    filepath = os.path.join(export_dir, f"{name}.fbx")

                    bpy.ops.export_scene.fbx(
                        filepath=filepath,
                        use_selection=True,
                        apply_unit_scale=True,
                        bake_space_transform=bake_space,
                        object_types={"MESH", "ARMATURE", "EMPTY"},
                        add_leaf_bones=add_leaf_bones,
                        bake_anim=False,
                    )
                    exported += 1
        except Exception as e:
            self.report({"ERROR"}, f"FBX export failed: {e}")
            restore_selection()
            return {"CANCELLED"}

        restore_selection()
        self.report({"INFO"}, f"Exported {exported} FBX file(s) to: {export_dir}")
        return {"FINISHED"}


class POPTOOLS_OT_unity_camera(bpy.types.Operator):
    bl_idname = "poptools.unity_camera"
    bl_label = "Unity Camera Mode"
    bl_description = "Enter Unity-style WASD scene view navigation"
    bl_options = {"REGISTER"}

    def execute(self, context):
        # Configure walk navigation for Unity-like scene view controls
        prefs = context.preferences.inputs
        
        # Simple scene view settings - no gravity, just fly around
        speed = context.scene.wrentools_camera_speed
        prefs.walk_navigation.use_mouse_reverse = False
        prefs.walk_navigation.mouse_speed = 1.0
        prefs.walk_navigation.walk_speed = speed
        prefs.walk_navigation.walk_speed_factor = 5.0
        prefs.walk_navigation.use_gravity = False  # No gravity - pure fly mode

        # Make sure we're in perspective view (not camera view) so position persists
        for area in context.screen.areas:
            if area.type == "VIEW_3D":
                for space in area.spaces:
                    if space.type == "VIEW_3D":
                        # Exit camera view if in it, so viewport position persists on exit
                        if space.region_3d.view_perspective == "CAMERA":
                            space.region_3d.view_perspective = "PERSP"
                        space.lock_camera = False
                        break
                break

        self.report({"INFO"}, "WASD=move, Mouse=look, Shift=fast, LMB/Enter=confirm, RMB=cancel")

        # Enter walk navigation mode
        bpy.ops.view3d.walk("INVOKE_DEFAULT")

        return {"FINISHED"}


class POPTOOLS_OT_origin_to_center(bpy.types.Operator):
    bl_idname = "poptools.origin_to_center"
    bl_label = "Origin to Center"
    bl_description = "Set origin to geometry center for all selected objects individually"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}

        # Filter to only mesh objects
        meshes = [obj for obj in selected if obj.type == "MESH"]
        
        if not meshes:
            self.report({"WARNING"}, "No mesh objects selected.")
            return {"CANCELLED"}

        # Store original selection
        orig_active = context.view_layer.objects.active
        orig_selected = list(context.selected_objects)

        # Set origin to center for each selected mesh individually
        count = 0
        for mesh_obj in meshes:
            # Deselect all first
            for obj in context.selected_objects:
                obj.select_set(False)
            
            # Select only this mesh and make it active
            mesh_obj.select_set(True)
            context.view_layer.objects.active = mesh_obj
            
            # Set origin to geometry center
            bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="MEDIAN")
            
            count += 1

        # Restore original selection
        for obj in orig_selected:
            if obj and obj.name in bpy.data.objects:
                obj.select_set(True)
        if orig_active and orig_active.name in bpy.data.objects:
            context.view_layer.objects.active = orig_active

        self.report({"INFO"}, f"Set origin to center for {count} mesh(es).")
        return {"FINISHED"}


class POPTOOLS_OT_arrange_in_grid(bpy.types.Operator):
    bl_idname = "poptools.arrange_in_grid"
    bl_label = "Arrange in Grid"
    bl_description = "Arrange all selected objects tightly on XY plane using bounding boxes (no intersections)"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}
        
        # Filter to only objects with bounding boxes (meshes, curves, etc.)
        objects_to_arrange = [obj for obj in selected if obj.type in {'MESH', 'CURVE', 'SURFACE', 'FONT', 'META', 'LIGHT', 'CAMERA', 'EMPTY'}]
        
        if not objects_to_arrange:
            self.report({"WARNING"}, "No arrangeable objects selected.")
            return {"CANCELLED"}
        
        # Calculate bounding boxes for all objects
        bboxes = []
        for obj in objects_to_arrange:
            # Ensure we're in object mode
            context.view_layer.objects.active = obj
            if obj.mode != 'OBJECT':
                bpy.ops.object.mode_set(mode='OBJECT')
            
            # Try to get bounding box
            try:
                if hasattr(obj, 'bound_box') and obj.bound_box:
                    # Get bounding box in world space
                    bbox_corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
                    
                    # Calculate bounding box dimensions
                    xs = [v.x for v in bbox_corners]
                    ys = [v.y for v in bbox_corners]
                    zs = [v.z for v in bbox_corners]
                    
                    width = max(xs) - min(xs)
                    height = max(ys) - min(ys)
                    depth = max(zs) - min(zs)
                else:
                    # Fallback: use object dimensions or a default size
                    if hasattr(obj, 'dimensions') and obj.dimensions.length > 0:
                        width = obj.dimensions.x
                        height = obj.dimensions.y
                        depth = obj.dimensions.z
                    else:
                        # Default size for objects without bounding box
                        width = 1.0
                        height = 1.0
                        depth = 0.0
            except:
                # Fallback if bounding box calculation fails
                width = 1.0
                height = 1.0
                depth = 0.0
            
            # Get current origin position
            origin = obj.matrix_world.translation
            
            bboxes.append({
                'obj': obj,
                'width': width,
                'height': height,
                'depth': depth,
                'origin': origin
            })
        
        if not bboxes:
            self.report({"WARNING"}, "Could not calculate bounding boxes.")
            return {"CANCELLED"}
        
        # Store original selection
        orig_active = context.view_layer.objects.active
        orig_selected = list(context.selected_objects)
        
        # Simple tight packing: arrange objects in rows, placing them as close as possible
        placed_objects = []
        current_x = 0.0
        current_y = 0.0
        row_height = 0.0
        row_start_x = 0.0
        
        # Sort by size (largest first) for better packing
        bboxes_sorted = sorted(bboxes, key=lambda bb: bb['width'] * bb['height'], reverse=True)
        
        for bbox in bboxes_sorted:
            obj = bbox['obj']
            width = bbox['width']
            height = bbox['height']
            current_z = bbox['origin'].z
            
            # Small padding to prevent intersections
            padding = 0.01
            
            # Check if this object fits on current row
            if current_x + width + padding > 10.0:  # Start new row if too wide
                current_y -= row_height + padding
                current_x = row_start_x
                row_height = 0.0
            
            # Place object
            x = current_x + width / 2
            y = current_y - height / 2
            
            # Update position
            obj.location.x = x
            obj.location.y = y
            obj.location.z = current_z
            
            # Track placed objects for collision detection
            placed_objects.append({
                'x': x,
                'y': y,
                'width': width,
                'height': height
            })
            
            # Update current position
            current_x += width + padding
            row_height = max(row_height, height)
            if current_x == width / 2 + padding:  # First object in row
                row_start_x = -width / 2
        
        # Center the arrangement around origin
        if placed_objects:
            # Calculate bounding box of all placed objects
            all_xs = [p['x'] for p in placed_objects]
            all_ys = [p['y'] for p in placed_objects]
            all_widths = [p['width'] for p in placed_objects]
            all_heights = [p['height'] for p in placed_objects]
            
            min_x = min(x - w/2 for x, w in zip(all_xs, all_widths))
            max_x = max(x + w/2 for x, w in zip(all_xs, all_widths))
            min_y = min(y - h/2 for y, h in zip(all_ys, all_heights))
            max_y = max(y + h/2 for y, h in zip(all_ys, all_heights))
            
            center_x = (min_x + max_x) / 2
            center_y = (min_y + max_y) / 2
            
            # Offset all objects to center around origin
            for bbox in bboxes_sorted:
                obj = bbox['obj']
                obj.location.x -= center_x
                obj.location.y -= center_y
        
        # Restore original selection
        for obj in orig_selected:
            if obj and obj.name in bpy.data.objects:
                obj.select_set(True)
        if orig_active and orig_active.name in bpy.data.objects:
            context.view_layer.objects.active = orig_active
        
        self.report({"INFO"}, f"Arranged {len(objects_to_arrange)} object(s) tightly on XY plane.")
        return {"FINISHED"}


class POPTOOLS_OT_apply_vertex_color_material(bpy.types.Operator):
    bl_idname = "poptools.apply_vertex_color_material"
    bl_label = "Apply Vertex Color Material"
    bl_description = "Create a vertex color material and apply it to all meshes under selected parent(s)"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}

        # Collect all mesh objects under selected parents (and selected meshes themselves)
        meshes = set()
        for obj in selected:
            if obj.type == "MESH":
                meshes.add(obj)
            self.collect_mesh_children(obj, meshes)

        if not meshes:
            self.report({"WARNING"}, "No mesh objects found in selection or children.")
            return {"CANCELLED"}

        # Create the vertex color material
        mat = self.create_vertex_color_material(context)

        # Apply to all meshes
        for mesh_obj in meshes:
            # Clear existing materials and add the vertex color material
            mesh_obj.data.materials.clear()
            mesh_obj.data.materials.append(mat)

        self.report({"INFO"}, f"Applied vertex color material to {len(meshes)} mesh(es).")
        return {"FINISHED"}

    def collect_mesh_children(self, obj, meshes):
        """Recursively collect all mesh children of an object."""
        for child in obj.children:
            if child.type == "MESH":
                meshes.add(child)
            self.collect_mesh_children(child, meshes)

    def create_vertex_color_material(self, context):
        """Create a material that displays vertex colors."""
        mat_name = context.scene.wrentools_vertexcolor_mat_name or "VertexColorMaterial"
        
        # Create new material (or reuse existing)
        mat = bpy.data.materials.get(mat_name)
        if mat is None:
            mat = bpy.data.materials.new(name=mat_name)

        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links

        # Clear existing nodes
        nodes.clear()

        # Create Material Output
        output = nodes.new(type="ShaderNodeOutputMaterial")
        output.location = (300, 0)

        # Create Principled BSDF
        bsdf = nodes.new(type="ShaderNodeBsdfPrincipled")
        bsdf.location = (0, 0)

        # Create Vertex Color (Color Attribute) node
        vertex_color = nodes.new(type="ShaderNodeVertexColor")
        vertex_color.location = (-300, 0)
        # Leave layer_name empty to use the active color attribute

        # Connect: Vertex Color -> Base Color -> Output
        links.new(vertex_color.outputs["Color"], bsdf.inputs["Base Color"])
        links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])

        return mat


def get_triangle_count(obj):
    """Get the triangle count of a mesh object. Read-only, safe to call during draw."""
    if obj.type != "MESH":
        return 0
    
    mesh = obj.data
    
    # Use total_face_vert_count / 3 for accurate triangle count (read-only, efficient)
    try:
        if hasattr(mesh, 'total_face_vert_count') and mesh.total_face_vert_count > 0:
            return int(mesh.total_face_vert_count / 3)
    except:
        pass
    
    # Fallback: try loop_triangles if already calculated
    try:
        if hasattr(mesh, 'loop_triangles'):
            loop_tris = mesh.loop_triangles
            if loop_tris and len(loop_tris) > 0:
                return len(loop_tris)
    except:
        pass
    
    # Last fallback: count polygons (less accurate but always available)
    try:
        if hasattr(mesh, 'polygons'):
            return len(mesh.polygons)
    except:
        pass
    
    return 0


def remove_degenerate_triangles(obj):
    """Remove degenerate triangles (lines/dots) from a mesh."""
    if obj.type != "MESH":
        return False
    
    mesh = obj.data
    bpy.context.view_layer.objects.active = obj
    
    # Make sure we're in object mode
    if mesh.is_editmode:
        bpy.ops.object.mode_set(mode="OBJECT")
    
    # Enter edit mode
    bpy.ops.object.mode_set(mode="EDIT")
    
    # Select all
    bpy.ops.mesh.select_all(action="SELECT")
    
    # Remove degenerate geometry (faces with zero area, duplicate vertices, etc.)
    bpy.ops.mesh.dissolve_degenerate(threshold=0.0001)
    
    # Remove loose vertices/edges
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.delete_loose(use_verts=True, use_edges=True, use_faces=False)
    
    # Return to object mode
    bpy.ops.object.mode_set(mode="OBJECT")
    
    return True


def smooth_mesh(obj, factor):
    """Apply smoothing to a mesh object using edit mode vertex smooth.
    Factor 0 = no smoothing, 1 = full smoothing (multiple iterations)."""
    if obj.type != "MESH":
        return False
    
    if factor <= 0:
        return True
    
    # Clamp factor to valid range
    factor = min(max(factor, 0.0), 1.0)
    
    # Iterations scale with factor (5-50 iterations for strong visible effect)
    iterations = max(5, int(factor * 50))
    # Smooth factor per iteration (1.0 = maximum smoothing per pass)
    smooth_factor = 1.0
    
    # Ensure object mode first
    bpy.context.view_layer.objects.active = obj
    if obj.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode="OBJECT")
    
    # Deselect all, select our object
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    
    # Go to edit mode and smooth vertices
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    
    # Apply vertex smoothing multiple times based on factor
    for _ in range(iterations):
        bpy.ops.mesh.vertices_smooth(factor=smooth_factor)
    
    # Return to object mode
    bpy.ops.object.mode_set(mode="OBJECT")
    
    return True


def expand_mesh(obj, amount):
    """Expand mesh vertices along their normals to restore silhouette.
    Amount is the distance to push vertices outward."""
    if obj.type != "MESH":
        return False
    
    if amount <= 0:
        return True
    
    import bmesh
    
    # Ensure object mode first
    bpy.context.view_layer.objects.active = obj
    if obj.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode="OBJECT")
    
    # Work with bmesh for direct vertex manipulation
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    
    # Ensure normals are up to date
    bm.normal_update()
    
    # Move each vertex along its normal
    for vert in bm.verts:
        vert.co += vert.normal * amount
    
    # Write back to mesh
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()
    
    return True


def limited_dissolve_mesh(obj, angle_degrees):
    """Apply limited dissolve to remove flat geometry based on angle threshold.
    Angle is in degrees - faces with angles less than this will be dissolved."""
    if obj.type != "MESH":
        return False
    
    if angle_degrees <= 0:
        return True
    
    import math
    
    # Convert degrees to radians
    angle_radians = math.radians(angle_degrees)
    
    # Ensure object mode first, then switch to edit mode
    bpy.context.view_layer.objects.active = obj
    if obj.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode="OBJECT")
    
    # Deselect all, select our object
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    
    # Go to edit mode
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    
    # Apply limited dissolve
    bpy.ops.mesh.dissolve_limited(angle_limit=angle_radians)
    
    # Return to object mode
    bpy.ops.object.mode_set(mode="OBJECT")
    
    return True


def merge_vertices_by_distance(obj, distance):
    """Merge vertices by distance using bmesh, preserving silhouette by avoiding edge/corner vertices.
    Then removes degenerate triangles."""
    if obj.type != "MESH":
        return False
    
    # Make sure we're in object mode
    bpy.context.view_layer.objects.active = obj
    if obj.mode != 'OBJECT':
        bpy.ops.object.mode_set(mode="OBJECT")
    
    import bmesh
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bm.faces.ensure_lookup_table()
    bm.edges.ensure_lookup_table()
    bm.verts.ensure_lookup_table()
    
    # Calculate vertex normals for silhouette preservation
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    
    # Identify edge and corner vertices (important for silhouette)
    # These are vertices with sharp edges or on boundaries
    edge_verts = set()
    for edge in bm.edges:
        if edge.is_boundary:
            edge_verts.add(edge.verts[0])
            edge_verts.add(edge.verts[1])
        else:
            # Check for sharp edges (edges between faces with large angle difference)
            try:
                face_angle = edge.calc_face_angle()
                if face_angle > 0.5:  # Sharp edge threshold (radians)
                    edge_verts.add(edge.verts[0])
                    edge_verts.add(edge.verts[1])
            except:
                # If calc_face_angle fails, assume it's an important edge
                edge_verts.add(edge.verts[0])
                edge_verts.add(edge.verts[1])
    
    # For better silhouette preservation, we'll use a two-pass approach:
    # 1. First merge vertices that are NOT on edges/corners (interior vertices)
    # 2. Then merge edge vertices more conservatively
    
    # Pass 1: Merge interior vertices (not on edges)
    interior_verts = [v for v in bm.verts if v not in edge_verts]
    if interior_verts:
        bmesh.ops.remove_doubles(bm, verts=interior_verts, dist=distance)
    
    # Pass 2: Merge edge vertices with a smaller threshold to preserve silhouette
    if edge_verts:
        edge_verts_list = list(edge_verts)
        # Use a more conservative distance for edge vertices (50% of original)
        bmesh.ops.remove_doubles(bm, verts=edge_verts_list, dist=distance * 0.5)
    
    # Update mesh
    bm.to_mesh(obj.data)
    obj.data.update()
    
    # Now remove degenerate triangles
    bm.faces.ensure_lookup_table()
    bm.edges.ensure_lookup_table()
    bm.verts.ensure_lookup_table()
    
    # Mark faces for deletion if any edge is shorter than merge distance
    faces_to_delete = []
    for face in bm.faces:
        if len(face.edges) < 3:  # Not a triangle
            faces_to_delete.append(face)
            continue
        
        # Check if any edge is shorter than merge distance
        is_degenerate = False
        for edge in face.edges:
            edge_length = edge.calc_length()
            if edge_length < distance * 0.99:  # Slightly less than merge distance to catch degenerates
                is_degenerate = True
                break
        
        # Also check if face area is very small (degenerate triangle)
        if not is_degenerate:
            face_area = face.calc_area()
            # If area is less than what a triangle with merge_distance edges would have, it's degenerate
            min_area = (distance * distance * 0.433)  # Area of equilateral triangle with side = distance
            if face_area < min_area * 0.1:  # Very small threshold
                is_degenerate = True
        
        if is_degenerate:
            faces_to_delete.append(face)
    
    # Delete degenerate faces
    if faces_to_delete:
        bmesh.ops.delete(bm, geom=faces_to_delete, context="FACES")
        bm.to_mesh(obj.data)
        obj.data.update()
    
    bm.free()
    
    # Remove loose vertices/edges after deletion
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.delete_loose(use_verts=True, use_edges=True, use_faces=False)
    
    bpy.ops.object.mode_set(mode="OBJECT")
    return True


def decimate_to_ratio(obj, ratio):
    """Decimate a mesh to a specific ratio."""
    if obj.type != "MESH" or ratio >= 1.0:
        return False
    
    # Remove any existing decimate modifier with our name
    mod_name = "LODify_Decimate"
    if mod_name in obj.modifiers:
        obj.modifiers.remove(obj.modifiers[mod_name])
    
    mod = obj.modifiers.new(name=mod_name, type="DECIMATE")
    mod.decimate_type = "COLLAPSE"
    mod.ratio = ratio
    mod.use_collapse_triangulate = True
    
    # Apply the modifier
    bpy.context.view_layer.objects.active = obj
    try:
        bpy.ops.object.modifier_apply(modifier=mod_name)
        return True
    except Exception as e:
        print(f"Failed to apply decimate modifier: {e}")
        return False


def decimate_to_triangle_count(obj, max_triangles):
    """Decimate a mesh until it has at most max_triangles triangles."""
    current_triangles = get_triangle_count(obj)
    if current_triangles <= max_triangles:
        return True
    
    # Estimate starting ratio
    target_ratio = max_triangles / current_triangles
    target_ratio = max(0.01, min(0.99, target_ratio * 1.1))  # Add 10% buffer
    
    # Apply decimation
    if not decimate_to_ratio(obj, target_ratio):
        return False
    
    # Check if we're close enough, if not, decimate more
    new_triangles = get_triangle_count(obj)
    if new_triangles > max_triangles:
        # Need to decimate more - calculate new ratio
        additional_ratio = max_triangles / new_triangles
        additional_ratio = max(0.01, min(0.99, additional_ratio * 1.05))
        
        # Apply additional decimation
        if not decimate_to_ratio(obj, additional_ratio):
            return False
    
        return True


class POPTOOLS_OT_create_octahedral_imposter(bpy.types.Operator):
    bl_idname = "poptools.create_octahedral_imposter"
    bl_label = "Create Octahedral Imposter"
    bl_description = "Create a simple octahedral imposter for the selected object"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}
        
        # Get the active object or first selected
        base_obj = context.view_layer.objects.active or selected[0]
        
        # Calculate bounding box to determine imposter size
        try:
            if hasattr(base_obj, 'bound_box') and base_obj.bound_box:
                bbox_corners = [base_obj.matrix_world @ Vector(corner) for corner in base_obj.bound_box]
                xs = [v.x for v in bbox_corners]
                ys = [v.y for v in bbox_corners]
                zs = [v.z for v in bbox_corners]
                
                width = max(xs) - min(xs)
                height = max(ys) - min(ys)
                depth = max(zs) - min(zs)
                
                # Use the largest dimension for the octahedron size
                size = max(width, height, depth) * 0.7  # Slightly smaller than bounding box
            else:
                # Fallback size
                size = 1.0
        except:
            size = 1.0
        
        # Store original selection
        orig_active = context.view_layer.objects.active
        orig_selected = list(context.selected_objects)
        
        # Get world position
        world_pos = base_obj.matrix_world.translation
        
        # Create octahedron
        bpy.ops.mesh.primitive_octahedron_add(size=size, location=world_pos)
        imposter = context.active_object
        imposter.name = f"{base_obj.name}_Imposter"
        
        # Create a simple material that can be used for billboarding
        mat = bpy.data.materials.new(name=f"{imposter.name}_Material")
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links
        
        # Clear existing nodes
        nodes.clear()
        
        # Create Material Output
        output = nodes.new(type="ShaderNodeOutputMaterial")
        output.location = (300, 0)
        
        # Create Principled BSDF
        bsdf = nodes.new(type="ShaderNodeBsdfPrincipled")
        bsdf.location = (0, 0)
        bsdf.inputs["Base Color"].default_value = (0.8, 0.8, 0.8, 1.0)
        bsdf.inputs["Metallic"].default_value = 0.0
        bsdf.inputs["Roughness"].default_value = 0.5
        
        # Connect BSDF to output
        links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
        
        # Apply material
        imposter.data.materials.append(mat)
        
        # Restore original selection
        for obj in orig_selected:
            if obj and obj.name in bpy.data.objects:
                obj.select_set(True)
        if orig_active and orig_active.name in bpy.data.objects:
            context.view_layer.objects.active = orig_active
        
        self.report({"INFO"}, f"Created octahedral imposter: {imposter.name}")
        return {"FINISHED"}


def get_lodify_default_export_path(context):
    """Compute the default LODify export path based on blend file and selected object."""
    filepath = bpy.data.filepath
    if not filepath:
        return ""
    
    # Get model name from active object
    active = context.view_layer.objects.active
    model_name = ""
    
    if active:
        if active.name.endswith("_LODs"):
            model_name = active.name.replace("_LODs", "")
        elif active.parent and active.parent.name.endswith("_LODs"):
            model_name = active.parent.name.replace("_LODs", "")
        elif active.name.endswith("_Full"):
            model_name = active.name.replace("_Full", "")
        elif "_LOD" in active.name:
            model_name = active.name.rsplit("_LOD", 1)[0]
        else:
            model_name = active.name
    
    if not model_name:
        model_name = "Unnamed"
    
    # Navigate directory structure:
    # Current file: ISLANDS/.BlendFiles/full.blend
    # Target: ISLANDS/(blendFileName)/Models/(modelName)/
    file_dir = os.path.dirname(filepath)
    parent_dir = os.path.dirname(file_dir)  # Go up one directory from .BlendFiles
    blend_name = os.path.splitext(os.path.basename(filepath))[0]  # Get blend file name without extension
    
    # Build path: ../(blendFileName)/Models/(modelName)
    export_path = os.path.join(parent_dir, blend_name, "Models", model_name)
    
    return export_path


class POPTOOLS_OT_lodify_reset_export_path(bpy.types.Operator):
    bl_idname = "poptools.lodify_reset_export_path"
    bl_label = "Reset Export Path"
    bl_description = "Reset export path to default based on blend file and object name"
    bl_options = {"REGISTER"}

    def execute(self, context):
        default_path = get_lodify_default_export_path(context)
        if default_path:
            context.scene.poptools_lodify_fbx_export_path = default_path
            self.report({"INFO"}, f"Export path reset to: {default_path}")
        else:
            self.report({"WARNING"}, "Could not determine default path. Save the file first.")
        return {"FINISHED"}


class POPTOOLS_OT_lodify(bpy.types.Operator):
    bl_idname = "poptools.lodify"
    bl_label = "LODify"
    bl_description = "Generate LOD levels for selected objects"
    bl_options = {"REGISTER", "UNDO"}

    def invoke(self, context, event):
        # Auto-set export path if empty or if object changed
        current_path = context.scene.poptools_lodify_fbx_export_path
        if not current_path:
            default_path = get_lodify_default_export_path(context)
            if default_path:
                context.scene.poptools_lodify_fbx_export_path = default_path
        
        wm = context.window_manager
        return wm.invoke_props_dialog(self, width=600)

    def draw(self, context):
        s = context.scene
        layout = self.layout
        
        layout.label(text="LODify - Level of Detail Generator", icon="MOD_DECIM")
        layout.separator()
        
        # Export FBX section at the top
        box = layout.box()
        box.label(text="Export FBX", icon="EXPORT")
        
        # Export path field
        row = box.row(align=True)
        row.prop(s, "poptools_lodify_fbx_export_path", text="")
        row.operator("poptools.lodify_reset_export_path", text="", icon="FILE_REFRESH")
        
        # Export button
        row = box.row()
        row.scale_y = 1.5
        row.alert = True
        op = row.operator("poptools.export_lod_full_fbx", text="Export FBX (All Children)", icon="EXPORT")
        
        layout.separator()
        
        # Show current object name
        active_obj = context.active_object
        if active_obj:
            box = layout.box()
            box.label(text=f"Object: {active_obj.name}", icon="OBJECT_DATA")
        
        layout.separator()
        
        # LOD Viewer slider (if LOD group is selected)
        active = context.view_layer.objects.active
        if active:
            lod_children = []
            if active.name.endswith("_LODs"):
                lod_children = sorted([child for child in active.children if child and hasattr(child, 'name')], 
                                     key=lambda x: (x.name.endswith("_Full"), x.name))
            elif active.parent and active.parent.name.endswith("_LODs"):
                lod_parent = active.parent
                lod_children = sorted([child for child in lod_parent.children if child and hasattr(child, 'name')], 
                                     key=lambda x: (x.name.endswith("_Full"), x.name))
            
            if lod_children:
                lod_children = [c for c in lod_children if c.name.endswith(("_Full", "_LOD0", "_LOD1", "_LOD2", "_LOD3"))][:5]
                if lod_children:
                    box = layout.box()
                    box.label(text="LOD Viewer", icon="VIEW3D")
                    current_lod = context.scene.poptools_lod_viewer_level
                    max_lod = min(len(lod_children) - 1, 4)
                    row = box.row()
                    row.prop(context.scene, "poptools_lod_viewer_level", text="Level", slider=True)
                    display_lod = min(current_lod, max_lod)
                    if 0 <= display_lod < len(lod_children):
                        current_lod_obj = lod_children[display_lod]
                        tri_count = get_triangle_count(current_lod_obj)
                        box.label(text=f"Showing: {current_lod_obj.name}", icon="CHECKMARK")
                        box.label(text=f"Triangles: {tri_count:,}", icon="MESH_DATA")
                    layout.separator()
        
        # LOD Levels
        for i in range(4):
            lod_props = [
                (f"poptools_lodify_lod{i}_merge", f"Merge Distance (LOD {i})"),
                (f"poptools_lodify_lod{i}_decimate", f"Decimate Amount (LOD {i})"),
                (f"poptools_lodify_lod{i}_max_tris", f"Max Triangles (LOD {i})"),
                (f"poptools_lodify_lod{i}_dissolve", f"Limited Dissolve (LOD {i})"),
                (f"poptools_lodify_lod{i}_smooth", f"Smoothness (LOD {i})"),
                (f"poptools_lodify_lod{i}_expand", f"Expand (LOD {i})"),
            ]
            
            col = layout.column(align=True)
            row = col.row()
            row.label(text=f"LOD {i}", icon="MESH_DATA")
            
            # Show triangle count if LOD exists
            if active:
                # Try to find the LOD object
                lod_obj = None
                if active.name.endswith("_LODs"):
                    model_name = active.name.replace("_LODs", "")
                    lod_name = f"{model_name}_LOD{i}"
                    lod_obj = bpy.data.objects.get(lod_name)
                elif active.parent and active.parent.name.endswith("_LODs"):
                    model_name = active.parent.name.replace("_LODs", "")
                    lod_name = f"{model_name}_LOD{i}"
                    lod_obj = bpy.data.objects.get(lod_name)
                elif active.name.endswith("_Full"):
                    model_name = active.name.replace("_Full", "")
                    lod_name = f"{model_name}_LOD{i}"
                    lod_obj = bpy.data.objects.get(lod_name)
                elif "_LOD" in active.name:
                    # Extract base name from LOD object
                    base_name = active.name.rsplit("_LOD", 1)[0]
                    lod_name = f"{base_name}_LOD{i}"
                    lod_obj = bpy.data.objects.get(lod_name)
                
                if lod_obj:
                    tri_count = get_triangle_count(lod_obj)
                    row.label(text=f"{tri_count:,} tris", icon="MESH_DATA")
            
            box = col.box()
            
            for prop_name, label in lod_props:
                if "merge" in prop_name:
                    box.prop(s, prop_name, text="Merge Distance")
                elif "decimate" in prop_name:
                    box.prop(s, prop_name, text="Decimate Amount")
                elif "max_tris" in prop_name:
                    box.prop(s, prop_name, text="Max Triangles")
                elif "dissolve" in prop_name:
                    box.prop(s, prop_name, text="Limited Dissolve")
                elif "smooth" in prop_name:
                    box.prop(s, prop_name, text="Smoothness")
                elif "expand" in prop_name:
                    box.prop(s, prop_name, text="Expand")
            
            op = box.operator("poptools.lodify_bake", text=f"Bake LOD {i}")
            op.lod_index = i
        
        layout.separator()
        op = layout.operator("poptools.lodify_bake_all", text="Bake All LODs + Original", icon="EXPORT")
        op.close_dialog = False

    def execute(self, context):
        return {"FINISHED"}


class POPTOOLS_OT_lodify_bake(bpy.types.Operator):
    bl_idname = "poptools.lodify_bake"
    bl_label = "Bake LOD"
    bl_options = {"REGISTER", "UNDO"}
    
    lod_index: IntProperty(default=0)

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}
        
        # Get the active object or first selected as the base
        base_obj = context.view_layer.objects.active or selected[0]
        
        if not base_obj:
            self.report({"WARNING"}, "No valid object selected.")
            return {"CANCELLED"}
        
        # Check if we're working with a LOD parent (empty object) or LOD child
        # If so, find the Full object to use as base - ALWAYS use Full for rebaking
        if base_obj.name.endswith("_LODs"):
            # Active is the LOD parent - find the Full object
            full_obj = None
            for child in base_obj.children:
                if child and hasattr(child, 'name') and child.name.endswith("_Full"):
                    full_obj = child
                    break
            
            if not full_obj or not hasattr(full_obj, 'type') or full_obj.type != "MESH":
                self.report({"WARNING"}, "Could not find Full mesh object in LOD group. Please select the Full object or original mesh.")
                return {"CANCELLED"}
            
            base_obj = full_obj
        elif base_obj.parent and base_obj.parent.name.endswith("_LODs"):
            # Active is a child of LOD parent (could be _Full, _LOD0, _LOD1, etc.)
            # ALWAYS find and use the Full object as the base for baking
            lod_parent = base_obj.parent
            full_obj = None
            for child in lod_parent.children:
                if child and hasattr(child, 'name') and child.name.endswith("_Full"):
                    full_obj = child
                    break
            
            if not full_obj or not hasattr(full_obj, 'type') or full_obj.type != "MESH":
                self.report({"WARNING"}, "Could not find Full mesh object. Please select a mesh object.")
                return {"CANCELLED"}
            
            base_obj = full_obj
        elif base_obj.type != "MESH":
            # Not a LOD-related object and not a mesh
            self.report({"WARNING"}, "Active object must be a mesh.")
            return {"CANCELLED"}
        
        # Final check that base_obj is a mesh
        if not hasattr(base_obj, 'type') or base_obj.type != "MESH":
            self.report({"WARNING"}, "Could not find a valid mesh object to use as base.")
            return {"CANCELLED"}
        
        lod_index = self.lod_index
        s = context.scene
        
        # Get LOD settings
        merge_dist = getattr(s, f"poptools_lodify_lod{lod_index}_merge", 0.0)
        decimate_amt = getattr(s, f"poptools_lodify_lod{lod_index}_decimate", 1.0)
        max_tris = getattr(s, f"poptools_lodify_lod{lod_index}_max_tris", 10000000)
        dissolve_angle = getattr(s, f"poptools_lodify_lod{lod_index}_dissolve", 0.0)
        smooth_factor = getattr(s, f"poptools_lodify_lod{lod_index}_smooth", 0.0)
        expand_amount = getattr(s, f"poptools_lodify_lod{lod_index}_expand", 0.0)
        
        # Use the object's name from hierarchy
        model_name = base_obj.name
        
        # Get the collection(s) that the base object belongs to
        # Use the first collection the object is in, or fall back to active collection
        target_collection = None
        for collection in bpy.data.collections:
            if base_obj.name in collection.objects:
                target_collection = collection
                break
        
        if not target_collection:
            target_collection = context.collection
        
        # Store world matrix (accounts for all parent transforms in hierarchy)
        orig_world_matrix = base_obj.matrix_world.copy()
        
        # Create parent at the same world location as the original object
        parent_name = f"{model_name}_LODs"
        parent = bpy.data.objects.get(parent_name)
        if not parent:
            parent = bpy.data.objects.new(parent_name, None)
            target_collection.objects.link(parent)
            # Set parent's world transform to match original object's world transform
            parent.matrix_world = orig_world_matrix
        
        # Ensure object mode
        context.view_layer.objects.active = base_obj
        if base_obj.mode != 'OBJECT':
            bpy.ops.object.mode_set(mode='OBJECT')
        
        # Duplicate the base object
        for obj in context.selected_objects:
            obj.select_set(False)
        base_obj.select_set(True)
        context.view_layer.objects.active = base_obj
        bpy.ops.object.duplicate()
        lod_obj = context.active_object
        lod_obj.name = f"{model_name}_LOD{lod_index}"
        
        # Ensure the duplicated object is in the same collection as the base object
        if target_collection and lod_obj.name not in target_collection.objects:
            # Remove from any other collections first
            for collection in bpy.data.collections:
                if lod_obj.name in collection.objects and collection != target_collection:
                    collection.objects.unlink(lod_obj)
            # Link to target collection
            if lod_obj.name not in target_collection.objects:
                target_collection.objects.link(lod_obj)
        
        # Clear parent temporarily to set world transform
        lod_obj.parent = None
        # Set world matrix to match original (includes all hierarchy transforms)
        lod_obj.matrix_world = orig_world_matrix
        
        # Parent to LODs group (but maintain world transform)
        lod_obj.parent = parent
        lod_obj.matrix_parent_inverse = parent.matrix_world.inverted()
        
        # Process the LOD
        if merge_dist > 0:
            merge_vertices_by_distance(lod_obj, merge_dist)
        
        if decimate_amt < 1.0:
            decimate_to_ratio(lod_obj, decimate_amt)
        
        # Check triangle count and decimate further if needed
            current_tris = get_triangle_count(lod_obj)
            if current_tris > max_tris:
                decimate_to_triangle_count(lod_obj, max_tris)
        
        # Apply limited dissolve if angle > 0
        if dissolve_angle > 0:
            limited_dissolve_mesh(lod_obj, dissolve_angle)
        
        # Apply smoothing if factor > 0
        if smooth_factor > 0:
            smooth_mesh(lod_obj, smooth_factor)
        
        # Expand along normals if amount > 0
        if expand_amount > 0:
            expand_mesh(lod_obj, expand_amount)
        
        # Remove degenerate triangles
        remove_degenerate_triangles(lod_obj)
        
        self.report({"INFO"}, f"Baked LOD {lod_index}: {get_triangle_count(lod_obj)} triangles")
        return {"FINISHED"}


class POPTOOLS_OT_lod_viewer_update(bpy.types.Operator):
    bl_idname = "poptools.lod_viewer_update"
    bl_label = "Update LOD Viewer"
    bl_options = {"INTERNAL"}
    
    @staticmethod
    def update_lod_visibility(context):
        """Update LOD visibility based on viewer level. Safe to call from property update."""
        try:
            active = context.view_layer.objects.active
            if not active:
                return
            
            # Find LOD parent
            lod_parent = None
            if active.name.endswith("_LODs"):
                lod_parent = active
            else:
                for obj in context.selected_objects:
                    if obj.name.endswith("_LODs"):
                        lod_parent = obj
                        break
            
            if not lod_parent:
                return
            
            # Get LOD children
            lod_children = [child for child in lod_parent.children 
                           if child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3", "_Full"))]
            # Sort: Full first, then LOD0, LOD1, LOD2, LOD3
            lod_children.sort(key=lambda x: (x.name.endswith("_Full"), x.name))
            
            # Limit to first 5 LODs (Full + LOD0-3)
            lod_children = lod_children[:5]
            
            if not lod_children:
                return
            
            current_lod = context.scene.poptools_lod_viewer_level
            
            # Clamp to available range (max 4, which gives us 5 total: 0=Full, 1-4=LOD0-3)
            max_lod = min(len(lod_children) - 1, 4)
            if current_lod > max_lod:
                current_lod = max_lod
                # Schedule property update for next frame
                bpy.app.timers.register(lambda: setattr(context.scene, "poptools_lod_viewer_level", current_lod), first_interval=0.01)
                return
            
            # Update visibility using deferred execution
            def update_visibility():
                for i, lod_obj in enumerate(lod_children):
                    if i == current_lod:
                        lod_obj.hide_viewport = False
                        lod_obj.hide_render = False
                    else:
                        lod_obj.hide_viewport = True
                        lod_obj.hide_render = True
            
            # Defer visibility update to avoid draw context issues
            bpy.app.timers.register(update_visibility, first_interval=0.01)
        except:
            pass  # Silently fail if we can't update


class POPTOOLS_OT_lodify_bake_all(bpy.types.Operator):
    bl_idname = "poptools.lodify_bake_all"
    bl_label = "Bake All LODs"
    bl_options = {"REGISTER", "UNDO"}
    
    close_dialog: BoolProperty(default=False)

    def execute(self, context):
        selected = selected_objects(context)
        if not selected:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}
        
        base_obj = context.view_layer.objects.active or selected[0]
        
        if not base_obj:
            self.report({"WARNING"}, "No valid object selected.")
            return {"CANCELLED"}
        
        # Check if we're working with an existing LOD group
        lod_parent = None
        full_obj = None
        is_regenerating = False
        
        if base_obj.name.endswith("_LODs"):
            lod_parent = base_obj
            is_regenerating = True
        elif base_obj.parent and base_obj.parent.name.endswith("_LODs"):
            # Active is a child of a LOD parent (could be _Full, _LOD0, etc.)
            lod_parent = base_obj.parent
            is_regenerating = True
        else:
            # Check if any selected object is a LOD parent or child of one
            for obj in selected:
                if obj is not None and hasattr(obj, 'name'):
                    if obj.name.endswith("_LODs"):
                        lod_parent = obj
                        is_regenerating = True
                        break
                    elif obj.parent and obj.parent.name.endswith("_LODs"):
                        lod_parent = obj.parent
                        is_regenerating = True
                        break
        
        if is_regenerating and lod_parent:
            # Find the Full object
            full_obj = None
            for child in lod_parent.children:
                if child is not None and hasattr(child, 'name') and child.name.endswith("_Full"):
                    full_obj = child
                    break
            
            if not full_obj or not hasattr(full_obj, 'type') or full_obj.type != "MESH":
                self.report({"WARNING"}, "Could not find Full object in LOD group. Please select the original object instead.")
                return {"CANCELLED"}
            
            base_obj = full_obj
            if not hasattr(base_obj, 'name'):
                self.report({"WARNING"}, "Full object has no name attribute.")
                return {"CANCELLED"}
            model_name = base_obj.name.replace("_Full", "")
            
            # Delete existing LOD objects (LOD0-3)
            lod_objects_to_delete = []
            for child in lod_parent.children:
                if child is not None and hasattr(child, 'name') and child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3")):
                    lod_objects_to_delete.append(child)
            
            # Delete the LOD objects
            for lod_obj in lod_objects_to_delete:
                if lod_obj is not None and hasattr(lod_obj, 'name') and lod_obj.name in bpy.data.objects:
                    try:
                        bpy.data.objects.remove(lod_obj, do_unlink=True)
                    except (RuntimeError, ReferenceError):
                        # Object might already be deleted or invalid
                        pass
        else:
            if not base_obj or not hasattr(base_obj, 'type') or base_obj.type != "MESH":
                self.report({"WARNING"}, "Active object must be a mesh.")
                return {"CANCELLED"}
            if not hasattr(base_obj, 'name'):
                self.report({"WARNING"}, "Object has no name attribute.")
                return {"CANCELLED"}
            model_name = base_obj.name
        
        s = context.scene
        export_dir = bpy.path.abspath(s.poptools_lodify_export_dir) if s.poptools_lodify_export_dir else ""
        
        # Export is optional - skip if no directory set
        do_export = bool(export_dir)
        if do_export:
            try:
                ensure_dir(export_dir)
            except (PermissionError, OSError) as e:
                self.report({"WARNING"}, f"Cannot create export directory ({str(e)}). Skipping export, creating LODs in scene only.")
                do_export = False
            except Exception as e:
                self.report({"WARNING"}, f"Export directory error: {str(e)}. Skipping export, creating LODs in scene only.")
                do_export = False
        
        # Ensure base_obj is valid at this point
        if not base_obj or base_obj.type != "MESH":
            self.report({"WARNING"}, "Invalid base object for LOD generation.")
            return {"CANCELLED"}
        
        # Store original world matrix (accounts for all parent transforms)
        orig_world_matrix = base_obj.matrix_world.copy()
        
        # Get the collection(s) that the base object belongs to
        # Use the first collection the object is in, or fall back to active collection
        target_collection = None
        for collection in bpy.data.collections:
            if base_obj.name in collection.objects:
                target_collection = collection
                break
        
        if not target_collection:
            target_collection = context.collection
        
        # Get or create parent
        parent_name = f"{model_name}_LODs"
        parent = bpy.data.objects.get(parent_name)
        if not parent:
            parent = bpy.data.objects.new(parent_name, None)
            target_collection.objects.link(parent)
            # Set parent's world transform to match base object's world transform
            parent.matrix_world = orig_world_matrix
        else:
            # If regenerating, parent already exists - just ensure Full is parented correctly
            if is_regenerating and full_obj and full_obj.name in bpy.data.objects:
                full_obj.parent = parent
                full_obj.matrix_parent_inverse = parent.matrix_world.inverted()
        
        # Store original selection
        orig_active = context.view_layer.objects.active
        orig_selected = list(context.selected_objects)
        
        # Create _Full version (only if not regenerating)
        if not is_regenerating:
            # Ensure we're in object mode
            context.view_layer.objects.active = base_obj
            if base_obj.mode != 'OBJECT':
                bpy.ops.object.mode_set(mode='OBJECT')
            
            # Create _Full version as a duplicate in the scene
            # Deselect all first
            for obj in context.selected_objects:
                obj.select_set(False)
            base_obj.select_set(True)
            context.view_layer.objects.active = base_obj
            bpy.ops.object.duplicate()
            full_obj = context.active_object
            full_obj.name = f"{model_name}_Full"
            
            # Ensure the duplicated object is in the same collection as the base object
            if target_collection and full_obj.name not in target_collection.objects:
                # Remove from any other collections first
                for collection in bpy.data.collections:
                    if full_obj.name in collection.objects and collection != target_collection:
                        collection.objects.unlink(full_obj)
                # Link to target collection
                if full_obj.name not in target_collection.objects:
                    target_collection.objects.link(full_obj)
            
            # Make sure we have a unique mesh data block (not shared with original)
            if full_obj.data == base_obj.data:
                full_obj.data = base_obj.data.copy()
            
            # Store world matrix and set it on the full object
            orig_world_matrix = base_obj.matrix_world.copy()
            full_obj.parent = None
            full_obj.matrix_world = orig_world_matrix
            
            # Parent to LODs group
            full_obj.parent = parent
            full_obj.matrix_parent_inverse = parent.matrix_world.inverted()
        else:
            # When regenerating, full_obj already exists and is set up
            # Just ensure it's properly parented
            if full_obj.parent != parent:
                full_obj.parent = parent
                full_obj.matrix_parent_inverse = parent.matrix_world.inverted()
        
        # Export Full as _Full (if export enabled)
        if do_export:
            # Ensure object mode
            context.view_layer.objects.active = full_obj
            if full_obj.mode != 'OBJECT':
                bpy.ops.object.mode_set(mode='OBJECT')
            # Deselect all and select full_obj
            for obj in context.selected_objects:
                obj.select_set(False)
            full_obj.select_set(True)
            context.view_layer.objects.active = full_obj
            
            full_name = f"{model_name}_Full"
            full_path = os.path.join(export_dir, f"{full_name}.gltf")
            
            try:
                bpy.ops.export_scene.gltf(
                    filepath=full_path,
                    use_selection=True,
                    export_format="GLTF_SEPARATE",
                )
            except Exception as e:
                self.report({"WARNING"}, f"Failed to export original: {e}. Continuing with LOD creation...")
                do_export = False
        
        # Store the original base object name to always reference it fresh
        base_obj_name = base_obj.name
        
        # Bake each LOD
        lod_objects = []
        for lod_index in range(4):
            # Always get a fresh reference to the base object
            base_obj = bpy.data.objects.get(base_obj_name)
            if not base_obj:
                self.report({"WARNING"}, f"Base object '{base_obj_name}' no longer exists, stopping LOD generation.")
                break
            
            merge_dist = getattr(s, f"poptools_lodify_lod{lod_index}_merge", 0.0)
            decimate_amt = getattr(s, f"poptools_lodify_lod{lod_index}_decimate", 1.0)
            max_tris = getattr(s, f"poptools_lodify_lod{lod_index}_max_tris", 10000000)
            dissolve_angle = getattr(s, f"poptools_lodify_lod{lod_index}_dissolve", 0.0)
            smooth_factor = getattr(s, f"poptools_lodify_lod{lod_index}_smooth", 0.0)
            expand_amount = getattr(s, f"poptools_lodify_lod{lod_index}_expand", 0.0)
            
            # Store world matrix (accounts for all parent transforms in hierarchy)
            orig_world_matrix = base_obj.matrix_world.copy()
            
            # Ensure object mode
            if base_obj.mode != 'OBJECT':
                context.view_layer.objects.active = base_obj
                bpy.ops.object.mode_set(mode='OBJECT')
            
            # Deselect all objects first
            for obj in context.selected_objects:
                obj.select_set(False)
            
            # Select and activate the base object
            base_obj.select_set(True)
            context.view_layer.objects.active = base_obj
            
            # Verify selection
            if not base_obj.select_get() or context.view_layer.objects.active != base_obj:
                self.report({"WARNING"}, f"Failed to select base object for LOD {lod_index}.")
                continue
            
            # Perform duplication - use direct API instead of operator for reliability
            try:
                # Create a new object by copying the mesh data
                new_mesh = base_obj.data.copy()
                lod_obj = bpy.data.objects.new(name=f"{model_name}_LOD{lod_index}", object_data=new_mesh)
                
                # Link to the same collection as the base object
                target_collection.objects.link(lod_obj)
                
                # Copy transform
                lod_obj.matrix_world = orig_world_matrix
                
            except Exception as e:
                self.report({"WARNING"}, f"Failed to duplicate object for LOD {lod_index}: {e}")
                continue
            
            if not lod_obj:
                self.report({"WARNING"}, f"Failed to duplicate object for LOD {lod_index}.")
                continue
            
            # Verify the duplicated object is valid
            if lod_obj.name not in bpy.data.objects:
                self.report({"WARNING"}, f"Duplicated object is invalid for LOD {lod_index}.")
                continue
            
            # Clear parent temporarily to set world transform
            lod_obj.parent = None
            # Set world matrix to match original (includes all hierarchy transforms)
            lod_obj.matrix_world = orig_world_matrix
            
            # Parent to LODs group (but maintain world transform)
            lod_obj.parent = parent
            lod_obj.matrix_parent_inverse = parent.matrix_world.inverted()
            lod_objects.append(lod_obj)
            
            # Process
            if merge_dist > 0:
                merge_vertices_by_distance(lod_obj, merge_dist)
            
            if decimate_amt < 1.0:
                decimate_to_ratio(lod_obj, decimate_amt)
            
            current_tris = get_triangle_count(lod_obj)
            if current_tris > max_tris:
                decimate_to_triangle_count(lod_obj, max_tris)
            
            # Apply limited dissolve if angle > 0
            if dissolve_angle > 0:
                limited_dissolve_mesh(lod_obj, dissolve_angle)
            
            # Apply smoothing if factor > 0
            if smooth_factor > 0:
                smooth_mesh(lod_obj, smooth_factor)
            
            # Expand along normals if amount > 0
            if expand_amount > 0:
                expand_mesh(lod_obj, expand_amount)
            
            # Remove degenerate triangles
            remove_degenerate_triangles(lod_obj)
            
            # Export (if enabled)
            if do_export:
                # Ensure object mode
                context.view_layer.objects.active = lod_obj
                if lod_obj.mode != 'OBJECT':
                    bpy.ops.object.mode_set(mode='OBJECT')
                # Deselect all and select lod_obj
                for obj in context.selected_objects:
                    obj.select_set(False)
                lod_obj.select_set(True)
                context.view_layer.objects.active = lod_obj
                
                lod_path = os.path.join(export_dir, f"{model_name}_LOD{lod_index}.gltf")
                try:
                    bpy.ops.export_scene.gltf(
                        filepath=lod_path,
                        use_selection=True,
                        export_format="GLTF_SEPARATE",
                    )
                except Exception as e:
                    self.report({"WARNING"}, f"Failed to export LOD {lod_index}: {e}")
        
        # Restore selection
        for obj in context.selected_objects:
            obj.select_set(False)
        for obj in orig_selected:
            if obj and obj.name in bpy.data.objects:
                obj.select_set(True)
        if orig_active and orig_active.name in bpy.data.objects:
            context.view_layer.objects.active = orig_active
        
        if do_export:
            if is_regenerating:
                self.report({"INFO"}, f"Regenerated and exported all LODs to: {export_dir}")
            else:
                self.report({"INFO"}, f"Created and exported all LODs to: {export_dir}")
        else:
            if is_regenerating:
                self.report({"INFO"}, f"Regenerated all LOD objects in scene (export skipped)")
            else:
                self.report({"INFO"}, f"Created all LOD objects in scene (export skipped)")
        
        # Close dialog if requested
        if self.close_dialog:
            # Use timer to close popup after operator finishes
            def close_popup():
                # Try to close any open popup dialogs by pressing ESC
                # This is a workaround since we can't directly close the dialog from another operator
                try:
                    # Simulate ESC key press to close dialog
                    bpy.ops.wm.call_menu(name="TOPBAR_MT_window")
                except:
                    pass
                return None  # Don't repeat timer
            
            # Schedule close for next frame
            bpy.app.timers.register(close_popup, first_interval=0.1)
        
        return {"FINISHED"}


class POPTOOLS_OT_popup(bpy.types.Operator):
    bl_idname = "poptools.popup"
    bl_label = "Wren Tools"
    bl_options = {"REGISTER"}

    def invoke(self, context, event):
        wm = context.window_manager
        # Reset close flag
        if hasattr(context.scene, 'poptools_close_popup'):
            context.scene.poptools_close_popup = False
        return wm.invoke_props_dialog(self, width=420)
    

    def draw(self, context):
        s = context.scene
        layout = self.layout

        col = layout.column(align=True)
        col.label(text="Decimate", icon="MOD_DECIM")
        box = col.box()
        box.prop(s, "poptools_decimate_ratio")
        row = box.row(align=True)
        row.prop(s, "poptools_decimate_apply")
        row.prop(s, "poptools_decimate_only_active")
        box.operator("poptools.decimate_selected", icon="CHECKMARK")

        col = layout.column(align=True)
        col.label(text="Camera", icon="VIEW_CAMERA")
        box = col.box()
        box.prop(s, "wrentools_camera_speed")
        box.operator("poptools.unity_camera", icon="CON_CAMERASOLVER")
        box.label(text="LMB/Enter=confirm, RMB=cancel", icon="INFO")

        col = layout.column(align=True)
        col.label(text="Vertex Color Material", icon="MATERIAL")
        box = col.box()
        box.prop(s, "wrentools_vertexcolor_mat_name")
        box.operator("poptools.apply_vertex_color_material", icon="CHECKMARK")

        col = layout.column(align=True)
        col.label(text="Transform", icon="OBJECT_ORIGIN")
        box = col.box()
        box.operator("poptools.origin_to_center", icon="DOT")
        box.operator("poptools.arrange_in_grid", icon="GRID")
        box.operator("poptools.create_octahedral_imposter", icon="MESH_ICOSPHERE")

        col = layout.column(align=True)
        col.label(text="Export FBX", icon="EXPORT")
        box = col.box()
        box.prop(s, "poptools_export_dir")
        box.prop(s, "poptools_export_mode")
        if s.poptools_export_mode == "SINGLE_FILE":
            box.prop(s, "poptools_export_filename")
        box.prop(s, "poptools_export_bake_space_transform")
        box.prop(s, "poptools_export_add_leaf_bones")
        box.prop(s, "poptools_export_use_active_collection")
        box.operator("poptools.export_selected_fbx", icon="FILE_TICK")

        col = layout.column(align=True)
        col.label(text="LODify", icon="MOD_DECIM")
        box = col.box()
        box.operator("poptools.lodify", icon="EXPORT")

    def execute(self, context):
        # Dialog uses buttons that run the operators; no-op here
        # Check if we should close
        if hasattr(context.scene, 'poptools_close_popup') and context.scene.poptools_close_popup:
            context.scene.poptools_close_popup = False
            return {"CANCELLED"}  # This closes the dialog
        return {"FINISHED"}


class POPTOOLS_PT_sidebar(bpy.types.Panel):
    bl_label = "Wren Tools"
    bl_idname = "POPTOOLS_PT_sidebar"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "Wren Tools"

    def draw(self, context):
        layout = self.layout
        layout.operator("poptools.popup", icon="WINDOW")
        layout.separator()
        
        # Triangle count display
        box = layout.box()
        box.label(text="Triangle Count", icon="MESH_DATA")
        
        selected = selected_objects(context)
        active = context.view_layer.objects.active
        
        if active and active.type == "MESH":
            tri_count = get_triangle_count(active)
            box.label(text=f"Active: {active.name}", icon="OBJECT_DATA")
            box.label(text=f"Triangles: {tri_count:,}")
        
        if selected:
            mesh_count = sum(1 for obj in selected if obj.type == "MESH")
            if mesh_count > 0:
                box.separator()
                box.label(text=f"Selected ({mesh_count} mesh):")
                total_tris = 0
                for obj in selected:
                    if obj.type == "MESH":
                        tris = get_triangle_count(obj)
                        total_tris += tris
                        box.label(text=f"  {obj.name}: {tris:,}", icon="MESH_DATA")
                
                if mesh_count > 1:
                    box.separator()
                    box.label(text=f"Total: {total_tris:,}", icon="CHECKMARK")
        
        layout.separator()
        layout.label(text="Quick Actions:")
        layout.operator("poptools.unity_camera", icon="VIEW_CAMERA")
        layout.operator("poptools.apply_vertex_color_material", icon="MATERIAL")
        layout.operator("poptools.origin_to_center", icon="OBJECT_ORIGIN")
        layout.operator("poptools.arrange_in_grid", icon="GRID")
        layout.operator("poptools.create_octahedral_imposter", icon="MESH_ICOSPHERE")
        layout.operator("poptools.decimate_selected", icon="MOD_DECIM")
        layout.operator("poptools.export_selected_fbx", icon="EXPORT")
        layout.separator()
        layout.operator("poptools.lodify", icon="MOD_DECIM")
        
        # Export Collection LODs button - always visible
        layout.separator()
        box = layout.box()
        box.alert = True  # Make it stand out with red/alert styling
        box.label(text="Collection Export", icon="OUTLINER_COLLECTION")
        row = box.row()
        row.scale_y = 1.5  # Make button larger
        op = row.operator("poptools.export_collection_lods", text="Export All Collection LODs", icon="EXPORT")
        
        # LOD Viewer slider
        active = context.view_layer.objects.active
        if active:
            # Check if active object is a LOD parent, child of LOD parent, or has LOD children
            lod_children = []
            lod_parent = None
            
            if active.name.endswith("_LODs"):
                # Active is the LOD parent itself
                lod_parent = active
                lod_children = [child for child in active.children if child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3", "_Full"))]
            elif active.parent and active.parent.name.endswith("_LODs"):
                # Active is a child of a LOD parent (e.g. _Full, _LOD0, etc.)
                lod_parent = active.parent
                lod_children = [child for child in lod_parent.children if child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3", "_Full"))]
            else:
                # Check if any selected object is a LOD parent or child of one
                for obj in context.selected_objects:
                    if obj.name.endswith("_LODs"):
                        lod_parent = obj
                        lod_children = [child for child in obj.children if child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3", "_Full"))]
                        break
                    elif obj.parent and obj.parent.name.endswith("_LODs"):
                        lod_parent = obj.parent
                        lod_children = [child for child in lod_parent.children if child.name.endswith(("_LOD0", "_LOD1", "_LOD2", "_LOD3", "_Full"))]
                        break
            
            if lod_children:
                # Sort LODs: Full first, then LOD0, LOD1, LOD2, LOD3
                lod_children.sort(key=lambda x: (x.name.endswith("_Full"), x.name))
                lod_children = lod_children[:5]  # Limit to 5 LODs (Full + LOD0-3)
                
                box = layout.box()
                box.label(text="LOD Viewer", icon="VIEW3D")
                
                # Get current LOD level from scene property
                current_lod = context.scene.poptools_lod_viewer_level
                max_lod = min(len(lod_children) - 1, 4)
                
                # Create slider (property update will handle visibility changes)
                row = box.row()
                row.prop(context.scene, "poptools_lod_viewer_level", text="Level", slider=True)
                
                # Show current LOD name and triangle count
                # Clamp current_lod to valid range
                display_lod = min(max(current_lod, 0), max_lod)
                if 0 <= display_lod < len(lod_children):
                    current_lod_obj = lod_children[display_lod]
                    if current_lod_obj and current_lod_obj.type == "MESH":
                        tri_count = get_triangle_count(current_lod_obj)
                        box.label(text=f"Showing: {current_lod_obj.name}", icon="CHECKMARK")
                        box.label(text=f"Triangles: {tri_count:,}", icon="MESH_DATA")
                    else:
                        box.label(text=f"Showing: {current_lod_obj.name if current_lod_obj else 'Unknown'}", icon="INFO")
                        box.label(text="No mesh data", icon="INFO")
                else:
                    box.label(text="No LOD at this level", icon="INFO")


classes = (
    POPTOOLS_OT_decimate_selected,
    POPTOOLS_OT_export_lod_full_fbx,
    POPTOOLS_OT_export_collection_lods,
    POPTOOLS_OT_export_selected_fbx,
    POPTOOLS_OT_unity_camera,
    POPTOOLS_OT_origin_to_center,
    POPTOOLS_OT_arrange_in_grid,
    POPTOOLS_OT_create_octahedral_imposter,
    POPTOOLS_OT_apply_vertex_color_material,
    POPTOOLS_OT_lodify_reset_export_path,
    POPTOOLS_OT_lodify,
    POPTOOLS_OT_lodify_bake,
    POPTOOLS_OT_lodify_bake_all,
    POPTOOLS_OT_lod_viewer_update,
    POPTOOLS_OT_popup,
    POPTOOLS_PT_sidebar,
)


def register():
    for c in classes:
        bpy.utils.register_class(c)

    bpy.types.Scene.poptools_decimate_ratio = FloatProperty(
        name="Ratio",
        description="Decimate ratio (1.0 = no change, 0.5 = half)",
        default=0.5,
        min=0.0001,
        max=1.0,
        subtype="FACTOR",
    )
    bpy.types.Scene.poptools_decimate_apply = BoolProperty(
        name="Apply",
        description="Apply the decimate modifier after creating it",
        default=True,
    )
    bpy.types.Scene.poptools_decimate_only_active = BoolProperty(
        name="Only Active",
        description="Only decimate the active object (if any) instead of all selected",
        default=False,
    )

    bpy.types.Scene.wrentools_camera_speed = FloatProperty(
        name="Fly Speed",
        description="Movement speed for Unity camera mode",
        default=5.0,
        min=0.1,
        max=50.0,
        subtype="NONE",
    )

    bpy.types.Scene.wrentools_vertexcolor_mat_name = StringProperty(
        name="Material Name",
        description="Name for the vertex color material",
        default="VertexColorMaterial",
    )

    bpy.types.Scene.poptools_export_dir = StringProperty(
        name="Export Directory",
        description="Folder to export FBX files into (supports // relative paths)",
        default="//exports_fbx/",
        subtype="DIR_PATH",
    )
    bpy.types.Scene.poptools_export_mode = EnumProperty(
        name="Mode",
        description="Export all selected to one file, or one file per object",
        items=(
            ("SINGLE_FILE", "Single FBX", "Export all selected objects into one FBX"),
            ("PER_OBJECT", "Per Object", "Export each selected object as its own FBX"),
        ),
        default="PER_OBJECT",
    )
    bpy.types.Scene.poptools_export_filename = StringProperty(
        name="Filename",
        description="Filename when exporting a single FBX",
        default="export.fbx",
    )
    bpy.types.Scene.poptools_export_bake_space_transform = BoolProperty(
        name="Bake Space Transform",
        description="Bake space transform on export (useful for some pipelines)",
        default=True,
    )
    bpy.types.Scene.poptools_export_add_leaf_bones = BoolProperty(
        name="Add Leaf Bones",
        description="FBX option (generally disable for game engines)",
        default=False,
    )
    bpy.types.Scene.poptools_export_use_active_collection = BoolProperty(
        name="Only Active Collection",
        description="Only export selected objects that are in the active collection",
        default=False,
    )

    # LODify properties
    bpy.types.Scene.poptools_lodify_export_dir = StringProperty(
        name="Export Directory",
        description="Folder to export LOD GLTF files into (supports // relative paths)",
        default="//exports_lod/",
        subtype="DIR_PATH",
    )
    
    bpy.types.Scene.poptools_lodify_fbx_export_path = StringProperty(
        name="FBX Export Path",
        description="Full path where FBX files will be exported (auto-computed from blend file and object name)",
        default="",
        subtype="DIR_PATH",
    )
    
    def update_lod_viewer(self, context):
        """Update LOD visibility when slider changes."""
        POPTOOLS_OT_lod_viewer_update.update_lod_visibility(context)
    
    bpy.types.Scene.poptools_lod_viewer_level = IntProperty(
        name="LOD Viewer Level",
        description="Current LOD level to display (0=Full, 1-4=LOD0-3)",
        default=0,
        min=0,
        max=4,
        update=update_lod_viewer,
    )
    
    # LOD level properties (4 levels)
    for i in range(4):
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_merge", FloatProperty(
            name=f"Merge Distance LOD {i}",
            description=f"Distance threshold for merging vertices in LOD {i}",
            default=0.0,
            min=0.0,
            subtype="DISTANCE",
        ))
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_decimate", FloatProperty(
            name=f"Decimate Amount LOD {i}",
            description=f"Decimation ratio for LOD {i} (1.0 = no change, 0.5 = half)",
            default=1.0,
            min=0.01,
            max=1.0,
            subtype="FACTOR",
        ))
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_max_tris", IntProperty(
            name=f"Max Triangles LOD {i}",
            description=f"Maximum triangle count for LOD {i}",
            default=10000000,
            min=1,
        ))
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_smooth", FloatProperty(
            name=f"Smoothness LOD {i}",
            description=f"Smoothing factor for LOD {i} (0 = none, 1 = full smoothing)",
            default=0.0,
            min=0.0,
            max=1.0,
            subtype="FACTOR",
        ))
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_expand", FloatProperty(
            name=f"Expand LOD {i}",
            description=f"Expand vertices along normals for LOD {i} (useful to restore silhouette)",
            default=0.0,
            min=0.0,
            max=1.0,
            subtype="DISTANCE",
        ))
        setattr(bpy.types.Scene, f"poptools_lodify_lod{i}_dissolve", FloatProperty(
            name=f"Limited Dissolve LOD {i}",
            description=f"Limited dissolve angle for LOD {i} in degrees (0 = disabled, removes flat geometry)",
            default=0.0,
            min=0.0,
            max=180.0,
            subtype="ANGLE",
        ))
    
    bpy.types.Scene.poptools_close_popup = BoolProperty(
        name="Close Popup",
        description="Internal flag to close popup dialog",
        default=False,
    )


def unregister():
    # Remove props
    del bpy.types.Scene.poptools_decimate_ratio
    del bpy.types.Scene.poptools_decimate_apply
    del bpy.types.Scene.poptools_decimate_only_active

    del bpy.types.Scene.wrentools_camera_speed
    del bpy.types.Scene.wrentools_vertexcolor_mat_name

    del bpy.types.Scene.poptools_export_dir
    del bpy.types.Scene.poptools_export_mode
    del bpy.types.Scene.poptools_export_filename
    del bpy.types.Scene.poptools_export_bake_space_transform
    del bpy.types.Scene.poptools_export_add_leaf_bones
    del bpy.types.Scene.poptools_export_use_active_collection

    del bpy.types.Scene.poptools_lodify_export_dir
    del bpy.types.Scene.poptools_lodify_fbx_export_path
    del bpy.types.Scene.poptools_lod_viewer_level
    del bpy.types.Scene.poptools_close_popup
    for i in range(4):
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_merge")
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_decimate")
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_max_tris")
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_smooth")
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_expand")
        delattr(bpy.types.Scene, f"poptools_lodify_lod{i}_dissolve")

    for c in reversed(classes):
        bpy.utils.unregister_class(c)


if __name__ == "__main__":
    register()
    