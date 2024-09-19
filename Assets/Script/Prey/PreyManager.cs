using UnityEngine;
using WrenUtils;

[ExecuteAlways]
public class PreyManager : MonoBehaviour
{
    /*

        Can spawn in clumps for butterflies etc.

    */

    public Transform debugWren;

    public bool spawnMaxOnWrenEnter;


    [Header("Spawn Settings")]
    public float spawnTime;


    public GameObject preyPrefab;

    public float spawnRadius;

    public Vector2 spawnHeight;


    public Vector2 spawnFoward;



    public float lastSpawnTime;


    [Header("On Eat Effects")]
    public AudioClip gotAteClip;

    public ParticleSystem gotAteParticleSystem;

    public float preyFullnessIncrease;

    public float preyStaminaIncrease;

    public PreyConfigSO preyConfig;


    public bool wrenInside;

    public Transform preyHolder;



    public Transform cage;

    public int maxPray = 100;

    public Transform[] spawnPoints;
    public Transform[] objectsOfInterest;



    public bool wrenEnterOnEnabled;
    public void OnEnable()
    {
        lastSpawnTime = Time.time - spawnTime;
        while (preyHolder.childCount > 0)
        {
            DestroyImmediate(preyHolder.GetChild(0).gameObject);
        }

        if (wrenEnterOnEnabled)
        {
            OnWrenEnter();
        }




    }

    public void OnTriggerEnter(Collider other)
    {
        if (God.IsOurWren(other))
        {

            OnWrenEnter();

        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (God.IsOurWren(other))
        {
            OnWrenExit();

        }
    }


    public int currentNumberOfPrey;
    void Update()
    {

        currentNumberOfPrey = preyHolder.childCount;

        CheckForNewPrey();

    }

    public virtual void CheckForNewPrey()
    {
        if (Time.time - lastSpawnTime > spawnTime && wrenInside)
        {
            SpawnNewBug();
        }
    }


    public void OnWrenEnter()
    {
        wrenInside = true;
        /*
                while (preyHolder.childCount > 0)
                {
                    DestroyImmediate(preyHolder.GetChild(0).gameObject);
                }

                if (spawnMaxOnWrenEnter)
                {
                    while (preyHolder.childCount < maxPray)
                    {
                        SpawnNewBug();
                    }
                }*
                */
    }

    public void OnWrenExit()
    {
        wrenInside = false;
        /*
                while (preyHolder.childCount > 0)
                {
                    DestroyImmediate(preyHolder.GetChild(0).gameObject);
                }
          */


    }

    public virtual void SpawnNewBug()
    {



        // destroy any over max
        while (preyHolder.childCount >= maxPray)
        {
            DestroyImmediate(preyHolder.GetChild(0).gameObject);
        }


        Vector3 spawnPos = transform.position;

        Vector3 offset = Random.insideUnitSphere * spawnRadius;

        spawnPos += offset;

        PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

        newPrey.Initialize(preyConfig, this);


        newPrey.transform.parent = preyHolder;


        lastSpawnTime = Time.time;

        // }
    }


    public virtual void PreyGotAte(PreyController b)
    {
        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;

        God.audio.Play(gotAteClip);

        God.wren.stats.FullnessAdd(preyFullnessIncrease);
        God.wren.stats.StaminaAdd(preyStaminaIncrease);

    }

}