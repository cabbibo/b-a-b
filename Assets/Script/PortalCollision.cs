using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalCollision : MonoBehaviour
{

    public ParticleSystem successParticles;

    public bool hasFired;
    public int sceneID;

    public Collider collider;


    public int biome;
    
   


    public void OnCollisionEnter(Collision c){



        if( c.collider.attachedRigidbody == God.wren.physics.rb && hasFired == false){

            hasFired = true;
            //successParticles.Play();
            God.sceneController.biome = biome;
            God.sceneController.LoadScene(sceneID);
            if( sceneID == 0 ){
                God.wren.inEther = true;
            }else{
                God.wren.inEther = false;
            }

            
            //God.wren.physics.rb.position += transform.forward * 2;

            collider.enabled = false;
        }

    }

}
