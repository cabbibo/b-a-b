using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class ScenePostSettings : MonoBehaviour
{

    public Material skyboxMaterial;

    public Material mainModelMaterial;
    public Material birdMaterial;
    public Material terrainMaterial;


    public bool fog;
    public bool depthOfField;
    public bool chromaticAberration;
    public bool bloom;
    public bool colorGrading;
    public bool vignette;

    public float fogIntensity;
    public float fogHeightPower;





    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set()
    {
        God.skyboxUpdater.UpdateSkybox(skyboxMaterial);

    }

}
