using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ChangeSkyboxFade : MonoBehaviour
{


    public Material skyboxMat;

    public float value;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        skyboxMat.SetFloat("_Fade",value);
    }
}
