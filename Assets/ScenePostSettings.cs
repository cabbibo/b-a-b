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

    public PostParameters postParameters;



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
        if (God.wren != null)
        {
            // God.wren.bird.SetMaterial(birdMaterial);
            God.wren.bird.featherMaterial = birdMaterial;
        }

        God.postController.SetPostParameters(postParameters);


    }

}
