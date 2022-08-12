using System;
using UnityEngine;
using UnityEngine.UI;

public static class UserIdService
{
    private const string idKey = "userId";
    private const int maxIdVal = 268435455;

    private static uint? _localPlayerId;

    private static uint GetGuid()
    {
        return (uint)UnityEngine.Random.Range(0, maxIdVal);
    }

    public static uint GetLocalUserId()
    {
        if (_localPlayerId != null) {
            return _localPlayerId.Value;
        }

        if (PlayerPrefs.HasKey(idKey)) {
            _localPlayerId = (uint)PlayerPrefs.GetInt(idKey);
        } else {
            _localPlayerId = GetGuid();
            PlayerPrefs.SetInt(idKey, (int)_localPlayerId.Value);
        }

        return _localPlayerId.Value;
    }

    public static bool IsLocalUser(uint userId) {
        return userId == GetLocalUserId();
    }

    public static void ClearLocalUserID() {
        _localPlayerId = null;
        PlayerPrefs.DeleteKey(idKey);
    }
}