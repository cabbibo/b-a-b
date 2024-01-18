using UnityEngine;
using WrenUtils;


public class PreyManager : MonoBehaviour
{


    public Transform debugWren;


    [Header("Spawn Settings")]
    public float spawnTime;

    [SerializeField]
    GameObject preyPrefab;

    [SerializeField]
    float spawnRadius;

    [SerializeField]
    Vector2 spawnHeight;

    [SerializeField]
    Vector2 spawnFoward;



    [SerializeField]
    float lastSpawnTime;


    [Header("On Eat Effects")]
    [SerializeField]
    AudioClip gotAteClip;

    [SerializeField]
    ParticleSystem gotAteParticleSystem;

    [SerializeField]
    float preyFullnessIncrease;

    [SerializeField]
    float preyStaminaIncrease;

    public PreyConfigSO preyConfig;


    public bool wrenInside;

    public Transform preyHolder;



    public Transform cage;




    public void OnEnable()
    {
        lastSpawnTime = Time.time;

        while (preyHolder.childCount > 0)
        {
            DestroyImmediate(preyHolder.GetChild(0).gameObject);
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (God.IsOurWren(other))
        {
            wrenInside = true;
        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (God.IsOurWren(other))
        {
            wrenInside = false;
        }
    }

    void Update()
    {
        if (Time.time - lastSpawnTime > spawnTime && wrenInside)
        {
            SpawnNewBug();
        }
    }

    void SpawnNewBug()
    {

        /* if (God.wren)
         {

             Vector3 spawnPos = God.wren.transform.position;
             spawnPos += Random.Range(spawnFoward.x, spawnFoward.y) * God.wren.transform.forward;
             spawnPos += Random.insideUnitSphere * spawnRadius;

             spawnPos.y += 1000;
             RaycastHit hit;
             if (Physics.Raycast(spawnPos, Vector3.down, out hit, 2000, 1 << 8))
             {
                 spawnPos = hit.point;
             }
             else
             {
                 return;
             }


             spawnPos += Vector3.up * Random.Range(spawnHeight.x, spawnHeight.y);



             PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

             newPrey.Initialize(preyConfig, this);


             newPrey.transform.parent = preyHolder;
             lastSpawnTime = Time.time;


         }
         else
         {
 */
        Vector3 spawnPos = transform.position;

        Vector3 offset = Random.insideUnitSphere * spawnRadius;

        spawnPos += offset;

        PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

        newPrey.Initialize(preyConfig, this);


        newPrey.transform.parent = preyHolder;
        lastSpawnTime = Time.time;

        // }
    }


    public void PreyGotAte(PreyController b)
    {
        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;

        God.audio.Play(gotAteClip);

        God.wren.stats.FullnessAdd(preyFullnessIncrease);
        God.wren.stats.StaminaAdd(preyStaminaIncrease);

    }
}