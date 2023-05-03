bl_info = {
    "name": "Object Stats Overlay",
    "author": "Your Name",
    "version": (1, 0),
    "blender": (2, 80, 0),
    "location": "3D View > Sidebar",
    "description": "Displays selected object's vertices and triangles count",
    "category": "3D View",
}

import bpy

class OBJECT_STATS_PT_panel(bpy.types.Panel):
    bl_label = "Object Stats"
    bl_idname = "OBJECT_STATS_PT_panel"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "Item"

    def draw(self, context):
        layout = self.layout
        object = context.active_object

        if object and object.type == 'MESH':
            triangles = sum(len(p.vertices) - 2 for p in object.data.polygons)
            vertices = len(object.data.vertices)

            col = layout.column(align=True)
            col.label(text="Vertices: " + str(vertices))
            col.label(text="Triangles: " + str(triangles))
        else:
            layout.label(text="Select a mesh object")

def register():
    bpy.utils.register_class(OBJECT_STATS_PT_panel)

def unregister():
    bpy.utils.unregister_class(OBJECT_STATS_PT_panel)

if __name__ == "__main__":
    register()
