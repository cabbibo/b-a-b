using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenInterfaceUtils : MonoBehaviour
{

    public InterfaceRing[] interfaceRings;

    public InterfacePointer interfacePointer;

    public WrenCompass wrenCompass;

    public int crystalsSpentPerPing = 10;




    // start with nothing on 
    public void OnEnable()
    {

        for (int i = 0; i < interfaceRings.Length; i++)
        {
            TurnOffRing(i);
        }

        TurnOffCompass();

    }


    public void PingAll()
    {

        God.audio.Play(God.sounds.interfacePingClip, 1, 1);
        God.wren.shards.SpendShards(crystalsSpentPerPing);
        for (int i = 0; i < interfaceRings.Length; i++)
        {
            PingRing(i);
        }

        interfacePointer.PingAll();

        PingCompass();
    }

    public void TurnOnRing(int ring)
    {
        interfaceRings[ring].SetFullOn(true);
    }

    public void TurnOffRing(int ring)
    {
        interfaceRings[ring].SetFullOn(false);
    }

    public void PingRing(int ring)
    {
        interfaceRings[ring].Ping();

    }

    public void SetRingFade(int ring, float fade)
    {
        interfaceRings[ring].SetFade(fade);
    }

    public void SetRingValue(int ring, float value)
    {
        interfaceRings[ring].SetValue(value);
    }

    public void TurnOffCompass()
    {

    }

    public void TurnOnCompass()
    {

    }

    public void PingCompass()
    {

    }



    public void TurnOnPointer(Transform t)
    {
        interfacePointer.TurnOnPointer(t);
    }

    public void AddPointer(Transform t)
    {
        interfacePointer.AddPointer(t); // makes sure we arent adding!
    }

    public void RemovePointer(Transform t)
    {
        interfacePointer.RemovePointer(t);
    }


    public void ClearPointers()
    {
        interfacePointer.ClearPointers();
    }


    public void PingPointer(Transform t)
    {
        interfacePointer.Ping(t);
    }

    public void PingAllPointers()
    {
        interfacePointer.PingAll();
    }

    public void SetPointerFade(Transform t, float fade)
    {
        interfacePointer.SetFade(t, fade);
    }



}
