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
import mathutils
from mathutils import Vector, Matrix
from bpy.props import (
    StringProperty,
    BoolProperty,
)


class GPLOD_OT_empty_with_cube(bpy.types.Operator):
    """Build mesh LODs from grease pencil geometry"""
    bl_idname = "gplod.empty_with_cube"
    bl_label = "Build Mesh LODs"
    bl_description = "Build mesh LODs from grease pencil geometry for all selected objects"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        # Get all selected objects at the start - ONLY process grease pencil objects
        selected_objects = [obj for obj in context.selected_objects if obj.type == 'GPENCIL']
        
        if not selected_objects:
            self.report({"WARNING"}, "No grease pencil objects selected.")
            return {"CANCELLED"}
        
        node_group = bpy.data.node_groups.get("GreasePencilToMesh")
        if not node_group:
            self.report({"WARNING"}, "GreasePencilToMesh node group not found")
            return {"CANCELLED"}
        
        created_empties = []
        
        for active in selected_objects:
            # Delete existing children that we previously created
            children_to_delete = []
            for child in active.children:
                # SAFETY: Never delete grease pencil objects
                if child.type == 'GPENCIL':
                    continue
                    
                # Check if it's an empty with the same name (our created empty)
                if child.type == 'EMPTY' and child.name.startswith(active.name):
                    # Also collect all descendants of this empty
                    def collect_descendants(obj):
                        descendants = []
                        # SAFETY: Never include grease pencil objects
                        if obj.type == 'GPENCIL':
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
            children_to_delete = [obj for obj in children_to_delete if obj.type != 'GPENCIL']
            
            # Delete collected objects and their mesh data
            for obj in children_to_delete:
                # SAFETY: Double-check we're not deleting a grease pencil
                if obj.type == 'GPENCIL':
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
        selected_objects = [obj for obj in context.selected_objects if obj.type == 'GPENCIL']
        
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
                if child.type == 'GPENCIL':
                    continue
                    
                # Check if it's an empty with the same name (our created empty)
                if child.type == 'EMPTY' and child.name.startswith(active.name):
                    # Also collect all descendants of this empty
                    def collect_descendants(obj):
                        descendants = []
                        # SAFETY: Never include grease pencil objects
                        if obj.type == 'GPENCIL':
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
            children_to_delete = [obj for obj in children_to_delete if obj.type != 'GPENCIL']
            
            if children_to_delete:
                objects_processed += 1
                total_deleted += len(children_to_delete)
                
                # Delete collected objects and their mesh data
                for obj in children_to_delete:
                    # SAFETY: Double-check we're not deleting a grease pencil
                    if obj.type == 'GPENCIL':
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
            if active.type == 'GPENCIL':
                for child in active.children:
                    if child.type == 'EMPTY' and child.name.startswith(active.name):
                        export_obj = child
                        break
            # If active is an empty parented to a grease pencil, use the grease pencil's name
            elif active.type == 'EMPTY' and active.parent and active.parent.type == 'GPENCIL':
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


