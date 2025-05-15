using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ActivityManager : MonoBehaviour
{
    public List<Activity> activities = new();

    public void Initialize()
    {
        GetAllActivities();

        for ( int i = 0; i < activities.Count; i++ ) {
            activities[i].Initialize();
        }
    }

    public void OnEnable()
    {
        GetAllActivities();

    }

    public void GetAllActivities()
    {
        activities.Clear();
        var allActivities = FindObjectsOfType<Activity>();

        for ( int i = 0; i < allActivities.Length; i++ ) {
            activities.Add( allActivities[i] );
        }
    }
}