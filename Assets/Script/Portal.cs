using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using WrenUtils;

public class Portal : MonoBehaviour
{

    public int sceneID;

    public bool demo;



    public int biome;
    public Transform startPoint;

    public PortalCollision portalCollision;

    public Collider collider;

    public Collision collision;

    public Transform collisionPointFront;
    public Transform collisionPointBack;





    public Transform collisionPoint;
    public ParticleSystem successParticles;



    public void OnEnable()
    {
        portalCollision.portal = this;
    }



    public void OnCollision(Collision c)
    {

        float l1 = Vector3.Distance(God.camera.transform.position, collisionPointFront.position);
        float l2 = Vector3.Distance(God.camera.transform.position, collisionPointBack.position);

        if (l1 < l2)
        {
            collisionPoint = collisionPointFront;
        }
        else
        {
            collisionPoint = collisionPointBack;
        }

        collision = c;
        if (demo)
        {
            God.sceneController.EndDemo(this);
        }
        else
        {
            God.sceneController.LoadSceneFromPortal(this);
        }




        collider.enabled = false;


    }

}
