using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SetGlobalLightDir : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        Shader.SetGlobalVector( "_LightDir" , transform.forward );

    }
}