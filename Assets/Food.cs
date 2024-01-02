using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class Food : MonoBehaviour
{
    public FoodSpawner spawner;

    public float fadeInTime;
    public float fadeOutTime;
    public float liveTime;


    public float spawnTime;
    public bool active;


    public Vector3 spawnPosition;
    public void OnSpawn(Vector3 newPosition)
    {

        spawnPosition = newPosition;


        if (active == true)
        {
            //StartCoroutine(Despawn());
        }
        else
        {
            Respawn();
        }


    }

    /*public IEnumerator Despawn()
    {
        yield return new WaitForSeconds(liveTime);
        active = false;
        gameObject.SetActive(false);
    }
*/

    public void Respawn()
    {
        spawnTime = Time.time;
        transform.position = spawnPosition;
        transform.localScale = Vector3.zero;
        active = true;
        gameObject.SetActive(true);
    }



    public float lastFoodSpawnTime;


    public float maxScale = 1;

    // Update is called once per frame
    void Update()
    {
        if (Time.time - spawnTime < fadeInTime)
        {
            transform.localScale = maxScale * Vector3.one * (Time.time - spawnTime) / fadeInTime;
        }

        if (Time.time - spawnTime > liveTime + fadeInTime && Time.time - spawnTime < liveTime + fadeOutTime + fadeInTime)
        {

            transform.localScale = maxScale * Vector3.one * (1 - ((Time.time - (spawnTime + liveTime + fadeInTime))) / fadeOutTime);
        }

        if (Time.time - spawnTime > fadeInTime + liveTime + fadeOutTime)
        {
            active = false;
            gameObject.SetActive(false);
        }

        DoMove();

    }

    public virtual void DoMove()
    {

    }


    void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {
            spawner.GotAte(this);
        }
    }



}
