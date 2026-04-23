using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PushableClouds))]
public class PushableCloudsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var cloud = (PushableClouds)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset Clouds"))
        {
            cloud.ResetCloud();
        }
    }
}
#endif


[ExecuteAlways]
public class PushableClouds : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSizeX = 10;
    public int gridSizeY = 5;
    public int gridSizeZ = 10;
    public float gridSpacing = 2f;

    [Header("Particle Settings")]
    public float baseSize = 1f;
    public float sizeVariation = 0.5f;
    public float sizeMultiplier = 2f; // Expand the circles

    [Header("Wren Interaction")]
    public float pushRadius = 5f;
    public float pushForce = 20f;

    [Header("Physics")]
    public float damping = 2f;
    public float returnForce = 5f;

    [Header("Compute Shader")]
    public ComputeShader computeShader;

    [Header("Rendering")]
    public Material cloudDepthMaterial;      // Renders particles to depth
    public Material depthBlurMaterial;       // Blurs the depth
    public Material compositePostMaterial;   // Composites onto main scene

    [Header("Blur Settings")]
    public int blurIterations = 3;
    [Range(0.5f, 5f)]
    public float blurSize = 2f;

    [Header("Render Textures")]
    public Vector2 renderSize = new Vector2(1920, 1080);

    [Header("Debug")]
    public MeshRenderer debugDepthRenderer;
    public MeshRenderer debugBlurredRenderer;
    public bool showDebug = false;

    // Compute buffers
    private ComputeBuffer particleBuffer;

    // Render textures
    private RenderTexture cloudDepthTexture;
    private RenderTexture cloudColorTexture;
    private RenderTexture blurredDepthTexture;
    private RenderTexture blurTempTexture;

    // Camera for rendering cloud layer
    private Camera cloudCamera;
    private GameObject cloudCameraObject;

    // Internal state
    private int particleCount;
    private int numGroups;
    private uint numThreads;
    private MaterialPropertyBlock mpb;

    // Struct size: 3 + 3 + 3 + 1 + 1 + 1 = 12 floats
    private const int STRUCT_SIZE = 12;

    // Kernel indices
    private int initKernel;
    private int simKernel;

    // Track last wren position for velocity calculation
    private Vector3 lastWrenPos;


    private void OnEnable()
    {
        Create();
    }


    private void OnDisable()
    {
        Destroy();
    }


    public void ResetCloud()
    {
        Destroy();
        Create();
    }


    private void Create()
    {
        particleCount = gridSizeX * gridSizeY * gridSizeZ;

        if (particleCount <= 0)
        {
            return;
        }

        // Create compute buffer
        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * STRUCT_SIZE);

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        // Cache kernel indices
        if (computeShader != null)
        {
            initKernel = computeShader.FindKernel("Initialize");
            simKernel = computeShader.FindKernel("Simulate");
        }

        // Create render textures
        CreateRenderTextures();

        // Create the cloud camera
        CreateCloudCamera();

        // Initialize particles on GPU
        InitializeParticles();

        if (God.wren != null)
        {
            lastWrenPos = God.wren.transform.position;
        }
    }


    private void CreateRenderTextures()
    {
        int width = (int)renderSize.x;
        int height = (int)renderSize.y;

        // Depth texture for cloud particles
        cloudDepthTexture = new RenderTexture(width, height, 24, RenderTextureFormat.Depth);
        cloudDepthTexture.Create();

        // Color texture (we'll use this for the particle colors)
        cloudColorTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        cloudColorTexture.Create();

        // Blurred depth texture
        blurredDepthTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        blurredDepthTexture.filterMode = FilterMode.Bilinear;
        blurredDepthTexture.Create();

        // Temp texture for blur ping-pong
        blurTempTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        blurTempTexture.filterMode = FilterMode.Bilinear;
        blurTempTexture.Create();
    }


    private void CreateCloudCamera()
    {
        // Create a child camera for rendering the cloud layer
        cloudCameraObject = new GameObject("CloudCamera");
        cloudCameraObject.transform.SetParent(transform);
        cloudCameraObject.hideFlags = HideFlags.HideAndDontSave;

        cloudCamera = cloudCameraObject.AddComponent<Camera>();
        cloudCamera.enabled = false; // We render manually
        cloudCamera.clearFlags = CameraClearFlags.SolidColor;
        cloudCamera.backgroundColor = Color.black;
        cloudCamera.cullingMask = 0; // We render procedurally, not via culling
        cloudCamera.depth = -100;
    }


    private void Destroy()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Dispose();
            particleBuffer = null;
        }

        if (cloudDepthTexture != null)
        {
            cloudDepthTexture.Release();
            cloudDepthTexture = null;
        }

        if (cloudColorTexture != null)
        {
            cloudColorTexture.Release();
            cloudColorTexture = null;
        }

        if (blurredDepthTexture != null)
        {
            blurredDepthTexture.Release();
            blurredDepthTexture = null;
        }

        if (blurTempTexture != null)
        {
            blurTempTexture.Release();
            blurTempTexture = null;
        }

        if (cloudCameraObject != null)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(cloudCameraObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(cloudCameraObject);
            }

            cloudCameraObject = null;
            cloudCamera = null;
        }
    }


    private void InitializeParticles()
    {
        if (particleBuffer == null || computeShader == null)
        {
            return;
        }

        uint y, z;
        computeShader.GetKernelThreadGroupSizes(initKernel, out numThreads, out y, out z);

        computeShader.SetBuffer(initKernel, "_ParticleBuffer", particleBuffer);
        computeShader.SetInt("_GridSizeX", gridSizeX);
        computeShader.SetInt("_GridSizeY", gridSizeY);
        computeShader.SetInt("_GridSizeZ", gridSizeZ);
        computeShader.SetFloat("_GridSpacing", gridSpacing);
        computeShader.SetVector("_GridCenter", transform.position);
        computeShader.SetFloat("_BaseSize", baseSize);
        computeShader.SetFloat("_SizeVariation", sizeVariation);

        numGroups = (particleCount + ((int)numThreads - 1)) / (int)numThreads;

        if (numGroups <= 0)
        {
            numGroups = 1;
        }

        computeShader.Dispatch(initKernel, numGroups, 1, 1);
    }


    private void Update()
    {
        if (particleBuffer == null)
        {
            return;
        }

        SimulateParticles();
        RenderCloudToTexture();
        BlurDepth();
        // Composite is done via OnRenderImage or post-processing stack
    }


    private void SimulateParticles()
    {
        if (computeShader == null || particleBuffer == null)
        {
            return;
        }

        uint y, z;
        computeShader.GetKernelThreadGroupSizes(simKernel, out numThreads, out y, out z);

        computeShader.SetBuffer(simKernel, "_ParticleBuffer", particleBuffer);
        computeShader.SetInt("_GridSizeX", gridSizeX);
        computeShader.SetInt("_GridSizeY", gridSizeY);
        computeShader.SetInt("_GridSizeZ", gridSizeZ);

        // Set wren position and velocity
        Vector3 wrenPos = transform.position; // Default to cloud center
        Vector3 wrenVel = Vector3.zero;

        if (God.wren != null)
        {
            wrenPos = God.wren.transform.position;
            wrenVel = (wrenPos - lastWrenPos) / Mathf.Max(Time.deltaTime, 0.001f);
            lastWrenPos = wrenPos;
        }

        computeShader.SetVector("_WrenPosition", wrenPos);
        computeShader.SetVector("_WrenVelocity", wrenVel);
        computeShader.SetFloat("_PushRadius", pushRadius);
        computeShader.SetFloat("_PushForce", pushForce);

        // Physics params
        computeShader.SetFloat("_DT", Time.deltaTime);
        computeShader.SetFloat("_Damping", damping);
        computeShader.SetFloat("_ReturnForce", returnForce);
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_BaseSize", baseSize);
        computeShader.SetFloat("_SizeVariation", sizeVariation);

        numGroups = (particleCount + ((int)numThreads - 1)) / (int)numThreads;

        if (numGroups <= 0)
        {
            numGroups = 1;
        }

        computeShader.Dispatch(simKernel, numGroups, 1, 1);
    }


    private void RenderCloudToTexture()
    {
        if (cloudCamera == null || cloudDepthMaterial == null || Camera.main == null)
        {
            return;
        }

        // Sync cloud camera with main camera
        cloudCamera.fieldOfView = Camera.main.fieldOfView;
        cloudCamera.nearClipPlane = Camera.main.nearClipPlane;
        cloudCamera.farClipPlane = Camera.main.farClipPlane;
        cloudCamera.aspect = Camera.main.aspect;
        cloudCamera.transform.position = Camera.main.transform.position;
        cloudCamera.transform.rotation = Camera.main.transform.rotation;

        // Set render target to our depth texture
        cloudCamera.SetTargetBuffers(cloudColorTexture.colorBuffer, cloudDepthTexture.depthBuffer);

        // Clear the render target
        RenderTexture.active = cloudColorTexture;
        GL.Clear(true, true, Color.clear);

        // Render the particles procedurally
        mpb.SetBuffer("_ParticleBuffer", particleBuffer);
        mpb.SetInt("_Count", particleCount);
        mpb.SetFloat("_SizeMultiplier", sizeMultiplier);
        mpb.SetMatrix("_CameraViewMatrix", Camera.main.worldToCameraMatrix);
        mpb.SetMatrix("_CameraProjectionMatrix", Camera.main.projectionMatrix);

        // Calculate bounds
        Vector3 boundsSize = new Vector3(
            gridSizeX * gridSpacing + pushRadius * 2,
            gridSizeY * gridSpacing + pushRadius * 2,
            gridSizeZ * gridSpacing + pushRadius * 2
        );

        // Draw to the cloud render texture
        Graphics.SetRenderTarget(cloudColorTexture.colorBuffer, cloudDepthTexture.depthBuffer);
        Graphics.DrawProcedural(
            cloudDepthMaterial,
            new Bounds(transform.position, boundsSize),
            MeshTopology.Triangles,
            particleCount * 6, // 6 verts per quad
            1,
            null,
            mpb,
            ShadowCastingMode.Off,
            false,
            gameObject.layer
        );

        RenderTexture.active = null;

        // Debug display
        if (showDebug && debugDepthRenderer != null)
        {
            debugDepthRenderer.sharedMaterial.SetTexture("_MainTex", cloudColorTexture);
        }
    }


    private void BlurDepth()
    {
        if (depthBlurMaterial == null)
        {
            return;
        }

        // First pass: convert depth to linear and initialize blur
        depthBlurMaterial.SetTexture("_DepthTexture", cloudDepthTexture);
        depthBlurMaterial.SetTexture("_ColorTexture", cloudColorTexture);
        depthBlurMaterial.SetFloat("_BlurSize", blurSize);
        depthBlurMaterial.SetFloat("_CameraNear", Camera.main.nearClipPlane);
        depthBlurMaterial.SetFloat("_CameraFar", Camera.main.farClipPlane);

        // Blit depth to blurred texture (with initial blur)
        Graphics.Blit(cloudColorTexture, blurredDepthTexture, depthBlurMaterial, 0);

        // Additional blur iterations
        for (int i = 0; i < blurIterations; i++)
        {
            depthBlurMaterial.SetFloat("_BlurSize", blurSize * (i + 1));

            // Horizontal blur
            depthBlurMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
            Graphics.Blit(blurredDepthTexture, blurTempTexture, depthBlurMaterial, 1);

            // Vertical blur
            depthBlurMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
            Graphics.Blit(blurTempTexture, blurredDepthTexture, depthBlurMaterial, 1);
        }

        // Debug display
        if (showDebug && debugBlurredRenderer != null)
        {
            debugBlurredRenderer.sharedMaterial.SetTexture("_MainTex", blurredDepthTexture);
        }

        // Set global textures for composite shader to use
        Shader.SetGlobalTexture("_CloudDepthTexture", blurredDepthTexture);
        Shader.SetGlobalTexture("_CloudColorTexture", cloudColorTexture);

        // Also set composite material if we have one
        if (compositePostMaterial != null)
        {
            compositePostMaterial.SetTexture("_CloudDepthTexture", blurredDepthTexture);
            compositePostMaterial.SetTexture("_CloudColorTexture", cloudColorTexture);
            compositePostMaterial.SetFloat("_CameraNear", Camera.main.nearClipPlane);
            compositePostMaterial.SetFloat("_CameraFar", Camera.main.farClipPlane);
        }
    }


    // Call this from a camera's OnRenderImage or use with post-processing
    public void CompositeOntoScene(RenderTexture src, RenderTexture dest)
    {
        if (compositePostMaterial != null && blurredDepthTexture != null)
        {
            compositePostMaterial.SetTexture("_CloudDepthTexture", blurredDepthTexture);
            compositePostMaterial.SetTexture("_CloudColorTexture", cloudColorTexture);
            Graphics.Blit(src, dest, compositePostMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Draw the cloud bounds
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Vector3 size = new Vector3(
            gridSizeX * gridSpacing,
            gridSizeY * gridSpacing,
            gridSizeZ * gridSpacing
        );
        Gizmos.DrawWireCube(transform.position, size);

        // Draw push radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        if (God.wren != null)
        {
            Gizmos.DrawWireSphere(God.wren.transform.position, pushRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, pushRadius);
        }
    }
}
