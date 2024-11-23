using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(WrenInterfaceUtils))]
public class WrenInterfaceUtilsEditor : Editor {
   public override void OnInspectorGUI()
    {  
        WrenInterfaceUtils god = (WrenInterfaceUtils)target;
        if(GUILayout.Button("PING"))
        {
            god.OnPing();
        }

        DrawDefaultInspector();
    }
}


#endif


public class WrenInterfaceUtils : MonoBehaviour
{

    public InterfaceRing[] interfaceRings;

    public InterfacePointer interfacePointer;

    public WrenCompass wrenCompass;

    public int crystalsSpentPerPing = 10;


    public WrenUIText warningText;






    // start with nothing on 
    public void OnEnable()
    {

        for (int i = 0; i < interfaceRings.Length; i++)
        {
            TurnOffRing(i);
        }

        TurnOffCompass();

    }

    // ping the current activity if in activity
    // otherwise ping everything!
    public void OnPing()
    {

        God.audio.Play(God.sounds.interfacePingClip, 1, 1);
        if (God.wren != null) God.wren.shards.SpendShards(crystalsSpentPerPing);
        if (God.state.currentlyActiveActivity != null)
        {
            PingCurrentActiveActivity();
        }
        else
        {
            PingAll();
        }
    }


    public void PingCurrentActiveActivity()
    {
        PingPointer(God.state.currentlyActiveActivity.mainPointOfInterest.transform);
    }


    public void PingAll()
    {

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
        if (interfaceRings[ring].value <= 0)
        {

            return;
        }



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
        print("PINGING POINTER");
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

    public void UpdatePointerState()
    {
        interfacePointer.UpdateState();
    }



}
