using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
[CustomEditor( typeof(CubemapSaver) )]
public class CubemapSaverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (CubemapSaver)target;

        if ( GUILayout.Button( "Bake" ) ) {
            script.Bake();
        }

        if ( GUILayout.Button( "Assign Faces" ) ) {
            script.AssignFaces();
        }


    }
}


#endif
public class CubemapSaver : MonoBehaviour
{
    public Cubemap         cubemap;
    public ReflectionProbe probe;
    public string          savePath = "Assets/Cubemaps/ReflectionCubemap.cubemap";

    [ContextMenu( "Save Reflection Probe To Cubemap" )]
    private void SaveCubemap()
    {

    }

    public void Bake()
    {

        if ( !probe ) {
            return;
        }

        var rt = new RenderTexture( probe.resolution , probe.resolution , 16 );
        rt.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        rt.hideFlags = HideFlags.HideAndDontSave;
        rt.Create();

        probe.RenderProbe( rt );

        //  var cubemap = new Cubemap( rt.width , TextureFormat.RGBAHalf , true );
        var tempTex = new Texture2D( rt.width , rt.height , TextureFormat.RGBAHalf , false );

        for ( int face = 0; face < 6; face++ ) {
            Graphics.SetRenderTarget( rt , 0 , (CubemapFace)face );
            tempTex.ReadPixels( new Rect( 0 , 0 , rt.width , rt.height ) , 0 , 0 );
            tempTex.Apply();
            cubemap.SetPixels( tempTex.GetPixels() , (CubemapFace)face );
        }

        cubemap.Apply( true );

        //    AssetDatabase.CreateAsset( cubemap , savePath );
        //   AssetDatabase.SaveAssets();

        DestroyImmediate( tempTex );
        rt.Release();
        DestroyImmediate( rt );


    }

    public Cubemap targetCubemap;
    public string  folderPath = "Assets/Cubemaps/ProbeOutput";

    [ContextMenu( "Assign PNG Faces To Cubemap" )]
    public void AssignFaces()
    {
#if UNITY_EDITOR
        if ( !targetCubemap ) {
            return;
        }

        for ( int face = 0; face < 6; face++ ) {
            string path = Path.Combine( folderPath , $"face{face}.png" );
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>( path );

            if ( !tex ) {
                continue;
            }

            var pixels = tex.GetPixels();
            targetCubemap.SetPixels( pixels , (CubemapFace)face );
        }

        targetCubemap.Apply( true );
        EditorUtility.SetDirty( targetCubemap );
        AssetDatabase.SaveAssets();
#endif
    }
}