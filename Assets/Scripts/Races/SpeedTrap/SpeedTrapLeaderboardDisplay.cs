using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTrapLeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TextMesh _allEntriesText;

    public void SetEntries (List<SpeedTrapLeaderboard.EntryData> entries) {

        var rend = _allEntriesText.GetComponent<MeshRenderer>();
        var mats = new List<Material>(entries.Count + 1);
        mats.Add(rend.sharedMaterial);
        
        var text = "";
        int count = 0;
        
        foreach(var entry in entries) {
            count ++;

            //var lineText = entry.id + ": " + entry.time;
            //var lineText = count + ": " + entry.time;
            
            var mat = NetworkStorage.Instance.GetIconMaterial(entry.id);
            mats.Add(mat);

            string lineText = "<quad material=" + count + " size=20 x=0.0 y=0.0 width=1.0 height=1.0/>";

            string timeText = "" +entry.time;

            if (entry.isLocalUser) {
                timeText = "<color=\"blue\">" + timeText + "</color>";
            }
            text += lineText + " " + timeText + "\n";
        }
        
        text = text.TrimEnd('\n');

        rend.sharedMaterials = mats.ToArray();

        _allEntriesText.text = text;
    }
}
