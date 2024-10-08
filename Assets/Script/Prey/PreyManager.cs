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

    public float spawnDistance;


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


    public int bugsPerCluster = 1;
    public float clusterRadius = 0;


    public enum SpawnType
    {
        RandomNearCenter,
        RandomWithinBounds,
        InFrontOfWren,
        BehindWren,
        Clustered
    }

    public enum AltitudeType
    {
        RandomRange,
        DesiredAltitude,
        OnGround

    }

    public SpawnType spawnType;
    public AltitudeType altitudeType;

    public virtual void SpawnNewBug()
    {



        // destroy any over max
        while (preyHolder.childCount >= maxPray)
        {
            DestroyImmediate(preyHolder.GetChild(0).gameObject);
        }



        Vector3 spawnPos = transform.position;


        if (spawnType == SpawnType.RandomNearCenter)
        {
            spawnPos = SpawnRandomNearCenter();
        }
        else if (spawnType == SpawnType.RandomWithinBounds)
        {
            spawnPos = SpawnRandomWithinBounds();
        }
        else if (spawnType == SpawnType.InFrontOfWren)
        {
            spawnPos = SpawnInFrontOfWren();
        }

        Vector3 groundPos = spawnPos;
        groundPos.y = 10000;
        RaycastHit hit;
        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 20000))
        {
            groundPos = hit.point + Vector3.up * spawnRadius * 2;
        }

        if (altitudeType == AltitudeType.RandomRange)
        {
            spawnPos.y = groundPos.y + preyConfig.minAltitude + Random.Range(0, preyConfig.maxAltitude - preyConfig.minAltitude);
        }
        else if (altitudeType == AltitudeType.DesiredAltitude)
        {
            spawnPos.y = groundPos.y + preyConfig.desiredAltitude;
        }
        else if (altitudeType == AltitudeType.OnGround)
        {
            spawnPos.y = groundPos.y;
        }

        for (int i = 0; i < bugsPerCluster; i++)
        {

            Vector3 extraOffset = Random.insideUnitSphere * clusterRadius;
            spawnPos += extraOffset;

            PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

            newPrey.Initialize(preyConfig, this);


            newPrey.transform.parent = preyHolder;


            lastSpawnTime = Time.time;
        }

        // }
    }


    public Vector3 SpawnRandomNearCenter()
    {
        Vector3 spawnPos = transform.position;

        Vector3 offset = Random.insideUnitSphere * spawnRadius;

        spawnPos += offset;

        return spawnPos;
    }


    public Vector3 rangeBoundsForSpawnMin;
    public Vector3 rangeBoundsForSpawnMax;
    public Vector3 SpawnRandomWithinBounds()
    {

        Vector3 spawnPos = new Vector3(Random.Range(rangeBoundsForSpawnMin.x, rangeBoundsForSpawnMax.x), Random.Range(rangeBoundsForSpawnMin.y, rangeBoundsForSpawnMax.y), Random.Range(rangeBoundsForSpawnMin.z, rangeBoundsForSpawnMax.z));

        return spawnPos;

    }

    public Vector3 SpawnInFrontOfWren()
    {
        Vector3 spawnPos = God.wren.transform.position + God.wren.transform.forward * spawnDistance;

        spawnPos.y = 10000;

        RaycastHit hit;
        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 20000))
        {
            spawnPos = hit.point + Vector3.up * spawnRadius * 2;
        }


        Vector3 offset = Random.insideUnitSphere * spawnRadius;

        spawnPos += offset;

        return spawnPos;

    }




    public virtual void PreyGotAte(PreyController b)
    {
        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;

        God.audio.Play(gotAteClip);

        God.wren.stats.FullnessAdd(preyFullnessIncrease);

        God.wren.shards.CollectShards(b.numCrystals, b.crystalType, b.transform.position);
        // God.wren.stats.StaminaAdd(preyStaminaIncrease);

    }

}