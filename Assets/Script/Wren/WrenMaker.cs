#if NORMCORE

using UnityEngine;
using Normal.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
 using System.Collections;
 using System.Collections.Generic;
 using WrenUtils;

    public class WrenMaker : MonoBehaviour {
        private Realtime _realtime;
        public string AvatarPrefabName = "WrenAvatar";
        public bool connected;
        public List<Wren> wrens;
        public GameObject offlineWren;

        private float startTime;

        public float timeoutValue = 6;

        public UnityEvent onGameStart;
        public UnityEvent onRoomConnect;

        ///public int wrenCount;
        public ComputeBuffer wrenBuffer;
        
     
        public float[] wrenBufferValues;

        public int wrenBufferStructSize;


        // Local Wren helpers
        public Wren localWren;
        public delegate void WrenCreation(Wren wren);
        public event WrenCreation localWrenCreated;

        public int oNumWrens;
        public int numWrens;

        private void Awake() {

            connected = false;
            startTime = Time.time;
            wrens = new List<Wren>();

            onGameStart.Invoke();
            // Get the Realtime component on this game object
            _realtime = GetComponent<Realtime>();

            // Notify us when Realtime successfully connects to the room
            _realtime.didConnectToRoom += DidConnectToRoom;
        }
        void Update(){

            UpdateWrenBuffer();

            if(  Time.time - startTime > timeoutValue && connected == false ){
                OfflineStart();
            }
        
        }

        public void UpdateWrenBuffer(){
            
            oNumWrens = numWrens;
            numWrens = wrens.Count;

            if( numWrens != oNumWrens ){
                RemakeWrenBuffer();
            }

            if( wrenBuffer != null ){
                for( int i = 0; i < numWrens; i++ ){
                    wrenBufferValues[i * wrenBufferStructSize + 0 ] = wrens[i].transform.position.x;
                    wrenBufferValues[i * wrenBufferStructSize + 1 ] = wrens[i].transform.position.y;
                    wrenBufferValues[i * wrenBufferStructSize + 2 ] = wrens[i].transform.position.z;
                    

                    //TODO: Will this be 0 for non-local wrens?????
                    wrenBufferValues[i * wrenBufferStructSize + 3 ] = wrens[i].physics.vel.x;
                    wrenBufferValues[i * wrenBufferStructSize + 4 ] = wrens[i].physics.vel.y;
                    wrenBufferValues[i * wrenBufferStructSize + 5 ] = wrens[i].physics.vel.z;

                    
                    wrenBufferValues[i * wrenBufferStructSize + 6 ] = wrens[i].input.left1;
                    wrenBufferValues[i * wrenBufferStructSize + 7 ] = wrens[i].input.right1;

                    wrenBufferValues[i * wrenBufferStructSize + 8 ] = wrens[i].state.hue1;
                    wrenBufferValues[i * wrenBufferStructSize + 9 ] = wrens[i].state.hue2;
                    wrenBufferValues[i * wrenBufferStructSize + 10 ] = wrens[i].state.hue3;
                    wrenBufferValues[i * wrenBufferStructSize + 11 ] = wrens[i].state.hue4;


                    

                }

                wrenBuffer.SetData(wrenBufferValues);
            }
        
        }   


        public void RemakeWrenBuffer(){


            if( wrenBuffer != null ){ wrenBuffer.Dispose(); wrenBuffer = null; }

            wrenBufferValues = new float[ numWrens  * wrenBufferStructSize ];
            wrenBuffer = new ComputeBuffer( numWrens , sizeof(float) * wrenBufferStructSize );

        }


        private void OfflineStart(){

            connected = true;

            GameObject g = GameObject.Instantiate(offlineWren);
            if(God.state){ God.state.LoadLocalState(); }
            SetWrenVals( g , false );
        }

        private void DidConnectToRoom(Realtime realtime) {

            if( connected == false ){

                connected = true;


                // Instantiate the CubePlayer for this client once we've successfully connected to the room
                GameObject g = Realtime.Instantiate(AvatarPrefabName,                 // Prefab name
                                    position: Vector3.zero,          // Start at exact Center
                                    rotation: Quaternion.identity, // No rotation
                            ownedByClient: true,                // Make sure the RealtimeView on this prefab is owned by this client
                    preventOwnershipTakeover: true,                // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                                useInstance: realtime);           // Use the instance of Realtime that fired the didConnectToRoom event.
        
          
                if( God.state ){ God.state.LoadLocalState(); }

                SetWrenVals( g , true );
                onRoomConnect.Invoke();


            }
        
        }

        public void SetWrenVals( GameObject g  , bool connected ){
            
                g.SetActive(true);
                Wren w =  g.GetComponent<Wren>();

                w.SetLocal( connected );
                
                Camera.main.GetComponent<LerpTo>().target = w.cameraWork.camTarget;

                localWren = w;

                God.instance._localWren = w;

                // checks if event is null or not
                localWrenCreated?.Invoke(w);



                localWren.LocalEnable();

                
        }

        public GameObject GetNextWren(Wren current) {
            var ind = wrens.IndexOf(current);
            if (ind == -1) {
                return null;
            }
            return wrens[(ind + 1) % wrens.Count].gameObject;
        }

        public int GetNormalClientId() {
            return _realtime.clientID;
        }
    }

#endif
