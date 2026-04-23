using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class AttackWren : MonoBehaviour
{
    public float forceTowardsWren;
    public float maxLength = 200;


    private Rigidbody rigidbody;

    // Update is called once per frame
    private void Update()
    {
        if ( rigidbody == null ) {
            rigidbody = GetComponent<Rigidbody>();
        }

        var wren = God.ClosestWren( transform.position );

        if ( wren ) {
            //print( "helllo" );
            var delta = wren.transform.position - transform.position;

            if ( delta.magnitude < maxLength ) {

                // print( "helllo2" );
                rigidbody.AddForce( delta * forceTowardsWren );
            }

        }
    }
}