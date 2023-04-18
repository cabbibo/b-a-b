using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class ToggleInvert : MonoBehaviour
{


    public GameObject checkMark;

    public void OnEnable()
    {

        print("values");
        print(God.input.invertY);
        if (God.input.invertY == true)
        {
            checkMark.SetActive(true);
        }
        else
        {
            checkMark.SetActive(false);
        }
    }


    public void Toggle()
    {
        God.input.invertY = !God.input.invertY;
        if (God.input.invertY == true)
        {
            checkMark.SetActive(true);
        }
        else
        {
            checkMark.SetActive(false);
        }
    }


}
