using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
//using WrenUtils;
using WrenUtils;


[ExecuteAlways]
public class PlaceParticlesOnDepthMap : MonoBehaviour
{
    public bool isActive;

    public int           splatAmount;
    public ComputeBuffer _VertBuffer;
    public ComputeShader shader;
    public int           kernel;

    public Camera camera;

    public Material              material;
    public MaterialPropertyBlock mpb;

    public int structSize = 16;

    public string kernelName;

    public Texture2D splatTexture;
    public int       splatTextureSize;

    // Start is called before the first frame update

    private int  numGroups;
    private uint numThreads;

    private RenderTextureDescriptor textureDescriptor;

    private RenderTexture depthTexture;
    private RenderTexture colorTexture;
    public  Vector2       renderSize = new(1920 , 1080);

    public MeshRenderer debugDepthRenderer;

    public float splatSize;
    public float splatSpeed;
    public float splatMatchAmount;

    public float normalForce;
    public float curlForce;
    public float curlSize;

    public float normalOffset;
    public bool  renderBackground;

    public float hueRandomness;
    public float saturationRandomness;
    public float lightnessRandomness;
    public float colorMultiplier;

    public Color backgroundColor = Color.black;


    public Color splatDiscardColor  = Color.black;
    public float splatDiscardCutoff = .1f;

    private void OnEnable()
    {
        if ( splatAmount < 1 ) {
            splatAmount = 1;
        }

        /*textureDescriptor = new RenderTextureDescriptor((int)renderSize.x, (int)renderSize.y, RenderTextureFormat.Depth, 24);
        texture = RenderTexture.GetTemporary(textureDescriptor);
        texture.filterMode = FilterMode.Trilinear;

        textureDescriptor = new RenderTextureDescriptor((int)renderSize.x, (int)renderSize.y, RenderTextureFormat.ARGBFloat, 0);
        colorTexture = RenderTexture.GetTemporary(textureDescriptor);*/


        // Create a color texture
        colorTexture = new RenderTexture( (int)renderSize.x , (int)renderSize.y , 0 , RenderTextureFormat.ARGB32 );
        colorTexture.Create();

        // Create a depth texture
        depthTexture = new RenderTexture( (int)renderSize.x , (int)renderSize.y , 24 , RenderTextureFormat.Depth );
        depthTexture.Create();


        _VertBuffer = new ComputeBuffer( splatAmount , sizeof(float) * structSize );
        kernel = shader.FindKernel( kernelName );
        GetNumThreads();
        GetNumGroups();


    }

    private void OnDisable()
    {

        depthTexture.Release();
        colorTexture.Release();

        if ( _VertBuffer != null ) {
            _VertBuffer.Release();
        }
    }


    public void GetNumThreads()
    {
        uint y;
        uint z;
        shader.GetKernelThreadGroupSizes( kernel , out numThreads , out y , out z );
    }

    public void GetNumGroups()
    {
        //numGroups = ((int)splatAmount + ((int)numThreads - 1)) / (int)numThreads;
        numGroups = (int)splatAmount / (int)numThreads;
    }


