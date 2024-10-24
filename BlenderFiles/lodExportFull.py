bl_info = {
    "name": "Generate LODs",
    "author": "Your Name",
    "version": (1, 0),
    "blender": (2, 80, 0),
    "location": "3D View > Sidebar",
    "description": "Create LODs for the selected object",
    "category": "3D View",
}

import bpy
import os

def create_LODs(obj, decimate_ratios):
    LODs = []
    for idx, ratio in enumerate(decimate_ratios):
        lod = obj.copy()
        lod.data = lod.data.copy()
        lod.name = obj.name + f"_LOD{idx}"
        
        bpy.context.collection.objects.link(lod)
       #bpy.context.view_layer.active_layer_collection.collection.objects.link(lod)

        if ratio < 1.0:
            decimate_mod = lod.modifiers.new("Decimate", "DECIMATE")
            decimate_mod.ratio = ratio
            bpy.context.view_layer.objects.active = lod
            bpy.ops.object.modifier_apply({"object": lod}, modifier=decimate_mod.name)
        
        
        LODs.append(lod)
    return LODs


def export_LODs_to_fbx(file_name, LODs):
    bpy.ops.object.select_all(action='DESELECT')
    for lod in LODs:
        lod.select_set(True)

    export_path = os.path.join(bpy.path.abspath("//"), f"{file_name}.fbx")
    bpy.ops.export_scene.fbx(filepath=export_path, use_selection=True, use_mesh_modifiers=True)

def update_lod_ratios(self, context):
    if context.area:
        context.area.tag_redraw()

class CreateLODsProps(bpy.types.PropertyGroup):
    lod0_ratio: bpy.props.FloatProperty(name="LOD0 Ratio", default=1.0, min=0.0, max=1.0, step=0.01)
    lod1_ratio: bpy.props.FloatProperty(name="LOD1 Ratio", default=0.5, min=0.0, max=1.0, step=0.01)
    lod2_ratio: bpy.props.FloatProperty(name="LOD2 Ratio", default=0.25, min=0.0, max=1.0, step=0.01)
    lod3_ratio: bpy.props.FloatProperty(name="LOD3 Ratio", default=0.1, min=0.0, max=1.0, step=0.01)
    file_name: bpy.props.StringProperty(name="File Name", default="")

def estimated_vertex_count(obj, ratio):
    return int(len(obj.data.vertices) * ratio)





class OBJECT_OT_create_lods(bpy.types.Operator):
    bl_idname = "object.create_lods"
    bl_label = "Create LODs"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        obj = context.active_object

        if obj is None or obj.type != "MESH":
            self.report({"ERROR"}, "Please select a mesh object")
            return {"CANCELLED"}

        file_name = context.scene.create_lods_props.file_name
        if not file_name:
            self.report({"ERROR"}, "Please provide a file name for export")
            return {"CANCELLED"}
        
        print("hi")

        lod_ratios = [
            context.scene.create_lods_props.lod0_ratio,
            context.scene.create_lods_props.lod1_ratio,
            context.scene.create_lods_props.lod2_ratio,
            context.scene.create_lods_props.lod3_ratio,
        ]
        
        
        print(context.scene.create_lods_props.lod0_ratio)
        
        LODs = create_LODs(obj, lod_ratios)

        export_LODs_to_fbx(file_name, LODs)

        return {"FINISHED"}
        
class OBJECT_LODS_PT_panel(bpy.types.Panel):
    bl_label = "Create LODs"
    bl_idname = "OBJECT_PT_lods"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "LODs"

    def draw(self, context):
        layout = self.layout
        obj = context.active_object

        if obj and obj.type == "MESH":
            props = context.scene.create_lods_props
            layout.prop(props, "file_name")

            layout.label(text="LOD Ratios:")
            layout.prop(props, "lod0_ratio")
            layout.prop(props, "lod1_ratio")
            layout.prop(props, "lod2_ratio")
            layout.prop(props, "lod3_ratio")

            layout.label(text="Estimated Vertex Count:")
            layout.label(text=f"LOD0: {estimated_vertex_count(obj, props.lod0_ratio)}")
            layout.label(text=f"LOD1: {estimated_vertex_count(obj, props.lod1_ratio)}")
            layout.label(text=f"LOD2: {estimated_vertex_count(obj, props.lod2_ratio)}")
            layout.label(text=f"LOD3: {estimated_vertex_count(obj, props.lod3_ratio)}")

            layout.operator("object.create_lods")
        else:
            layout.label(text="Please select a mesh object")



def update_active_object(self, context):
    for area in bpy.context.screen.areas:
        if area.type == "VIEW_3D":
            area.tag_redraw()
            break

bpy.types.Object.active_object_update = bpy.props.BoolProperty(update=update_active_object)

def register():
    bpy.utils.register_class(CreateLODsProps)
    bpy.utils.register_class(OBJECT_OT_create_lods)
    bpy.utils.register_class(OBJECT_LODS_PT_panel)
    bpy.types.Scene.create_lods_props = bpy.props.PointerProperty(type=CreateLODsProps)

def unregister():
    bpy.utils.unregister_class(CreateLODsProps)
    bpy.utils.unregister_class(OBJECT_OT_create_lods)
    bpy.utils.unregister_class(OBJECT_LODS_PT_panel)
    del bpy.types.Scene.create_lods_props

if __name__ == "__main__":
    register()
