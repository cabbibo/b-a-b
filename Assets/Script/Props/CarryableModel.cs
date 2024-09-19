using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Normal.Realtime.Serialization;
using Normal.Realtime;

[RealtimeModel]
public partial class CarryableModel
{
    [RealtimeProperty(1, true, true)]
    private bool _beingCarried;

    [RealtimeProperty(2, true, true)]
    private int _lastCarrierId;
}


