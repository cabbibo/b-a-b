using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;


public class WrenBeacon : MonoBehaviour
{

    public Wren wren;

    public Collection collection;

    public Renderer beaconRenderer;

    public GameObject nest;


    public void OnEnable(){

        beaconRenderer.materials[0].SetInt( "_WhichHue" , 4 );
        beaconRenderer.materials[1].SetInt( "_WhichHue" , 3 );
        beaconRenderer.materials[2].SetInt( "_WhichHue" , 2 );
        beaconRenderer.materials[3].SetInt( "_WhichHue" , 1 );
        beaconRenderer.materials[4].SetInt( "_WhichHue" , 0 );

    }

    public void OnDisable(){
        

    }

    public void Create(){
        transform.parent = GameObject.FindGameObjectWithTag("Beacons").transform;
    }

    public void Demolish(){

        Destroy(gameObject);

    }

    public void UpdateColors(){

    }


    void FixedUpdate(){

        
        foreach( Wren w  in God.wrens){

            float upForce = 0;

            Vector3 dist = w.transform.position - transform.position;

            dist = Vector3.Scale( dist , Vector3.left + Vector3.forward );


            float mag = dist.magnitude;

            if( mag < 200 ){ upForce = ( 1000)  / (mag+10); }

            if( w.transform.position.y > nest.transform.position.y ){
                upForce = 0;
            }

            if(!w.state.onGround){
                w.physics.rb.AddForce( Vector3.up * upForce );
            }

        }
    }

    public void PlaceBeacon(Vector3 p){
        wren.state.SetBeaconLocation(p);
        wren.state.SetBeaconOn( true );
        collection.OnBeaconPlace();
    }

}
