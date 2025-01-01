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


    public Gradient sunGradient;
    public Gradient moonGradient;







    public bool mainPost;
    public bool glitch;
    public bool fog;
    public bool depthOfField;
    public bool chromaticAberration;
    public bool bloom;
    public bool colorGrading;
    public bool vignette;

    public bool splatEffect;



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

        God.postController.mainPost = mainPost;
        God.postController.glitchEffect = glitch;
        God.postController.fogEffect = fog;
        God.postController.depthOfField = depthOfField;
        God.postController.chromaticAberration = chromaticAberration;
        God.postController.bloom = bloom;
        God.postController.colorGrading = colorGrading;
        God.postController.vignette = vignette;
        God.postController.splatEffect = splatEffect;

    }

}
