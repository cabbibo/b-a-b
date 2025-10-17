using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [PostProcess( typeof(AstigmaRenderer) , PostProcessEvent.BeforeStack , "Astigma" )]
    public class Astigma : PostProcessEffectSettings
    {
        public FloatParameter   intensity     = new() { value = 1.0f };
        public FloatParameter   scale         = new() { value = 1.0f };
        public FloatParameter   cutoff        = new() { value = 0.0f };
        public FloatParameter   aspectRatio   = new() { value = 1.0f };
        public FloatParameter   angle         = new() { value = 0.0f };
        public FloatParameter   numSamples    = new() { value = 5.0f };
        public FloatParameter   numDirections = new() { value = 4.0f };
        public BoolParameter    useTexture    = new() { value = false };
        public TextureParameter texture       = new() { value = null };
        public ColorParameter   color         = new() { value = new Color( 0.0f , 0.0f , 0.0f , 0.0f ) };
    }

    public sealed class AstigmaRenderer : PostProcessEffectRenderer<Astigma>
    {
        public const string AstigmaShader     = "VertexFragment/Astigma";
        public const string BlurAstigmaShader = "VertexFragment/AstigmaBlur";

        public override void Render( PostProcessRenderContext context )
        {
            var shader = Shader.Find( AstigmaShader );

            if ( shader == null ) {
                return;
            }

            var sheet = context.propertySheets.Get( shader );

            if ( sheet == null ) {
                return;
            }

            sheet.properties.SetFloat( "_Intensity" , settings.intensity );
            sheet.properties.SetFloat( "_Scale" , settings.scale );
            sheet.properties.SetFloat( "_Cutoff" , settings.cutoff );
            sheet.properties.SetFloat( "_AspectRatio" , settings.aspectRatio );
            sheet.properties.SetFloat( "_Angle" , settings.angle );
            sheet.properties.SetFloat( "_NumSamples" , settings.numSamples );
            sheet.properties.SetFloat( "_NumDirections" , settings.numDirections );
            sheet.properties.SetColor( "_AstigmaColor" , settings.color );
            sheet.properties.SetVector( "_ScreenParams" ,
                new Vector4( context.width , context.height , 1.0f / context.width , 1.0f / context.height ) );

            sheet.properties.SetInt( "_UseTexture" , settings.useTexture ? 1 : 0 );

            if ( settings.texture.value != null ) {
                sheet.properties.SetTexture( "_BokehTex" , settings.texture );
            }

            if ( !settings.useTexture ) {

                context.command.BlitFullscreenTriangle( context.source , context.destination , sheet , 0 );
            } else {


                int reducer = 1;
                int newWidth = context.width / reducer;
                int newHeight = context.height / reducer;
                sheet.properties.SetVector( "_ScreenParams" ,
                    new Vector4( newWidth , newHeight , 1.0f / newWidth , 1.0f / newHeight ) );
                var bokehTexture = RenderTexture.GetTemporary( newWidth , newHeight , 0 );

                context.command.BlitFullscreenTriangle( context.source , bokehTexture , sheet , 0 );

                var shader1 = Shader.Find( BlurAstigmaShader );

                if ( shader1 == null ) {
                    return;
                }


                var halfSize = RenderTexture.GetTemporary( context.width / 2 , context.height / 2 , 0 );
                context.command.BlitFullscreenTriangle( bokehTexture , halfSize );

                var quarterSize = RenderTexture.GetTemporary( context.width / 4 , context.height / 4 , 0 );
                context.command.BlitFullscreenTriangle( bokehTexture , quarterSize );

                var eighthSize = RenderTexture.GetTemporary( context.width / 8 , context.height / 8 , 0 );
                context.command.BlitFullscreenTriangle( bokehTexture , eighthSize );


                var sheet1 = context.propertySheets.Get( shader1 );
                sheet1.properties.SetFloat( "_Intensity" , settings.intensity );
                sheet1.properties.SetFloat( "_Scale" , settings.scale );
                sheet1.properties.SetFloat( "_Cutoff" , settings.cutoff );
                sheet1.properties.SetFloat( "_AspectRatio" , settings.aspectRatio );
                sheet1.properties.SetFloat( "_Angle" , settings.angle );
                sheet1.properties.SetFloat( "_NumSamples" , settings.numSamples );
                sheet1.properties.SetFloat( "_NumDirections" , settings.numDirections );
                sheet1.properties.SetVector( "_ScreenParams" ,
                    new Vector4( context.width , context.height , 1.0f / context.width , 1.0f / context.height ) );

                sheet1.properties.SetInt( "_UseTexture" , settings.useTexture ? 1 : 0 );

                if ( settings.texture.value != null ) {
                    sheet1.properties.SetTexture( "_BokehTex" , settings.texture );
                }

                sheet1.properties.SetTexture( "_Half" , halfSize );
                sheet1.properties.SetTexture( "_Quarter" , quarterSize );
                sheet1.properties.SetTexture( "_Eighth" , eighthSize );
                sheet1.properties.SetTexture( "_Full" , bokehTexture );


                context.command.BlitFullscreenTriangle( context.source , context.destination , sheet1 , 0 );

                RenderTexture.ReleaseTemporary( bokehTexture );
                RenderTexture.ReleaseTemporary( halfSize );
                RenderTexture.ReleaseTemporary( quarterSize );
                RenderTexture.ReleaseTemporary( eighthSize );

            }


        }
    }
}