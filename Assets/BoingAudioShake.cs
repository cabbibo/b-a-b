using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class BoingAudioShake : MonoBehaviour
{

    
    public float dampening = .9f;
    public float returnForce = 1;
    public float startForce = 1;

    public AudioPowerMeasurer audioPowerMeasurer;
    public Vector3 shakeOffset = Vector3.zero;//new Vector3(0.1f, 0.1f, 0.1f);
    
    Vector3 startPos;

    Vector3 velocity;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {  
        
        Vector3 force = (Vector3.zero - transform.localPosition) * returnForce;

        force += shakeOffset * audioPowerMeasurer.lerpedPower; // Use lerped power for smoother shake
        velocity += force;
        velocity *= dampening;


        transform.localPosition += velocity;


//Quaternion q = Quaternion.LookRotation(Vector3.zero - transform.position + velocity * cameraVelocityLook - Camera.main.transform.forward * cameraForwardLook);


//transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10);
        
    }
}



