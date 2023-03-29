using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class SkyboxUpdater : MonoBehaviour
{
    private CommandBuffer commandBuffer;
    private Cubemap cubemap;
    private RenderTexture renderTexture;

    public Material material;

    public MaterialPropertyBlock mpb;


    public float fade;

    private void OnEnable()
    {
        // Initialize cubemap/render texture
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        var resolution = RenderSettings.defaultReflectionResolution;
        renderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf) { autoGenerateMips = false, useMipMap = true };
        renderTexture.Create();

        cubemap = new Cubemap(resolution, TextureFormat.RGBAHalf, true) { filterMode = FilterMode.Trilinear };
        cubemap.Apply(false, true);

        RenderSettings.customReflection = cubemap;

        // Initialize command buffer
        var mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        commandBuffer = new CommandBuffer();

        var projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(90, 1, 0.1f, 1), true);
        commandBuffer.SetProjectionMatrix(projectionMatrix);

        // Matrices for rendering the six cubemap faces
        var matrices = new[] { Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.right, Vector3.down), -Vector3.one).inverse,
        Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.left, Vector3.down), -Vector3.one).inverse,
        Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.up, Vector3.forward), -Vector3.one).inverse,
        Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.down, Vector3.back), -Vector3.one).inverse,
        Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.down), -Vector3.one).inverse,
        Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.back, Vector3.down), -Vector3.one).inverse };

        // Set the camera to render each face into a temporary texture, and then copy that texture into the final cubemap
        for (var face = CubemapFace.PositiveX; (int)face < 6; face++)
        {
            commandBuffer.SetViewMatrix(matrices[(int)face]);
            commandBuffer.SetRenderTarget(renderTexture, 0);
            commandBuffer.ClearRenderTarget(true, true, Color.clear);
            commandBuffer.DrawMesh(mesh, Matrix4x4.identity, material);
            commandBuffer.GenerateMips(renderTexture);
            commandBuffer.CopyTexture(renderTexture, 0, cubemap, (int)face);
        }
    }


    Material mat;

    void Update()
    {
        // mat = RenderSettings.skybox;

        // mat.SetFloat("_Fade", fade);


    }

    public void UpdateSkybox()
    {
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
}
