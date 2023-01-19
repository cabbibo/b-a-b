using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using WrenUtils;

public class WaterCarryable : MonoBehaviour
{

    public float maxAmount;
    public float currentAmount;

    public Transform respawnTransform;
    public Vector3 respawnOffset;

    public float fillSpeed;

    public bool filling;

    public bool drying;
    public float dripSpeed;

    public Carryable carryable;

    public float maxScale;

    public bool atSpawnPoint;
    public bool beingCarried;



    public ParticleSystem particles;
    // Update is called once per frame
    void Update()
    {


        // currently carrying in a location where
        // the water is fading away
        if( drying == true ){
            currentAmount -= dripSpeed;
            if( currentAmount < 0  ){
                currentAmount = 0;
                OnDry();
            }
        }

        // currently at one of the ponds
        // filling up our water
        if( filling ){
            currentAmount += fillSpeed;
            if( currentAmount > maxAmount ){
                currentAmount = maxAmount;
                OnFull();
            }
        }


        transform.localScale = Vector3.one * maxScale * (currentAmount / maxAmount);
        
        if( carryable.BeingCarried != true && beingCarried == true  ){
            Drop();
        }

        if( carryable.BeingCarried == true && beingCarried == false  ){
            Pickup();
        }
    
    }

    void Start(){
        OnDry();
    }


    Loop fillLoop;
    Loop  carryLoop;
    void Drop(){

        if( fillLoop != null ){ God.audio.EndLoop( fillLoop ); }
        if( carryLoop != null){ God.audio.EndLoop(carryLoop);}
       // OnStartDrying();
        beingCarried = false;
    }

    void Pickup(){

        beingCarried = true;
    }


    public void OnStartFilling(){

        
        currentAmount = 0;
        if( fillLoop != null ){ God.audio.EndLoop( fillLoop ); }
        if( carryLoop != null){ God.audio.EndLoop(carryLoop);}

        drying = false;
        filling = true;
        
        var shape = particles.shape;
        shape.radius = 10;
        
        var main = particles.main;
        main.startSpeed = -5;

        fillLoop = God.audio.MakeLoop( God.sounds.waterFillingLoop );
        fillLoop.Start();

    }

    void OnFull(){
        filling = false;
        var shape = particles.shape;
        shape.radius = 5;
        
        var main = particles.main;
        main.startSpeed = 0;
        
        God.audio.Play( God.sounds.waterFilledSound );
        if( fillLoop != null ){ God.audio.EndLoop( fillLoop ); }
        if( carryLoop != null){ God.audio.EndLoop(carryLoop);}
    }

    public void OnStartDrying(){

        print( "STarted drign");
        drying = true;
        filling = false;

        var shape = particles.shape;
        shape.radius = 0;
        
        var main = particles.main;
        main.startSpeed = 5;

        
        if( fillLoop != null ){ God.audio.EndLoop( fillLoop ); }
        if( carryLoop != null){ God.audio.EndLoop(carryLoop);}
        carryLoop = God.audio.MakeLoop( God.sounds.carryingWaterLoop );
        carryLoop.Start();
    }

    void OnDry(){


        God.audio.Play( God.sounds.waterDroppedSound);
        foreach( Wren w in God.wrens ){
            w.carrying.DropIfCarrying(carryable);
        }
        if( fillLoop != null ){ God.audio.EndLoop( fillLoop ); }
        if( carryLoop != null){ God.audio.EndLoop(carryLoop);}

        
          var shape = particles.shape;
        shape.radius = 0;
        
        var main = particles.main;
        main.startSpeed = 0;

        drying = false;
        
        transform.position = respawnTransform.position + respawnOffset;

        transform.GetComponent<Rigidbody>().position = transform.position;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

    }

    void OnTriggerEnter( Collider c ){

    }
}
