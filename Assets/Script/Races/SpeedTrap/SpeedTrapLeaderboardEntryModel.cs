using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime.Serialization;
using Normal.Realtime;

[RealtimeModel]
public partial class SpeedTrapLeaderboardEntryModel
{
    [RealtimeProperty(1, true)]
    private float _speedTrapTime;
}


