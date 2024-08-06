using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class EtherStateController : MonoBehaviour
{

    public Portal[] portals;

    public void OnEnable()
    {
        for (int i = 0; i < portals.Length; i++)
        {

            if (God.state.questsCompleted[i])
            {
                portals[i].SetPortalFull();
            }
            else
            {
                portals[i].SetPortalOff();
            }
        }
    }


    public void OnDisable()
    {

    }
}
