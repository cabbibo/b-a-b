using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenSlipStream : MonoBehaviour
{

    public GameObject slipStreamPrefab;
    public float streamPlaceDelta;
    
    public Wren wren;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        
        if( wren.input.left1 > .5f && wren.physics.onGround == false ){
            print("itsWorking");
            CheckStreamPlace();
        }
    }


    public float lastDist;
    public Vector3 lastPosition;
    void CheckStreamPlace(){


        Vector3 dif = lastPosition - transform.position;

        if( dif.magnitude > streamPlaceDelta ){
            PlaceNewPoint();
        }



    }

    void PlaceNewPoint(){
        lastPosition = transform.position;
        Instantiate( slipStreamPrefab , transform.position , transform.rotation);
    }
}
