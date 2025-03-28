using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class EtherStateController : MonoBehaviour
{

    public Scene scene;
    public bool allOn;

    public void OnEnable()
    {
        for (int i = 0; i < scene.portals.Length; i++)
        {

            if (allOn)
            {
                scene.portals[i].SetPortalFull();
            }
            else
            {

                /*if (God.state.questsCompleted[i])
                {
                    scene.portals[i].SetPortalFull();
                }
                else
                {
                    scene.portals[i].SetPortalOff();
                }*/


            }
        }
    }


    public void OnDisable()
    {

    }
}
