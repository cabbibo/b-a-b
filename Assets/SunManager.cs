using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

[ExecuteAlways]
public class SunManager : MonoBehaviour
{


    public bool auto;
    public Light sun;
    public Material sky;

    public Light moon;

    public float daySpeed = 30;

    public float nightSpeed = 10;

    public float transitionSpeed = 3;

    public float totalCycleLength;


    public float timeInDay;
    public float dayNess;

    public float timeInNight;
    public float nightNess;


    public float sunRadius;

    public Transform sunRotator;

    public float moonRadius;
    public Transform moonRotator;


    public float rawTimeInCycle;
    public float normalizedTimeInCycle;


    public Gradient dayColor;
    public Gradient nightColor;

    public AnimationCurve dayRemapper;
    public AnimationCurve nightRemapper;





    public void OnEnable()
    {
        sky = RenderSettings.skybox;

        totalCycleLength = daySpeed + nightSpeed;



    }


    public void Update()
    {
        totalCycleLength = daySpeed + nightSpeed;


        if (auto)
        {

            rawTimeInCycle = Time.time % totalCycleLength;
        }
        else
        {
            rawTimeInCycle = rawTimeInCycle % totalCycleLength;
        }

        float normalizedTimeInCycle = rawTimeInCycle / totalCycleLength;

        timeInDay = dayRemapper.Evaluate(Mathf.Clamp01(rawTimeInCycle / daySpeed));

        dayNess = 1 - Mathf.Abs(timeInDay - 0.5f) * 2;

        timeInNight = nightRemapper.Evaluate(Mathf.Clamp01((rawTimeInCycle - daySpeed) / nightSpeed));

        nightNess = 1 - Mathf.Abs(timeInNight - 0.5f) * 2;









        sunRotator.localRotation = Quaternion.Euler(new Vector3(200 * timeInDay + 170, 0, 0));
        sun.transform.localPosition = new Vector3(0, 0, sunRadius);

        sun.color = dayColor.Evaluate(timeInDay);




        moonRotator.localRotation = Quaternion.Euler(new Vector3(200 * timeInNight + 170, 0, 0));
        moon.transform.localPosition = new Vector3(0, 0, moonRadius);

        moon.color = nightColor.Evaluate(timeInNight);

        sun.enabled = false;
        moon.enabled = false;




        if (timeInDay < .00001f || timeInDay > .99999f)
        {
            sun.enabled = false;
        }
        else
        {

            God.sun.transform.position = sun.transform.position;
            God.sun.transform.rotation = sun.transform.rotation;
            God.sun.color = sun.color;
            God.sun.enabled = true;
        }

        if (timeInNight < .00001f || timeInNight > .99999f)
        {
            moon.enabled = false;
        }
        else
        {
            God.sun.transform.position = moon.transform.position;
            God.sun.transform.rotation = moon.transform.rotation;
            God.sun.color = moon.color;
            God.sun.enabled = true;
        }

        Shader.SetGlobalFloat("_DayNess", dayNess);
        Shader.SetGlobalFloat("_NightNess", nightNess);
        Shader.SetGlobalFloat("_TimeInDay", timeInDay);
        Shader.SetGlobalFloat("_TimeInNight", timeInNight);

        Shader.SetGlobalVector("_SunDirection", -sun.transform.forward);
        Shader.SetGlobalVector("_MoonDirection", -moon.transform.forward);
        Shader.SetGlobalVector("_SunColor", sun.color);
        Shader.SetGlobalVector("_MoonColor", moon.color);

        Shader.SetGlobalVector("_SunPosition", sun.transform.position);
        Shader.SetGlobalVector("_MoonPosition", moon.transform.position);


    }




}
