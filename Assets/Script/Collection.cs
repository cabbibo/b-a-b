using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Collection : MonoBehaviour
{

    public GameObject collectableHolder;

    public Renderer ring;

    public GameObject[] holders;
    public GameObject[] circles;
    public GameObject[] centers;
    public LineRenderer[] connectionLines;

    public float radius;
    public float startAngle;
    public float endAngle;



    public void Start(){
        OnBeaconPlace();
        OnCollect();
    }


    public void CreateHolders(){

        holders = new GameObject[ God.collectableController.collectables.Length ];
        circles = new GameObject[ God.collectableController.collectables.Length ];
        centers = new GameObject[ God.collectableController.collectables.Length ];
        connectionLines = new LineRenderer[ God.collectableController.collectables.Length ];

        for( int i = 0; i < God.collectableController.collectables.Length; i++ ){

            float val  = (float)i/((float)God.collectableController.collectables.Length-1);
            GameObject holder =GameObject.Instantiate( collectableHolder );
            holders[i] = holder;
            holder.SetActive(true);

            float a  =  ((float)val * ((endAngle-startAngle)/360)) + startAngle/360;

            a *= Mathf.PI * 2;
            holder.transform.parent = transform;
            holder.transform.localPosition = new Vector3( Mathf.Sin( a) , .1f, -Mathf.Cos( a)) * radius;

            circles[i] = holder.transform.GetChild(0).gameObject;
            centers[i] = holder.transform.GetChild(1).gameObject;

            centers[i].GetComponent<MeshFilter>().mesh = God.collectableController.collectables[i].collectedMeshFilter.mesh;
            centers[i].GetComponent<MeshRenderer>().materials = God.collectableController.collectables[i].collectedMeshFilter.transform.GetComponent<MeshRenderer>().materials;

            connectionLines[i] = holder.GetComponent<LineRenderer>();
            connectionLines[i].useWorldSpace = false;
            
        }


        updateRingMat();
        OnBeaconPlace();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void updateRingMat(){
        ring.material.SetInt("_NumCollectables" ,God.collectableController.collectables.Length );
        ring.material.SetFloat("_StartAngle",  2 * Mathf.PI * startAngle/360);
        ring.material.SetFloat("_AngleLength", 2 * Mathf.PI * (endAngle-startAngle)/360);
    }
    public void OnCollect(){

        for( int i = 0; i < God.collectableController.collectables.Length; i ++ ){
            if( God.collectableController.collectablesCollected[i] > 0 ){
                centers[i].SetActive(true);

                print("IM ACTIVE");
                circles[i].GetComponent<MeshRenderer>().material.SetFloat("_Active",2);

                connectionLines[i].material.SetFloat("_On" , 1);
                
                UpdateLineRenderPositions(i);

            }else{
                
                connectionLines[i].material.SetFloat("_On" , .5f);
                circles[i].GetComponent<MeshRenderer>().material.SetFloat("_Active",0);
                centers[i].SetActive(false);
            }
        }

    
        updateRingMat();
    }

    public void OnBeaconPlace(){
        
        RaycastHit hit;
        for( int i = 0; i < holders.Length; i++ ){
            
            if (Physics.Raycast(holders[i].transform.position + Vector3.up * 20, -Vector3.up, out hit)) {

                holders[i].transform.position = hit.point + hit.normal;
                holders[i].transform.LookAt( transform.position , hit.normal);

                UpdateLineRenderPositions(i);
                
            }else{
                print("nothingHit");
            }

        }
    }

    public void UpdateLineRenderPositions(int i){

       // Vector3 position = holders[i].transform.InverseTransformPoint( transform.position );
        Vector3 position = holders[i].transform.InverseTransformPoint( transform.position + Vector3.up * 10);
        Vector3 direction = holders[i].transform.InverseTransformPoint( transform.position ).normalized;
        float magnitude =  holders[i].transform.InverseTransformPoint( transform.position).magnitude;


        connectionLines[i].SetPosition(0, Vector3.zero + direction*5 );
        connectionLines[i].SetPosition(1, Vector3.zero + direction*5 + Vector3.up * 10 );
        connectionLines[i].SetPosition(2,position - direction*5);

        connectionLines[i].SetPosition(3,position - direction*5);

        if( i != 0 ){
         position  =  holders[i-1].transform.InverseTransformPoint( transform.position + Vector3.up * 10);
         direction =  holders[i-1].transform.InverseTransformPoint( transform.position ).normalized;
         magnitude =  holders[i-1].transform.InverseTransformPoint( transform.position).magnitude;
         position = holders[i].transform.InverseTransformPoint( holders[i-1].transform.TransformPoint(position) );
         direction = holders[i].transform.InverseTransformDirection( holders[i-1].transform.TransformDirection(direction) );
         connectionLines[i].SetPosition(3,position - direction*5);
        }



    }


}
