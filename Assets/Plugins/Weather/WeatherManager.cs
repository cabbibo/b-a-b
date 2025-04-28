using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    public RainManager rainManager;

    public RainbowManager rainbowManager;
    public SnowManager snowManager;
    public SunManager sunManager;

    public StarManager starManager;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SetValues(WeatherSettings WS)
    {
        // WS.SetValues(this);

//        print("SETTING");

        sunManager.auto = WS.sun_auto;
        sunManager.rawTimeInCycle = WS.sun_rawTimeInCycle;
        sunManager.showSunRenderer = WS.sun_showSunRenderer;
        sunManager.daySpeed = WS.sun_daySpeed;
        sunManager.nightSpeed = WS.sun_nightSpeed;

    }


}
