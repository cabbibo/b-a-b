using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace WrenUtils{
public class Scene : MonoBehaviour
{


    public UnityEvent OnLoadEvent;
    public string name;
    public Material skyboxMaterial;
    public string  physicsParameters;

    public Transform[] startPositions;



    // Start is called before the first frame update
    void Start()
    {
        SceneLoaded();
    }


    public void SceneLoaded(){

         RenderSettings.skybox = skyboxMaterial;

         print( God.wren );
         print( physicsParameters );

         print("SETTING UP Position HERE!");

        
        God.wren.parameters.Load(physicsParameters);

        print( God.sceneController.oldScene );
        if( God.sceneController.oldScene < startPositions.Length){
            God.wren.startingPosition = startPositions[God.sceneController.oldScene];
        }else{
            God.wren.startingPosition = startPositions[0];
        }
        print(God.wren.startingPosition);

        God.wren.FullReset();

        LerpTo lt = Camera.main.GetComponent<LerpTo>();
        lt.enabled = true;

        
        OnLoadEvent.Invoke();

    }

    

}
}
