using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDF : MonoBehaviour
{

    public Transform[] objects;

    public Transform measurer;

    public int closestObject;
    public float closestDistance;
    public Vector3 normal;

    public DebugLines lines;


    public float x;
    public float y;
    public float z;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetSDFInfo( measurer.position);
    }


    void GetSDFInfo(Vector3 p){

        Vector2 d = map( p );


        closestDistance = d.x;
        closestObject = (int)d.y;

        float eps = .1f;

        x = map(p + Vector3.right * eps).x- map(p-Vector3.right * eps).x;
        y = map(p+Vector3.up * eps).x - map(p-Vector3.up * eps).x;
        z = map(p+Vector3.forward * eps).x - map(p-Vector3.forward * eps).x;
        
        normal = new Vector3(x,y,z );
        
        normal = normal.normalized;


        lines.SetLine( 0 , p , p - normal * closestDistance );
     


    }

    public Vector2 map( Vector3 p ){

        float cD = 1000000;
        float cID = 1000000;

        for( int i = 0; i < objects.Length; i++ ){

            Vector3 dif = objects[i].InverseTransformPoint( p );

            float dist = dif.magnitude - .5f;
            dist *= objects[i].localScale.x;

            if( Mathf.Abs(dist) < Mathf.Abs(cD) ){
                cD = dist;
                cID = i;
            }

        }

        return new Vector2( cD , cID );

    }
}
