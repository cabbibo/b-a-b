using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SafeCameraPositionSet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        Shader.SetGlobalVector("_SafeCameraPosition", transform.position);
    }
}
