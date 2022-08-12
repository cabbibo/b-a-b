using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
public class WrenTargetingSystem : MonoBehaviour
{

    public WrenCameraWork cameraWork;
    public Wren wren;

    public string guidePrefabName = "TargetMark";

    public float maxTargetDistance = 20;

    public GameObject targetMark;
    public bool targeted;

    public int maxTargets;
    
    public GameObject possibleTargetPrefab;
    public GameObject closestTargetPrefab;
    public GameObject[] possibleTargetReps;
    public GameObject closestTargetRep;
    // Update is called once per frame

    public List<Transform> possibleTargets;

    public bool started = false;
    void OnEnable(){

        possibleTargetReps = new GameObject[ maxTargets ];
//        print(possibleTargetPrefab);
        for( int i = 0; i < maxTargets; i++ ){
            possibleTargetReps[i] = GameObject.Instantiate( possibleTargetPrefab );
            possibleTargetReps[i].SetActive(false);
        }
        closestTargetRep  = GameObject.Instantiate( possibleTargetPrefab );
        closestTargetRep.SetActive(false);

        started = true;

    }
    void Update()
    {


        if( started ){
            GetPossibleTargets();
        }

        if( wren.input.triangle > .5f && wren.input.o_triangle < .5f ){
            ToggleTarget();
        }
    }

    public Transform closestTarget;


    void GetPossibleTargets(){

        possibleTargets.Clear();

        Vector3 p = new Vector3();
        float closest = 1000000;
        closestTarget = null;
        for( int i = 0; i< God.targetableObjects.Count; i++ ){
    
            p = Camera.main.WorldToScreenPoint( God.targetableObjects[i].position );

            if( p.x > 0 && p.x < Camera.main.pixelWidth  &&
                p.y > 0 && p.y < Camera.main.pixelHeight &&
                p.z > 0 && p.z < maxTargetDistance ){
                possibleTargets.Add( God.targetableObjects[i] );

                float x = Mathf.Abs( p.x - Camera.main.pixelWidth/2);
                float y = Mathf.Abs( p.y - Camera.main.pixelWidth/2);
                float z = 0;//p.z;

                p.Set( x,y,z);
                if( p.magnitude < closest ){
                    closestTarget = God.targetableObjects[i];
                    closest = p.magnitude;
                    
                }


            }

        }

        for( var i = 0; i < possibleTargetReps.Length; i++ ){
            if( i < possibleTargets.Count ){

                p = Camera.main.transform.position - possibleTargets[i].position;
                possibleTargetReps[i].transform.position =  Camera.main.transform.position - p.normalized * 3;//possibleTargets[i].position;
                possibleTargetReps[i].SetActive( true );
            }else{
                possibleTargetReps[i].SetActive( false );
            }
        }

        if( closestTarget != null ){
            
            p = Camera.main.transform.position -closestTarget.position;
            closestTargetRep.transform.position =  Camera.main.transform.position - p.normalized * 2;//possibleTargets[i].position;
             
            closestTargetRep.SetActive( true );

        }else{
            closestTargetRep.SetActive( false );
        }


    }
    void ToggleTarget(){


        if( !targeted ){
            TryTarget();
        }else{
            ReleaseTarget();
        }
    }

    void TryTarget(){

        print("trying");

        /*
        RaycastHit hit;
        if( Physics.Raycast( Camera.main.transform.position , Camera.main.transform.forward,  out hit ) ){
            Success(hit);
        }else{
            Failed();
        }*/

        if( closestTarget != null ){
            print("letsgo");
            print(closestTarget);
            Success( closestTarget );
        }else{
            Failed();
        }

    }


    void Failed(){
        print("failed");
    }

    void Success( Transform target ){

        cameraWork.objectTargeted = target;
        cameraWork.objectTargetedPosition = Vector3.zero;
        targeted = true;
        targetMark = Realtime.Instantiate( guidePrefabName);
        
            print("letsgo2");
       
        RealtimeTransform rtt = targetMark.GetComponent<RealtimeTransform>();
        rtt.RequestOwnership();

        
        targetMark.transform.position = target.position;// + hit.normal;
        targetMark.transform.rotation = target.rotation;
        targetMark.transform.parent = target;
        //targetMark.transform.LookAt( hit.point + hit.normal );
        //targetMark.transform.Rotate( 90, 0, 0 );

    }
    void Success(RaycastHit hit){

        cameraWork.objectTargeted = hit.transform;
        cameraWork.objectTargetedPosition = hit.transform.InverseTransformPoint(hit.point);
        targeted = true;
        targetMark = Realtime.Instantiate( guidePrefabName);
        
       
        RealtimeTransform rtt = targetMark.GetComponent<RealtimeTransform>();
        rtt.RequestOwnership();

        
        targetMark.transform.position = hit.point + hit.normal;
        targetMark.transform.LookAt( hit.point + hit.normal );
        targetMark.transform.Rotate( 90, 0, 0 );

    }

    void ReleaseTarget(){
        if( cameraWork.objectTargeted ){
            cameraWork.objectTargeted = null;
            targeted = false;
            Realtime.Destroy(targetMark);

        }else{
            print("Something funky here");
        }
    }
}
