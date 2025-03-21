using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PlacePoints : MonoBehaviour
{

    public int numToCreate;
    public GameObject prefab;
    public float radius;

    public float minScale;
    public float maxScale;

    public Transform lookTarget;
    public float rotationRandomnees;

    public void OnEnable(){

  

while( transform.childCount > 0){
    GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
}

        for( int i = 0; i < numToCreate;i++){

            Vector3 pos = Random.insideUnitSphere * radius;
           // pos.y = 0;
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            go.transform.LookAt(lookTarget);
            go.transform.Rotate( Random.Range(-rotationRandomnees, rotationRandomnees), Random.Range(-rotationRandomnees, rotationRandomnees),  Random.Range(-rotationRandomnees, rotationRandomnees));

            go.transform.parent = transform;


        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
