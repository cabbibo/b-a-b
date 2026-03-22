// AudioListenerTextureGPU.cs
// Drop on any GameObject. Assign a material that has shader "Hidden/AudioSpectrumToTexture" (below).
// Produces a 1xN RGBA texture on GPU (RenderTexture) with exponential smoothing done on GPU.
// Still needs CPU spectrum read (AudioListener.GetSpectrumData), but avoids GetPixels/SetPixels.

using UnityEngine;

public class AudioListenerTexture : MonoBehaviour
{
    [Range( 64 , 4096 )]
    public int size = 1024; // output width (bins)

    public int       audioChannel = 0;
    public FFTWindow window       = FFTWindow.Triangle;

    [Tooltip( "Material using shader Hidden/AudioSpectrumToTexture" )]
    public Material blitMat;

    [Tooltip( "Smoothing factor: 0 = no history, 0.9 = heavy smoothing" )]
    [Range( 0f , 0.99f )]
    public float decay = 0.8f;

    public RenderTexture audioRT; // GPU texture you sample in shaders

    private float[]   samples;
    private Texture2D sampleTex; // tiny 1x(size) texture fed from CPU spectrum (no readback)
    private Color[]   samplePixels;
    private int       _sizeCached = -1;

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnDisable()
    {
        Release();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void LateUpdate()
    {
        if ( !blitMat ) {
            return;
        }

        if ( _sizeCached != size ) {
            Rebuild();
        }

        // CPU -> small upload texture (no GetPixels/SetPixels on the output)
        AudioListener.GetSpectrumData( samples , audioChannel , window );

        for ( int i = 0; i < size; i++ ) {
            // pack 4 adjacent bins into RGBA
            int baseIdx = i * 4;
            samplePixels[i].r = baseIdx + 0 < samples.Length ? samples[baseIdx + 0] : 0f;
            samplePixels[i].g = baseIdx + 1 < samples.Length ? samples[baseIdx + 1] : 0f;
            samplePixels[i].b = baseIdx + 2 < samples.Length ? samples[baseIdx + 2] : 0f;
            samplePixels[i].a = baseIdx + 3 < samples.Length ? samples[baseIdx + 3] : 0f;
        }


        sampleTex.SetPixels( samplePixels );
        sampleTex.Apply( false , false );

        blitMat.SetTexture( "_SamplesTex" , sampleTex );
        blitMat.SetFloat( "_Decay" , decay );

        // GPU history + smoothing via blit into audioRT
        var tmp = RenderTexture.GetTemporary( audioRT.descriptor );
        Graphics.Blit( audioRT , tmp ); // copy previous frame
        blitMat.SetTexture( "_PrevTex" , tmp );
        Graphics.Blit( null , audioRT , blitMat );
        RenderTexture.ReleaseTemporary( tmp );

        Shader.SetGlobalTexture( "_AudioMap" , audioRT );
    }

    private void Rebuild()
    {
        Release();

        _sizeCached = size;

        // we pack 4 samples per pixel, so want 4*size bins at least
        int sampleCount = size * 4;
        samples = new float[sampleCount];

        sampleTex = new Texture2D( size , 1 , TextureFormat.RGBAFloat , false , true );
        sampleTex.wrapMode = TextureWrapMode.Clamp;
        sampleTex.filterMode = FilterMode.Bilinear;
        samplePixels = new Color[size];

        /*audioRT = new RenderTexture( size , 1 , 0 , RenderTextureFormat.ARGBHalf , RenderTextureReadWrite.Linear );
        audioRT.wrapMode = TextureWrapMode.Clamp;
        audioRT.filterMode = FilterMode.Bilinear;
        audioRT.useMipMap = false;
        audioRT.autoGenerateMips = false;
        audioRT.Create();*/

        // init to 0
        var prev = RenderTexture.active;
        RenderTexture.active = audioRT;
        GL.Clear( false , true , Color.clear );
        RenderTexture.active = prev;
    }

    private void Release()
    {
        /*   if ( audioRT ) {
               audioRT.Release();
               DestroyImmediate( audioRT );
               audioRT = null;
           }*/

        if ( sampleTex ) {
            DestroyImmediate( sampleTex );
            sampleTex = null;
        }

        samples = null;
        samplePixels = null;
        _sizeCached = -1;
    }
}