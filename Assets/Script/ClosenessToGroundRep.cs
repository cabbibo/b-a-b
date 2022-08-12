using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosenessToGroundRep : MonoBehaviour
{

    public Wren wren;
    public LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = wren.transform.position + wren.physics.groundDirection * wren.physics.distToGround;
        line.SetPosition( 0 , transform.position);
        line.SetPosition( 1, wren.transform.position );
    }
}
