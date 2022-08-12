using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlantArea : MonoBehaviour
{

    public GameObject[] plantPrefabs;

    public bool eventAdded;

    public GameObject collider;

    // Start is called before the first frame update
    void Awake()
    {
        eventAdded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if( God.wren && eventAdded == false ){
            AddEvent();
        }
    }


    void AddEvent(){
        God.wren.waterController.dropHitEvent.AddListener( DropHit );
        eventAdded = true;
    }

    void DropHit( Vector3 p , GameObject g){

        if( g == collider ){
            GameObject go = Instantiate( plantPrefabs[Random.RandomRange(0,plantPrefabs.Length)] , p , Quaternion.identity );
            go.transform.parent = this.transform;
            go.transform.localScale *= Random.Range( 2.1f,6.2f);
        }
    }
}
