using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsTarget : MonoBehaviour
{

    public float pullForce;
    public Vector3 target;
    public bool pulling;

    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {

        rigidbody = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if( pulling ){
            rigidbody.AddForce( -(transform.position - target) * pullForce);
        }
        
    }
}
