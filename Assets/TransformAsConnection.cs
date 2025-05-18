using System.Collections;
using System.Collections.Generic;
using IMMATERIA;
using UnityEngine;


[ExecuteAlways]
public class TransformAsConnection : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public float     width  = 1;
    public float     height = 1;

    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        var dif = from.position - to.position;
        transform.position = Vector3.Lerp( from.position , to.position , .5f );
        transform.LookAt( to.position );
        transform.localScale = new Vector3( width , height , dif.magnitude );

    }
}