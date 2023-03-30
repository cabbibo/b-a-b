using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using IMMATERIA;

using static Unity.Mathematics.math;
using Unity.Mathematics;


[ExecuteAlways]
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



    public ParticleSystem gotAteParticleSystem;
    public AudioClip gotAteClip;

    public TransformBuffer tb;

    // Start is called before the first frame update
    void OnEnable()
    {

        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        butterflys = new GameObject[numButterflys];

        positions = new float3[numButterflys];
        velocities = new float3[numButterflys];
        active = new bool[numButterflys];

        for (int i = 0; i < numButterflys; i++)
        {
            Vector3 fPos = new Vector3(UnityEngine.Random.Range(-spawnRange.x, spawnRange.x),
                                        UnityEngine.Random.Range(-spawnRange.y, spawnRange.y),
                                        UnityEngine.Random.Range(-spawnRange.z, spawnRange.z));

            fPos += transform.position;
            GameObject bug = Instantiate(butterflyPrefab, fPos, Quaternion.identity);
            bug.SetActive(true);
            bug.GetComponent<Butterfly>().bs = this;
            bug.transform.parent = this.transform;
            butterflys[i] = bug;
            positions[i] = fPos;
            active[i] = true;
            velocities[i] = float3(0, 0, 0);//new float3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));


        }




    }

    void OnDisable()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }

    void Destroy()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }

    public float maxSpeed = 1;
    public int numToUpdate = 32;
    public int lastUpdated = 0;

    public float updateAllRadius;
    public float updateNoneRadius;


    public float randomFromInt(int i)
    {
        int customSeed = 1234;
        UnityEngine.Random.InitState(customSeed);
        return UnityEngine.Random.value;
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


        float3 force;
        for (int i = 0; i < numToUpdate; i++)
        {

            int fID = (lastUpdated + i) % butterflys.Length;
            force = 0;

            force += CohesionForce(fID);
            force += AlignmentForce(fID);
            force += SeperationForce(fID);
            force += SharkRepelForce(fID);


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

    float3 SharkRepelForce(int i)
    {
        float3 diff = positions[i] - sharkPos;
        float dist = length(diff);
        if (dist < sharkRepelRadius)
        {
            return diff * sharkRepelForce * length(sharkSpeed);
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

    public void GotAte(Butterfly b)
    {

        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;

        WrenUtils.God.audio.Play(gotAteClip);
        b.gameObject.SetActive(false);
        for (int i = 0; i < butterflys.Length; i++)
        {
            if (b.gameObject == butterflys[i])
                active[i] = false;

        }

        // God.wren.stats.FullnessAdd(bugFullnessAdd);
        // God.wren.stats.StaminaAdd(bugStaminaAdd);

    }
}
