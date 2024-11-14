using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using WrenUtils;

[ExecuteAlways]
public class SetShadowMapAsGlobalTexture : MonoBehaviour
{
    public string textureSemanticName = "_SunCascadedShadowMap";


#if UNITY_EDITOR
    public bool reset;
#endif

    private RenderTexture shadowMapRenderTexture;
    private CommandBuffer commandBuffer;
    public Light lightComponent;
    
    void OnEnable()
    {
        lightComponent = GetComponent<Light>();
        SetupCommandBuffer();
    }

    void OnDisable()
    {
        lightComponent.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBuffer);
        ReleaseCommandBuffer();
    }

#if UNITY_EDITOR
    void Update()
    {
        if(reset)
        {
            OnDisable();
            OnEnable();
            reset = false;
        }
    }
#endif

    void SetupCommandBuffer()
    {
        commandBuffer = new CommandBuffer();
        
        RenderTargetIdentifier shadowMapRenderTextureIdentifier = BuiltinRenderTextureType.CurrentActive;
        commandBuffer.SetGlobalTexture(textureSemanticName, shadowMapRenderTextureIdentifier);

        lightComponent.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer);
    }

    void ReleaseCommandBuffer()
    {
        commandBuffer.Clear();
    }
}