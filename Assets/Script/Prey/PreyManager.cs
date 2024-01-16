using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class PreyManager : MonoBehaviour
{


    [Header("Spawn Settings")]
    public float spawnTime;

    [SerializeField]
    GameObject preyPrefab;

    [SerializeField]
    float spawnRadius;

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

    void Update()
    {
        if (Time.time - lastSpawnTime > spawnTime)
        {
            SpawnNewBug();
        }
    }

    void SpawnNewBug()
    {
        if (God.wren)
        {

            Vector3 spawnPos = God.wren.transform.position;

            Vector3 offset = Random.insideUnitSphere * spawnRadius;

            spawnPos += offset;

            PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

            newPrey.Initialize(preyConfig, this);


            newPrey.transform.parent = transform;
            lastSpawnTime = Time.time;
        }
        else
        {

            Vector3 spawnPos = transform.position;

            Vector3 offset = Random.insideUnitSphere * spawnRadius;

            spawnPos += offset;

            PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

            newPrey.Initialize(preyConfig, this);


            newPrey.transform.parent = transform;
            lastSpawnTime = Time.time;

        }
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