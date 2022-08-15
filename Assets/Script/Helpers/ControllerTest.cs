using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;


public class ControllerTest : MonoBehaviour
{


    private Player player; // The Rewired Player
    
public AnimationCurve leftYRemap;
public AnimationCurve rightYRemap;
public AnimationCurve leftXRemap;
public AnimationCurve rightXRemap;

public bool invertX;
public bool invertY;

    public Vector2 left;
    public Vector2 right;


    public Vector2 alwaysLeft;
    public Vector2 alwaysRight;

    public float r1;
    public bool r1Pressed;
    public float l1;
    public bool l1Pressed;

    public float r2;
    public float l2;

    public bool r3;
    public bool l3;

    public bool dUp;
    public bool dDown;
    public bool dLeft;
    public bool dRight;

    
    public bool dUpPressed;
    public bool dDownPressed;
    public bool dLeftPressed;
    public bool dRightPressed;

    public bool triangle;
    public bool trianglePressed;

    
    public bool circle;
    public bool circlePressed;

    
    public bool square;
    public bool squarePressed;

    
    public bool x;
    public bool xPressed;


    public bool swapLR;

    public bool menuPressed;

    // Start is called before the first frame update
    void OnEnable()
    {
        player = ReInput.players.GetPlayer(0);
    }

    // Update is called once per frame
    void Update()
    {

      if( player == null ){
        player = ReInput.players.GetPlayer(0);
      }

      float invX = invertX ? -1:1;
      float invY = invertY ? -1:1;
      
      left = new Vector2(player.GetAxis("leftX")*invX,player.GetAxis("leftY")*invY);
      right = new Vector2(player.GetAxis("rightX")*invX,player.GetAxis("rightY")*invY);

      alwaysLeft = left;
      alwaysRight = right;

      if( swapLR ){
         right = new Vector2(player.GetAxis("leftX")*invX,player.GetAxis("leftY")*invY);
         left = new Vector2(player.GetAxis("rightX")*invX,player.GetAxis("rightY")*invY);
      }

        r1 = player.GetAxis("R1");
        r1Pressed = player.GetButtonDown("R1");
        l1 = player.GetAxis("L1");
        l1Pressed = player.GetButtonDown("L1");

        r2 = player.GetAxis("R2");
        l2 = player.GetAxis("L2");

        dUp = player.GetButton("D-Up");
        dDown = player.GetButton("D-Down");        
        dLeft = player.GetButton("D-Left");
        dRight = player.GetButton("D-Right");


        dUpPressed = player.GetButtonDown("D-Up");
        dDownPressed = player.GetButtonDown("D-Down");        
        dLeftPressed = player.GetButtonDown("D-Left");
        dRightPressed = player.GetButtonDown("D-Right");

        l3 = player.GetButton("L3");
        r3 = player.GetButton("R3");

        triangle = player.GetButton("Triangle");
        trianglePressed = player.GetButtonDown("Triangle");


        
        circle = player.GetButton("Circle");
        circlePressed = player.GetButtonDown("Circle");
        
        square = player.GetButton("Square");
        squarePressed = player.GetButtonDown("Square");

        x = player.GetButton("X");
        xPressed = player.GetButtonDown("X");


        
        menuPressed = player.GetButtonDown("Menu");

    

    }


public float leftY{
  get {return leftYRemap.Evaluate( Mathf.Abs(left.y)) * Mathf.Sign(left.y);}
}

public float leftX{
  get {return  leftXRemap.Evaluate( Mathf.Abs(left.x)) * Mathf.Sign(left.x);}
}


public float rightY{
    get {return rightYRemap.Evaluate( Mathf.Abs(right.y)) * Mathf.Sign(right.y);}
}

public float rightX{
    get {return  rightXRemap.Evaluate( Mathf.Abs(right.x)) * Mathf.Sign(right.x);}
}
}
