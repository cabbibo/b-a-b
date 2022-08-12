using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectRemotePoint : MonoBehaviour
{
    
    public CollectablePoint collect;

    public void OnTriggerEnter(Collider c){
        print( c.tag );

        print("Collecting remote point");
        if( collect.collected == false ){
            collect.OnCollect();
        }
    }
}
