using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class OsscilateRotate : MonoBehaviour
{

    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(transform.right * .03f * speed);
        transform.Rotate(transform.up * .03f * speed);
        transform.Rotate(transform.forward * .03f * speed);
    }
}
