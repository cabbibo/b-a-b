using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugSpawner : MonoBehaviour
{

    public GameObject bugPrefab;

    public List<GameObject> bugs;

    public Transform Cage;



    public float spawnTime;
    public float spawnSize;

    public float forwardAmount;


    public float oldSpawnTime;



     public float speed;
    public float drag;
    public float osscilationSize;
    public float life;

    public float dieRate;

    public float maxSpeed;
    public float maxScale;


    // Start is called before the first frame update
    void Start()
    {
        SpawnNewBug();
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if( Time.time - oldSpawnTime > spawnTime ){
            SpawnNewBug();
        }


    }


    void SpawnNewBug(){



        if( God.wren ){

        
            Vector3 spawnPos = God.wren.transform.position;

            Vector3 offset = Random.insideUnitSphere * spawnSize;

            Vector3 fPos = spawnPos + offset;
            fPos += God.wren.transform.forward * forwardAmount;

            if( Cage != null ){
                fPos = new Vector3(
                                Random.Range( -.5f,.5f) * Cage.lossyScale.x,
                                Random.Range( -.5f,.5f) * Cage.lossyScale.y,
                                Random.Range( -.5f,.5f) * Cage.lossyScale.z
                            );
                fPos += Cage.transform.position;
            }


            GameObject bug = Instantiate( bugPrefab , fPos , Quaternion.identity );

            // Get the bug spinning

            Bug bugComp = bug.GetComponent<Bug>();
            bugComp.enabled = true;
            bugComp.bugSpawner = this;

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


    public ParticleSystem gotAteParticleSystem;
    public float bugHealthAdd;
    public void BugGotAte( Bug b ){


        gotAteParticleSystem.Play();
        gotAteParticleSystem.transform.position = b.transform.position;
        print("GOtAteSpawner");

        God.wren.state.HealthAdd( bugHealthAdd );

    }
}
