using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public class WrenDeathAnimation : MonoBehaviour
{

    public Wren wren;

    public float animationLength = 5;

    public Vector3 startPosition;
    public Vector3 endPosition;
    public float startToEndDist;
    public float startTime;

    public Transform animationRepresent;

    public bool animating = false;

    public Transform tmpTarget;
    public float tmpLerpSpeed;
    public float tmpSLerpSpeed;
    

    public LerpTo  lerpTo;
    public void OnDeath(){

        print("Dying");
        wren.state.dead = true;
    
        God.audio.Play(God.sounds.deathClip);

        lerpTo = Camera.main.GetComponent<LerpTo>();
        tmpTarget = lerpTo.target;

        print(tmpTarget);
        tmpLerpSpeed = lerpTo.lerpSpeed;
        tmpSLerpSpeed = lerpTo.slerpSpeed;



        lerpTo.lerpSpeed = .05f;
        lerpTo.slerpSpeed = .1f;
        lerpTo.target = animationRepresent;
        lerpTo.lookTarget = animationRepresent;

        animationRepresent.GetComponent<TrailRenderer>().enabled = true;
        animationRepresent.GetComponent<MeshRenderer>().enabled = true;

        // TODO: MAKE audio fade for loops bet possible
        God.audio.FadeIn(God.sounds.rebirthLoop,1,5);
        
        startPosition =  wren.transform.position;
        endPosition = wren.startingPosition.position+ Vector3.up * 10;


        startToEndDist = length(startPosition-endPosition);

        wren.bird.HitGround();

        startTime = Time.time;
        animating = true;
        
    }

    public void Update(){



        float fAnimationLength =   animationLength * startToEndDist / 3000;

        fAnimationLength = Mathf.Clamp( fAnimationLength , 2 , 30 );
        if( Time.time - startTime < fAnimationLength && animating == true ){




            float timeInAnimation = (Time.time - startTime) / ( fAnimationLength);


            timeInAnimation = timeInAnimation * timeInAnimation * (3.0f - 2.0f * timeInAnimation);


            float3 p1 = startPosition;
            float3 p2 = (float3)startPosition+ float3(0,startToEndDist/3,0);
            float3 p3 = (float3)endPosition + float3(0,startToEndDist/3,0);
            float3 p4 = endPosition;


            float3 fPos = cubicCurve( timeInAnimation ,  p1 , p2, p3,p4);

            animationRepresent.position = fPos;




        }

        if(  Time.time - startTime >= fAnimationLength && animating == true ){
            animating = false;
            EndAnimation();
        }






    }

    public void EndAnimation(){
        print("reset");
        print( tmpTarget );
        lerpTo.RemoveLookTarget();
        lerpTo.target = tmpTarget;
        wren.stats.health  = wren.stats.maxHealth;
        wren.stats.stamina = wren.stats.maxStamina;
        //wren.FullReset();
        
         lerpTo.lerpSpeed = tmpLerpSpeed;
         lerpTo.slerpSpeed = tmpSLerpSpeed;

         Vector3 fPos = wren.startingPosition.position + Vector3.up * 10;
        wren.Crash(fPos);
        wren.state.LookAt( (float3)fPos + (float3)Camera.main.transform.forward * float3(1,0,1) );
        wren.bird.ResetAtLocation( fPos );//Values();
        
        //wren.bird.Explode();
        wren.bird.HitGround();
        
        animationRepresent.GetComponent<TrailRenderer>().enabled = false;
        animationRepresent.GetComponent<MeshRenderer>().enabled = false;
        wren.state.dead = false;
    }

    public void OnEnable(){
        
        animationRepresent.GetComponent<TrailRenderer>().enabled = false;
        animationRepresent.GetComponent<MeshRenderer>().enabled = false;
    }


    
float3 cubicCurve( float t , float3  c0 , float3 c1 , float3 c2 , float3 c3 ){

  float s  = 1 - t;

  float3 v1 = c0 * ( s * s * s );
  float3 v2 = 3 * c1 * ( s * s ) * t;
  float3 v3 = 3 * c2 * s * ( t * t );
  float3 v4 = c3 * ( t * t * t );

  float3 value = v1 + v2 + v3 + v4;

  return value;

}

}
