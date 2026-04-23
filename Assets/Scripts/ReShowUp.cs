using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReShowUp : MonoBehaviour
{

    public Transform tracker;
    public float radius;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( (transform.position - tracker.position).magnitude > radius ){
            transform.position = tracker.position + tracker.forward * radius;
        }
    }
}
