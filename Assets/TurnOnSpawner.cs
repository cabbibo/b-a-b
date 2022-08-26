using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnSpawner : MonoBehaviour
{

    public GameObject go;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter( Collider c ){

        if( God.IsOurWren(c) ){
            go.SetActive(true);
        }
    }

    public void OnTriggerExit( Collider c ){
        if( God.IsOurWren(c) ){
            go.SetActive(false);
        }
    }
}
