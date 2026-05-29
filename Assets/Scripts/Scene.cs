using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Crest;
using UnityEngine.Playables;


namespace WrenUtils
{
    public class Scene : MonoBehaviour
    {
        public UnityEvent OnLoadEvent;

        public new string        name;
        public PhysicsParams physicsParams;

        public Portal[] portals;

        public Transform baseStartPosition;

        public bool isDemo;
        public bool startInFlight;

        public ScenePostSettings postSettings;
        public PlayableDirector  playableDirector;
        public ActivityManager   activityManager;


        public void SceneLoaded( int newScene , bool loadedFromPortal )
        {

//            print( "======= SCENE LOADED =======" );

            // Set WrenState
            God.wren.state.inInterface = false;
            God.wren.airInterface.Toggle( false );
            God.wren.fullInterface.Toggle( false );
            God.interfaceTutorial.SetOff();
            God.cameraManager.lerpManager.enabled = true;
            God.cameraManager.SetBaseState();

            God.playableDirector = playableDirector;


            if ( newScene == 0 ) {
                God.wren.inEther = true;
                Camera.main.GetComponent<UnderwaterRenderer>().enabled = false;
            } else {
                God.wren.inEther = false;
                Camera.main.GetComponent<UnderwaterRenderer>().enabled = true;
            }


            // Sets up our demo info
            for ( int i = 0; i < portals.Length; i++ ) {
                portals[i].demo = isDemo;
            }


            var startPos = baseStartPosition.position;

            if ( God.wren != null ) {

                God.wren.parameters.LoadPhysics( physicsParams );

                SetWrenStartPosition( loadedFromPortal );

                if ( startInFlight ) {
                    God.wren.state.TakeOff();
                }

            }


            postSettings.Set();
            activityManager.Initialize();


            OnLoadEvent.Invoke();


        }


        public void SetWrenStartPosition( bool loadedFromPortal )
        {
//            print( "======= SETTING POSITION  =======" );

            var startPos = new Vector3( 1000 , 0 , 0 );


            // If we dont load from the portal, we grab the last saved position!
            // Otherwise we use the portal!
            if ( loadedFromPortal == false ) {
//                print( "======= NOT LOADED FROM PORTAL =======" );
                //                print("loaded from portal false");
                // loading from last position
                startPos = God.state.lastPosition;
            } else {
                if ( God.state.currentQuestID >= 0 ) {

                    //                  print( "======= HAVE A QUEST ID  =======" );
//                    print( "qid " + God.state.currentQuestID );

                    if ( God.state.currentQuestID >= portals.Length ) {
                        print( "TOO HIGH" );
                        startPos = baseStartPosition.position;
                        God.state.SetCurrentBiome( -1 );
                    } else {

//                        print( "LOADING FROM PORTAL" );
                        God.state.SetLastPosition( portals[God.state.currentQuestID].startPoint.position );
                        // return / spawn at gate that is our current biome!
                        // when bird dies, we respawn at our first starting position
                        startPos = portals[God.state.currentQuestID].startPoint.position;

                    }

                } else {

                    print( "======= NEGATIVE QUEST ID =======" );
                    startPos = God.state.lastPosition;

                }

            }

            // print(startPos);

            God.wren.SetFullPosition( startPos );


        }
    }
}