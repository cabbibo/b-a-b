using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CopyWrenPosition : MonoBehaviour
{

    public float sizeQuantize = 1;

    public bool copyRotation = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (God.wren != null)
        {
            transform.position = God.wren.transform.position;


            if (sizeQuantize > 0)
            {
                transform.position = new Vector3(
                     Mathf.Round(transform.position.x * sizeQuantize) / sizeQuantize,
                     Mathf.Round(transform.position.y * sizeQuantize) / sizeQuantize,
                     Mathf.Round(transform.position.z * sizeQuantize) / sizeQuantize);
            }

            if (copyRotation)
            {
                transform.rotation = God.wren.transform.rotation;
            }
        }
    }
}