    // Update is called once per frame
    private void LateUpdate()
    {

        if ( isActive ) {

            // Set out depth camera properties to be the same as the main camera
            camera.fieldOfView = Camera.main.fieldOfView;
            camera.nearClipPlane = Camera.main.nearClipPlane;
            camera.farClipPlane = Camera.main.farClipPlane;
            camera.aspect = Camera.main.aspect;

            // set our camera to have the same transform as the main camera
            camera.transform.position = Camera.main.transform.position;
            camera.transform.rotation = Camera.main.transform.rotation;

            camera.SetTargetBuffers( depthTexture.colorBuffer , depthTexture.depthBuffer );
            //camera.depthTextureMode = DepthTextureMode.DepthNormals;
            camera.Render();

            camera.targetTexture = colorTexture;
            camera.Render();

            debugDepthRenderer.sharedMaterial.SetTexture( "_MainTex" , depthTexture );

            shader.SetMatrix( "_CameraViewMatrix" , Camera.main.worldToCameraMatrix );
            shader.SetMatrix( "_CameraViewMatrixInverse" , Camera.main.worldToCameraMatrix.inverse );
            shader.SetMatrix( "_CameraProjectionMatrix" , Camera.main.projectionMatrix );
            shader.SetMatrix( "_CameraProjectionMatrixInverse" , Camera.main.projectionMatrix.inverse );
            shader.SetMatrix( "_CameraViewProjectionMatrixInverse" ,
                Camera.main.worldToCameraMatrix.inverse * Camera.main.projectionMatrix.inverse );
            shader.SetVector( "_CameraPosition" , Camera.main.transform.position );
            shader.SetFloat( "_CameraNear" , Camera.main.nearClipPlane );
            shader.SetFloat( "_CameraFar" , Camera.main.farClipPlane );

            God.instance.SetWrenCompute( kernel , shader );

            float x = 1 - Camera.main.farClipPlane / Camera.main.nearClipPlane;
            float y = Camera.main.farClipPlane / Camera.main.nearClipPlane;
            float z = x / Camera.main.farClipPlane;
            float w = y / Camera.main.farClipPlane;

            shader.SetVector( "_ZBufferParams" , new Vector4( x , y , z , w ) );
            shader.SetTexture( kernel , "_DepthTexture" , depthTexture );
            shader.SetTexture( kernel , "_ColorTexture" , colorTexture );

            shader.SetFloat( "_Time" , Time.time );

            shader.SetBuffer( kernel , "_VertBuffer" , _VertBuffer );
            shader.SetInt( "_VertBuffer_COUNT" , splatAmount );
            shader.SetFloat( "_SplatSpeed" , splatSpeed );
            shader.SetFloat( "_NormalForce" , normalForce );
            shader.SetFloat( "_CurlForce" , curlForce );
            shader.SetFloat( "_CurlSize" , curlSize );
            shader.SetFloat( "_NormalOffset" , normalOffset );

            shader.Dispatch( kernel , numGroups , 1 , 1 );

            if ( mpb == null ) {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetBuffer( "_VertBuffer" , _VertBuffer );
            mpb.SetInt( "_Count" , splatAmount );
            mpb.SetFloat( "_Size" , splatSize );
            mpb.SetFloat( "_NormalMatch" , splatMatchAmount );
            mpb.SetFloat( "_HueRandomness" , hueRandomness );
            mpb.SetFloat( "_SaturationRandomness" , saturationRandomness );
            mpb.SetFloat( "_LightnessRandomness" , lightnessRandomness );
            mpb.SetFloat( "_ColorMultiplier" , colorMultiplier );
            mpb.SetTexture( "_SplatTexture" , splatTexture );
            mpb.SetInt( "_SplatTextureSize" , splatTextureSize );
            mpb.SetColor( "_DiscardColor" , splatDiscardColor );
            mpb.SetFloat( "_DiscardCutoff" , splatDiscardCutoff );

            Graphics.DrawProcedural( material , new Bounds( transform.position , Vector3.one * 500000 ) , MeshTopology.Triangles ,
                splatAmount * 3 * 2 , 1 , null , mpb , ShadowCastingMode.Off , true , LayerMask.NameToLayer( "Splats" ) );


            // turn off main camera if renderBackground is false
            if ( renderBackground == false ) {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = backgroundColor;

                // turn off all the rendering of all the layers but "Splats" and "Wren"
                Camera.main.cullingMask = LayerMask.GetMask( "Splats" , "Wren" );

            } else {
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                Camera.main.cullingMask = ~0;

            }


        } else {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            Camera.main.cullingMask = ~0;
        }

    }

    public void Reset()
    {

        if ( _VertBuffer != null ) {
            _VertBuffer.Release();
        }


        if ( splatAmount < 1 ) {
            splatAmount = 1;
        }

        _VertBuffer = new ComputeBuffer( splatAmount , sizeof(float) * structSize );
    }
}