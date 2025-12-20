using UnityEngine;


/// <summary>
/// Attach this to the main camera to composite the pushable clouds effect.
/// Make sure the PushableClouds component is in the scene and properly configured.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class PushableCloudsPost : MonoBehaviour
{
    [Header("References")]
    public PushableClouds pushableClouds;
    public Material compositeMaterial;

    [Header("Settings")]
    [Range(0.1f, 10f)]
    public float normalStrength = 2f;

    [Range(0f, 1f)]
    public float cloudOpacity = 0.8f;

    public Color cloudTint = Color.white;
    public Vector3 lightDirection = new Vector3(0.5f, 1f, 0.5f);


    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (compositeMaterial == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        // Update material properties
        compositeMaterial.SetFloat("_NormalStrength", normalStrength);
        compositeMaterial.SetFloat("_CloudOpacity", cloudOpacity);
        compositeMaterial.SetColor("_CloudColor", cloudTint);
        compositeMaterial.SetVector("_LightDir", lightDirection.normalized);

        // The cloud textures are set by PushableClouds via Shader.SetGlobalTexture
        // or directly on the material, so we just blit with the composite shader
        Graphics.Blit(src, dest, compositeMaterial);
    }
}

