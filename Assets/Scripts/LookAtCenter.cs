using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LookAtCenter : MonoBehaviour
{
    public Transform center;

    // Update is called once per frame
    void Update()
    {


        if (center == null)
        {
            transform.LookAt(Vector3.zero);
        }
        else
        {
            transform.LookAt(center.position);
        }
    }


}
