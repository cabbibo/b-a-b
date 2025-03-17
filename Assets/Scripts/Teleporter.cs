using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Teleporter : MonoBehaviour
{

    public Booster start;
    public Booster end;


    public void OnStartHit()
    {

    }

    public void OnEndHit()
    {
        God.wren.PhaseShift(start.transform);

    }
}
