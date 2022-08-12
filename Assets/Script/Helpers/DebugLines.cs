using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLines : MonoBehaviour
{

    public int numDebugLines;
    public LineRenderer[] lineRenderers;

    public float forceMultiplyVal = .05f;

    // Start is called before the first frame update
    void Start(){

        lineRenderers = new LineRenderer[numDebugLines];
        for( int i = 0; i < numDebugLines; i++ ){
            GameObject go = new GameObject();
            go.transform.parent = transform;
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.startWidth = .1f;
            lr.endWidth = 0;
            lineRenderers[i] = lr;
        }
        
        
    }
    


    public void SetLine( int id , Vector3 p1 , Vector3 p2 ){
        lineRenderers[id].SetPosition( 0 , p1 );
        lineRenderers[id].SetPosition( 1 , p2 );
    }


      public void SetLine( int id , Vector3 p1 , Vector3 p2 , float startWidth , float endWidth ){
        lineRenderers[id].SetPosition( 0 , p1 );
        lineRenderers[id].SetPosition( 1 , p2 );
        lineRenderers[id].startWidth = startWidth;
        lineRenderers[id].endWidth = endWidth;
    }

    public void SetForceLine( int id , Vector3 p , Vector3 f ){
        lineRenderers[id].SetPosition( 0 , p );
        lineRenderers[id].SetPosition( 1 , p + f * forceMultiplyVal );
    }
}
