using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public ControllerTest controller;
    public Transform      leftHandle;
    public Transform      rightHandle;

    public Transform leftRing;
    public Transform rightRing;

    public Transform leftBumper2;
    public Transform rightBumper2;

    public float bumperUpAmount;
    public float bumperTravelAmount;
    public float handleTravelSize;

    public float ringSize;
    public float ringTravelSize;

    public Transform beak;
    public float     beakRotationAmount;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        leftHandle.localPosition = new Vector3( controller.left.x * handleTravelSize , controller.left.y * handleTravelSize , 0 );
        rightHandle.localPosition = new Vector3( controller.right.x * handleTravelSize , controller.right.y * handleTravelSize , 0 );

        leftBumper2.localPosition = new Vector3( 0 , controller.l2 * bumperTravelAmount + bumperUpAmount , 0 );
        rightBumper2.localPosition = new Vector3( 0 , controller.r2 * bumperTravelAmount + bumperUpAmount , 0 );
        leftRing.localScale = Vector3.one * (ringSize - ringTravelSize * (controller.l3 ? 1 : 0));
        rightRing.localScale = Vector3.one * (ringSize - ringTravelSize * (controller.r3 ? 1 : 0));


        float rotationValue = controller.left.x + controller.right.x;
        beak.localEulerAngles = new Vector3( 0 , 0 , beakRotationAmount * rotationValue + 180 );
        beak.localScale = new Vector3( .8f , .4f + (controller.left.y + controller.right.y) * -.1f , 1 );
        beak.localPosition = new Vector3( rotationValue * handleTravelSize * .5f , -90 , 0 );


    }
}