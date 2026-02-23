# Blender Add-on: Grease Pencil LOD Helpers
# Convert geometry nodes output to child mesh objects
#
# Install: Blender > Edit > Preferences > Add-ons > Install... (pick this .py) > Enable
# Use: 3D View > Sidebar (N) > "GP LOD Helpers" tab

bl_info = {
    "name": "Grease Pencil LOD Helpers",
    "author": "Isaac Cohen",
    "version": (1, 0, 0),
    "blender": (3, 6, 0),
    "location": "View3D > Sidebar > GP LOD Helpers",
    "description": "Tools for converting geometry nodes output to mesh objects",
    "category": "Object",
}

import bpy
import bmesh
import re
import os
from bpy.props import (
    StringProperty,
    BoolProperty,
)

# Support both Blender 3.x ('GPENCIL') and Blender 4.x ('GREASEPENCIL')
GP_TYPES = {'GPENCIL', 'GREASEPENCIL'}

def is_grease_pencil(obj):
    """Check if object is a grease pencil (works for both Blender 3.x and 4.x)"""
    return obj.type in GP_TYPES


class GPLOD_Preferences(bpy.types.AddonPreferences):
    """Addon preferences for GP LOD Helpers"""
    bl_idname = __name__

    asset_library_path: StringProperty(
        name="Asset Library Path",
        description="Path to the .blend file containing the GreasePencilToMesh node group",
        subtype='FILE_PATH',
        default=""
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="Asset Library Settings:")
        layout.prop(self, "asset_library_path")
        layout.label(text="Set this to the .blend file containing your GreasePencilToMesh node group")


def get_or_load_node_group(node_group_name="GreasePencilToMesh"):
    """Get the node group, loading it from asset library if needed"""
    node_group = bpy.data.node_groups.get(node_group_name)
    
    if node_group:
        return node_group
    
    # Try to load from asset library
    addon_prefs = bpy.context.preferences.addons.get(__name__)
    if not addon_prefs:
        return None
    
    asset_path = addon_prefs.preferences.asset_library_path
    if not asset_path or not os.path.exists(bpy.path.abspath(asset_path)):
        return None
    
    asset_path = bpy.path.abspath(asset_path)
    
    # Append the node group from the library
    with bpy.data.libraries.load(asset_path, link=False) as (data_from, data_to):
        if node_group_name in data_from.node_groups:
            data_to.node_groups = [node_group_name]
    
    return bpy.data.node_groups.get(node_group_name)


