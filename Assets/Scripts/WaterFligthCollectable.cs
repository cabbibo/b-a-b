using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using WrenUtils;

public class WaterFligthCollectable : MonoBehaviour
{

    public CollectablePoint pool1;
    public CollectablePoint pool2;
    public CollectablePoint pool3;

    public bool pool1Collected;
    public bool pool2Collected;
    public bool pool3Collected;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider c){



    


        bool hasCollected = false;
        CollectablePoint pool = null;
        Carryable carryable = null;
        GameObject go = null;
        PullTowardsTarget  ptt = null;

        if( c.attachedRigidbody != null ){

        if( c.attachedRigidbody.gameObject.name == "MountainPoolWater1"){
            pool = pool1;
            hasCollected = true;
            go = c.attachedRigidbody.gameObject;
            carryable = go.GetComponent<Carryable>();
            ptt = go.GetComponent<PullTowardsTarget>();
        }



        if( c.attachedRigidbody.gameObject.name == "MountainPoolWater2"){
            pool = pool2;
            hasCollected = true;
            go = c.attachedRigidbody.gameObject;
            carryable = go.GetComponent<Carryable>();
            ptt = go.GetComponent<PullTowardsTarget>();
        }



        if( c.attachedRigidbody.gameObject.name == "MountainPoolWater3"){
            pool = pool3;
            hasCollected = true;
            go = c.attachedRigidbody.gameObject;
            carryable = go.GetComponent<Carryable>();
            ptt = go.GetComponent<PullTowardsTarget>();
        }

        
      



        if( c.attachedRigidbody == God.wren.physics.rb ){

            print("Wren entered");

            foreach( Carryable carry in God.wren.carrying.CarriedItems ){


                if( carry.gameObject.name == "MountainPoolWater1"){
                     pool = pool1;
                    hasCollected = true;
                    go = carry.gameObject;
                    carryable = carry;
                    ptt = go.GetComponent<PullTowardsTarget>();
                    break;
                }

                  if( carry.gameObject.name == "MountainPoolWater2"){
                     pool = pool2;
                    hasCollected = true;
                    go = carry.gameObject;
                    carryable = carry;
                    ptt = go.GetComponent<PullTowardsTarget>();
                    break;
                }


                if( carry.gameObject.name == "MountainPoolWater3"){
                     pool = pool3;
                    hasCollected = true;
                    go = carry.gameObject;
                    carryable = carry;
                    ptt = go.GetComponent<PullTowardsTarget>();
                    break;
                }

            }

        }


          if( hasCollected && pool != null && ptt != null && go != null && carryable != null ){
            pool.OnCollect();
            ptt.target = pool.transform.position + Vector3.up * 10;
            ptt.pulling = true;
            if( God.wren ){ God.wren.carrying.DropIfCarrying( carryable ); }
            go.GetComponent<Rigidbody>().drag = 10;
            WaterCarryable wc = go.GetComponent<WaterCarryable>();
            //wc.atSpawnPoint = true;
            wc.respawnTransform = pool.transform;
            wc.respawnOffset =  Vector3.up * 10;
            //wc.OnStartFilling();
            //wc.
        }

        }
        

    }
}
