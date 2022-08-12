using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{



    public List<Transform> leftWing;
    private List<Vector3> basePositions;
    private List<Quaternion> baseRotations;



    // Start is called before the first frame update
    void Start()
    {

        basePositions = new List<Vector3>();
        baseRotations = new List<Quaternion>();

        foreach( Transform t in leftWing){
            basePositions.Add(t.localPosition);
            baseRotations.Add(t.localRotation);
        } 

    }


    private Vector3 v;
    private Quaternion r;
    // Update is called once per frame
    void Update()
    {

        int id = 0;
        foreach( Transform t in leftWing ){

            print("hmmm");

            v = basePositions[id];
            r = baseRotations[id];

            r  = Quaternion.AngleAxis(Mathf.Sin( Time.time ) * 30, Vector3.up) * r;

            t.localPosition = v;
            t.localRotation = r;

            id ++;
        }
        
    }

}
