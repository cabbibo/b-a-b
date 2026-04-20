using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    public float targetTimeScale    = 1f;
    public float minTimeScale       = .001f;
    public float maxTimeScale       = 10f;
    public float lerpTimeScaleSpeed = .1f;


    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        Time.timeScale = Mathf.Clamp( Mathf.Lerp( Time.timeScale , targetTimeScale , lerpTimeScaleSpeed ) , minTimeScale ,
            maxTimeScale );

    }

    public void SetTargetTimeScale( float tts )
    {
        targetTimeScale = tts;
    }

    public void ResetTimeScale()
    {
        targetTimeScale = 1;
    }
}