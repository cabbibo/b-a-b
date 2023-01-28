using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerUIControl : MonoBehaviour
{

public ControllerTest c;


public GameObject x;
public GameObject circle;
public GameObject square;
public GameObject triangle;


public GameObject dLeft;
public GameObject dRight;
public GameObject dDown;
public GameObject dUp;



public GameObject leftStick;
public GameObject l3;
public GameObject l2;
public GameObject l1;


public GameObject rightStick;
public GameObject r3;
public GameObject r2;
public GameObject r1;


public float amountDown;
public float moveSize;

    public float r1Start;
    public float r2Start;

    public float l1Start;
    public float l2Start;

    
    void Start(){

    }

    // Update is called once per frame
    void Update()
    {


        if( c.dDown  ){ dDown.SetActive(true);   }else{ dDown.SetActive(false);}
        if( c.dLeft  ){ dLeft.SetActive(true);   }else{ dLeft.SetActive(false);}
        if( c.dRight ){ dRight.SetActive(true);  }else{ dRight.SetActive(false);}
        if( c.dUp    ){ dUp.SetActive(true);     }else{ dUp.SetActive(false);}




        if( c.x  ){ x.SetActive(true);   }else{ x.SetActive(false);}
        if( c.circle  ){ circle.SetActive(true);   }else{ circle.SetActive(false);}
        if( c.triangle ){ triangle.SetActive(true);  }else{ triangle.SetActive(false);}
        if( c.square    ){ square.SetActive(true);     }else{ square.SetActive(false);}

        if( c.l3    ){ l3.SetActive(true);     }else{ l3.SetActive(false);}
        if( c.r3    ){ r3.SetActive(true);     }else{ r3.SetActive(false);}


        l1.transform.localPosition = new Vector3( 0 , c.l1 * amountDown ,0);  
        l2.transform.localPosition = new Vector3( 0 , c.l2 * amountDown ,0);  
        r1.transform.localPosition = new Vector3( 0 , c.r1 * amountDown ,0);  
        r2.transform.localPosition = new Vector3( 0 , c.r2 * amountDown ,0);  


        leftStick.transform.localPosition = new Vector3( c.left.x*moveSize, c.left.y*moveSize,0);  
        rightStick.transform.localPosition = new Vector3( c.right.x*moveSize, c.right.y*moveSize,0);  

    }


}
