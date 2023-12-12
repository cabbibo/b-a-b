using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



namespace WrenUtils
{

    [ExecuteAlways]
    public class Scene : MonoBehaviour
    {


        public UnityEvent OnLoadEvent;
        public string name;
        public Material skyboxMaterial;
        public string physicsParameters;

        public Portal[] portals;

        public Transform baseStartPosition;

        public bool isDemo;



        // Start is called before the first frame update
        void OnEnable()
        {
            SceneLoaded();
        }



        public void SceneLoaded()
        {

            RenderSettings.skybox = skyboxMaterial;
            God.skyboxUpdater.material = skyboxMaterial;
            God.skyboxUpdater.UpdateSkybox();

            God.currentScene = this;

            // Sets up our demo info
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].demo = isDemo;
            }

            if (God.wren != null)
            {

                God.wren.parameters.Load(physicsParameters);


                if (God.sceneController.biome >= 0)
                {
                    // return / spawn at gate that is our current biome!
                    // when bird dies, we respawn at our first starting position
                    God.wren.startingPosition = portals[God.sceneController.biome].startPoint;

                }
                else
                {
                    God.wren.startingPosition = baseStartPosition;
                }

                God.wren.FullReset();
            }




            /*  if( God.sceneController.oldScene < startPositions.Length){
                  God.wren.startingPosition = startPositions[God.sceneController.oldScene];
              }else{
                  God.wren.startingPosition = startPositions[0];
              }*/




            LerpTo lt = Camera.main.GetComponent<LerpTo>();
            lt.enabled = true;


            OnLoadEvent.Invoke();


            God.sceneController.OnSceneFinishedLoading(this);

        }







    }
}
