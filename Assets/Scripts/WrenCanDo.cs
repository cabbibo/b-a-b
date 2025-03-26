using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenCanDo : MonoBehaviour
{
    public bool takeOff;
    public bool hover;
    public bool boost;
    public bool ping;
    public bool disintegrate;

    public bool call;
    public bool magnitize;
    public bool placeBeacon;
    public bool rewind;

    public bool carry;


    public bool hasLearnedFlight;
    public bool hasLearnedTakeOff;
    public bool hasLearnedHover;
    public bool hasLearnedBoost;
    public bool hasLearnedPing;
    public bool hasLearnedDisintegrate;

    public bool hasLearnedCall;
    public bool hasLearnedMagnitize;
    public bool hasLearnedPlaceBeacon;
    public bool hasLearnedRewind;

    public bool hasLearnedCarry;


    public void SaveState()
    {
        PlayerPrefs.SetInt("takeOff", takeOff ? 1 : 0);
        PlayerPrefs.SetInt("hover", hover ? 1 : 0);
        PlayerPrefs.SetInt("boost", boost ? 1 : 0);
        PlayerPrefs.SetInt("ping", ping ? 1 : 0);
        PlayerPrefs.SetInt("disintegrate", disintegrate ? 1 : 0);

        PlayerPrefs.SetInt("call", call ? 1 : 0);
        PlayerPrefs.SetInt("magnitize", magnitize ? 1 : 0);
        PlayerPrefs.SetInt("placeBeacon", placeBeacon ? 1 : 0);
        PlayerPrefs.SetInt("rewind", rewind ? 1 : 0);

        PlayerPrefs.SetInt("carry", carry ? 1 : 0);




        PlayerPrefs.SetInt("hasLearnedFlight", hasLearnedFlight ? 1 : 0);

        PlayerPrefs.SetInt("hasLearnedTakeOff", hasLearnedTakeOff ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedHover", hasLearnedHover ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedBoost", hasLearnedBoost ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedPing", hasLearnedPing ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedDisintegrate", hasLearnedDisintegrate ? 1 : 0);

        PlayerPrefs.SetInt("hasLearnedCall", hasLearnedCall ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedMagnitize", hasLearnedMagnitize ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedPlaceBeacon", hasLearnedPlaceBeacon ? 1 : 0);
        PlayerPrefs.SetInt("hasLearnedRewind", hasLearnedRewind ? 1 : 0);

        PlayerPrefs.SetInt("hasLearnedCarry", hasLearnedCarry ? 1 : 0);




    }

    public void LoadState()
    {
        takeOff = PlayerPrefs.GetInt("takeOff", 0) == 1;
        hover = PlayerPrefs.GetInt("hover", 0) == 1;
        boost = PlayerPrefs.GetInt("boost", 0) == 1;
        ping = PlayerPrefs.GetInt("ping", 0) == 1;
        disintegrate = PlayerPrefs.GetInt("disintegrate", 0) == 1;

        call = PlayerPrefs.GetInt("call", 0) == 1;
        magnitize = PlayerPrefs.GetInt("magnitize", 0) == 1;
        placeBeacon = PlayerPrefs.GetInt("placeBeacon", 0) == 1;
        rewind = PlayerPrefs.GetInt("rewind", 0) == 1;

        carry = PlayerPrefs.GetInt("carry", 0) == 1;

        hasLearnedFlight = PlayerPrefs.GetInt("hasLearnedFlight", 0) == 1;
        hasLearnedHover = PlayerPrefs.GetInt("hasLearnedHover", 0) == 1;
        hasLearnedBoost = PlayerPrefs.GetInt("hasLearnedBoost", 0) == 1;
        hasLearnedPing = PlayerPrefs.GetInt("hasLearnedPing", 0) == 1;
        hasLearnedDisintegrate = PlayerPrefs.GetInt("hasLearnedDisintegrate", 0) == 1;

        hasLearnedCall = PlayerPrefs.GetInt("hasLearnedCall", 0) == 1;
        hasLearnedMagnitize = PlayerPrefs.GetInt("hasLearnedMagnitize", 0) == 1;
        hasLearnedPlaceBeacon = PlayerPrefs.GetInt("hasLearnedPlaceBeacon", 0) == 1;
        hasLearnedRewind = PlayerPrefs.GetInt("hasLearnedRewind", 0) == 1;

        hasLearnedCarry = PlayerPrefs.GetInt("hasLearnedCarry", 0) == 1;



    }


    public void ResetState()
    {

        takeOff = false;
        hover = false;
        boost = false;
        ping = false;
        disintegrate = false;

        call = false;
        magnitize = false;
        placeBeacon = false;
        rewind = false;

        carry = false;

        hasLearnedFlight = false;

        hasLearnedHover = false;
        hasLearnedBoost = false;
        hasLearnedPing = false;
        hasLearnedDisintegrate = false;

        hasLearnedCall = false;
        hasLearnedMagnitize = false;
        hasLearnedPlaceBeacon = false;
        hasLearnedRewind = false;

        hasLearnedCarry = false;


    }





}


