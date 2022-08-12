using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyWrenPosition : MonoBehaviour
{

    public float sizeQuantize = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {if(God.wren != null){
        transform.position = God.wren.transform.position;

        transform.position =  new Vector3(
             Mathf.Round(transform.position.x * sizeQuantize) / sizeQuantize,
             Mathf.Round(transform.position.y * sizeQuantize) / sizeQuantize,
             Mathf.Round(transform.position.z * sizeQuantize) / sizeQuantize);
    }}
}