class GPLOD_OT_empty_with_cube(bpy.types.Operator):
    """Build mesh LODs from grease pencil geometry"""
    bl_idname = "gplod.empty_with_cube"
    bl_label = "Build Mesh LODs"
    bl_description = "Build mesh LODs from grease pencil geometry for all selected objects"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        # Get all selected objects at the start - ONLY process grease pencil objects
        selected_objects = [obj for obj in context.selected_objects if is_grease_pencil(obj)]
        
        if not selected_objects:
            self.report({"WARNING"}, "No grease pencil objects selected.")
            return {"CANCELLED"}
        
        node_group = get_or_load_node_group("GreasePencilToMesh")
        if not node_group:
            self.report({"WARNING"}, "GreasePencilToMesh node group not found. Set the asset library path in addon preferences (Edit > Preferences > Add-ons > GP LOD Helpers).")
            return {"CANCELLED"}
        
        created_empties = []
        
        for active in selected_objects:
            # Delete existing children that we previously created
            children_to_delete = []
            for child in active.children:
                # SAFETY: Never delete grease pencil objects
                if is_grease_pencil(child):
                    continue
                    
                # Check if it's an empty with the same name (our created empty)
                if child.type == 'EMPTY' and child.name.startswith(active.name):
                    # Also collect all descendants of this empty
                    def collect_descendants(obj):
                        descendants = []
                        # SAFETY: Never include grease pencil objects
                        if is_grease_pencil(obj):
                            return descendants
                        descendants.append(obj)
                        for c in obj.children:
                            descendants.extend(collect_descendants(c))
                        return descendants
                    children_to_delete.extend(collect_descendants(child))
                # Also check for LOD objects directly parented (must be MESH or EMPTY type)
                elif (child.type in {'MESH', 'EMPTY'} and 
                      child.name.startswith(active.name) and "_LOD" in child.name):
                    children_to_delete.append(child)
            
            # SAFETY: Final verification - filter out any grease pencil objects
            children_to_delete = [obj for obj in children_to_delete if not is_grease_pencil(obj)]
            
            # Delete collected objects and their mesh data
            for obj in children_to_delete:
                # SAFETY: Double-check we're not deleting a grease pencil
                if is_grease_pencil(obj):
                    continue
                # Store mesh reference before deleting object
                mesh_to_delete = obj.data if obj.type == 'MESH' else None
                bpy.data.objects.remove(obj, do_unlink=True)
                # Delete mesh if it has no remaining users
                if mesh_to_delete and mesh_to_delete.users == 0:
                    bpy.data.meshes.remove(mesh_to_delete)
            
            # Create an empty object with the same name
            empty_obj = bpy.data.objects.new(active.name, None)
            
            # Link to the same collection as the original
            linked = False
            for coll in active.users_collection:
                coll.objects.link(empty_obj)
                linked = True
                break
            
            # Fallback: link to scene collection
            if not linked:
                context.scene.collection.objects.link(empty_obj)
            
            # Set transform to match original
            empty_obj.matrix_world = active.matrix_world.copy()
            
            # Parent empty to original object (maintaining world transform)
            empty_obj.parent = active
            empty_obj.matrix_parent_inverse = active.matrix_world.inverted()
            
            # Create a cube mesh
            cube_mesh = bpy.data.meshes.new(f"{active.name}_Cube")
            bm = bmesh.new()
            bmesh.ops.create_cube(bm, size=1.0)
            bm.to_mesh(cube_mesh)
            bm.free()
            
            # Create cube object
            cube_obj = bpy.data.objects.new(f"{active.name}_Cube", cube_mesh)
            
            # Link cube to same collection
            for coll in active.users_collection:
                coll.objects.link(cube_obj)
                break
            if not linked:
                context.scene.collection.objects.link(cube_obj)
            
            # Parent cube to the empty
            cube_obj.parent = empty_obj
            cube_obj.matrix_parent_inverse = empty_obj.matrix_world.inverted()
            
            # Add Geometry Nodes modifier with GreasePencilToMesh
            mod = cube_obj.modifiers.new(name="GreasePencilToMesh", type="NODES")
            mod.node_group = node_group
            
            # Find and set the Object input to the original selected object
            for item in mod.node_group.interface.items_tree:
                if item.item_type == 'SOCKET' and item.in_out == 'INPUT':
                    if item.socket_type == 'NodeSocketObject':
                        # Get the identifier for this input
                        identifier = item.identifier
                        mod[identifier] = active
                        break
            
            # Apply the geometry nodes modifier to get actual geometry
            context.view_layer.objects.active = cube_obj
            bpy.ops.object.modifier_apply(modifier=mod.name)
            
            # Separate by material
            # First deselect all, then select cube
            bpy.ops.object.select_all(action='DESELECT')
            cube_obj.select_set(True)
            context.view_layer.objects.active = cube_obj
            
            # Go to edit mode and separate by material
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.mesh.select_all(action='SELECT')
            bpy.ops.mesh.separate(type='MATERIAL')
            bpy.ops.object.mode_set(mode='OBJECT')
            
            # Get all objects that were created (they will be selected)
            # The original cube_obj plus any new separated objects
            separated_objects = [obj for obj in context.selected_objects]
            
            # Also check for objects parented to the empty that are meshes
            all_children = [obj for obj in empty_obj.children if obj.type == 'MESH']
            for obj in all_children:
                if obj not in separated_objects:
                    separated_objects.append(obj)
            
            # Sort objects by LOD number extracted from material names
            def get_lod_number(obj):
                """Extract LOD number from material name like 'vertex_color_LOD1'"""
                for slot in obj.material_slots:
                    if slot.material:
                        match = re.search(r'LOD(\d+)', slot.material.name)
                        if match:
                            return int(match.group(1))
                return 999  # Objects without matching material go to end
            
            separated_objects.sort(key=get_lod_number)
            
            # Rename them as LOD0, LOD1, etc. (sequential based on sorted order)
            original_name = active.name
            for i, obj in enumerate(separated_objects):
                obj.name = f"{original_name}_LOD{i}"
                # Ensure they're parented to the empty
                if obj.parent != empty_obj:
                    obj.parent = empty_obj
                    obj.matrix_parent_inverse = empty_obj.matrix_world.inverted()
            
            created_empties.append(empty_obj)
        
        # Select all created empties
        bpy.ops.object.select_all(action='DESELECT')
        for empty_obj in created_empties:
            empty_obj.select_set(True)
        if created_empties:
            context.view_layer.objects.active = created_empties[0]
        
        self.report({"INFO"}, f"Built LODs for {len(created_empties)} object(s)")
        return {"FINISHED"}


