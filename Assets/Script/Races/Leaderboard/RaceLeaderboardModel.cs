using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime.Serialization;
using Normal.Realtime;

[RealtimeModel]
public partial class RaceLeaderboardModel
{
    [RealtimeProperty(1, true, true)]
    private RealtimeDictionary<RaceLeaderboardEntryModel> _raceEntries;
}




