using UnityEngine;
using UnityEditor;

public class AssignMaterialToAllChildren : EditorWindow
{
    private Material material;

    [MenuItem( "Tools/Assign Material To Children" )]
    private static void Init()
    {
        var window = (AssignMaterialToAllChildren)GetWindow( typeof(AssignMaterialToAllChildren) );
        window.Show();
    }

    private void OnGUI()
    {

        material = (Material)EditorGUILayout.ObjectField( "Material" , material , typeof(Material) , false );

        if ( GUILayout.Button( "Assign To Children" ) && material ) {
            foreach (var go in Selection.gameObjects) {
                var renderers = go.GetComponentsInChildren<Renderer>( true );
                foreach (var r in renderers)
                    r.sharedMaterial = material;
            }
        }
    }
}