using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControllerUIFeedbackGroup : MonoBehaviour
{
    public InterfaceTutorial interfaceTutorial;

    public Color color1;
    public Color color2;

    public float pulseSpeed = 1f;

    public bool button1;
    public bool button2;
    public bool button3;
    public bool button4;

    public bool dUp;
    public bool dDown;
    public bool dLeft;
    public bool dRight;

    public bool leftStick;
    public bool rightStick;

    public bool allDirectionsLeftStick;
    public bool allDirectionsRightStick;

    public bool upLeftStick;
    public bool upRightStick;
    public bool downLeftStick;
    public bool downRightStick;
    public bool leftLeftStick;
    public bool leftRightStick;
    public bool rightLeftStick;
    public bool rightRightStick;

    public bool l1;
    public bool l2;
    public bool l3;

    public bool r1;
    public bool r2;
    public bool r3;

    public bool l1Up;
    public bool l2Up;

    public bool r1Up;
    public bool r2Up;


    public bool hold;
    public bool press;
    public bool release;
    public bool tap;
}