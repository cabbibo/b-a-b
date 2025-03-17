using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

[ExecuteAlways]
public class SelfCycle : Cycle
{


    public void OnEnable()
    {

        Reset();
        _Destroy();
        _Create();
        _OnGestate();
        _OnGestated();
        _OnBirth();
        _OnBirthed();
        _OnLive();
    }


    public void LateUpdate()
    {
        if (gestating) { _WhileGestating(1); }
        if (birthing) { _WhileBirthing(1); }
        if (living) { _WhileLiving(1); }
        if (dying) { _WhileDying(1); }

        if (created) { _WhileDebug(); }
    }

    public void OnDisable()
    {
        _WhileDying(1);
        _OnDied();
        _Destroy();
    }


    public override void WhileDebug()
    {
    }
}
