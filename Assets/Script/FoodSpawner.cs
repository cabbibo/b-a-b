using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class FoodSpawner : MonoBehaviour
{


    public IslandData data;

    public int foodDataIndex;

    public Transform[] foods;

    public Food[] foodComponents;

    public int maxFood;

    public int currentActiveFood = 0;

    public GameObject foodPrefab;

    public int numFoodToSpawnPerBurst = 1;



    public void OnEnable()
    {

        lastFoodSpawnTime = Time.time;

        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        currentActiveFood = 0;

        foods = new Transform[maxFood];
        foodComponents = new Food[maxFood];
        for (int i = 0; i < maxFood; i++)
        {
            foods[i] = Instantiate(foodPrefab).transform;
            foods[i].parent = transform;

            foods[i].gameObject.SetActive(false);
            foodComponents[i] = foods[i].GetComponent<Food>();
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



    public float lastFoodSpawnTime;



    // Update is called once per frame
    void Update()
    {


        float v = data.currentFoodValues[foodDataIndex];

        // print(v);


        if (Time.time - lastFoodSpawnTime > foodSpawnTimeMultiplier / v)
        {

            //            print("helllo");
            SpawnFood();
        }


    }


    public float foodSpawnTimeMultiplier;
    public void SpawnFood()
    {

        Vector3 spawnPosition;

        if (God.wren == null)
        {
            spawnPosition = data.debugValueTransform.position;
        }
        else
        {
            spawnPosition = God.wren.transform.position + God.wren.transform.forward * 10;
        }

        for (int i = 0; i < numFoodToSpawnPerBurst; i++)
        {


            // todo Spawn basedOnMap;
            foods[currentActiveFood].position = spawnPosition + Random.insideUnitSphere * 10;
            foods[currentActiveFood].gameObject.SetActive(true);
            foodComponents[currentActiveFood].OnSpawn(spawnPosition + Random.insideUnitSphere * 10);
            foodComponents[currentActiveFood].spawner = this;


            currentActiveFood++;
            if (currentActiveFood >= maxFood)
            {
                currentActiveFood = 0;
            }
        }

        lastFoodSpawnTime = Time.time;


    }

    public ParticleSystem gotAteParticleSystem;
    public AudioClip[] gotAteClips;

    public float clipVolumeFalloff;
    public float clipPitchLow;
    public float clipPitchHigh;
    public float clipPitchDistanceLow;
    public float clipPitchDistanceHigh;


    public float bugFullnessAdd;
    public float bugStaminaAdd;

    public void GotAte(Food f)
    {

        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = f.transform.position;
        gotAteParticleSystem.transform.LookAt(WrenUtils.God.camera.transform.position);

        float d = (f.transform.position - WrenUtils.God.wren.transform.position).magnitude;
        float pitch = Mathf.Lerp(clipPitchLow, clipPitchHigh, (d - clipPitchDistanceLow) / (clipPitchDistanceHigh - clipPitchDistanceLow));


        WrenUtils.God.audio.Play(gotAteClips, 1, pitch);

        WrenUtils.God.wren.stats.FullnessAdd(bugFullnessAdd);
        WrenUtils.God.wren.stats.StaminaAdd(bugStaminaAdd);


    }

}
