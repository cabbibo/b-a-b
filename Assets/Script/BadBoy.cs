using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;

[ExecuteAlways]
public class BadBoy : MonoBehaviour
{



    public BadBoyManager manager;

    // if goes outside of cage, if far enough away from wren, will return to cage
    public Transform cage;
    public GameObject body;




    public Transform[] transformsOfInterest;


    public LineRenderer focusLR;

    public Transform debugTransform;

    /*

        public float desiredDistanceFromGround;
        public float frontAvoidanceDistance;
        public float regularNoise;
        public float focusedNoise;
        public float huntingNoise;
        public float collisionAngleHurtWren;// if facing wren enough on collision, will hurt wren
        public float collisionAngleHurtSelf;// if wren facing right direction on collision, will hurt self
    */

    public Rigidbody rb;


    public List<Vector3> forces = new List<Vector3>();

    public int trailLength;
    public Transform[] trailTransforms;
    public bool[] trailsAte;

    public Vector3[] trailVels;



    public Transform trailHolder;
    public GameObject trailPrefab;
    public float trailFollowLerp;
    public float trailMaxScale;
    public float trailMinScale;


    // params
    public float distanceForLooseInterest = 10;
    public float distanceForGainIntereset = 7;
    public float distanceForHunt = 6;




    public float interestMeter = 0;
    public float interestFillSpeed = .001f;

    public float interestDrainSpeed = .03f;

    public float interestDrainSpeedWhenNotInCage = .1f;

    public float aimingValueHuntCutoff = .9f;


    public float regularSpeed = 1;
    public float focusedSpeed = 1.5f;
    public float huntingSpeed = 2;


    public float aimingWhileHuntingWrenForce = 1;
    public float aimingWhileFocusedOnWrenForce = 1;
    public float aimingTowardsDirectionOfInterestForce = 1;

    public float turnBackTowardsCageForce = 1;
    public float turnTowardsVelocityForce = 1;


    public float transformOfInterestDistanceReach = 1;

    public float minSpeed = 1;
    public float maxSpeed = 10;




    // values

    public float distToWren;
    public float oDistToWren;
    public Vector3 directionToWren;
    public Vector3 oDirectionToWren;

    public Vector3 directionToCenter;
    public float distToCenter;

    public Vector3 directionToTransformOfInterest;
    public float distToTransformOfInterest;

    public float aimingValue;




    public bool inCage = true;
    public bool isFocusingOnWren = false;
    public bool isHuntingWren = false;

    public bool oIsFocusingOnWren = false;
    public bool oIsHuntingWren = false;
    public Transform oTransformOfInterest;
    public Transform transformOfInterest;

    public Vector3 velocity;




    Vector3 v1;
    Vector3 v2;

