using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class connectWithLine : MonoBehaviour
{
    // Start is called before the first frame update

    LineRenderer lr;
    public Transform other;
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        lr.SetPosition(0, transform.position );
        lr.SetPosition(1, other.transform.position );
    }
}
