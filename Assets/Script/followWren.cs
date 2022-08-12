using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followWren : MonoBehaviour
{


    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( God.wren ){ transform.position = God.wren.transform.position + offset; }
    }
}
