using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LighthouseSpin : MonoBehaviour
{
    public float speed = 1;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        transform.Rotate( Vector3.up * Time.deltaTime * speed );

    }
}