using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SetGlobalCameraInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnEnable()
    {

        Shader.SetGlobalVector("_MainCameraPos", transform.position);
        Shader.SetGlobalVector("_MainCameraForward", transform.forward);
        Shader.SetGlobalVector("_MainCameraRight", transform.right);
        Shader.SetGlobalVector("_MainCameraUp", transform.up);

    }

    // Update is called once per frame
    void Update()
    {

        Shader.SetGlobalVector("_MainCameraPos", transform.position);
        Shader.SetGlobalVector("_MainCameraForward", transform.forward);
        Shader.SetGlobalVector("_MainCameraRight", transform.right);
        Shader.SetGlobalVector("_MainCameraUp", transform.up);

    }
}
