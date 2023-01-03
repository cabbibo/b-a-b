using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertInterface : WrenInterface
{

    public bool invertX;
    public bool invertY;
    public bool swapLR;

    public Renderer invertXRenderer;
    public Renderer invertYRenderer;
    public Renderer swapLRRenderer;

    public Color activeColor;
    public Color inactiveColor;

    public FullInterface fullInterface;

    private bool oX;

public bool active;

public GameObject activeInterfaceIndicator;

 public void OnEnable(){
    UpdateValues();
    active = true;


    // Get our current position and show it on that map
 }

  public override void Activate()
    {

        active = true;
        enabled = true;
        UpdateValues();
        activeInterfaceIndicator.SetActive(true);

    }

    public override void Deactivate()
    {

        active = false;
        enabled = false;
        
        activeInterfaceIndicator.SetActive(false);

    }



    // Update is called once per frame
    void UpdateInterface()
    {
        if( God.input.x && !oX){
            NextValue();
        }

        oX = God.input.x;
    
    }

   

    public void Update(){
        // transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
        // transform.LookAt( Camera.main.transform.position);
         UpdateInterface();
    }


    void NextValue(){
        if( invertX == false && invertY == false && swapLR == false ){
            invertX = true;
            invertY = false;
            swapLR = false;
        }else if( invertX == true && invertY == false && swapLR == false ){
            invertX = true;
            invertY = true;
            swapLR = false;
        }else if( invertX == true && invertY == true && swapLR == false ){
            invertX = true;
            invertY = false;
            swapLR = true;
        }else if( invertX == true && invertY == false && swapLR == true ){
            invertX = false;
            invertY = true;
            swapLR = false;
        }else if( invertX == false && invertY == true && swapLR == false ){
            invertX = false;
            invertY = true;
            swapLR = true;
        }else if( invertX == false && invertY == true && swapLR == true ){
            invertX = false;
            invertY = false;
            swapLR = true;
        }else if( invertX == false && invertY == false && swapLR == true ){
            invertX = true;
            invertY = true;
            swapLR = true;
        }else{
            invertX = false;
            invertY = false;
            swapLR = false;
        }

        UpdateValues();
    }

    public void UpdateValues(){
        God.input.invertY = invertY;
        God.input.invertX = invertX;
        God.input.swapLR = swapLR;

        invertXRenderer.material.SetColor("_Color", invertX ? activeColor : inactiveColor );
        invertYRenderer.material.SetColor("_Color", invertY ? activeColor : inactiveColor );
        swapLRRenderer.material.SetColor("_Color", swapLR ? activeColor : inactiveColor );
    }
}
