using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;
using WrenUtils;

public class Wren : MonoBehaviour
{





    /*

        References

    */

    public bool inEther;

    public TextMesh title;
    public FullBird bird;
    public WrenInput input;
    public WrenPhysics physics;
    public WrenCameraWork cameraWork;

    public WrenSynths sounds;
    public WrenCarrying carrying;

    public WrenCompass compass;


    public WrenParams parameters;

    public WrenState state;
    public WrenStats stats;

    public WrenMaker maker;


    public WrenBeacon beacon;
    public WrenColors colors;

    public WrenWaterController waterController;


    public Collection collection;

    public FullInterface fullInterface;
    public AirInterface airInterface;


    public Transform startingPosition;

    public WrenDeathAnimation deathAnimation;

    public float _ScaleMultiplier = 1;

    public GameObject soul;

    public RealtimeTransform rt_Tranform;

    public bool canMove;

    public WrenGrowthManager growth;

    public bool autoTakeOff;


    void OnEnable()
    {

        canMove = false;
        inEther = true;


        // Makes our beacon ( aka nest ) link to us
        if (beacon) { beacon.Create(); }
        parameters.Reset();

        FindInfo();


        startingPosition = GameObject.Find("StartPosition").transform;

        maker.wrens.Add(this);
        if (collection) { collection.CreateHolders(); }



    }


    public void LocalEnable()
    {


        state.CheckForOriginals();
        state.SetInRace(-1);
        rt_Tranform.RequestOwnership();

        FullReset();



    }

    void OnDisable()
    {
        maker.wrens.Remove(this);
        if (beacon) { beacon.Demolish(); }
    }

    public void PhaseShift(Vector3 p)
    {

        // teleport bird
        // teleport camera
        //gpu bird teleport

        print("PHASE SHIFT");
        print(p);

        cameraWork.Offset(p - transform.position);
        bird.PhaseShift(p - transform.position);

        physics.TransportToPosition(p, physics.rb.velocity);


        /// bird.TeleportToLocation();
        /// 

    }

    public void FullReset()
    {


        physics.Reset();


        Vector3 fPos = startingPosition.position + Vector3.up * physics.groundUpVal;
        Crash(fPos);
        state.LookAt(fPos + startingPosition.forward);
        bird.ResetAtLocation(fPos);//Values();


        cameraWork.Reset();

    }


    public void Crash(Vector3 p)
    {
        physics.TransportToPosition(GroundIntersection(p) + Vector3.up * physics.groundUpVal, Vector3.zero);
        state.HitGround();
        if (autoTakeOff)
        {
            state.TakeOff();
        }
    }


    public Vector3 GroundIntersection(Vector3 p)
    {

        RaycastHit hit;

        Vector3 newPos = p;

        // Ignore ourselves for collision hit
        var layerMask = (1 << 10);
        layerMask = ~layerMask;
        if (Physics.Raycast(p + Vector3.up * 3, -Vector3.up, out hit, 100000, layerMask))
        {
            newPos = hit.point;
        }
        else
        {
            if (Physics.Raycast(p - Vector3.up * 3, Vector3.up, out hit, 100000, layerMask))
            {
                newPos = hit.point;
            }
            else
            {
                newPos = p - Vector3.up * physics.groundUpVal;
            }
        }



        return newPos;

    }


    public Vector3 GroundNormal(Vector3 p)
    {

        RaycastHit hit;

        Vector3 newPos = p;

        // Ignore ourselves for collision hit
        var layerMask = (1 << 10);
        layerMask = ~layerMask;
        if (Physics.Raycast(p + Vector3.up * 3, -Vector3.up, out hit, 100000, layerMask))
        {
            newPos = hit.normal;
        }
        else
        {
            if (Physics.Raycast(p - Vector3.up * 3, Vector3.up, out hit, 100000, layerMask))
            {
                newPos = hit.normal;
            }
            else
            {
                newPos = Vector3.up;//p - Vector3.up * physics.groundUpVal;
            }
        }



        return newPos;

    }




