using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class SetSkyboxLight : MonoBehaviour
{

    public Scene scene;


    // Update is called once per frame
    void Update()
    {
        scene.skyboxMaterial.SetVector("_LightDir", transform.forward);
    }
}
