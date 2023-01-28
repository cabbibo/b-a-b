using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenGrowthManager : MonoBehaviour
{

    public Wren wren;
    public WrenNetworked networkInfo;
    public WrenState state;
    public WrenStats stats;

    public float hurtCollisionMultiplier;
    public float hurtCollisionCutoff;

    public float flapStaminaSubtractor;
    public float flapStaminaSubtractorMin;
    public float flapStaminaSubtractorMax;


    public float staminaCooldownTime = 1;
    public float staminaCooldownTimeMin = 0;
    public float staminaCooldownTimeMax = 1000;

    public float staminaRefillSpeed = .01f;
    public float staminaRefillSpeedMin = 0;
    public float staminaRefillSpeedMax = 0.01f;

    public float healthRefillSpeed;
    public float healthRefillSpeedMin = 0;
    public float healthRefillSpeedMax = 0;

    public float hungerGrowSpeed;
    public float hungerGrowSpeedMin;
    public float hungerGrowSpeedMax;


    public float wetnessGrowSpeed;
    public float wetnessGrowSpeedMin;
    public float wetnessGrowSpeedMax;

    public float wetnessRefillSpeed;
    public float wetnessRefillSpeedMin;
    public float wetnessRefillSpeedMax;



    // we have to go some place with no wind? 
    public float sleepGrowSpeed;
    public float sleepGrowSpeedMin;
    public float sleepGrowSpeedMax;
    
    public float sleepRefillSpeed;
    public float sleepRefillSpeedMin;
    public float sleepRefillSpeedMax;


    // have to land in 
    public float thirstGrowSpeed;
    public float thirstGrowSpeedMin;
    public float thirstGrowSpeedMax;

    

    public float sadnessGrowSpeed;
    public float sadnessGrowSpeedMin;
    public float sadnessGrowSpeedMax;

    public float boredomGrowSpeed;
    public float boredomGrowSpeedMin;
    public float boredomGrowSpeedMax;


    
    public float hungryHealthSubtractor;
    public float thirstyHealthSubtractor;
    public float tiredHealthSubtractor;
    public float sadnessHealthSubtractor;
    public float boredomHealthSubtractor;

    public float satisfiedHealthAdder;







    // if we are on ground, refill stamina at max speed
    // if we are on ground for long enough, begin to refill health
    // if we are more full, refill stamina faster
    // if we are on ground for long enough, awakeness refills
    // if we have been playing for too long, our awakeness starts to drain?!?!
    // if we are completely full, refill health
    // if we are wet, stamina refills slower drains faster
    // if we not awake/ alert, stamina refills slower drains faster
    // if we are wet, we get hungry quick
    //
    // totally full, totally wet, totally sleepy leads to health draining.
    // when pick up a crystal, health fully refills, refills all other states when you are near by




    //TODO: do we need to pass state on this one?
    public void HurtCollision(Collision c){


        if( state.isLocal){

            stats.HealthAdd( -c.relativeVelocity.magnitude * hurtCollisionMultiplier );
            God.audio.Play( God.sounds.hurtClip );
            if( God.glitchHit != null ){ God.glitchHit.StartGlitch(); }
        
        }
    }



    float lastFlapTime;
    public void updateGrowth(){


            float d = Mathf.Abs( wren.input.o_left2 - wren.input.left2);
            stats.StaminaAdd( -d * flapStaminaSubtractor );

            if( d > 0 ){
                lastFlapTime = Time.time;
            }

            d = Mathf.Abs( wren.input.o_right2 - wren.input.right2);
            stats.StaminaAdd( -d * flapStaminaSubtractor );

            if( d > 0 ){
                lastFlapTime = Time.time;
            }

            if( state.onGround ){
                staminaCooldownTime = 0;
            }
            
            
            if( Time.time - lastFlapTime  > staminaCooldownTime ){
                stats.StaminaAdd( staminaRefillSpeed);
            }


            healthRefillSpeed = Mathf.Clamp( healthRefillSpeed, healthRefillSpeedMin , healthRefillSpeedMax );
            hungerGrowSpeed = Mathf.Clamp( hungerGrowSpeed, hungerGrowSpeedMin , hungerGrowSpeedMax );
            sleepGrowSpeed = Mathf.Clamp( sleepGrowSpeed, sleepGrowSpeedMin , sleepGrowSpeedMax );
            thirstGrowSpeed = Mathf.Clamp( thirstGrowSpeed, thirstGrowSpeedMin , thirstGrowSpeedMax );


            // update our thirst, sleep and hunger stats
            stats.FullnessAdd( -hungerGrowSpeed );
            stats.QuenchednessAdd( -thirstGrowSpeed );
            stats.AwakenessAdd( -sleepGrowSpeed );


            // start killing if are too cold
            if( stats.fullness == 0 ){
                stats.HealthAdd(-hungryHealthSubtractor);
            }

            if( stats.quenchedness == 0 ){
                stats.QuenchednessAdd(-thirstyHealthSubtractor);
            }

            if( stats.awakeness == 0 ){
                stats.AwakenessAdd(-tiredHealthSubtractor);
            }


            if( stats.fullness > .5 && stats.quenchedness > .5 && stats.awakeness > .8 ){

                stats.HealthAdd( satisfiedHealthAdder );

            }
    


        


    }


}
