using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class PersistentWrenModel
{
    [RealtimeProperty(1, true)]
    private uint _playerID;
    


    [RealtimeProperty(2, true, true)]
    private float _hue1;

    [RealtimeProperty(3, true, true)]
    private float _hue2;

    [RealtimeProperty(4, true, true)]
    private float _hue3;

    [RealtimeProperty(5, true, true)]
    private float _hue4;
}


