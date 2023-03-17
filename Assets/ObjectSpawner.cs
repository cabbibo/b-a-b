using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ObjectSpawner : MonoBehaviour
{

    public int numberOfObjects;
    public Vector2 scaleRange;

    public Vector2 distRange;

    public float amountShown;


    public GameObject[] prefabs;

    public GameObject[] spawned;

    void OnEnable(){


        
       for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
     

        spawned = new GameObject[numberOfObjects];

        for( int i = 0; i < numberOfObjects; i++ ){

            GameObject prefab = prefabs[Random.Range(0,prefabs.Length)];
            spawned[i] = Instantiate(prefab);
            spawned[i].transform.parent = transform;
            spawned[i].transform.rotation = Random.rotation;
            spawned[i].transform.position = Random.insideUnitSphere * Random.Range(distRange.x , distRange.y);
            spawned[i].transform.localScale *= Random.Range(scaleRange.x , scaleRange.y);

        }


    }

    

    // Update is called once per frame
    void Update()
    {
        int id = (int)(amountShown * (float)numberOfObjects);
        

        for( int i = 0; i < numberOfObjects; i++ ){
            if( i < id ){
                spawned[i].SetActive(true);
            }else{
                spawned[i].SetActive(false);
            }
        }

    }



}
