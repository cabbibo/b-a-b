using UnityEngine;
using UnityEditor;

public class AddColliderFromLOD : EditorWindow
{
    private int lodIndex = 1;

    [MenuItem( "Tools/Add MeshCollider From LOD" )]
    private static void Open()
    {
        GetWindow<AddColliderFromLOD>( "LOD Collider Tool" );
    }

    private void OnGUI()
    {
        lodIndex = EditorGUILayout.IntField( "LOD Index" , lodIndex );

        if ( GUILayout.Button( "Rebuild Colliders From LOD (Search Children)" ) ) {
            AddColliders();
        }
    }

    private void AddColliders()
    {
        foreach (var root in Selection.gameObjects) {
            // remove all existing colliders in hierarchy
            var colliders = root.GetComponentsInChildren<Collider>( true );
            foreach (var c in colliders) DestroyImmediate( c );

            // find all LODGroups
            var lodGroups = root.GetComponentsInChildren<LODGroup>( true );

            foreach (var lodGroup in lodGroups) {
                var lods = lodGroup.GetLODs();

                if ( lodIndex >= lods.Length ) {
                    continue;
                }

                foreach (var r in lods[lodIndex].renderers) {
                    var mf = r.GetComponent<MeshFilter>();

                    if ( !mf ) {
                        continue;
                    }

                    var mc = r.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }
        }
    }
}