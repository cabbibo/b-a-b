using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CarryableActivityCaller : MonoBehaviour
{

    public Carryable carryable;
    public Activity activity;

    public Transform carryableStartPosition;


    public void OnEnable()
    {
        carryable.gameObject.SetActive(true);
    }



    public void OnPickup()
    {

        // play pick up sound
        // show that we are carrying it
        // show where we want to take it

    }

    public void OnDrop()
    {

        // see if we brought it to where we wanted to go?
        // when it hits the ground?

        // if so, YAY, succeed activity
        // otherwise fail activity

    }

    // maybe activity area is defined by the crystal if its on? so as long as its close to use we are ok?
    // otherwise, we are 'leaving' the area


    public void TurnOnActivityEvent()
    {


        print("setting");
        //carryable.gameObject.SetActive(true);

        carryable.TryToResetPosition(God.wren.carrying, carryableStartPosition.position);


    }

    public void TurnOffActivityEvent()
    {

        print("unsetting");
        //carryable.gameObject.SetActive(false);

    }



}
