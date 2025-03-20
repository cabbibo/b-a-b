using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class OsscilateRotate : MonoBehaviour
{

    public float speed = 1;

    public Vector3 speedVector = new Vector3(1,1,1);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right * .03f * speed * speedVector.x);
        transform.Rotate(Vector3.up * .03f * speed* speedVector.y);
        transform.Rotate(Vector3.forward * .03f * speed* speedVector.z);
    }
}
