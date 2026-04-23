using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class SkyboxUpdater : MonoBehaviour
{
    private CommandBuffer commandBuffer;
    private Cubemap       cubemap;
    private RenderTexture renderTexture;

    public Material material;

    public MaterialPropertyBlock mpb;


    public float fade;

    public int framesPerUpdate = 10;

    public int resolution = 1024;


    private void OnEnable()
    {
        // Initialize cubemap/render texture
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        //  resolution = RenderSettings.defaultReflectionResolution;
        renderTexture = new RenderTexture( resolution , resolution , 0 , RenderTextureFormat.ARGBHalf )
            { autoGenerateMips = false , useMipMap = true };

        renderTexture.enableRandomWrite = true;
        renderTexture.Create();


        cubemap = new Cubemap( resolution , TextureFormat.RGBAHalf , true ) { filterMode = FilterMode.Trilinear };
        cubemap.Apply( false , true );

        RenderSkybox();


    }

    private void OnDisable()
    {
        if ( commandBuffer != null ) {
            commandBuffer.Release();
            commandBuffer = null;
        }

        if ( renderTexture != null ) {
            renderTexture.Release();
            renderTexture = null;
        }

        if ( cubemap != null ) {
            //   cubemap.Release();
            cubemap = null;
        }
    }

    public void RenderSkybox()
    {


        // Initialize command buffer
        var mesh = Resources.GetBuiltinResource<Mesh>( "Sphere.fbx" );

        if ( commandBuffer != null ) {
            commandBuffer.Release();
        }

        commandBuffer = new CommandBuffer();

        var projectionMatrix = GL.GetGPUProjectionMatrix( Matrix4x4.Perspective( 90 , 1 , 0.1f , 1 ) , true );
        commandBuffer.SetProjectionMatrix( projectionMatrix );


        var position = Vector3.zero; // Camera.main.transform.position;
        // Matrices for rendering the six cubemap faces
        var matrices = new[]
        {
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.right , Vector3.down ) , -Vector3.one ).inverse ,
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.left , Vector3.down ) , -Vector3.one ).inverse ,
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.up , Vector3.forward ) , -Vector3.one ).inverse ,
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.down , Vector3.back ) , -Vector3.one ).inverse ,
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.forward , Vector3.down ) , -Vector3.one ).inverse ,
            Matrix4x4.TRS( position , Quaternion.LookRotation( Vector3.back , Vector3.down ) , -Vector3.one ).inverse
        };

        // Set the camera to render each face into a temporary texture, and then copy that texture into the final cubemap
        for ( var face = CubemapFace.PositiveX; (int)face < 6; face++ ) {
            commandBuffer.SetViewMatrix( matrices[(int)face] );
            commandBuffer.SetRenderTarget( renderTexture , 0 );
            commandBuffer.ClearRenderTarget( true , true , Color.clear );
            commandBuffer.DrawMesh( mesh , Matrix4x4.identity , material );
            commandBuffer.GenerateMips( renderTexture );
            commandBuffer.CopyTexture( renderTexture , 0 , cubemap , (int)face );
        }


        Graphics.ExecuteCommandBuffer( commandBuffer );

        Shader.SetGlobalTexture( "_Skybox" , cubemap );


        RenderSettings.customReflection = cubemap;


        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe( Vector3.zero , null , out sh ); // OR
        sh = RenderSettings.ambientProbe;

        // Pack into array of Vector4s
        var shData = new Vector4[9];

        for ( int i = 0; i < 3; i++ ) // R, G, B channels
        {
            shData[0][i] = sh[i , 0]; // L0
            shData[1][i] = sh[i , 1]; // L1
            shData[2][i] = sh[i , 2];
            shData[3][i] = sh[i , 3];
            shData[4][i] = sh[i , 4]; // L2
            shData[5][i] = sh[i , 5];
            shData[6][i] = sh[i , 6];
            shData[7][i] = sh[i , 7];
            shData[8][i] = sh[i , 8];
        }


        Shader.SetGlobalVectorArray( "_CustomSH9" , shData );


        RenderSettings.skybox = material;

    }


    private Material mat;

    private int frame = 0;

    private void Update()
    {

        frame++;

        if ( frame >= framesPerUpdate ) {
            frame = 0;
            // print( "REndER" );
            RenderSkybox();
        }

    }

    public void UpdateSkybox( Material m )
    {
        material = m;

        // if (commandBuffer == null) { OnEnable(); }
        RenderSkybox();

    }
}