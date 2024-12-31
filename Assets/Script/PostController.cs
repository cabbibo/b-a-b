using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

using WrenUtils;
using Crest;

[ExecuteAlways]
public class PostController : MonoBehaviour
{


    public PostProcessVolume volume;
    private VolumeProfile profile;


    private MainPost mainPost;
    private Bloom bloom;
    private ColorGrading colorGrading;
    private Vignette vignette;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private FogEffect fogEffect;
    private DepthOfField depthOfField;


    // other controllers
    public CustomFog customFog;
    public UnderwaterRenderer crestUnderwaterRenderer;



    public float _Hue;
    public float _Saturation;
    public float _Lightness;
    public float _Blend;
    public float _Fade;



    public Texture2D biomeMap;

    public float depthOfFieldFocusDistance;

    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out mainPost);
        volume.profile.TryGetSettings(out bloom);
        volume.profile.TryGetSettings(out colorGrading);
        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out lensDistortion);
        volume.profile.TryGetSettings(out chromaticAberration);
        volume.profile.TryGetSettings(out fogEffect);
        volume.profile.TryGetSettings(out depthOfField);


    }


    public float angleOffset;
    public float sizeToFullSaturation;



    public void Update()
    {

        if (God.wren != null)
        {
            CartToPolar(God.wren.transform.position);
        }


        //        print(post);
        mainPost._Hue.value = _Hue;
        mainPost._Saturation.value = _Saturation;
        mainPost._Lightness.value = _Lightness;
        mainPost._Blend.value = _Blend;
        mainPost._Fade.value = _Fade;

        if (God.wren != null)
        {
            depthOfFieldFocusDistance = Vector3.Distance(God.wren.transform.position, God.camera.transform.position);
        }
        // todo if we are focusing on something else make that be the focus object ( even better make them both be in focus)
        depthOfField.focusDistance.value = depthOfFieldFocusDistance;

    }



    Vector2 CartToPolar(Vector3 position)
    {

        float angle = Mathf.Atan2(position.x, position.z);
        float radius = (new Vector2(position.x, position.z)).magnitude;


        float x = (position.x + 2048) / 4096;
        float y = (position.z + 2048) / 4096;

        Color c = biomeMap.GetPixelBilinear(x, y, 0);

        //        print( c.a);


        float h, s, v;

        Color.RGBToHSV(c, out h, out s, out v);

        _Hue = h;
        _Blend = c.a;



        /* angle = (angle > 0 ? angle : (2*Mathf.PI + angle));
         angle /= 2 * Mathf.PI;

         _Hue = angle;
         _Hue += angleOffset;
         _Hue %= 1;


         print( _Hue );*/

        return new Vector2(angle, radius);

    }


    public void FadeOut()
    {
        StartCoroutine(DoFadeOut());
    }

    public void FadeIn()
    {
        StartCoroutine(DoFadeIn());
    }

    public float fadeInSpeed = 1;
    public float fadeOutSpeed = 1;

    IEnumerator DoFadeOut()
    {
        float t = 0;
        while (t < 1)
        {
            t += .03f * fadeOutSpeed;
            _Fade = t;
            yield return null;
        }
    }

    IEnumerator DoFadeIn()
    {
        float t = 1;
        while (t > 0)
        {
            t -= .03f * fadeInSpeed;
            _Fade = t;
            yield return null;
        }
    }



}
