using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;
public class RibbonCollectionPrize : MonoBehaviour
{

    public float lerpLength;
    public float lerpSpeed;
    public float slerpSpeed;
    public Curve curve;

    public LerpTo cameraControls;

    public Transform lookTarget;
    public Transform posTarget;


    public Transform tmpLookTarget;
    public Transform tmpPosTarget;


    public bool lerping;

    public float lerpStartTime;

    public float tmpLerpSpeed;
    public float tmpSlerpSpeed;



    MeshRenderer curveRenderer;
    

    public void OnCollect(){

        tmpLookTarget = cameraControls.lookTarget;
        tmpPosTarget = cameraControls.target;
        tmpSlerpSpeed = cameraControls.slerpSpeed;
        tmpLerpSpeed = cameraControls.lerpSpeed;


        cameraControls.lookTarget = lookTarget;
        cameraControls.target = posTarget;
        cameraControls.slerpSpeed  = slerpSpeed;
        cameraControls.slerpSpeed  = lerpSpeed;

        lerping = true;
        lerpStartTime = Time.time;

        curveRenderer = curve.gameObject.GetComponent<MeshRenderer>();




    }


    public void SetCollected(){

        curveRenderer = curve.gameObject.GetComponent<MeshRenderer>();
        curveRenderer.material.SetFloat("_Amount" , 1 );
    }

    void OnFinish(){

        
         cameraControls.lookTarget = tmpLookTarget;
         cameraControls.target = tmpPosTarget;
         cameraControls.slerpSpeed = tmpSlerpSpeed;
         cameraControls.lerpSpeed = tmpLerpSpeed;

        curveRenderer.material.SetFloat("_Amount" , 1 );
    }

    // Update is called once per frame
    void Update()
    {
        if( lerping ){
            float v = (Time.time - lerpStartTime ) / lerpStartTime;
            if( v >= 1 ){
                OnFinish();
            }


            curveRenderer.material.SetFloat("_Amount" , v );
            posTarget.position = curve.GetPositionFromValueAlongCurve( 1-v );
        }
    }
}