class GPLOD_OT_delete_mesh_lods(bpy.types.Operator):
    """Delete mesh LOD children of the selected objects"""
    bl_idname = "gplod.delete_mesh_lods"
    bl_label = "Delete Mesh LODs"
    bl_description = "Delete mesh LOD children of all selected grease pencil objects"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        # Get all selected objects at the start - ONLY process grease pencil objects
        selected_objects = [obj for obj in context.selected_objects if is_grease_pencil(obj)]
        
        if not selected_objects:
            self.report({"WARNING"}, "No grease pencil objects selected.")
            return {"CANCELLED"}
        
        total_deleted = 0
        objects_processed = 0
        
        for active in selected_objects:
            # Collect children to delete
            children_to_delete = []
            for child in active.children:
                # SAFETY: Never delete grease pencil objects
                if is_grease_pencil(child):
                    continue
                    
                # Check if it's an empty with the same name (our created empty)
                if child.type == 'EMPTY' and child.name.startswith(active.name):
                    # Also collect all descendants of this empty
                    def collect_descendants(obj):
                        descendants = []
                        # SAFETY: Never include grease pencil objects
                        if is_grease_pencil(obj):
                            return descendants
                        descendants.append(obj)
                        for c in obj.children:
                            descendants.extend(collect_descendants(c))
                        return descendants
                    children_to_delete.extend(collect_descendants(child))
                # Also check for LOD objects directly parented (must be MESH or EMPTY type)
                elif (child.type in {'MESH', 'EMPTY'} and 
                      child.name.startswith(active.name) and "_LOD" in child.name):
                    children_to_delete.append(child)
            
            # SAFETY: Final verification - filter out any grease pencil objects
            children_to_delete = [obj for obj in children_to_delete if not is_grease_pencil(obj)]
            
            if children_to_delete:
                objects_processed += 1
                total_deleted += len(children_to_delete)
                
                # Delete collected objects and their mesh data
                for obj in children_to_delete:
                    # SAFETY: Double-check we're not deleting a grease pencil
                    if is_grease_pencil(obj):
                        continue
                    # Store mesh reference before deleting object
                    mesh_to_delete = obj.data if obj.type == 'MESH' else None
                    bpy.data.objects.remove(obj, do_unlink=True)
                    # Delete mesh if it has no remaining users
                    if mesh_to_delete and mesh_to_delete.users == 0:
                        bpy.data.meshes.remove(mesh_to_delete)
        
        if total_deleted == 0:
            self.report({"INFO"}, "No LOD children found to delete.")
            return {"CANCELLED"}
        
        self.report({"INFO"}, f"Deleted {total_deleted} LOD object(s) from {objects_processed} parent(s)")
        return {"FINISHED"}