    public float lastFlapTime;
    void Update()
    {

        if (state.isLocal)
        {


            input.SetInput();

            // state.inInterface = God.menu.menuOn;

            // Only drop items in air ( is that correct? )
            if (input.o_triangle < .5 && input.triangle > .5 && state.canTakeOff)
            {
                if (compass != null)
                {
                    compass.Toggle();
                }
            }

            if (canMove)
            {


                // Only drop items in air ( is that correct? )
                if (input.o_ex < .5 && input.ex > .5 && physics.onGround == false)
                {
                    carrying.DropFirstCarriedItem();
                }

                if (input.o_ex < .5 && input.ex > .5 && physics.onGround == true && state.inInterface == false && state.canTakeOff)
                {
                    God.audio.Play(God.sounds.takeoffClip);
                    state.TakeOff();
                }










                if (input.left1 > .1f && !physics.onGround)
                {
                    // God.audio.FadeLoop(God.sounds.dropParticlesLoop , 1 , .01f);
                    bird.leftWingTrailFromFeathers_gpu.emitting = 1;
                }
                else
                {
                    // God.audio.FadeLoop(God.sounds.dropParticlesLoop , 0, .01f);
                    bird.leftWingTrailFromFeathers_gpu.emitting = 0;
                }


                if (input.right1 > .1f && !physics.onGround)
                {
                    //God.audio.FadeLoop(God.sounds.dropParticlesLoop , 1 , .01f);
                    bird.rightWingTrailFromFeathers_gpu.emitting = 1;
                }
                else
                {
                    //God.audio.FadeLoop(God.sounds.dropParticlesLoop , 0, .01f);
                    bird.rightWingTrailFromFeathers_gpu.emitting = 0;
                }


                /*
                            float d = Mathf.Abs( input.o_left2 - input.left2);
                            state.stamina -= d;

                            if( d > 0 ){
                                lastFlapTime = Time.time;
                            }

                            d = Mathf.Abs( input.o_right2 - input.right2);
                            state.stamina -= d;

                            if( d > 0 ){
                                lastFlapTime = Time.time;
                            }


                            if( state.stamina < 0 ){
                                state.stamina = 0;
                            }

                            float staminaCooldownTime = 1;
                            float staminaRefillSpeed = .1f;   

                            if( Time.time - lastFlapTime  > staminaCooldownTime ){
                                state.stamina += staminaRefillSpeed;
                                if( state.stamina > state.maxStamina ){
                                    state.stamina = state.maxStamina;
                                }
                            }


                */

                // ONLY do interface stuff when we aren't 
                // in the ether!

                if (!inEther)
                {
                    if (input.o_circle < .5 && input.circle > .5)
                    {
                        state.inInterface = !state.inInterface;
                        ToggleInterface(state.inInterface);
                    }



                    if (input.dLeft > .5f && input.o_dLeft <= .5f)
                    {

                        Ray ray = new Ray();
                        ray.origin = Camera.main.transform.position;
                        ray.direction = Camera.main.transform.forward;
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 10000))
                        {
                            print(hit.collider.gameObject.name);
                            beacon.PlaceBeacon(hit.point);
                        }

                    }

                    if (input.o_dUp < .5 && input.dUp > .5)
                    {

                        if (state.beaconOn == true)
                        {


                            Crash(startingPosition.position);
                            ToggleInterface(false);
                            state.HitGround();
                            state.LookAt(transform.position + beacon.transform.forward);
                            physics.Reset();
                            bird.ResetAtLocation(beacon.nest.transform.position);

                            cameraWork.Reset();

                            //   God.AbortCurrentRace();

                            //   // If we are in a race, end it!
                            //   if( God.wren.state.inRace != -1 ){
                            //       print("WREN IN RACE: " + God.wren.state.inRace);
                            //       God.races[God.wren.state.inRace].AbortRace();
                            //   }

                            //   God.localWrenState.inRace

                            //if( state.SetInRace )

                        }

                    }
                }
                else
                {
                    beacon.PlaceBeacon(Vector3.one * 1000000f);
                }

            }


