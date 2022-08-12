using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Normal.Realtime.Serialization;
using Normal.Realtime;


public static class RealtimeHelpers {
    public static bool TryGetEntry<T>(this RealtimeDictionary<T> dic, uint key, out T value) where T : IModel, new()
    {
        foreach(var kvp in dic) {
            if (kvp.Key == key) {
                value = kvp.Value;
                return true;
            };
        }
        value = default(T);
        return false;
    }
}