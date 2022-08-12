using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class connectSuntoSunShaftsPOSTFX : MonoBehaviour
{

    public Transform sun;
    PostProcessProfile postProfile;
    // Start is called before the first frame update
    void Start()
    {
        postProfile = GetComponent<PostProcessVolume>().profile;
    }

    // Update is called once per frame
    void Update()
    {
        if(sun != null)
        {
            var sunShats = postProfile.GetSetting<SunShaftsHDRP>();
            if (sunShats != null)
            {
                sunShats.sunTransform.value = sun.transform.position;
            }
        }
    }
}
