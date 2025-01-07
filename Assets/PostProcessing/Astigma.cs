using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{

    [PostProcess(typeof(AstigmaRenderer), PostProcessEvent.BeforeStack, "Astigma")]
    public class Astigma : PostProcessEffectSettings
    {
        public FloatParameter intensity = new FloatParameter { value = 1.0f };
        public FloatParameter scale = new FloatParameter { value = 1.0f };
        public FloatParameter cutoff = new FloatParameter { value = 0.0f };
        public FloatParameter aspectRatio = new FloatParameter { value = 1.0f };
        public FloatParameter angle = new FloatParameter { value = 0.0f };
        public FloatParameter numSamples = new FloatParameter { value = 5.0f };
        public FloatParameter numDirections = new FloatParameter { value = 4.0f };

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

            sheet.properties.SetFloat("_Intensity", settings.intensity);
            sheet.properties.SetFloat("_Scale", settings.scale);
            sheet.properties.SetFloat("_Cutoff", settings.cutoff);
            sheet.properties.SetFloat("_AspectRatio", settings.aspectRatio);
            sheet.properties.SetFloat("_Angle", settings.angle);
            sheet.properties.SetFloat("_NumSamples", settings.numSamples);
            sheet.properties.SetFloat("_NumDirections", settings.numDirections);
            sheet.properties.SetVector("_ScreenParams", new Vector4(context.width, context.height, 1.0f / context.width, 1.0f / context.height));



            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}