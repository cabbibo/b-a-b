using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using WrenUtils;

public class WrenCompass : WrenInterface
{

    public Texture2D[] icons;
    public LineRenderer[] lines;

    public GameObject wrenPointerPrefab;

    public GameObject[] pointers;
    public Renderer[] renderers;

    public int oWrenCount;

    public Wren wren;

    public bool active;


    public void OnEnable()
    {   
        active = true;
        if( wren == null ){
            wren = God.wren;
        }
    }


    public  void OnDisable(){
        active = false;
    }
    // Update is called once per frame
    public void Update()
    {

            if( God.wrens.Count != oWrenCount ){
                WrensChanged();
            }

     
            transform.position = God.wren.transform.position;
        


      
            int id = 0;
            Vector3 v2 = new Vector3();
            foreach( Wren w in God.wrens){

                if( w != wren ){

                    v2 = w.transform.position - transform.position;

                    print( v2 );
                    
                    
                    float v = v2.magnitude;

                

                    float closeness = Mathf.Clamp( 1000 / v , 0 , 5 )/5;
                    print(closeness);
                    pointers[id].transform.position = transform.position + v2.normalized * 8 + v2.normalized * 2 + v2.normalized * 5* closeness;
                    lines[id].SetPosition(0, transform.position + v2.normalized * 8);
                    lines[id].SetPosition(1, pointers[id].transform.position);
                    lines[id].startWidth = closeness * 2;
                    lines[id].endWidth = 0;


                    
                    id ++;
                }
            }
        

    }

    public void WrensChanged(){

        print( "changing wren");

        for(int i = 0; i < pointers.Length; i++ ){
            Destroy( pointers[i]);
        }

        icons = new Texture2D[God.wrens.Count-1];
        lines = new LineRenderer[God.wrens.Count-1];
        pointers = new GameObject[God.wrens.Count-1];
        renderers = new Renderer[God.wrens.Count-1];

        int id = 0;
        foreach( Wren w in God.wrens){

            if( w != God.wren ){
            pointers[id] = GameObject.Instantiate( wrenPointerPrefab );
           
            pointers[id].transform.parent = transform;
            pointers[id].transform.position = Vector3.zero;
            pointers[id].transform.rotation = Quaternion.identity;
            pointers[id].SetActive(true);


            icons[id] = w.colors.icon;
            lines[id] = pointers[id].GetComponent<LineRenderer>();
            renderers[id] = pointers[id].GetComponent<Renderer>();

            renderers[id].material.SetTexture("_MainTex", icons[id]);
            lines[id].material.SetTexture("_MainTex", icons[id]);

            id ++;
            }
        }


        oWrenCount = God.wrens.Count;


    }

    public void UpdateColors(){

    }
}
