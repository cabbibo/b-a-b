using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ObjectField : MonoBehaviour
{
    public float size;
    public int numObjects;

    public float minSize;
    public float maxSize;
    
    public Transform linkedPosition;
    public GameObject[] possibleObjects;

    public GameObject[] objects;

    public Vector3[] originalPositions;
    public float [] scales;

    // Start is called before the first frame update
    void OnEnable()
    {

        ClearChildren();
        objects = new GameObject[numObjects];
        originalPositions = new Vector3[numObjects];
        scales = new float[numObjects];

        for( var i = 0;  i < numObjects; i++ ){
            objects[i] = GameObject.Instantiate(possibleObjects[Random.Range(0,possibleObjects.Length)]);
            objects[i].transform.parent = transform;
            originalPositions[i] = new Vector3( Random.Range(-size, size ),
                                                Random.Range(-size, size ),
                                                Random.Range(-size, size ));

            print( originalPositions[i] );
            objects[i].transform.position = originalPositions[i];
            objects[i].transform.rotation = Random.rotation;
            scales[i] =  Random.Range(minSize,maxSize); 
            objects[i].transform.localScale = Vector3.one * scales[i]; 
            objects[i].SetActive(true);
        }
         
        
    }

    // Update is called once per frame
    void Update()
    {

        for( var i = 0;  i < numObjects; i++ ){
        if( objects[i].transform.position.x - linkedPosition.position.x  < -size ){
            objects[i].transform.position += Vector3.right * size * 2;
        }

        if( objects[i].transform.position.x - linkedPosition.position.x  > size ){
            objects[i].transform.position -= Vector3.right * size * 2;
        }

        if( objects[i].transform.position.y - linkedPosition.position.y  < -size ){
            objects[i].transform.position += Vector3.up * size * 2;
        }

        if( objects[i].transform.position.y - linkedPosition.position.y  > size ){
            objects[i].transform.position -= Vector3.up * size * 2;
        }

         if( objects[i].transform.position.z - linkedPosition.position.z  < -size ){
            objects[i].transform.position += Vector3.forward * size * 2;
        }

        if( objects[i].transform.position.z - linkedPosition.position.z  > size ){
            objects[i].transform.position -= Vector3.forward * size * 2;
        }
        }
        
    }


    public void ClearChildren()
 {
     Debug.Log(transform.childCount);
     int i = 0;

     //Array to hold all child obj
     GameObject[] allChildren = new GameObject[transform.childCount];

     //Find all child obj and store to that array
     foreach (Transform child in transform)
     {
         allChildren[i] = child.gameObject;
         i += 1;
     }

     //Now destroy them
     foreach (GameObject child in allChildren)
     {
         DestroyImmediate(child.gameObject);
     }

     Debug.Log(transform.childCount);
 }
}
