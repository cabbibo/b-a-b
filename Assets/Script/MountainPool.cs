using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class MountainPool : MonoBehaviour
{

    public WaterCarryable water;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider c){
        if( c.attachedRigidbody != null && God.wren != null ){
        if( c.attachedRigidbody == God.wren.physics.rb ){
            God.wren.carrying.PickUpItem( water.gameObject );
            water.OnStartFilling();// = true;

        }}
    }


    public void OnTriggerExit( Collider c ){
if( c.attachedRigidbody != null && God.wren != null ){
        if( c.attachedRigidbody == God.wren.physics.rb ){
            water.OnStartDrying();
        }
}

    }
}
