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



        
        God.wren.parameters.Load(physicsParameters);


        // return / spawn at gate that is our current biome!
        // when bird dies, we respawn at our first starting position
        God.wren.startingPosition = startPositions[God.sceneController.biome+1];
      /*  if( God.sceneController.oldScene < startPositions.Length){
            God.wren.startingPosition = startPositions[God.sceneController.oldScene];
        }else{
            God.wren.startingPosition = startPositions[0];
        }*/


       
        God.wren.FullReset();

        LerpTo lt = Camera.main.GetComponent<LerpTo>();
        lt.enabled = true;

        
        OnLoadEvent.Invoke();

    }



    

    

}
}
