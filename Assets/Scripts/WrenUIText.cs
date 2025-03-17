using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WrenUIText : MonoBehaviour
{

    public Transform baseTransform;
    public TextMeshPro text;

    public float upAmount = 0.5f;
    public float lerpTowardsCamera = .5f;


    public Material material;

    public MaterialPropertyBlock mpb;

    public bool fullOn;
    public bool doFade;

    public bool pulse;

    public float pulseSpeed;


    public float fadeOutSpeed;
    public float fadeInSpeed;

    [Header("Debug")]
    public float fadeValue;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnEnable()
    {
        fadeValue = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {



        fadeValue = Mathf.Lerp(fadeValue, fullOn ? 1 : 0, fullOn ? fadeInSpeed : fadeOutSpeed);

        if (pulse)
        {
            fadeValue = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        }




        // dont need to update if 0 alpha
        if (fadeValue > 0.01f)
        {

            /*if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }*/


            //mpb.SetFloat("_Fade", fadeValue);

            text.enabled = true;
            //text.SetPropertyBlock(mpb);


            text.faceColor = new Color(text.faceColor.r, text.faceColor.g, text.faceColor.b, fadeValue);

            Vector3 basePos = baseTransform.position + Vector3.up * upAmount;


            Vector3 towardsCamera = Camera.main.transform.position - basePos;

            transform.position = basePos + towardsCamera * lerpTowardsCamera;

            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);


        }
        else
        {
            text.enabled = false;
        }



    }

    public void SetFade(float v)
    {
        fadeValue = v;
    }


    public void SetFullOn(bool b)
    {
        fullOn = b;
    }

    // immediately set to full brightness
    public void Ping()
    {
        fullOn = false;
        fadeValue = 1;
    }

    public void PulseOn()
    {
        pulse = true;
    }

    public void PulseOff()
    {

        pulse = false;

    }


}
