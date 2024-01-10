using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BugSpawner : MonoBehaviour
{

    public GameObject bugPrefab;

    public List<GameObject> bugs;

    public Transform Cage;



    public float spawnTime;
    public float spawnSize;

    public float forwardAmount;


    public float oldSpawnTime;

    public float spawnRange;
    public Vector2 heightRange;


    public float speed;
    public float drag;
    public float osscilationSize;
    public float life;

    public float dieRate;

    public float maxSpeed;
    public float maxScale;

    public ParticleSystem gotAteParticleSystem;
    public AudioClip gotAteClip;
    public float bugFullnessAdd;
    public float bugStaminaAdd;


    // Start is called before the first frame update
    void Start()
    {
        SpawnNewBug();

    }

    // Update is called once per frame
    void Update()
    {


        if (Time.time - oldSpawnTime > spawnTime)
        {
            SpawnNewBug();
        }


    }


    void SpawnNewBug()
    {



        if (God.wren)
        {


            Vector3 spawnPos = God.wren.transform.position;

            Vector3 offset = Random.insideUnitSphere * spawnSize;

            Vector3 fPos = spawnPos + offset;
            fPos += God.wren.transform.forward * forwardAmount;

            if (Cage != null)
            {
                fPos = new Vector3(
                                Random.Range(-spawnRange, spawnRange) * Cage.lossyScale.x,
                                Random.Range(.9f, 1) * Cage.lossyScale.y,
                                Random.Range(-spawnRange, spawnRange) * Cage.lossyScale.z
                            );
                fPos += Cage.transform.position;

                RaycastHit hit;
                if (Physics.Raycast(fPos, Vector3.up * -1, out hit))
                {
                    fPos = hit.point + Vector3.up * Random.Range(heightRange.x, heightRange.y);
                }
            }


            GameObject bug = Instantiate(bugPrefab, fPos, Quaternion.identity);
            bug.SetActive(true);


            // Get the bug spinning

            Bug bugComp = bug.GetComponent<Bug>();
            bugComp.enabled = true;
            bugComp.bugSpawner = this;

            if (Cage != null)
            {
                bugComp.cage = Cage;
            }

            bugComp.speed = speed;
            bugComp.drag = drag;
            bugComp.osscilationSize = osscilationSize;
            bugComp.life = life;

            bugComp.dieRate = dieRate;

            bugComp.maxSpeed = maxSpeed;
            bugComp.maxScale = maxScale;

            bug.transform.parent = transform;



            oldSpawnTime = Time.time;

        }

    }

    public void BugGotAte(Bug b)
    {


        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;

        God.audio.Play(gotAteClip);


        God.wren.stats.FullnessAdd(bugFullnessAdd);
        God.wren.stats.StaminaAdd(bugStaminaAdd);

    }
}
