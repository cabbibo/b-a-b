using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Teleporter : MonoBehaviour
{
    public Booster start;
    public Booster end;

    public Vector2 wormholeSpeed = Vector2.one * .1f;

    public bool bidirectional = true;

    public void OnStartHit()
    {
        God.audio.Play( God.sounds.teleportClip );
        God.postController.WormHole( OnWormholeToEnd , wormholeSpeed );
        // God.wren.PhaseShift( start.transform );
    }

    public void OnEndHit()
    {

        God.audio.Play( God.sounds.teleportClip );
        God.postController.WormHole( OnWormholeToStart , wormholeSpeed );
        // God.wren.PhaseShift( start.transform );

    }

    public void OnWormholeToStart()
    {
        God.wren.PhaseShift( start.transform );

    }

    public void OnWormholeToEnd()
    {
        God.wren.PhaseShift( start.transform );

    }
}