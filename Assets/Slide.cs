using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


// Has some text!
public class Slide : MonoBehaviour
{


    public string text;

    public Transform lookTarget;
    public Transform textTarget;


    public bool orbit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set()
    {

        God.cameraManager.slideManager.SetSlide(this);
        God.text.SetInfoText(text, transform);

    }

    public void Release()
    {
        God.cameraManager.slideManager.ReleaseSlide();
        God.text.SetInfoText("");
    }


}
