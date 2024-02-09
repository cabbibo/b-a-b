using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class PlaceClusterOnScene : MonoBehaviour
{


    public float verticalOffset;

    // Start is called before the first frame update
    void OnEnable()
    {

        RaycastHit hit;

        LayerMask mask = LayerMask.GetMask("Terrain");


        foreach (Transform t in transform)
        {


            if (Physics.Raycast(t.position + Vector3.up * 100, Vector3.down, out hit, 10000, mask))
            {
                transform.position = hit.point;
                transform.position += Vector3.up * verticalOffset;

            }
        }

    }

}
