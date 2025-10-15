using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WrenUtils;

[ExecuteAlways]
public class Portal : MonoBehaviour
{
    public int sceneID;

    public bool demo;

    public bool isOn;

    public int       biome;
    public int       questID;
    public Transform startPoint;
    public Transform portalBase;

    public PortalCollision portalCollision;

    public Collider collider;

    public Collision collision;

    public Transform collisionPointFront;
    public Transform collisionPointBack;

    public MeshRenderer gateRenderer;
    public MeshRenderer portalRenderer;

    public float portalShownAmount;


    public Transform      collisionPoint;
    public ParticleSystem successParticles;


    public void OnEnable()
    {
        portalCollision.portal = this;
    }


    private MaterialPropertyBlock portalMPB;

    public void Update()
    {

        if ( portalMPB == null ) {
            portalMPB = new MaterialPropertyBlock();
        }


        portalRenderer.GetPropertyBlock( portalMPB );
        portalMPB.SetFloat( "_PortalOpen" , isOn ? 1 : 0 );
        portalMPB.SetVector( "_BasePosition" , portalBase.position );
        portalMPB.SetFloat( "_PortalAmountShown" , portalShownAmount );
        portalMPB.SetFloat( "_OpenAmount" , portalShownAmount );
        portalMPB.SetTexture( "_OtherWorldCubemap" , God.sceneController.cubemaps[sceneID] );
        portalRenderer.SetPropertyBlock( portalMPB );
    }


    public void OnCollision( Collision c )
    {

        float l1 = Vector3.Distance( God.camera.transform.position , collisionPointFront.position );
        float l2 = Vector3.Distance( God.camera.transform.position , collisionPointBack.position );

        if ( l1 < l2 ) {
            collisionPoint = collisionPointFront;
        } else {
            collisionPoint = collisionPointBack;
        }

        collision = c;


        if ( demo ) {
            print( "Testing" );
            God.sceneController.EndDemo( this );
        } else {
            God.sceneController.LoadSceneFromPortal( this );
        }

        collider.enabled = false;

    }


    public void OpenPortal()
    {
        isOn = true;
        collider.enabled = true;
        portalRenderer.enabled = true;
        portalShownAmount = 0;
    }

    public void SetPortalFull()
    {

        isOn = true;
        print( gameObject.name + " SETTING FULL" );
        collider.enabled = true;
        portalRenderer.enabled = true;
        portalShownAmount = 1;
    }


    public void SetPortalOff()
    {
        isOn = false;
        collider.enabled = false;
        portalRenderer.enabled = false;
        portalShownAmount = 0;

    }
}