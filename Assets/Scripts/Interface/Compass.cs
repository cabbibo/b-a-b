using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{

    public DebugLines lines;
    public bool isLocal;

    public float maxDist;
    public float minDist;



    public WrenMaker maker;
    public Wren wren;


    // Start is called before the first frame update
    void OnEnable()
    {
    
        GameObject go = GameObject.FindGameObjectWithTag("Realtime");
       
       if( go != null ){
           maker = go.GetComponent<WrenMaker>();
           maker.wrens.Add(wren);
       }
        
    }

    void OnDisable()
    {

    
        GameObject go = GameObject.FindGameObjectWithTag("Realtime");
       
       if( go != null ){
           maker = go.GetComponent<WrenMaker>();
           maker.wrens.Remove( wren );;
       }
        
    }

    // Update is called once per frame
    void Update()
    {
        if( isLocal ){

            int index= 0;
            foreach( Wren wren in maker.wrens ){
                if( wren.gameObject != gameObject){

                    Vector3 dif = wren.transform.position - transform.position;
                    float dist = dif.magnitude; 

                    lines.SetLine( index , transform.position , wren.transform.position ,  100 / dist , 0 );
                    index ++;

                }
            }
        }
    }
}
