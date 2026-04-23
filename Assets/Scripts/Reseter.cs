using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Reseter : MonoBehaviour
{


    public void Reset()
    {
        God.wren.state.TransportToPosition(God.currentScene.baseStartPosition.position, Vector3.zero);
        God.wren.state.HitGround();
    }

}
