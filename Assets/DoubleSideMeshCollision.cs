using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[ExecuteAlways]
public class DoubleSideMeshCollision : MonoBehaviour
{
    public MeshCollider meshCollider;

    // Start is called before the first frame update
    private void OnEnable()
    {

//        print( meshCollider );
        var mesh = meshCollider.sharedMesh;
        mesh.SetIndices( mesh.GetIndices( 0 ).Concat( mesh.GetIndices( 0 ).Reverse() ).ToArray() , MeshTopology.Triangles , 0 );
        meshCollider.sharedMesh = mesh;

    }

    // Update is called once per frame
    private void Update()
    {

    }
}