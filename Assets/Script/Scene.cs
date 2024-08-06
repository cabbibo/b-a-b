using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Crest;



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


        public void SceneLoaded(int newScene, bool loadedFromPortal)
        {



            // Set WrenState
            God.wren.state.inInterface = false;
            God.wren.airInterface.Toggle(false);
            God.wren.fullInterface.Toggle(false);

            if (newScene == 1)
            {
                God.wren.inEther = true;
                Camera.main.GetComponent<UnderwaterRenderer>().enabled = false;
            }
            else
            {
                God.wren.inEther = false;
                Camera.main.GetComponent<UnderwaterRenderer>().enabled = true;
            }


            God.skyboxUpdater.UpdateSkybox(skyboxMaterial);

            // Sets up our demo info
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].demo = isDemo;
            }


            Vector3 startPos = baseStartPosition.position;

            if (God.wren != null)
            {

                God.wren.parameters.Load(physicsParameters);

                SetWrenStartPosition(loadedFromPortal);

                if (startInFlight)
                {
                    God.wren.state.TakeOff();
                }

            }



            OnLoadEvent.Invoke();


        }


        public void SetWrenStartPosition(bool loadedFromPortal)
        {


            Vector3 startPos = new Vector3(1000, 0, 0);



            // If we dont load from the portal, we grab the last saved position!
            // Otherwise we use the portal!
            if (loadedFromPortal == false)
            {
                print("loaded from portal false");
                // loading from last position
                startPos = God.state.lastPosition;
            }
            else
            {
                if (God.state.currentQuestID >= 0)
                {
                    if (God.state.currentQuestID >= portals.Length)
                    {
                        startPos = baseStartPosition.position;
                        God.state.SetCurrentBiome(-1);
                    }
                    else
                    {

                        God.state.SetLastPosition(portals[God.state.currentQuestID].startPoint.position);
                        // return / spawn at gate that is our current biome!
                        // when bird dies, we respawn at our first starting position
                        startPos = portals[God.state.currentQuestID].startPoint.position;

                    }

                }
                else
                {
                    startPos = God.state.lastPosition;

                }

            }

            print(startPos);

            God.wren.SetFullPosition(startPos);



        }



    }
}
