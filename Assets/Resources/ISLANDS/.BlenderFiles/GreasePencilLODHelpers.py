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
from bpy.props import (
    StringProperty,
    BoolProperty,
)


class GPLOD_OT_empty_with_cube(bpy.types.Operator):
    """Build mesh LODs from grease pencil geometry"""
    bl_idname = "gplod.empty_with_cube"
    bl_label = "Build Mesh LODs"
    bl_description = "Build mesh LODs from grease pencil geometry"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        active = context.view_layer.objects.active
        
        if not active:
            self.report({"WARNING"}, "No active object selected.")
            return {"CANCELLED"}
        
        # Delete existing children that we previously created
        children_to_delete = []
        for child in active.children:
            # Check if it's an empty with the same name (our created empty)
            if child.type == 'EMPTY' and child.name.startswith(active.name):
                # Also collect all descendants of this empty
                def collect_descendants(obj):
                    descendants = [obj]
                    for c in obj.children:
                        descendants.extend(collect_descendants(c))
                    return descendants
                children_to_delete.extend(collect_descendants(child))
            # Also check for LOD objects directly parented
            elif child.name.startswith(active.name) and "_LOD" in child.name:
                children_to_delete.append(child)
        
        # Delete collected objects
        for obj in children_to_delete:
            bpy.data.objects.remove(obj, do_unlink=True)
        
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
        node_group = bpy.data.node_groups.get("GreasePencilToMesh")
        if node_group:
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
            
            # Rename them as LOD0, LOD1, etc.
            original_name = active.name
            for i, obj in enumerate(separated_objects):
                obj.name = f"{original_name}_LOD{i}"
                # Ensure they're parented to the empty
                if obj.parent != empty_obj:
                    obj.parent = empty_obj
                    obj.matrix_parent_inverse = empty_obj.matrix_world.inverted()
        else:
            self.report({"WARNING"}, "GreasePencilToMesh node group not found")
        
        # Select the empty
        bpy.ops.object.select_all(action='DESELECT')
        empty_obj.select_set(True)
        context.view_layer.objects.active = empty_obj
        
        self.report({"INFO"}, f"Created empty '{empty_obj.name}' with LOD children")
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
        col.label(text="Create Child", icon="EMPTY_DATA")
        box = col.box()
        box.operator("gplod.empty_with_cube", icon="MESH_CUBE")

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
        box.label(text="Create Child", icon="EMPTY_DATA")
        box.operator("gplod.empty_with_cube", icon="MESH_CUBE")
        
        # Info about selected object
        active = context.view_layer.objects.active
        if active:
            box.separator()
            box.label(text=f"Active: {active.name}", icon="OBJECT_DATA")


# Registration
classes = (
    GPLOD_OT_empty_with_cube,
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
