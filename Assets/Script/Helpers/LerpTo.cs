using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class LerpTo : MonoBehaviour
{

    public Transform target;
    public float lerpSpeed;
    public float slerpSpeed;

    public Transform lookTarget;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if( wantsToRelease && Time.time - startLookTime > releaseTime ){
            lookTarget = null;
            
        }
        if( target != null ){
                transform.position = Vector3.Lerp(transform.position , target.position , lerpSpeed );

                if( lookTarget != null ){
                    
                    transform.rotation = Quaternion.Slerp(transform.rotation , Quaternion.LookRotation( lookTarget.position-transform.position , Vector3.up ) , slerpSpeed );
                }else{
                    transform.rotation = Quaternion.Slerp(transform.rotation , target.rotation , slerpSpeed );
                }
        }else{
            // gives us a target if we dont have one!
            /*if( God.wren != null ){
                target = God.wren.cameraWork.camTarget;
            }*/    
           
        }
    }


    public void OnDisable(){
//        print("disabled");
    }

    public void SetLookTarget( Transform t ){
        lookTarget = t;
    }

    public void RemoveLookTarget(){
        lookTarget = null;
    }


    public float startLookTime;
    public bool wantsToRelease;
    public float releaseTime;

    public void SetLookReleaseTime(  float time ){

        wantsToRelease = true;
        startLookTime = Time.time;
        releaseTime = time;
    }

    
}
