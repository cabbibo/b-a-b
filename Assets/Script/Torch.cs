using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Normal.Realtime;

public class Torch: MonoBehaviour
{

    public bool lit;
    public string firePrefab = "Fire";


    public ParticleSystem flameParticleSystem;



    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnEnable(){
        if( lit ){
            Light();
        }
    }

    void OnTriggerEnter(Collider c){

    
        if( c.tag == "Fire"){

            print( "readyToLight");
            if( lit == false ){
                //var tc = c.gameObject.GetComponent<TimingCarryable>();
                
               // if( tc ){ tc.OnDry(); }
               print("lighting");
                Light();
            }
        }

        if( c.tag == "Wren" ){

            print( c.gameObject );
            print(c.attachedRigidbody.gameObject );

            if( c.attachedRigidbody.gameObject.GetComponent<WrenCarrying>().CarriedItems.Count == 0 && lit ){

                print("MADE IT HERE");
                GameObject go = Realtime.Instantiate( firePrefab , transform );
                go.transform.parent = transform;
                go.GetComponent<TimingCarryable>().respawnTransform = transform;
                go.GetComponent<TimingCarryable>().Reset();

            }
        }

    }




    void Light(){
        lit = true;
        flameParticleSystem.Play();
        lit  = true;
    }
}
