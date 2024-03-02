using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenDebugInfo : MonoBehaviour
{

    public Wren wren;

    public WrenPhysics physics;
    public WrenStats stats;
    public FullBird bird;


    public bool showStatsInterface;
    public bool showBirdPoints;
    public bool showBirdFeatherPoints;

    public bool showForces;

    // Update is called once per frame
    void Update()
    {

        physics.showDebugForces = showForces;
        stats.debugStats = showStatsInterface;


    }
}
