using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpeedTrapHit : MonoBehaviour
{
 
 
    public SpeedTrap speedTrap;
    void OnTriggerEnter(Collider c){

        if( God.IsOurWren( c ) ){
            speedTrap.OnHit();
        }

    }
}