class GPLOD_OT_export_to_unity(bpy.types.Operator):
    """Export selected hierarchies to Unity folder"""
    bl_idname = "gplod.export_to_unity"
    bl_label = "Export to Unity Folder"
    bl_description = "Export all selected hierarchies as FBX to ../Resources/ISLANDS/[BlendFileName]/Models/"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        import os
        
        # Get all selected objects at the start
        selected_objects = [obj for obj in context.selected_objects]
        
        if not selected_objects:
            self.report({"WARNING"}, "No objects selected.")
            return {"CANCELLED"}
        
        # Get the blend file path
        blend_path = bpy.data.filepath
        if not blend_path:
            self.report({"ERROR"}, "Please save the .blend file first.")
            return {"CANCELLED"}
        
        # Get blend file directory and name (without extension)
        blend_dir = os.path.dirname(blend_path)
        blend_name = os.path.splitext(os.path.basename(blend_path))[0]
        
        # Build export path: ../Resources/ISLANDS/[BlendFileName]/Models/ObjectName.fbx
        parent_dir = os.path.dirname(blend_dir)
        export_dir = os.path.join(parent_dir, "Resources", "ISLANDS", blend_name, "Models")
        
        # Create directory if it doesn't exist
        os.makedirs(export_dir, exist_ok=True)
        
        exported_count = 0
        exported_paths = []
        
        # Helper function to select hierarchy
        def select_hierarchy(obj):
            obj.select_set(True)
            for child in obj.children:
                select_hierarchy(child)
        
        for active in selected_objects:
            # Get the object to export - if it's a grease pencil, find its LOD child empty
            export_obj = active
            object_name = active.name
            
            # If active is grease pencil, look for the LOD parent child
            if is_grease_pencil(active):
                for child in active.children:
                    if child.type == 'EMPTY' and child.name.startswith(active.name):
                        export_obj = child
                        break
            # If active is an empty parented to a grease pencil, use the grease pencil's name
            elif active.type == 'EMPTY' and active.parent and is_grease_pencil(active.parent):
                object_name = active.parent.name
            
            # Select the hierarchy for export
            bpy.ops.object.select_all(action='DESELECT')
            select_hierarchy(export_obj)
            context.view_layer.objects.active = export_obj
            
            # Build full export path
            export_path = os.path.join(export_dir, f"{object_name}.fbx")
            
            # Export as FBX
            bpy.ops.export_scene.fbx(
                filepath=export_path,
                use_selection=True,
                object_types={'EMPTY', 'MESH'},
                use_mesh_modifiers=True,
                add_leaf_bones=False,
                bake_space_transform=True,
                path_mode='AUTO'
            )
            
            exported_count += 1
            exported_paths.append(export_path)
        
        if exported_count == 1:
            self.report({"INFO"}, f"Exported to: {exported_paths[0]}")
        else:
            self.report({"INFO"}, f"Exported {exported_count} object(s) to: {export_dir}")
        return {"FINISHED"}


class GPLOD_OT_popup(bpy.types.Operator):
    """Open GP LOD Helpers popup"""
    bl_idname = "gplod.popup"
    bl_label = "GP LOD Helpers"
    bl_options = {"REGISTER"}

    def invoke(self, context, event):
        wm = context.window_manager
        return wm.invoke_props_dialog(self, width=300)

    def draw(self, context):
        layout = self.layout
        
        col = layout.column(align=True)
        col.label(text="Build LODs", icon="MESH_DATA")
        box = col.box()
        box.operator("gplod.empty_with_cube", icon="MESH_CUBE")
        box.operator("gplod.delete_mesh_lods", icon="TRASH")
        
        col = layout.column(align=True)
        col.label(text="Export", icon="EXPORT")
        box = col.box()
        box.operator("gplod.export_to_unity", icon="FILE_FOLDER")

    def execute(self, context):
        return {"FINISHED"}


class GPLOD_PT_sidebar(bpy.types.Panel):
    """GP LOD Helpers panel in the sidebar"""
    bl_label = "GP LOD Helpers"
    bl_idname = "GPLOD_PT_sidebar"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "GP LOD Helpers"

    def draw(self, context):
        layout = self.layout
        
        # Popup button
        layout.operator("gplod.popup", icon="WINDOW", text="Open Tools Popup")
        
        layout.separator()
        
        # Direct access to tools
        box = layout.box()
        box.label(text="Build LODs", icon="MESH_DATA")
        box.operator("gplod.empty_with_cube", icon="MESH_CUBE")
        box.operator("gplod.delete_mesh_lods", icon="TRASH")
        
        box = layout.box()
        box.label(text="Export", icon="EXPORT")
        box.operator("gplod.export_to_unity", icon="FILE_FOLDER")
        
        # Info about selected object
        active = context.view_layer.objects.active
        if active:
            box.separator()
            box.label(text=f"Active: {active.name}", icon="OBJECT_DATA")


# Registration
classes = (
    GPLOD_Preferences,
    GPLOD_OT_empty_with_cube,
    GPLOD_OT_delete_mesh_lods,
    GPLOD_OT_export_to_unity,
    GPLOD_OT_popup,
    GPLOD_PT_sidebar,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
