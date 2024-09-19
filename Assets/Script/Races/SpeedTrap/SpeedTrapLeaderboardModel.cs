using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime.Serialization;
using Normal.Realtime;

[RealtimeModel]
public partial class SpeedTrapLeaderboardModel
{
    [RealtimeProperty(1, true, true)]
    private RealtimeDictionary<SpeedTrapLeaderboardEntryModel> _speedTrapEntries;
}




