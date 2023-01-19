using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;
public class SlipStream : MonoBehaviour
{

    public float forceMultiplier;
    public float rightingMultiplier;
    public float rightingForwardAmount;

    public float lerpAmount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Wren wrenInside;

    // Update is called once per frame
    void FixedUpdate()
    {

        if( wrenInside != null ){
            wrenInside.physics.rb.AddForce( transform.forward * forceMultiplier );


Rigidbody rb = wrenInside.physics.rb;


//rb.velocity = Vector3.Lerp( rb.velocity , rb.velocity.magnitude * transform.forward , lerpAmount );
/*

Vector3 p1 = wrenInside.transform.position + rb.velocity.normalized;
Vector3 p2 = wrenInside.transform.position +transform.forward;

Vector3 d = p2 - p1;

Vector3 force = d * rightingMultiplier * (1-Vector3.Dot( rb.velocity.normalized, transform.forward));

wrenInside.physics.rb.AddForceAtPosition(force , wrenInside.transform.position + wrenInside.transform.forward * rightingForwardAmount );


wrenInsi*/


       // Straightens out!
        Vector3 v1 = Vector3.Cross( rb.velocity , transform.forward );
        rb.AddTorque(  v1 * rightingMultiplier );
        }
        
    }

    void OnTriggerEnter(Collider c){
        
        if( c.tag == "Wren" ){
            if(c.attachedRigidbody.gameObject == God.wren.gameObject ){
                wrenInside = God.wren;
            }
        }
    }

    void OnTriggerExit(Collider c){
        wrenInside = null;
    }   
}
