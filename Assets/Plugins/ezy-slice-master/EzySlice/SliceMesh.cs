using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;


[ExecuteAlways]
public class SliceMesh : MonoBehaviour
{

    public GameObject objectToSlice; // non-null
    public Transform cutPlane;
    public MeshFilter outputMesh;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        
        SlicedHull hull = Slice( cutPlane.position , cutPlane.up );

        outputMesh.mesh = hull.upperHull;
    }

/**
 * Example on how to slice a GameObject in world coordinates.
 */
public SlicedHull Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection) {

    
	return objectToSlice.Slice(planeWorldPosition, planeWorldDirection);
}
    // Update is called once per frame
    void Update()
    {
        
    }
}
