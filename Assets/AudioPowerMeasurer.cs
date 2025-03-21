using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class AudioPowerMeasurer : MonoBehaviour
{

    public AudioSource audioSource;

    public float totalPower;

    public float addValue;

    public float lerpedPower;
    public float lerpPowerSpeed;


    public float[] values;
    public int totalCount;
    public int totalChannels;



    public void OnEnable()
    {
        if( audioSource == null ){
            audioSource = GetComponent<AudioSource>();
        }
        totalPower = 0;
        addValue = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {


            for (int i = 0; i < totalCount; i++)
            {

                float n = (float)i / (float)totalCount;
                totalPower += Mathf.Abs(values[i * totalChannels]);

            }

            totalPower /= (float)totalCount;
            addValue += totalPower;


            lerpedPower = Mathf.Lerp(lerpedPower, totalPower, lerpPowerSpeed);
          
        

    }

    void OnAudioFilterRead(float[] data, int channels)
    {

  
            totalChannels = channels;
            totalCount = data.Length / channels;
            if (values == null || values.Length != data.Length)
            {
                values = new float[totalCount * channels];
            }

            Array.Copy(data, 0, values, 0, data.Length);


    }
}
