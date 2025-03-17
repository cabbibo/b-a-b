using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CarryableActivityCaller : MonoBehaviour
{

    public Carryable carryable;
    public Activity activity;

    public Transform carryableStartPosition;

    public Transform carryableDropPosition;
    public float carryableDropRadius;




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


        //print("setting");
        //carryable.gameObject.SetActive(true);

        carryable.TryToResetPosition(God.wren.carrying, carryableStartPosition.position);


    }

    public void TurnOffActivityEvent()
    {

        //  print("unsetting");
        //carryable.gameObject.SetActive(false);

        carryable.TryToResetPosition(God.wren.carrying, carryableStartPosition.position);

    }

    public void AreaEntered()
    {

        print("area entered");

        God.wren.interfaceUtils.warningText.SetFade(0);

        // Dont reset if we are already in it
        if (activity.doingActivity == false)
        {

            carryable.TryToResetPosition(God.wren.carrying, carryableStartPosition.position);
        }
    }

    public void AreaExited()
    {

        print("area exited");
        carryable.TryToResetPosition(God.wren.carrying, carryableStartPosition.position);
        God.wren.interfaceUtils.warningText.SetFade(0);
    }

    public void Update()
    {
        if (activity.doingActivity == true)
        {


            float dist = Vector3.Distance(carryable.transform.position, carryableDropPosition.position);

            if (dist < carryableDropRadius) // you win and are close enough

            {
                activity.AddToComplete(10000);//FINISH
            }


        }
    }


    public void WhileLeaving(float v)
    {
        print("while leaving");
        print(v);

        God.wren.interfaceUtils.warningText.text.text = "LEAVING ACTIVITY IN " + Mathf.Floor(((1 - v) * activity.timeAllowedWhileExitedActivityArea) * 100) / 100 + " SECONDS";
        God.wren.interfaceUtils.warningText.SetFade((Mathf.Sin(v * 50) + 1) / 2);
    }




}
