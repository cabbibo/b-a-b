using UnityEngine;
using EasyButtons;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClickPlacer : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject prefab;

    public Vector2 scaleRange      = new(1f , 1f);
    public Vector2 scaleRandomness = new(1f , 1f);

    public Vector2 scaleDragSizeRange = new(0.1f , 10f);

    public string[] layers;

    public Vector3 offset;

    public Vector3 rotationalOffset;

    public Vector3 rotationRandomness;

    public Vector3 offsetRandomness;


    public float upImportance;

    public bool CastMouseRay( Ray ray , out RaycastHit hit )
    {
        return Physics.Raycast( ray , out hit , Mathf.Infinity , LayerMask.GetMask( layers ) );
    }


    [Button( "ReplaceObjectsDown" )]
    public void ReplaceObjectsDown()
    {
        Undo.RegisterChildrenOrderUndo( transform , "Replace Objects Down" );

        for ( int i = 0; i < transform.childCount; i++ ) {
            var child = transform.GetChild( i );

            if ( child == null ) {
                continue;
            }

            var p = child.position;
            var n = child.up;
            var f = child.forward;
            child.position = p + n * 1110.01f; // Move up slightly to avoid z-fighting

            var ray = new Ray( p , -n );

            RaycastHit hit;

            if ( Physics.Raycast( ray , out hit , Mathf.Infinity , LayerMask.GetMask( layers ) ) ) {
                child.transform.position = hit.point;
                child.transform.rotation = Quaternion.LookRotation( f , hit.normal );


                child.transform.position += child.transform.up * child.transform.localScale.y *
                                            (offset.y + Random.Range( -offsetRandomness.y , offsetRandomness.y ));
                child.transform.position += child.transform.right * child.transform.localScale.x *
                                            (offset.x + Random.Range( -offsetRandomness.x , offsetRandomness.x ));
                child.transform.position += child.transform.forward * child.transform.localScale.z *
                                            (offset.z + Random.Range( -offsetRandomness.z , offsetRandomness.z ));

            }

        }


    }


    public void PlaceObject( Vector3 point , Vector3 normal , Vector3 forward )
    {
        Undo.RegisterChildrenOrderUndo( transform , "Placed Object" );

        float forwardMagnitude = forward.magnitude;


        var go = (GameObject)PrefabUtility.InstantiatePrefab( prefab );

        go.transform.parent = transform;
        go.transform.position = point;


        var upVector = normal;
        upVector = Vector3.Slerp( upVector , Vector3.up , upImportance );

        forward = Vector3.ProjectOnPlane( forward , upVector );

        go.transform.rotation = Quaternion.LookRotation( forward , upVector );

        go.transform.Rotate(
            rotationalOffset + new Vector3(
                Random.Range( -rotationRandomness.x , rotationRandomness.x ) ,
                Random.Range( -rotationRandomness.y , rotationRandomness.y ) ,
                Random.Range( -rotationRandomness.z , rotationRandomness.z )
            )
        );


        float normalizedSizeInRange = Mathf.InverseLerp( scaleDragSizeRange.x , scaleDragSizeRange.y , forwardMagnitude );
        float finalScale = Mathf.Lerp( scaleRange.x , scaleRange.y , normalizedSizeInRange );
        finalScale *= Random.Range( scaleRandomness.x , scaleRandomness.y );


        go.transform.localScale = Vector3.one * finalScale;

        go.transform.position += go.transform.up * go.transform.localScale.y *
                                 (offset.y + Random.Range( -offsetRandomness.y , offsetRandomness.y ));
        go.transform.position += go.transform.right * go.transform.localScale.x *
                                 (offset.x + Random.Range( -offsetRandomness.x , offsetRandomness.x ));
        go.transform.position += go.transform.forward * go.transform.localScale.z *
                                 (offset.z + Random.Range( -offsetRandomness.z , offsetRandomness.z ));

    }

    public void Reset()
    {

        while (transform.childCount > 0) DestroyImmediate( transform.GetChild( 0 ).gameObject );
    }
#endif
}