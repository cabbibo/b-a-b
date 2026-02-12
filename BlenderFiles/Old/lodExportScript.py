import bpy
import os

# Get the current user's home directory
home_directory = os.path.expanduser("~")

# Set the output folder inside the user's home directory
output_folder = os.path.join(home_directory, "Desktop", "LOD_Export")

# Replace with the desired output folder path
LOD_suffixes = ["_LOD0", "_LOD1", "_LOD2"]
LOD_decimate_ratios = [1.0, 0.5, 0.25]

print("TEST1")

# Ensure the output folder exists
if not os.path.exists(output_folder):
    os.makedirs(output_folder)

def create_LODs(obj, suffixes, decimate_ratios):
    LODs = []
    for suffix, ratio in zip(suffixes, decimate_ratios):
        lod = obj.copy()
        lod.data = lod.data.copy()
        lod.name = obj.name + suffix
        bpy.context.collection.objects.link(lod)
        print("TEST")

        if ratio < 1.0:
            decimate_mod = lod.modifiers.new("Decimate", "DECIMATE")
            decimate_mod.ratio = ratio
            bpy.context.view_layer.objects.active = lod
            bpy.ops.object.modifier_apply({"object": lod}, modifier=decimate_mod.name)

        LODs.append(lod)
    return LODs

def export_LODs(output_folder, obj, LODs):
    for lod in LODs:
        lod.select_set(True)
        
        
        
        
        
        # Example object, suffixes, and decimate_ratios
obj = bpy.context.active_object
suffixes = ["_LOD0", "_LOD1", "_LOD2", "_LOD3"]
decimate_ratios = [.8, 0.2, 0.1, 0.03]




# Create a new collection
new_collection = bpy.data.collections.new(obj.name)
bpy.context.scene.collection.children.link(new_collection)

# Call the create_LODs function
LODs = create_LODs(obj, suffixes, decimate_ratios)

# Move the LODs to the new collection and deselect the original object
for lod in LODs:
    new_collection.objects.link(lod)
    bpy.context.collection.objects.unlink(lod)
    lod.select_set(True)
obj.select_set(False)

print("Exporting to:", os.path.join(output_folder, "LOD_Collection.fbx"))


# Export the new collection as an FBX file
bpy.ops.export_scene.fbx(
    filepath=os.path.join(output_folder, obj.name + ".fbx"),
    use_selection=True,
    global_scale=1.0,
)