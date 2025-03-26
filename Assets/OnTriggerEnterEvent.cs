using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

public class OnTriggerEnterEvent : MonoBehaviour
{

    public UnityEvent onTriggerEnterEvent;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");

        if (God.IsOurWren(other))
        {
            onTriggerEnterEvent.Invoke();
        }
    }


}
