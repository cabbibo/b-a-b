using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using IMMATERIA;
using static Helpers;

using static Unity.Mathematics.math;
using Unity.Mathematics;

public class ButterflySpawner : MonoBehaviour
{

    public GameObject tmpShark;


    public GameObject butterflyPrefab;

    public GameObject[] butterflys;

    public int numButterflys;

    public Transform Cage;

    public Vector3 spawnRange;

    public float3[] positions;
    public float3[] velocities;
    public bool[] active;

    public float centerForce;

    public float3 sharkPos;
    public float3 sharkSpeed;

    public float sharkRepelRadius;
    public float sharkRepelForce;


    public float sharkAttractRadius;
    public float sharkAttractForce;



    public AudioClip[] gotAteClips;

    public float clipVolumeFalloff;
    public float clipPitchLow;
    public float clipPitchHigh;
    public float clipPitchDistanceLow;
    public float clipPitchDistanceHigh;

    public TransformBuffer tb;
    public Cycle tbParent;

    public Transform startLocation;


    public float bugFullnessAdd;
    public float bugStaminaAdd;

    public bool destroyOnEat;


    // Start is called before the first frame update
    void OnEnable()
    {

        for (int i = this.transform.childCount; i > 0; --i)
            Destroy(this.transform.GetChild(0).gameObject);

        butterflys = new GameObject[numButterflys];

        positions = new float3[numButterflys];
        velocities = new float3[numButterflys];
        active = new bool[numButterflys];

        tb.Deactivate();
        tb.transforms = new Transform[numButterflys];
        for (int i = 0; i < numButterflys; i++)
        {
            Vector3 fPos = new Vector3(UnityEngine.Random.Range(-spawnRange.x, spawnRange.x),
                                        UnityEngine.Random.Range(-spawnRange.y, spawnRange.y),
                                        UnityEngine.Random.Range(-spawnRange.z, spawnRange.z));

            fPos += startLocation.position;// transform.position;
            GameObject bug = Instantiate(butterflyPrefab, fPos, Quaternion.identity);
            bug.SetActive(true);
            bug.GetComponent<Butterfly>().bs = this;
            bug.transform.parent = this.transform;

            //            print(bug);
            butterflys[i] = bug;
            positions[i] = fPos;
            active[i] = true;
            velocities[i] = float3(0, 0, 0);//new float3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));

            tb.transforms[i] = bug.transform;
        }




        if (tbParent != null)
        {
            tbParent.SafeInsert(tb);

            if (tbParent.living)
            {
                //  print("alive");
                tbParent.JumpStart(tb);
            }
            else
            {
                tb.Activate();
            }
        }





    }

    void OnDisable()
    {

        if (tbParent != null)
        {
            if (tbParent.living)
            {
                //   print("alive");
                tbParent.JumpDeath(tb);
            }
            else
            {
                tb.Deactivate();
            }
        }



        for (int i = this.transform.childCount; i > 0; --i)
        {
            Destroy(this.transform.GetChild(0).gameObject);
            butterflys = new GameObject[0];

            positions = new float3[0];
            velocities = new float3[0];
            active = new bool[0];


        }
    }

    void Destroy()
    {
        for (int i = this.transform.childCount; i > 0; --i)
        {
            Destroy(this.transform.GetChild(0).gameObject);

            butterflys = new GameObject[0];

            positions = new float3[0];
            velocities = new float3[0];
            active = new bool[0];
        }
    }

    public float maxSpeed = 1;
    public int numToUpdate = 32;
    public int lastUpdated = 0;

    public float updateAllRadius;
    public float updateNoneRadius;


    public float randomFromInt(int i)
    {
        return (float)Mathf.Sin(((float)i * 10131.9494f + (float)i * 441.414f)) * 0.5f + 0.5f;
    }

    // Update is called once per frame
    void Update()
    {

        if (WrenUtils.God.wren)
        {
            sharkPos = WrenUtils.God.wren.transform.position;
            sharkSpeed = WrenUtils.God.wren.physics.vel;
        }
        else
        {
            sharkSpeed = float3(tmpShark.transform.position) - sharkPos;
            sharkPos = tmpShark.transform.position;
        }


        float3 dist = float3(transform.position) - sharkPos;
        float len = length(dist);

        float smoothVal = (len - updateAllRadius) / (updateNoneRadius - updateAllRadius);
        smoothVal = saturate(smoothVal);
        numToUpdate = (int)((1 - smoothVal) * (float)butterflys.Length);

        // always update at least 1!
        numToUpdate = Mathf.Max(1, numToUpdate);



        float3 force;
        for (int i = 0; i < numToUpdate; i++)
        {

            int fID = (lastUpdated + i) % butterflys.Length;
            force = 0;

            force += CohesionForce(fID);
            force += AlignmentForce(fID);
            force += SeperationForce(fID);
            force += SharkRepelForce(fID);
            force += SharkAttractForce(fID);


            velocities[fID] += force;

            if (length(velocities[fID]) > maxSpeed)
            {
                velocities[fID] = normalize(velocities[fID]) * maxSpeed;
            }



        }

        for (int i = 0; i < butterflys.Length; i++)
        {



            force = 0;
            force += float3(centerForce * (transform.position - butterflys[i].transform.position)) * (randomFromInt(i) * .5f + .8f);

            velocities[i] += force;

            positions[i] += velocities[i];

            butterflys[i].transform.position = positions[i];
            butterflys[i].transform.rotation = Quaternion.Slerp(butterflys[i].transform.rotation, Quaternion.LookRotation(velocities[i], Vector3.up), .1f);

            //velocities[i] *= .9f;
        }


        lastUpdated += numToUpdate;
        lastUpdated %= butterflys.Length;

    }


