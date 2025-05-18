using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;

public class KeepOnSurface : MonoBehaviour
{
    public Transform anchor;


    public float liftForce   = 50;
    public float rotateForce = 3;
    public float matchAnchorRotationForce;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    private Rigidbody          rb;
    private SampleHeightHelper _sampleHeightHelper = new();

    public void OnEnable()
    {
        transform.position = anchor.position;
    }

    private void FixedUpdate()
    {
        if ( rb == null ) {
            rb = GetComponent<Rigidbody>();
        }

        if ( anchor != null ) {

            _sampleHeightHelper.Init( transform.position , 10 , true );
            Vector3 displacement;
            Vector3 normal;
            Vector3 waterSurfaceVel;

            _sampleHeightHelper.Sample( out displacement , out normal , out waterSurfaceVel );


            print( displacement );
            float height = OceanRenderer.Instance.SeaLevel + displacement.y;
            float dif = height - transform.position.y;

            rb.AddForce( Vector3.up * liftForce * dif );

            if ( dif > .5f ) {
                //rb.position = new Vector3( rb.position.x , height + .5f , rb.position.z );
            }


            var dif2 = anchor.position - transform.position;
            dif2.y = 0;


            rb.AddForce( dif2 * 3 );

            MatchRotationForce( transform.up , normal , rotateForce );

            MatchRotationForce( transform.forward , anchor.forward , matchAnchorRotationForce );
            MatchRotationForce( transform.right , anchor.right , matchAnchorRotationForce );


        }
    }

    private void MatchRotationForce( Vector3 from , Vector3 to , float force )
    {

        var axis = Vector3.Cross( from , to );
        float angle = Mathf.Acos( Mathf.Clamp( Vector3.Dot( from , to ) , -1f , 1f ) );
        var torque = axis.normalized * angle * force;
        rb.AddTorque( torque , ForceMode.VelocityChange );
    }
}