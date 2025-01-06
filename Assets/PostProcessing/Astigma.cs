using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{

    [PostProcess(typeof(AstigmaRenderer), PostProcessEvent.BeforeStack, "Astigma")]
    public class Astigma : PostProcessEffectSettings
    {
        public FloatParameter amount = new FloatParameter { value = 1.0f };
        public TextureParameter gritTexture = new TextureParameter { value = null };
        public TextureParameter gritTexture2 = new TextureParameter { value = null };
        public TextureParameter gritTexture3 = new TextureParameter { value = null };
        public TextureParameter gritTexture4 = new TextureParameter { value = null };

    }

    public sealed class AstigmaRenderer : PostProcessEffectRenderer<Astigma>
    {
        public const string AstigmaShader = "VertexFragment/Astigma";

        public override void Render(PostProcessRenderContext context)
        {
            var shader = Shader.Find(AstigmaShader);

            if (shader == null)
            {
                return;
            }

            var sheet = context.propertySheets.Get(shader);

            if (sheet == null)
            {
                return;
            }

            sheet.properties.SetFloat("_Amount", settings.amount);
            /*            sheet.properties.SetTexture("_GritTexture", settings.gritTexture);
                        sheet.properties.SetTexture("_GritTexture2", settings.gritTexture2);
                        sheet.properties.SetTexture("_GritTexture3", settings.gritTexture3);
                        sheet.properties.SetTexture("_GritTexture4", settings.gritTexture4);*/



            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}