    float3 projectPointOnLine(float3 linePoint, float3 lineVec, float3 point)
    {
        //get vector from point on line to point in space
        float3 linePointToPoint = point - linePoint;

        float t = dot(linePointToPoint, lineVec);

        return linePoint + lineVec * t;

    }

    float3 SharkRepelForce(int i)
    {
        float3 diff = positions[i] - sharkPos;
        float dist = length(diff);

        if (dist < sharkRepelRadius)
        {

            if (length(sharkSpeed) < .01f) return 0;
            float3 p = projectPointOnLine(sharkPos, normalize(sharkSpeed), positions[i]);
            diff = positions[i] - p;

            return (normalize(diff) / dist) * sharkRepelForce;// * length(sharkSpeed);
        }
        else
        {
            return 0;
        }
    }


    float3 SharkAttractForce(int i)
    {
        float3 diff = positions[i] - sharkPos;
        float dist = length(diff);
        if (dist < sharkAttractRadius)
        {
            return -diff * sharkAttractForce * length(sharkSpeed);
        }
        else
        {
            return 0;
        }
    }

    public float cohesionDistance = 1;
    public float cohesionStrength = 1;

    float3 CohesionForce(int i)
    {
        float3 center = 0;
        int count = 0;
        for (int j = 0; j < butterflys.Length; j++)
        {
            if (i != j && active[j])
            {
                float3 diff = positions[i] - positions[j];
                float dist = length(diff);
                if (dist < cohesionDistance)
                {
                    center += positions[j];
                    count++;
                }
            }
        }

        if (count > 0)
        {
            center /= count;
            return (center - positions[i]) * cohesionStrength;
        }
        else
        {
            return 0;
        }
    }


    public float alignmentDistance;
    public float alignmentStrength;
    float3 AlignmentForce(int i)
    {
        float3 alignment = 0;
        for (int j = 0; j < butterflys.Length; j++)
        {
            if (i != j && active[j])
            {
                float3 oVel = velocities[j];
                float3 diff = positions[i] - positions[j];
                float dist = length(diff);
                if (dist < alignmentDistance)
                {
                    alignment += oVel;
                }

            }
        }
        return alignment * alignmentStrength;
    }



    public float seperationStrength = 1;
    public float seperationDistance = 1;
    float3 SeperationForce(int i)
    {
        float3 seperation = 0;
        for (int j = 0; j < butterflys.Length; j++)
        {
            if (i != j && active[j])
            {
                float3 diff = positions[i] - positions[j];
                float dist = length(diff);
                if (dist < seperationDistance)
                {
                    seperation += diff * (1 / dist);
                }
            }
        }

        return seperation * seperationStrength;

    }


    public Helpers.FloatEvent OnEat;

    public void GotAte(Butterfly b)
    {
        float d = length(b.transform.position - WrenUtils.God.wren.transform.position);


        ParticleSystem ps = WrenUtils.God.particleSystems.smallCollectionParticleSystem;

        ps.transform.position = WrenUtils.God.wren.transform.position; //b.transform.position;
        ps.transform.LookAt(WrenUtils.God.camera.transform.position);
        ps.Emit((int)(30 / length(d)));


        float pitch = Mathf.Lerp(clipPitchLow, clipPitchHigh, (d - clipPitchDistanceLow) / (clipPitchDistanceHigh - clipPitchDistanceLow));


        WrenUtils.God.audio.Play(gotAteClips, 1, pitch);

        if (destroyOnEat)
        {
            // TODO remove from transform buffer, and from list that is calculating forces
            b.gameObject.SetActive(false);
            for (int i = 0; i < butterflys.Length; i++)
            {
                if (b.gameObject == butterflys[i])
                    active[i] = false;

            }
        }


        OnEat.Invoke(bugFullnessAdd);
        WrenUtils.God.wren.stats.FullnessAdd(bugFullnessAdd);
        WrenUtils.God.wren.stats.StaminaAdd(bugStaminaAdd);

    }
}