    public void OnEnable()
    {

        v1 = new Vector3();
        v2 = new Vector3();

        focusLR = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();

        allForces = new List<Force>();
        allTorques = new List<Vector3>();

        transformOfInterest = transformsOfInterest[Random.Range(0, transformsOfInterest.Length)];
        oTransformOfInterest = transformOfInterest;

        oDistToWren = 100000;
        oDirectionToWren = Vector3.one;

        distToWren = 100000;
        directionToWren = Vector3.one;

        oIsFocusingOnWren = false;
        oIsHuntingWren = false;


        isFocusingOnWren = false;
        isHuntingWren = false;

        rb.position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)) + cage.position;
        transform.position = rb.transform.position;
        rb.velocity = transform.forward * minSpeed;

        while (trailHolder.childCount > 0)
        {
            DestroyImmediate(trailHolder.GetChild(0).gameObject);
        }

        trailTransforms = new Transform[trailLength];
        trailsAte = new bool[trailLength];
        trailVels = new Vector3[trailLength];

        for (int i = 0; i < trailLength; i++)
        {
            GameObject g = Instantiate(trailPrefab, trailHolder);
            g.GetComponent<BadBoyTrailSegment>().badBoy = this;
            g.GetComponent<BadBoyTrailSegment>().trailID = i;
            trailTransforms[i] = g.transform;
            trailTransforms[i].GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", Color.green);

        }


    }



    public int numShardsOnEat = 2;
    public int numShardsOnAllAte = 50;


    // the type of shard we are to eat!
    public float type = 3;
    public void OnWrenAte(int id)
    {

        if (trailsAte[id] == true)
        {
            return;
        }

        trailTransforms[id].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
        trailsAte[id] = true;
        God.audio.Play(God.sounds.eatClip, Random.Range(.5f, 1));
        God.particleSystems.Emit(God.particleSystems.eatParticleSystem, trailTransforms[id].position, 100);
        God.wren.shards.CollectShards(numShardsOnEat, type);


        bool allAte = true;
        for (int i = 0; i < trailsAte.Length; i++)
        {
            if (trailsAte[i] == false)
            {
                allAte = false;
            }
        }

        if (allAte)
        {
            OnAllAte();
        }




    }


    public void SetComplete()
    {

        for (int i = 0; i < trailTransforms.Length; i++)
        {
            trailTransforms[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
            trailsAte[i] = true;
        }

        completed = true;
    }

    void OnAllAte()
    {

        print("OnAllAte");

        completed = true;

        // Some sort of explosion

        God.audio.Play(God.sounds.largeSuccessSound, 1);
        God.particleSystems.Emit(God.particleSystems.largeSuccessParticleSystem, transform.position, 1000);
        God.wren.shards.CollectShards(numShardsOnAllAte, type);


        manager.OnBadBoyComplete(this);
        //gameObject.SetActive(false);


    }

    public Transform targetOnceCompleted;


    public float forceTowardsTargetOnceComplete = 1;
    public float curlForceOnceComplete = 1;
    public float randomForceOnceComplete = 1;
    public float dampeningOnceComplete = .9f;

    Vector3 tmp1;
    Vector3 tmp2;




    // Update is called once per frame
    void FixedUpdate()
    {
        oDistToWren = distToWren;
        oDirectionToWren = directionToWren;

        oIsFocusingOnWren = isFocusingOnWren;
        oIsHuntingWren = isHuntingWren;
        oTransformOfInterest = transformOfInterest;


        Transform fTransform = debugTransform;

        if (God.wren != null)
        {
            fTransform = God.wren.transform;
        }




        if (fTransform != null)
        {

            distToWren = Vector3.Distance(fTransform.position, transform.position);
            directionToWren = (fTransform.position - transform.position).normalized;
            directionToCenter = (cage.position - transform.position).normalized;
            distToCenter = Vector3.Distance(cage.position, transform.position);
            aimingValue = Vector3.Dot(transform.forward, directionToWren);


            directionToTransformOfInterest = (transformOfInterest.position - transform.position).normalized;
            distToTransformOfInterest = Vector3.Distance(transformOfInterest.position, transform.position);

            Vector3 localPosition = cage.InverseTransformPoint(transform.position);

            localPosition = new Vector3(Mathf.Abs(localPosition.x), Mathf.Abs(localPosition.y), Mathf.Abs(localPosition.z));


            // if we are outside cage, loose interst and also turn back towards cage
            // even if we are hunting wren
            if (localPosition.x > .5f || localPosition.y > .5f || localPosition.z > .5f)
            {
                Vector3 v1 = transform.forward - directionToCenter;
                AddForce(v1 * turnBackTowardsCageForce, transform.position + transform.forward);
                interestMeter -= interestDrainSpeedWhenNotInCage;
                inCage = false;
            }
            else
            {
                inCage = true;
            }

            // fill and loose interest depending on how far away we are from wren

            if (distToWren < distanceForGainIntereset)
            {
                interestMeter += interestFillSpeed;
            }
            if (distToWren > distanceForLooseInterest)
            {
                interestMeter -= interestDrainSpeed;
            }


            // to checks for interest!
            if (interestMeter < 0)
            {
                interestMeter = 0;
                isFocusingOnWren = false;
                isHuntingWren = false;
            }

            if (interestMeter > 1)
            {
                interestMeter = 1;
                isFocusingOnWren = true;
            }

            // to checks for interest!
            if (aimingValue > aimingValueHuntCutoff && distToWren < distanceForHunt)
            {
                isHuntingWren = true;
            }


            /*







            FORCE TIME









            */

            // move forward
            AddForce(transform.forward * regularSpeed, transform.position);



            // turn  while focusing
            // move towards while focusing
            if (isFocusingOnWren)
            {

                Vector3 v1 = transform.forward - directionToWren;
                AddForce(v1 * aimingWhileFocusedOnWrenForce, transform.position + transform.forward);

                AddForce(transform.forward * focusedSpeed, transform.position);

            }


            // turn harder while hunting
            // move towards while hunting
            if (isHuntingWren)
            {
                Vector3 v1 = transform.forward - directionToWren;
                AddForce(v1 * aimingWhileHuntingWrenForce, transform.position + transform.forward);
                AddForce(transform.forward * huntingSpeed, transform.position);

            }



            if (!isFocusingOnWren && !isHuntingWren)
            {


                Vector3 v1 = transform.forward - directionToTransformOfInterest;


                AddForce(v1 * aimingTowardsDirectionOfInterestForce, transform.position + transform.forward);


                // changeLocationOfInterest
                if (distToTransformOfInterest < transformOfInterestDistanceReach)
                {
                    transformOfInterest = transformsOfInterest[Random.Range(0, transformsOfInterest.Length)];
                }


            }


            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }

            if (rb.velocity.magnitude < minSpeed)
            {
                if (rb.velocity.magnitude > 0)
                {
                    rb.velocity = rb.velocity.normalized * minSpeed;
                }
                else
                {
                    rb.velocity = transform.forward * minSpeed;
                }
            }




            v1 = transform.forward - rb.velocity.normalized;
            AddForce(v1 * turnTowardsVelocityForce, transform.position + transform.forward);

            // Straightens out!
            // Vector3 crossTorque = Vector3.Cross(rb.velocity, transform.forward);
            //rb.AddTorque(crossTorque * turnTowardsVelocityForce);

            /*

                
                
                
                Viz stuff







            */




            if (isHuntingWren && oIsHuntingWren == false)
            {
                // God.wren.cameraWork.objectTargeted = transform;
            }

            if (isHuntingWren == false && oIsHuntingWren == true)
            {
                //   God.wren.cameraWork.objectTargeted = null;
            }



        }
        else
        {
            /// we reallly far away
            distToWren = 100000;
            directionToWren = Vector3.one;
        }


        ResolveForces();

        velocity = rb.velocity;





        /*


            Trail stuff


        */

        if (completed == false)
        {
            trailTransforms[0].position = Vector3.Lerp(trailTransforms[0].position, transform.position, trailFollowLerp);
            trailTransforms[0].LookAt(transform.position);

            for (int i = trailLength - 1; i > 0; i--)
            {
                trailTransforms[i].position = Vector3.Lerp(trailTransforms[i].position, trailTransforms[i - 1].position, trailFollowLerp);
                trailTransforms[i].LookAt(trailTransforms[i - 1].position);


                float n = (i / (float)trailLength);
                float fScale = Mathf.Min(n * 10, (1 - n));
                bool isAte = trailsAte[i];
                float extraScale = isAte ? .1f : 1;
                trailTransforms[i].localScale = Vector3.one * extraScale * Mathf.Lerp(trailMinScale, trailMaxScale, fScale);
            }
        }
        else
        {


            for (int i = 0; i < trailTransforms.Length; i++)
            {
                float n = (i / (float)trailLength);
                float fScale = Mathf.Min(n * 10, (1 - n));
                bool isAte = trailsAte[i];
                float extraScale = isAte ? .1f : 1;
                trailTransforms[i].localScale = Vector3.one * extraScale * Mathf.Lerp(trailMinScale, trailMaxScale, fScale);

                tmp1 = trailTransforms[i].position - targetOnceCompleted.position;

                trailVels[i] += -tmp1.normalized * forceTowardsTargetOnceComplete;
                trailVels[i] += Vector3.Cross(tmp1.normalized, Vector3.up) * curlForceOnceComplete;

                tmp2 = Random.insideUnitSphere;
                tmp2.Normalize();

                trailVels[i] += tmp2 * randomForceOnceComplete;

                trailTransforms[i].position += trailVels[i];
                trailVels[i] *= dampeningOnceComplete;




            }

        }







    }
    void Update()
    {


        Transform fTransform = debugTransform;

        if (God.wren != null)
        {
            fTransform = God.wren.transform;
        }

        /*

           GPU VIZ

           */


        // draw line to wren

        if (isFocusingOnWren || isHuntingWren)
        {
            focusLR.enabled = true;
            focusLR.SetPosition(0, transform.position);
            focusLR.SetPosition(1, fTransform.position);
        }
        else
        {
            focusLR.enabled = false;
            focusLR.SetPosition(0, transform.position);
            focusLR.SetPosition(1, transform.position);

        }

        if (forceBuffer != null)
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetBuffer("_ForceBuffer", forceBuffer);
            mpb.SetInt("_Count", maxForces);
            mpb.SetMatrix("_Transform", transform.localToWorldMatrix);
            mpb.SetMatrix("_BirdTransform", fTransform.localToWorldMatrix);

            Graphics.DrawProcedural(forceDebugMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, maxForces * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));


        }
    }

    public float eatAmount = 1;
    public float shardsLostPerHit = 1000;
    public bool completed;




    public void OnCollisionEnter(Collision collision)
    {

        //  print("helllo");
        //OnEnable();

        if (God.IsOurWren(collision.collider))
        {
            print("wren hit");
            print(-collision.relativeVelocity.magnitude * .1f * eatAmount);
            God.wren.shards.SpendShards((int)shardsLostPerHit);
            // God.wren.stats.HealthAdd(-collision.relativeVelocity.magnitude * .1f * eatAmount);
        }
        else
        {

            rb.position = rb.position + collision.contacts[0].normal * 2;
            rb.velocity = collision.impulse.normalized * 1;
            rb.angularVelocity = Vector3.zero;
            transform.LookAt(collision.contacts[0].point + collision.impulse.normalized * 1);

        }

    }


    public void AddForce(Vector3 force, Vector3 position)
    {
        allForces.Add(new Force(force, position));
        // rb.AddForceAtPosition(force, position);
    }

    public void AddTorque(Vector3 torque)
    {
        allTorques.Add(torque);
        //rb.AddTorque(torque);
    }

    public void AddForce(Vector3 force)
    {

        allForces.Add(new Force(force, rb.transform.position));
        //  rb.AddForce(force);

    }




    public struct Force
    {
        public Vector3 force;
        public Vector3 position;
        public Force(Vector3 f, Vector3 p)
        {
            force = f;
            position = p;
        }
    }
    public List<Force> allForces = new List<Force>();
    public List<Vector3> allTorques = new List<Vector3>();



    public int totalForcesApplied;

    public ComputeBuffer forceBuffer;
    public float[] forceBufferArray;
    public int maxForces;

    public Material forceDebugMaterial;
    MaterialPropertyBlock mpb;

    public bool showDebugForces = true;



    public void ResolveForces()
    {


        totalForcesApplied = allForces.Count;


        if (allForces.Count == 0)
        {
            return;
        }


        if (showDebugForces)
        {




            if (forceBuffer == null)
            {
                forceBuffer = new ComputeBuffer(maxForces, 6 * sizeof(float));
                forceBufferArray = new float[maxForces * 6];
            }

            for (int i = 0; i < allForces.Count; i++)
            {
                forceBufferArray[i * 6 + 0] = allForces[i].force.x;
                forceBufferArray[i * 6 + 1] = allForces[i].force.y;
                forceBufferArray[i * 6 + 2] = allForces[i].force.z;
                forceBufferArray[i * 6 + 3] = allForces[i].position.x;
                forceBufferArray[i * 6 + 4] = allForces[i].position.y;
                forceBufferArray[i * 6 + 5] = allForces[i].position.z;
            }

            forceBuffer.SetData(forceBufferArray);


        }



        for (int i = 0; i < allForces.Count; i++)
        {

            Debug.DrawLine(allForces[i].position, allForces[i].position + allForces[i].force * .04f, Color.red, 1);
            rb.AddForceAtPosition(allForces[i].force, allForces[i].position);
        }

        allForces.Clear();

        for (int i = 0; i < allTorques.Count; i++)
        {
            rb.AddTorque(allTorques[i]);
        }
        allTorques.Clear();


    }

}

