using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;


public class InterfaceRing : MonoBehaviour
{

    public float radius;
    public float thickness;

    public int numSegments;

    public float value;

    public float[] tickValues;
    public Color[] tickColors;
    public bool showTickValues;

    public Material material;

    public MaterialPropertyBlock mpb;

    public int numDivisions;
    public bool showDivisions;

    public bool fullOn;
    public bool doFade;


    public float fadeOutSpeed;
    public float fadeInSpeed;

    [Header("Debug")]
    public float fadeValue;

    public void OnEnable()
    {
        value = 0;
        fadeValue = 0;
    }


    public void LateUpdate()
    {

        //if (doFade)
        //{
        fadeValue = Mathf.Lerp(fadeValue, fullOn ? 1 : 0, fullOn ? fadeInSpeed : fadeOutSpeed);
        //}

        // dont need to update if 0 alpha
        if (fadeValue > 0.01f || value <= 0)
        {

            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetInt("_Count", numSegments);
            mpb.SetFloat("_Radius", radius);
            mpb.SetFloat("_Thickness", thickness);
            mpb.SetFloat("_Value", value);
            mpb.SetFloat("_Fade", fadeValue);

            Vector4[] colorArray = new Vector4[tickColors.Length];
            for (int i = 0; i < tickColors.Length; i++)
            {
                colorArray[i] = tickColors[i];
            }

            if (showTickValues)
            {
                mpb.SetFloatArray("_TickValues", tickValues);
                mpb.SetVectorArray("_TickColors", colorArray);
                mpb.SetInt("_ShowTickValues", showTickValues ? 1 : 0);
            }

            if (showDivisions)
            {
                mpb.SetInt("_NumDivisions", numDivisions);
                mpb.SetInt("_ShowDivisions", showDivisions ? 1 : 0);
            }


            mpb.SetVector("_WrenPos", God.wren.transform.position);


            Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, numSegments * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

        }

    }

    public void SetFade(float v)
    {
        fadeValue = v;
    }

    public void SetValue(float v)
    {
        value = v;
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


}
