using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


public class FoodSpawner : MonoBehaviour
{
    public IslandController data;

    public int foodDataIndex;
    public int biomeDataIndex;

    public Transform[] foods;

    public Food[] foodComponents;

    public int maxFood;

    public int currentActiveFood = 0;

    public GameObject[] foodPrefabs;

    public int numFoodToSpawnPerBurst = 1;


    public void OnEnable()
    {

        lastFoodSpawnTime = Time.time;

        while (transform.childCount > 0) DestroyImmediate( transform.GetChild( 0 ).gameObject );

        currentActiveFood = 0;

        print( gameObject.name );
        // Print every up the whole hierarchy
        bool isTop = false;
        var t = transform;

        while (!isTop)
            if ( t.parent == null ) {
                isTop = true;
            } else {
                t = t.parent;
                print( t.name );
            }


        foods = new Transform[maxFood];
        foodComponents = new Food[maxFood];

        for ( int i = 0; i < maxFood; i++ ) {
            foods[i] = Instantiate( foodPrefabs[Random.Range( 0 , foodPrefabs.Length )] ).transform;
            foods[i].parent = transform;

            foods[i].gameObject.SetActive( false );
            foodComponents[i] = foods[i].GetComponent<Food>();
        }

    }


    private void OnDisable()
    {
        for ( int i = transform.childCount; i > 0; --i ) {
            DestroyImmediate( transform.GetChild( 0 ).gameObject );
        }
    }

    private void Destroy()
    {
        for ( int i = transform.childCount; i > 0; --i ) {
            DestroyImmediate( transform.GetChild( 0 ).gameObject );
        }
    }


    public float lastFoodSpawnTime;


    // Update is called once per frame
    private void Update()
    {


        float v = data.currentIsland.currentFoodValues[foodDataIndex];
        float v2 = data.currentIsland.currentBiomeValues[biomeDataIndex];

        // print(v);


        if ( Time.time - lastFoodSpawnTime > foodSpawnTimeMultiplier / v && v2 > 0 ) {

            //            print("helllo");
            SpawnFood();
        }


    }


    public float foodSpawnTimeMultiplier;
    public float foodForwardDistance;
    public float foodRadiusSpawn;

    public void SpawnFood()
    {

        Vector3 spawnPosition;

        if ( God.wren == null ) {
            spawnPosition = data.currentIsland.debugValueTransform.position;
        } else {
            spawnPosition = God.wren.transform.position + God.wren.transform.forward * foodForwardDistance;
        }

        for ( int i = 0; i < numFoodToSpawnPerBurst; i++ ) {


            // todo Spawn basedOnMap;
            foods[currentActiveFood].position = spawnPosition + Random.insideUnitSphere * 10;
            foods[currentActiveFood].gameObject.SetActive( true );
            foodComponents[currentActiveFood].OnSpawn( spawnPosition + Random.insideUnitSphere * 10 );
            foodComponents[currentActiveFood].spawner = this;


            currentActiveFood++;

            if ( currentActiveFood >= maxFood ) {
                currentActiveFood = 0;
            }
        }

        lastFoodSpawnTime = Time.time;


    }

    public ParticleSystem gotAteParticleSystem;
    public AudioClip[]    gotAteClips;

    public float clipVolumeFalloff;
    public float clipPitchLow;
    public float clipPitchHigh;
    public float clipPitchDistanceLow;
    public float clipPitchDistanceHigh;


    public float bugFullnessAdd;
    public float bugStaminaAdd;

    public int numCrystalsOnEat;

    public void GotAte( Food f )
    {

        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = f.transform.position;
        gotAteParticleSystem.transform.LookAt( God.camera.transform.position );

        float d = (f.transform.position - God.wren.transform.position).magnitude;
        float pitch = Mathf.Lerp( clipPitchLow , clipPitchHigh ,
            (d - clipPitchDistanceLow) / (clipPitchDistanceHigh - clipPitchDistanceLow) );


        God.audio.Play( gotAteClips , 1 , pitch );

        God.wren.stats.FullnessAdd( bugFullnessAdd );
        God.wren.stats.StaminaAdd( bugStaminaAdd );

        God.wren.shards.CollectShards( numCrystalsOnEat , (float)biomeDataIndex , f.transform.position );


    }
}