using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShakeBasedOnAudio : MonoBehaviour
{

    public AudioPowerMeasurer audioPowerMeasurer;

    public Vector3 shakeOffset = Vector3.zero;//new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 rotationOffset =  Vector3.zero;//new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 scaleOffset =  Vector3.zero;//new Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 sinAddShake =  Vector3.zero;//nnew Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 sinAddRotation =  Vector3.zero;//nnew Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 sinAddScale=  Vector3.zero;//nnew Vector3(1.0f, 1.0f, 1.0f);

    public Vector3 sinAddShakeSpeed =  Vector3.zero;//nnew Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 sinAddRotationSpeed =  Vector3.zero;//nnew Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 sinAddScaleSpeed =  Vector3.zero;//nnew Vector3(1.0f, 1.0f, 1.0f);



    //public float powerCut
    



    // Start is called before the first frame update
    void OnEnable()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = shakeOffset * audioPowerMeasurer.lerpedPower; // Use lerped power for smoother shake
        transform.localPosition += new Vector3(Mathf.Sin(audioPowerMeasurer.addValue * sinAddShakeSpeed.x) * sinAddShake.x, Mathf.Sin(audioPowerMeasurer.addValue * sinAddShakeSpeed.y) * sinAddShake.y, Mathf.Sin(audioPowerMeasurer.addValue * sinAddShakeSpeed.z) * sinAddShake.z);


        transform.localRotation = Quaternion.Euler(rotationOffset * audioPowerMeasurer.lerpedPower) * Quaternion.Euler(new Vector3(Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.x) * sinAddRotation.x, Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.y) * sinAddRotation.y, Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.z) * sinAddRotation.z));
       // transform.localRotation *= Quaternion.Euler(new Vector3(Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.x) * sinAddRotation.x, Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.y) * sinAddRotation.y, Mathf.Sin(audioPowerMeasurer.addValue * sinAddRotationSpeed.z) * sinAddRotation.z));

        transform.localScale = Vector3.one + scaleOffset * audioPowerMeasurer.lerpedPower; // Use lerped power for smoother shake
        //transform.localScale += new Vector3(Mathf.Sin(audioPowerMeasurer.addValue * sinAddScaleSpeed.x) * sinAddScale.x, Mathf.Sin(audioPowerMeasurer.addValue * sinAddScaleSpeed.y) * sinAddScale.y, Mathf.Sin(audioPowerMeasurer.addValue * sinAddScaleSpeed.z) * sinAddScale.z);
        
    }
}
