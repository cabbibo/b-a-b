using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenInput : MonoBehaviour
{



    public Wren wren;

    public ControllerTest controller;
    public Transform leftStickNetworkData;
    public Transform rightStickNetworkData;
    public Transform leftExtraNetworkData;
    public Transform rightExtraNetworkData;
    public Transform finalExtraNetworkData;

    public bool invertY;

    /*

       our controller values

   */
    public float leftX;
    public float leftY;
    public float left2;


    public float rightX;
    public float rightY;
    public float right2;

    public float left3;
    public float square;
    public float triangle;

    public float right3;
    public float ex;
    public float circle;


    public float left1;
    public float right1;
    public float fDebug;


    public float dLeft;
    public float dRight;
    public float dUp;
    public float dDown;


    public float o_leftX;
    public float o_leftY;
    public float o_left2;



    public float o_rightX;
    public float o_rightY;
    public float o_right2;

    public float o_left3;
    public float o_square;
    public float o_triangle;

    public float o_right3;
    public float o_ex;
    public float o_circle;



    public float o_dLeft;
    public float o_dRight;
    public float o_dUp;
    public float o_dDown;


    public void GetInput()
    {


        o_leftX = leftX;
        o_leftY = leftY;

        o_rightX = rightX;
        o_rightY = rightY;
        o_right2 = right2;

        o_left3 = left3;
        o_square = square;
        o_triangle = triangle;

        o_right3 = right3;
        o_ex = ex;
        o_circle = circle;

        leftX = leftStickNetworkData.localPosition.x;
        leftY = leftStickNetworkData.localPosition.y;
        left2 = leftStickNetworkData.localPosition.z;

        rightX = rightStickNetworkData.localPosition.x;
        rightY = rightStickNetworkData.localPosition.y;
        right2 = rightStickNetworkData.localPosition.z;

        left3 = leftExtraNetworkData.localPosition.x;
        square = leftExtraNetworkData.localPosition.y;
        triangle = leftExtraNetworkData.localPosition.z;

        right3 = rightExtraNetworkData.localPosition.x;
        ex = rightExtraNetworkData.localPosition.y;
        circle = rightExtraNetworkData.localPosition.z;

        left1 = finalExtraNetworkData.localPosition.x;
        right1 = finalExtraNetworkData.localPosition.y;
        fDebug = finalExtraNetworkData.localPosition.z;


        if (wren.stats.stamina <= 0)
        {
            left2 = 0;
            right2 = 0;
        }


    }


    public void SetInput()
    {



        o_dDown = dDown;
        o_dUp = dUp;
        o_dLeft = dLeft;
        o_dRight = dRight;

        o_leftX = leftX;
        o_leftY = leftY;
        o_left2 = left2;

        o_rightX = rightX;
        o_rightY = rightY;
        o_right2 = right2;

        o_left3 = left3;
        o_square = square;
        o_triangle = triangle;

        o_right3 = right3;
        o_ex = ex;
        o_circle = circle;

        leftX = controller.left.x;
        leftY = controller.left.y;// * (God.input.invertY ? -1 : 1);
        left2 = controller.l2;


        rightX = controller.right.x;
        rightY = controller.right.y;// * (God.input.invertY ? -1 : 1);
        right2 = controller.r2;


        if (wren.stats.stamina <= 0)
        {
            left2 = 0;
            right2 = 0;
        }


        leftStickNetworkData.localPosition = new Vector3(leftX, leftY, left2);
        rightStickNetworkData.localPosition = new Vector3(rightX, rightY, right2);


        left3 = controller.l3 ? 1 : 0;
        square = controller.square ? 1 : 0;
        triangle = controller.triangle ? 1 : 0;
        leftExtraNetworkData.localPosition = new Vector3(left3, square, triangle);


        right3 = controller.r3 ? 1 : 0;
        ex = controller.x ? 1 : 0;
        circle = controller.circle ? 1 : 0;
        rightExtraNetworkData.localPosition = new Vector3(right3, ex, circle);


        right1 = controller.r1;
        left1 = controller.l1;
        finalExtraNetworkData.localPosition = new Vector3(left1, right1, fDebug);




        // These ones don't currently need to be networked!
        dLeft = controller.dLeft ? 1 : 0;
        dRight = controller.dRight ? 1 : 0;
        dUp = controller.dUp ? 1 : 0;
        dDown = controller.dDown ? 1 : 0;




    }



}
