using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PlaceOnTerrain : MonoBehaviour
{

    public float verticalOffset;
    // Start is called before the first frame update
    void OnEnable()
    {

RaycastHit hit;

LayerMask mask = LayerMask.GetMask("Terrain");



if ( Physics.Raycast (transform.position + Vector3.up * 100 , Vector3.down , out hit, 10000, mask) )
{
       transform.position = hit.point;
        transform.position += Vector3.up * verticalOffset;  

 }
    
    }
}