class GPLOD_OT_align_grease_pencils(bpy.types.Operator):
    """Align two grease pencil objects based on matching strokes"""
    bl_idname = "gplod.align_grease_pencils"
    bl_label = "Align Grease Pencils"
    bl_description = "Align the second selected grease pencil to the first based on matching strokes"
    bl_options = {"REGISTER", "UNDO"}

    def get_stroke_data(self, gp_obj):
        """Extract stroke data from a grease pencil object in world space"""
        strokes_data = []
        gp_data = gp_obj.data
        world_matrix = gp_obj.matrix_world
        
        for layer in gp_data.layers:
            if layer.hide:
                continue
            for frame in layer.frames:
                for stroke in frame.strokes:
                    if len(stroke.points) < 2:
                        continue
                    
                    points_world = [world_matrix @ Vector((p.co.x, p.co.y, p.co.z)) for p in stroke.points]
                    
                    total_length = 0
                    for i in range(len(points_world) - 1):
                        total_length += (points_world[i+1] - points_world[i]).length
                    
                    centroid = sum(points_world, Vector()) / len(points_world)
                    
                    strokes_data.append({
                        'points': points_world,
                        'point_count': len(points_world),
                        'length': total_length,
                        'centroid': centroid,
                        'start': points_world[0],
                        'end': points_world[-1],
                        'layer': layer.info,
                        'stroke': stroke,
                    })
        
        return strokes_data

    def calculate_stroke_similarity(self, stroke1, stroke2):
        """Calculate similarity score between two strokes (lower = more similar)"""
        point_count_diff = abs(stroke1['point_count'] - stroke2['point_count'])
        length_diff = abs(stroke1['length'] - stroke2['length'])
        
        if stroke1['length'] > 0 and stroke2['length'] > 0:
            length_ratio = min(stroke1['length'], stroke2['length']) / max(stroke1['length'], stroke2['length'])
        else:
            length_ratio = 0
        
        point_ratio = min(stroke1['point_count'], stroke2['point_count']) / max(stroke1['point_count'], stroke2['point_count'])
        
        similarity = (1 - length_ratio) * 100 + (1 - point_ratio) * 50 + point_count_diff * 2
        
        return similarity

    def find_best_matching_strokes(self, strokes1, strokes2):
        """Find the best matching stroke pair between two sets of strokes"""
        best_match = None
        best_score = float('inf')
        
        for s1 in strokes1:
            for s2 in strokes2:
                if abs(s1['point_count'] - s2['point_count']) > max(3, s1['point_count'] * 0.3):
                    continue
                
                score = self.calculate_stroke_similarity(s1, s2)
                if score < best_score:
                    best_score = score
                    best_match = (s1, s2)
        
        return best_match, best_score

    def calculate_alignment_transform(self, source_stroke, target_stroke):
        """Calculate the transformation to align source stroke to target stroke"""
        source_start = source_stroke['start']
        source_end = source_stroke['end']
        target_start = target_stroke['start']
        target_end = target_stroke['end']
        
        source_vec = source_end - source_start
        target_vec = target_end - target_start
        
        source_len = source_vec.length
        target_len = target_vec.length
        
        if source_len < 0.0001 or target_len < 0.0001:
            return Matrix.Identity(4), Vector()
        
        scale_factor = target_len / source_len
        
        source_vec_norm = source_vec.normalized()
        target_vec_norm = target_vec.normalized()
        
        rotation_quat = source_vec_norm.rotation_difference(target_vec_norm)
        rotation_matrix = rotation_quat.to_matrix().to_4x4()
        
        scale_matrix = Matrix.Scale(scale_factor, 4)
        
        transform = rotation_matrix @ scale_matrix
        
        rotated_scaled_start = transform @ source_start
        translation = target_start - rotated_scaled_start
        
        return transform, translation

    def execute(self, context):
        selected_gps = [obj for obj in context.selected_objects if obj.type == 'GPENCIL']
        
        if len(selected_gps) != 2:
            self.report({"WARNING"}, "Please select exactly 2 grease pencil objects.")
            return {"CANCELLED"}
        
        active = context.view_layer.objects.active
        if active not in selected_gps:
            self.report({"WARNING"}, "Active object must be one of the selected grease pencils.")
            return {"CANCELLED"}
        
        target_gp = active
        source_gp = [gp for gp in selected_gps if gp != active][0]
        
        target_strokes = self.get_stroke_data(target_gp)
        source_strokes = self.get_stroke_data(source_gp)
        
        if not target_strokes:
            self.report({"WARNING"}, f"No strokes found in target '{target_gp.name}'")
            return {"CANCELLED"}
        
        if not source_strokes:
            self.report({"WARNING"}, f"No strokes found in source '{source_gp.name}'")
            return {"CANCELLED"}
        
        match, score = self.find_best_matching_strokes(target_strokes, source_strokes)
        
        if match is None:
            self.report({"WARNING"}, "Could not find matching strokes between the two grease pencils.")
            return {"CANCELLED"}
        
        target_stroke, source_stroke = match
        
        transform, translation = self.calculate_alignment_transform(source_stroke, target_stroke)
        
        translation_matrix = Matrix.Translation(translation)
        
        source_gp.matrix_world = translation_matrix @ transform @ source_gp.matrix_world
        
        self.report({"INFO"}, f"Aligned '{source_gp.name}' to '{target_gp.name}' (match score: {score:.2f})")
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
        col.label(text="Grease Pencil Tools", icon="GREASEPENCIL")
        box = col.box()
        box.operator("gplod.align_grease_pencils", icon="CON_LOCLIKE")
        
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
        box.label(text="Grease Pencil Tools", icon="GREASEPENCIL")
        box.operator("gplod.align_grease_pencils", icon="CON_LOCLIKE")
        
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
    GPLOD_OT_empty_with_cube,
    GPLOD_OT_delete_mesh_lods,
    GPLOD_OT_export_to_unity,
    GPLOD_OT_align_grease_pencils,
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
