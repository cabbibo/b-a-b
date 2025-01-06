using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenCanDo : MonoBehaviour
{
    public bool hover;
    public bool boost;
    public bool ping;
    public bool disintegrate;

    public bool call;
    public bool magnitize;
    public bool placeBeacon;
    public bool rewind;

    public bool carry;


    public void SaveState()
    {
        PlayerPrefs.SetInt("hover", hover ? 1 : 0);
        PlayerPrefs.SetInt("boost", boost ? 1 : 0);
        PlayerPrefs.SetInt("ping", ping ? 1 : 0);
        PlayerPrefs.SetInt("disintegrate", disintegrate ? 1 : 0);

        PlayerPrefs.SetInt("call", call ? 1 : 0);
        PlayerPrefs.SetInt("magnitize", magnitize ? 1 : 0);
        PlayerPrefs.SetInt("placeBeacon", placeBeacon ? 1 : 0);
        PlayerPrefs.SetInt("rewind", rewind ? 1 : 0);

        PlayerPrefs.SetInt("carry", carry ? 1 : 0);
    }

    public void LoadState()
    {

        hover = PlayerPrefs.GetInt("hover", 0) == 1;
        boost = PlayerPrefs.GetInt("boost", 0) == 1;
        ping = PlayerPrefs.GetInt("ping", 0) == 1;
        disintegrate = PlayerPrefs.GetInt("disintegrate", 0) == 1;

        call = PlayerPrefs.GetInt("call", 0) == 1;
        magnitize = PlayerPrefs.GetInt("magnitize", 0) == 1;
        placeBeacon = PlayerPrefs.GetInt("placeBeacon", 0) == 1;
        rewind = PlayerPrefs.GetInt("rewind", 0) == 1;

        carry = PlayerPrefs.GetInt("carry", 0) == 1;

    }




}
