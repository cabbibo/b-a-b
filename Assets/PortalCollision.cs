using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WrenUtils;

public class PortalCollision : MonoBehaviour
{

    public Portal portal;

    public bool hasFired;



    public void OnCollisionEnter(Collision c){

        if( c.collider.attachedRigidbody == God.wren.physics.rb && hasFired == false){
            portal.OnCollision(c);
        }

    }

}
