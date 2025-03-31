using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "WeatherSettings", menuName = "WeatherSettings", order = 1)]
public class WeatherSettings : ScriptableObject
{


    public bool sun_auto;
    public float sun_rawTimeInCycle;

    public bool sun_showSunRenderer;

    public float sun_daySpeed;
    public float sun_nightSpeed;
    public float sun_transitionSpeed;



    public void SetValues(WeatherManager WM)
    {
        WM.sunManager.auto = sun_auto;
        WM.sunManager.rawTimeInCycle = sun_rawTimeInCycle;
        WM.sunManager.showSunRenderer = sun_showSunRenderer;
        WM.sunManager.daySpeed = sun_daySpeed;
        WM.sunManager.nightSpeed = sun_nightSpeed;
    }




}