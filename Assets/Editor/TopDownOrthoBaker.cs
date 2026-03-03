// Assets/Editor/TopDownOrthoTextureBaker.cs

using System.IO;
using UnityEditor;
using UnityEngine;

public class TopDownOrthoBaker : EditorWindow
{
    private Terrain   terrain;
    private LayerMask layers     = ~0;
    private int       resolution = 2048;
    private Color     clearColor = new(0 , 0 , 0 , 0);

    private bool  useHDR         = false; // EXR
    private bool  includeSkybox  = false; // usually off for masks/albedo bakes
    private float nearClip       = 0.01f;
    private float farClipPadding = 50f; // extra beyond terrain height to be safe

    [MenuItem( "Tools/Bake/Top-Down Ortho Texture (Terrain Sized)" )]
    private static void Open()
    {
        GetWindow<TopDownOrthoBaker>( "Top-Down Ortho Bake" );
    }

    private void OnGUI()
    {
        terrain = (Terrain)EditorGUILayout.ObjectField( "Terrain" , terrain , typeof(Terrain) , true );
        layers = LayerMaskField( "Layers" , layers );
        resolution = Mathf.Clamp( EditorGUILayout.IntField( "Resolution" , resolution ) , 16 , 16384 );

        EditorGUILayout.Space( 6 );
        includeSkybox = EditorGUILayout.Toggle( "Clear w/ Skybox" , includeSkybox );
        clearColor = EditorGUILayout.ColorField( "Clear Color" , clearColor );
        useHDR = EditorGUILayout.Toggle( "HDR (EXR)" , useHDR );

        EditorGUILayout.Space( 6 );
        nearClip = Mathf.Max( 0.001f , EditorGUILayout.FloatField( "Near Clip" , nearClip ) );
        farClipPadding = Mathf.Max( 0f , EditorGUILayout.FloatField( "Far Clip Padding" , farClipPadding ) );

        EditorGUILayout.Space( 10 );

        using (new EditorGUI.DisabledScope( terrain == null )) {
            if ( GUILayout.Button( "Bake To File..." , GUILayout.Height( 28 ) ) ) {
                Bake();
            }
        }
    }

    private void Bake()
    {
        var td = terrain.terrainData;

        if ( !td ) {
            Debug.LogError( "Terrain has no TerrainData." );
            return;
        }

        // Terrain world bounds
        var tPos = terrain.transform.position;
        var size = td.size;
        var center = tPos + new Vector3( size.x * 0.5f , 0f , size.z * 0.5f );

        // Ortho camera setup
        float orthoSize = size.z * 0.5f; // camera's vertical half-size in world units
        float aspect = size.x <= 0f ? 1f : size.x / size.z;

        float maxY = tPos.y + size.y;
        float camY = maxY + 10f; // place above terrain
        float farClip = camY - tPos.y + size.y + farClipPadding;

        // Pick save path
        string ext = useHDR ? "exr" : "png";
        string defaultName = $"{terrain.name}_TopDown_{resolution}.{ext}";
        string path = EditorUtility.SaveFilePanel( "Save Top-Down Bake" , Application.dataPath , defaultName , ext );

        if ( string.IsNullOrEmpty( path ) ) {
            return;
        }

        var go = new GameObject( "__TopDownOrthoBakeCam__" );
        go.hideFlags = HideFlags.HideAndDontSave;

        var cam = go.AddComponent<Camera>();
        cam.enabled = false;
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        cam.aspect = aspect;
        cam.transform.position = new Vector3( center.x , camY , center.z );
        cam.transform.rotation = Quaternion.Euler( 90f , 0f , 0f );
        cam.cullingMask = layers;
        cam.nearClipPlane = nearClip;
        cam.farClipPlane = farClip;

        cam.clearFlags = includeSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        cam.backgroundColor = clearColor;

        // Render target
        var rtFormat = useHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
        var rt = new RenderTexture( resolution , resolution , 24 , rtFormat )
        {
            antiAliasing = 1 ,
            wrapMode = TextureWrapMode.Clamp ,
            filterMode = FilterMode.Bilinear ,
            name = "__TopDownOrthoBakeRT__"
        };

        var prevRT = RenderTexture.active;
        var prevCamRT = cam.targetTexture;

        try {
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;

            if ( useHDR ) {
                // EXR (float-ish). Needs linear readback.
                var tex = new Texture2D( resolution , resolution , TextureFormat.RGBAHalf , false , true );
                tex.ReadPixels( new Rect( 0 , 0 , resolution , resolution ) , 0 , 0 , false );
                tex.Apply( false , false );

                byte[] bytes = tex.EncodeToEXR( Texture2D.EXRFlags.OutputAsFloat );
                File.WriteAllBytes( path , bytes );
                DestroyImmediate( tex );
            } else {
                var tex = new Texture2D( resolution , resolution , TextureFormat.RGBA32 , false , false );
                tex.ReadPixels( new Rect( 0 , 0 , resolution , resolution ) , 0 , 0 , false );
                tex.Apply( false , false );

                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes( path , bytes );
                DestroyImmediate( tex );
            }

            Debug.Log( $"Top-down bake saved: {path}" );
            AssetDatabase.Refresh();
        }
        finally {
            cam.targetTexture = prevCamRT;
            RenderTexture.active = prevRT;

            if ( rt ) {
                rt.Release();
            }

            DestroyImmediate( rt );
            DestroyImmediate( go );
        }
    }

    // Unity doesn't have a built-in LayerMask field in EditorGUILayout that shows names like the inspector,
    // so we implement it.
    private static LayerMask LayerMaskField( string label , LayerMask selected )
    {
        string[] layers = GetAllLayerNames( out int[] layerNumbers );
        int maskWithoutEmpty = 0;

        for ( int i = 0; i < layerNumbers.Length; i++ ) {
            int layer = layerNumbers[i];

            if ( ((1 << layer) & selected.value) != 0 ) {
                maskWithoutEmpty |= 1 << i;
            }
        }

        maskWithoutEmpty = EditorGUILayout.MaskField( label , maskWithoutEmpty , layers );

        int mask = 0;

        for ( int i = 0; i < layerNumbers.Length; i++ ) {
            if ( (maskWithoutEmpty & (1 << i)) != 0 ) {
                mask |= 1 << layerNumbers[i];
            }
        }

        selected.value = mask;
        return selected;
    }

    private static string[] GetAllLayerNames( out int[] layerNumbers )
    {
        var names = new System.Collections.Generic.List<string>();
        var nums = new System.Collections.Generic.List<int>();

        for ( int i = 0; i < 32; i++ ) {
            string n = LayerMask.LayerToName( i );

            if ( !string.IsNullOrEmpty( n ) ) {
                names.Add( n );
                nums.Add( i );
            }
        }

        layerNumbers = nums.ToArray();
        return names.ToArray();
    }
}