            carrying.UpdateCarriedItems();
            sounds.UpdateSound();
            cameraWork.CameraWork();

        }
        else
        {

            input.GetInput();


            if (input.left1 > .1f)
            {
                bird.leftWingTrailFromFeathers_gpu.emitting = 1;
            }
            else
            {
                bird.leftWingTrailFromFeathers_gpu.emitting = 0;
            }


            if (input.right1 > .1f)
            {
                bird.rightWingTrailFromFeathers_gpu.emitting = 1;
            }
            else
            {
                bird.rightWingTrailFromFeathers_gpu.emitting = 0;
            }



        }

    }
    void FixedUpdate()
    {

        if (state.isLocal && canMove)
        {
            physics.UpdatePhysics();
        }

        if (!inEther)
        {
            growth.updateGrowth();
        }

    }





    public virtual void LocalReset()
    {
        bird.ResetFeatherValues();
        physics.LocalReset();
        cameraWork.Reset();

    }

    public virtual void ResetToOtherPlayer(GameObject other)
    {
        bird.ResetFeatherValues();
        physics.ResetToOther(other);

    }




    public void ToggleInterface(bool onOff)
    {
        //  interface.gameObject.SetActive(onOff);

        //bird.Explode();
        //bird.HitGround();

        //physics.rb.isKinematic = onOff;


        print("TOGGLING ON OFF: " + onOff);
        state.inInterface = onOff;//true;

        if (physics.onGround)
        {
            if (fullInterface)
            {

                fullInterface.Toggle(onOff);
            }
        }
        else
        {
            if (airInterface) { airInterface.Toggle(onOff); }
            //compass.gameObject.SetActive(onOff);//.Toggle( onOff);
        }

    }



    public void FindInfo()
    {

        GameObject terrain = GameObject.Find("Terrain");
        if (terrain)
        {
            physics.terrain = terrain.GetComponent<Terrain>();
            physics.terrainCollider = terrain.GetComponent<Collider>();
        }
        input.controller = GameObject.Find("Rewired Input Manager").GetComponent<ControllerTest>();
        maker = GameObject.FindGameObjectWithTag("Realtime")?.GetComponent<WrenMaker>();
        fullInterface = God.groundInterface;//GameObject.FindGameObjectWithTag("Interface").GetComponent<FullInterface>();
        airInterface = God.airInterface;//GameObject.FindGameObjectWithTag("Interface").GetComponent<FullInterface>();

        God.groundInterface.wren = this;
        God.airInterface.wren = this;


    }



    public void SetLocal(bool connected)
    {

        state.isLocal = true;

        if (connected)
        {
            input.leftStickNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
            input.rightStickNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
            input.leftExtraNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
            input.rightExtraNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
            input.finalExtraNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
        }

    }


    void OnCollisionEnter(Collision c)
    {
        if (state.isLocal)
        {

            if (c.collider.tag == "Food")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.collider.gameObject);
                }
            }
            else if (c.collider.tag == "Ball")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.collider.gameObject);
                }
            }
            else if (c.collider.tag == "Fire")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.collider.gameObject);
                }
            }
            else if (c.collider.tag == "Resetable")
            {

            }
            else if (c.collider.tag == "Prey")
            {

            }
            else if (c.collider.tag == "Death")
            {
                state.OnDie();

            }
            else if (c.collider.tag == "Bug")
            {

            }
            else
            {
                Crash(c);


            }
        }
    }



    public void Crash(Collision c)
    {
        if (!state.onGround)
        {
            //if( c.impulse.magnitude != 0 ){
            //ToggleInterface(false);
            God.audio.Play(God.sounds.hitGroundClip);
            state.HitGround(c);

            if (state.isLocal)
            {
                carrying.GroundHit(Carryable.DropSettings.FromCrash(c));
            }

            if (!inEther)
            {
                growth.HurtCollision(c);
            }

            //}

            if (autoTakeOff)
            {
                state.TakeOff();
            }
        }
    }
    void OnTriggerEnter(Collider c)
    {
        if (state.isLocal)
        {

            if (c.tag == "Food")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.gameObject);
                }
            }
            else if (c.tag == "Ball")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.gameObject);
                }
            }
            else if (c.tag == "Fire")
            {
                if (!state.onGround)
                {
                    carrying.PickUpItem(c.gameObject);
                }
            }
            else if (c.tag == "Resetable")
            {

            }
            else if (c.tag == "Prey")
            {

            }
            else if (c.tag == "WaterDrop")
            {
                carrying.PickUpItem(c.gameObject);
            }
            else if (c.tag == "Boost")
            {
                Booster b = c.gameObject.GetComponent<Booster>();
                b.OnBoost(this);


            }
            else
            {

                if (waterController) { waterController.TriggerEnter(c); }

            }
        }
    }


    void OnTriggerExit(Collider c)
    {
        if (waterController) { waterController.TriggerExit(c); }
    }







}
