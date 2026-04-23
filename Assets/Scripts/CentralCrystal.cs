using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CentralCrystal : MonoBehaviour
{
    

    public void OnEnable()
    {

        God.targetableObjects.Add(this.transform);
    }

    public void OnDisable()
    {

        God.targetableObjects.Remove(this.transform);
    }



   public void OnTriggerEnter(Collider c){

        if( c.attachedRigidbody == God.wren.physics.rb){
            God.targetableObjects.Remove(this.transform);
        }

    }


}
