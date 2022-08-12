using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackWren : MonoBehaviour
{
    

    public float forceTowardsWren;
    public float maxLength = 200;


    Rigidbody rigidbody;
    // Update is called once per frame
    void Update()
    {
        if( rigidbody == null ){ rigidbody = GetComponent<Rigidbody>(); }
        Wren wren = God.ClosestWren(transform.position);
        if( wren ){
            Vector3 delta = wren.transform.position - transform.position;

            if( delta.magnitude < maxLength ){
            rigidbody.AddForce( delta * forceTowardsWren );
            }

        }
    }
}
