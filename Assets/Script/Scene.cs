using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



namespace WrenUtils
{

    public class Scene : MonoBehaviour
    {


        public UnityEvent OnLoadEvent;
        public string name;
        public Material skyboxMaterial;
        public string physicsParameters;

        public Portal[] portals;

        public Transform baseStartPosition;

        public bool isDemo;
        public bool startInFlight;


        public void SceneLoaded(int newSceneID, bool loadedFromPortal)
        {

            print("7) Wren Scene Loaded");

            //print("scene calling scene Loaded");


            Camera.main.gameObject.GetComponent<LerpTo>().enabled = true;
            God.wren.state.inInterface = false;
            God.wren.airInterface.Toggle(false);
            God.wren.fullInterface.Toggle(false);

            if (newSceneID == 1)
            {
                God.wren.inEther = true;
            }
            else
            {

                God.wren.inEther = false;
            }

            God.wren.canMove = true;


            God.skyboxUpdater.UpdateSkybox(skyboxMaterial);

            God.currentScene = this;

            // Sets up our demo info
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].demo = isDemo;
            }

            Vector3 startPos = baseStartPosition.position;


            if (God.wren != null)
            {

                //                print("wren exists");

                God.wren.parameters.Load(physicsParameters);


                // If we dont load from the portal, we grab the last saved position!
                // Otherwise we use the portal!
                if (loadedFromPortal == false)
                {
                    // loading from last position
                    print("Setting from last position");
                    print(God.state.lastPosition);
                    startPos = God.state.lastPosition;
                }
                else
                {
                    if (God.state.currentBiomeID >= 0)
                    {
                        print("totalPortals");
                        print(portals.Length);

                        if (God.state.currentBiomeID >= portals.Length)
                        {
                            startPos = baseStartPosition.position;
                            God.state.SetCurrentBiome(-1);
                        }
                        else
                        {

                            God.state.SetLastPosition(portals[God.state.currentBiomeID].startPoint.position);
                            print("Getting Correct Biome");
                            print(portals[God.state.currentBiomeID].startPoint);
                            print(portals[God.state.currentBiomeID].startPoint.position);

                            // return / spawn at gate that is our current biome!
                            // when bird dies, we respawn at our first starting position
                            startPos = portals[God.state.currentBiomeID].startPoint.position;
                        }



                    }
                    else
                    {
                        print("helllllo");
                        God.wren.startingPosition.position = God.state.lastPosition;

                    }
                }

                God.wren.SetFullPosition(startPos);

                if (startInFlight)
                {
                    God.wren.state.TakeOff();
                }

            }




            LerpTo lt = Camera.main.GetComponent<LerpTo>();
            lt.enabled = true;



            OnLoadEvent.Invoke();





        }







    }
}
