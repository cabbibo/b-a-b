using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class FloorIsLava : MonoBehaviour
{

    public Transform startLocation;

    public LavaOfTheFloor[] lavas;


    public FloorIsLavaActivityCaller activityCaller;


    // Start is called before the first frame update
    void Start()
    {
        foreach (LavaOfTheFloor lava in lavas)
        {
            lava.floorIsLava = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnIsLavaHit()
    {
        Debug.Log("Floor is lava!");
        God.wren.PhaseShift(startLocation);
        activityCaller.OnLavaHit();
    }
}
