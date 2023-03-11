using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpOnCurveAmount : MonoBehaviour
{

    public CurveMaterialController materialController;

    public float lerpSpeed;
    float lerpStartTime;
    float targetLerp;

    bool lerping;
    float lerpStartValue;
    float lerpValue;
  
    // Update is called once per frame
    void Update()
    {

        if( lerping ){
            lerpValue = (Time.time - lerpStartTime) / lerpSpeed;
            if( lerpStartValue > 1 ){
                lerpValue = 1;
                lerping = false;
            }


            materialController.amount = Mathf.Lerp( lerpStartValue , targetLerp,lerpValue);
        }
        
    }

    public void LerpOn( float speed ){
        targetLerp = 1;
        lerpStartTime = Time.time;
        lerpSpeed = speed;
        lerping = true;
        lerpStartValue = materialController.amount;
    }

    public void LerpOff(float speed){
        targetLerp = 0;
        lerpStartTime = Time.time;
        lerpSpeed = speed;
        lerping = true;
        lerpStartValue = materialController.amount;
    }

    


}
