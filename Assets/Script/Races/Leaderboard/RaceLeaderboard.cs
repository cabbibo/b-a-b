using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

public class RaceLeaderboard : RealtimeComponent<RaceLeaderboardModel>
{
    [SerializeField] private RaceLeaderboardDisplay[] _displays;

    [SerializeField] private bool _debug;
    private float _debugTime;

    public class EntryData : IComparable<EntryData>
    {
        public float time;
        public uint id;
        public bool isLocalUser;

        public EntryData(uint id, float time, bool isLocal)
        {
            this.id = id;
            this.time = time;
            this.isLocalUser = isLocal;
        }

        public int CompareTo(EntryData entry)
        {
            return this.time.CompareTo(entry.time);
        }
    }

    protected override void OnRealtimeModelReplaced(RaceLeaderboardModel previousModel, RaceLeaderboardModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.raceEntries.modelAdded -= OnEntryAdded;
            previousModel.raceEntries.modelRemoved -= OnEntryRemoved;
            currentModel.raceEntries.modelReplaced -= OnEntryReplaced;
        }

        if (currentModel != null)
        {
            // Register for model changes
            currentModel.raceEntries.modelAdded += OnEntryAdded;
            currentModel.raceEntries.modelRemoved += OnEntryRemoved;
            currentModel.raceEntries.modelReplaced += OnEntryReplaced;
            RefreshUI();
        }
    }

    private void OnEntryAdded(RealtimeDictionary<RaceLeaderboardEntryModel> dic, uint key, RaceLeaderboardEntryModel entryModel, bool remote)
    {
        RefreshUI();
    }

    private void OnEntryRemoved(RealtimeDictionary<RaceLeaderboardEntryModel> dic, uint key, RaceLeaderboardEntryModel entryModel, bool remote)
    {
        RefreshUI();
    }

    private void OnEntryReplaced(RealtimeDictionary<RaceLeaderboardEntryModel> dic, uint key, RaceLeaderboardEntryModel oldModel, RaceLeaderboardEntryModel newModel, bool remote)
    {
        RefreshUI();
    }

    public void AddLocalPlayerEntry(float time)
    {
        var playerId = UserIdService.GetLocalUserId();
        AddEntry(playerId, time);
    }

    public void AddEntry(uint playerId, float time)
    {
        var pid = playerId;

        print(playerId);
        print(time);
        // No way to check if key exists in RealtimeDictionary (lol???) so have to do it this way
        foreach (var kvp in model.raceEntries)
        {
            if (kvp.Key == pid)
            {
                // Check if new time is less than previous existing
                if (time >= kvp.Value.raceTime)
                {
                    return;
                }
                break;
            }
        }

        var entry = new RaceLeaderboardEntryModel();
        entry.raceTime = time;
        model.raceEntries[pid] = entry;
    }

    private List<EntryData> GetSortedEntries()
    {
        var localId = UserIdService.GetLocalUserId();
        var entries = new List<EntryData>();
        foreach (var p in model.raceEntries)
        {
            var isLocal = p.Key == localId;
            entries.Add(new EntryData(p.Key, p.Value.raceTime, isLocal));
        }
        entries.Sort();
        return entries;
    }

    public void RefreshUI()
    {
        var entries = GetSortedEntries();

        foreach (var d in _displays)
        {
            d.SetEntries(entries);
        }
    }

#if UNITY_EDITOR
    private void OnGUI() {
        if (_debug) {
            if (GUILayout.Button("Clear")) {
                foreach(var m in model.raceEntries) {
                    model.raceEntries.Remove(m.Key);
                }
            }

            GUILayout.BeginHorizontal();
            if (float.TryParse(GUILayout.TextField(_debugTime.ToString(), GUILayout.MinWidth(128f)), out _debugTime)) {

            }
            if (GUILayout.Button("Add Entry")) {
                AddLocalPlayerEntry(_debugTime);
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Clear User ID")) {
                UserIdService.ClearLocalUserID();
            }

            if (GUILayout.Button("Refresh UI")) {
                RefreshUI();
            }
        }
    }
#endif

}
