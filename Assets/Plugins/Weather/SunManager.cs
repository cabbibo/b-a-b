using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SunManager : MonoBehaviour
{
    public bool osscilate;

    public bool auto;
    public bool useLookTarget;


    public Transform lookTarget;
    public Transform targetPosition;
    public Light     sun;
    public Material  sky;


    public float daySpeed = 30;

    public float nightSpeed = 10;


    public float totalCycleLength;


    public float timeInDay;
    public float dayNess;

    public float timeInNight;
    public float nightNess;

    public float     sunRadius;
    public Transform sunRotator;

    public float rawTimeInCycle;
    public float normalizedTimeInCycle;


    public Gradient dayColor;
    public Gradient nightColor;

    public AnimationCurve dayRemapper;
    public AnimationCurve nightRemapper;

    public float osscilateBase;
    public float osscilateSize;
    public float osscilateSpeed;

    public bool     showSunRenderer;
    public Renderer sunRenderer;

    public Material sunMaterial;


    public void OnEnable()
    {
        sky = RenderSettings.skybox;

        totalCycleLength = daySpeed + nightSpeed;


    }


    public void Update()
    {


        totalCycleLength = daySpeed + nightSpeed;


        if ( auto ) {

            rawTimeInCycle += Time.deltaTime;
            rawTimeInCycle = rawTimeInCycle % totalCycleLength;

        } else {
            rawTimeInCycle = rawTimeInCycle % totalCycleLength;
        }

        if ( osscilate ) {
            rawTimeInCycle = osscilateBase + Mathf.Sin( Time.time * osscilateSpeed ) * osscilateSize;
        }

        float normalizedTimeInCycle = rawTimeInCycle / totalCycleLength;

        timeInDay = dayRemapper.Evaluate( Mathf.Clamp01( rawTimeInCycle / daySpeed ) );

        dayNess = 1 - Mathf.Abs( timeInDay - 0.5f ) * 2;

        timeInNight = nightRemapper.Evaluate( Mathf.Clamp01( (rawTimeInCycle - daySpeed) / nightSpeed ) );

        nightNess = 1 - Mathf.Abs( timeInNight - 0.5f ) * 2;


        sunRotator.localRotation = Quaternion.Euler( new Vector3( 200 * timeInDay + 170 , 0 , 0 ) );
        sun.transform.localPosition = new Vector3( 0 , 0 , sunRadius );
        //sun.transform.LookAt(new Vector3(-2048, 0, -2048));


        if ( useLookTarget ) {
            sun.transform.position = targetPosition.position;
            sun.transform.LookAt( lookTarget.position );

        }


        if ( timeInDay < .00001f || timeInDay > .99999f ) {
            sunRotator.localRotation = Quaternion.Euler( new Vector3( 200 * timeInNight + 170 , 0 , 0 ) );
            sun.transform.localPosition = new Vector3( 0 , 0 , sunRadius );
            sun.color = nightColor.Evaluate( timeInNight );
        } else {

            sunRotator.localRotation = Quaternion.Euler( new Vector3( 200 * timeInDay + 170 , 0 , 0 ) );
            sun.transform.localPosition = new Vector3( 0 , 0 , sunRadius );
            sun.color = dayColor.Evaluate( timeInDay );
        }


        sunRenderer.enabled = showSunRenderer;
        sunRenderer.material = sunMaterial;


        Shader.SetGlobalFloat( "_DayNess" , dayNess );
        Shader.SetGlobalFloat( "_NightNess" , nightNess );
        Shader.SetGlobalFloat( "_TimeInDay" , timeInDay );
        Shader.SetGlobalFloat( "_TimeInNight" , timeInNight );

        Shader.SetGlobalVector( "_SunDirection" , -sun.transform.forward );
        Shader.SetGlobalVector( "_SunColor" , sun.color );
        Shader.SetGlobalVector( "_SunPosition" , sun.transform.position );


    }

    public void SetMidday()
    {

        auto = false;
        rawTimeInCycle = daySpeed / 2;
    }

    public void SetMidnight()
    {

        auto = false;
        rawTimeInCycle = daySpeed + nightSpeed / 2;

    }
}