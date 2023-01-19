using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;
public class StartingPlatform : MonoBehaviour
{
    public RingSet race;
    public bool start;

    void OnTriggerEnter(Collider c ){
        if( c.attachedRigidbody.gameObject == God.wren.gameObject){  
            if( start ){
                race.StartPlatformHit();
            }else{
                race.EndPlatformHit();
            }

        }
    
    }

    void OnTriggerExit(Collider c){
        if( c.attachedRigidbody.gameObject == God.wren.gameObject){

            if( start ){
                race.StartPlatformExit();
            }else{
                race.EndPlatformExit();
            }
        }

    }
}
