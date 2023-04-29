using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Reseter : MonoBehaviour
{


    public Wren wren;

    void Update()
    {

        if (wren.input.o_square < .5f && wren.input.square > .5f && wren.state.canTakeOff)
        {
            print("here3");
            Call();
        }

    }

    public void Call()
    {
        print("HERE");
        wren.state.TransportToPosition(God.currentScene.baseStartPosition.position, Vector3.zero);
        wren.state.HitGround();
    }

}
