using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FourRingInterface : WrenInterface
{

    public BaseInterface fullInterface;

    public bool active;

    public Transform[] rings;
    public AngleToColor[] angleValues;
    public Transform selectedRingRep;

    public LineRenderer lr;

    public Transform selectionObject1;
    public Transform selectionObject2;

    public int selection;
    public int oSelection;
    public float selectionVelocity;
    public float selectionPosition;
    public float oSelectionPosition;

    public float selectionChangeSpeed;
    public float selectionCenteringForce;
    public float selectionDampening;


    public float rotateChangeSpeed;
    public float rotateDampening;
    public float rotateVel;

    public float repDepth;

    public float baseScale;
    public float selectedScaleMultiplier;

    public float biggestScale;
    public float scaleSubtractor;

    public float ringRepUp;

    public Transform selectedRing;
    public Transform oSelectedRing;

    public float repPos;


    // Start is called before the first frame update
    public void OnEnable()
    {   
        
        
        selection = 1;
        oSelection = 1;
        
        newSelection();

        angleValues = new AngleToColor[rings.Length];
        for( int i = 0; i < rings.Length; i++ ){
            angleValues[i] = rings[i].GetComponent<AngleToColor>();
        }


        
        if( God.wren ){
            angleValues[0].setValue(God.wren.state.hue1);
            angleValues[1].setValue(God.wren.state.hue2);
            angleValues[2].setValue(God.wren.state.hue3);
            angleValues[3].setValue(God.wren.state.hue4);
        }

        UpdateInterface();

        active = true;

    }


    public void OnDisable(){
        active = false;
        gameObject.SetActive(false);
    }

    public override void Activate()
    {

        active = true;
        gameObject.SetActive(true);

    }

    public override void Deactivate()
    {

        active = false;
        gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {


         //transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10 - Camera.main.transform.up * 1;
         //transform.LookAt( Camera.main.transform.position);
        
        if(active){
            UpdateInterface();
        }
        
    }

    void UpdateInterface(){
        

        oSelectionPosition = selectionPosition;

        selectionVelocity += God.input.alwaysLeft.y * .01f * selectionChangeSpeed;
        selectionVelocity += (selectionPosition - (Mathf.Floor(selectionPosition) +.5f )) * .01f * selectionCenteringForce;
        
        selectionPosition += selectionVelocity;

        selectionVelocity *= selectionDampening;
        

        if( selectionPosition >= rings.Length ){
            selectionPosition = (float)rings.Length - .001f;
        }

        if( selectionPosition <= 0 ){
            selectionPosition = 0;//rings.Length;
        }


        if( Mathf.Floor( selectionPosition ) != Mathf.Floor(oSelectionPosition )){
            newSelection();
        }


        rotateVel += God.input.alwaysLeft.x * .01f * rotateChangeSpeed;
        rotateVel *= rotateDampening;


        selectedRing.Rotate( Vector3.forward * rotateVel , Space.Self );





        /*
            representation

        */

        
        
        for( int i = 0; i< rings.Length; i++ ){
            rings[i].localPosition = new Vector3( 0,(float)i * repDepth,0);

            rings[i].localScale = Vector3.one * (biggestScale-scaleSubtractor*(i)/(float)rings.Length) * baseScale;
            if( i == selection ){
                rings[i].localScale *= selectedScaleMultiplier;
            }
        }

        selectionObject1.localPosition = new Vector3( .3f, (selectionPosition - .5f) * repDepth, 0);
        selectionObject2.localPosition = new Vector3( 0, Mathf.Floor( selectionPosition) * repDepth,0 );


        lr.SetPosition(0,selectionObject1.position);
        lr.SetPosition(1,selectionObject2.position);
        
        SetHue1( angleValues[0].value );
        SetHue2( angleValues[1].value );
        SetHue3( angleValues[2].value );
        SetHue4( angleValues[3].value );
    }

    void newSelection(){
        oSelection = selection;
        selection = (int) selectionPosition;

        oSelectedRing = rings[oSelection];
        selectedRing = rings[selection];

        selectedRingRep.transform.parent = selectedRing;
        selectedRingRep.transform.localScale = Vector3.one * 2;
        selectedRingRep.transform.localPosition = Vector3.forward * ringRepUp;
        selectedRingRep.transform.localEulerAngles = new Vector3( 180 , 0 , 0 );
        
       // selectionPosition = Mathf.Floor(selectionPosition) + .5f;
        selectionVelocity = 0;
        rotateVel = 0;


    }



        public void SetHue1(float v){
        if( God.wren != null ){ God.wren.state.SetHue1(v); }
    }

    public void SetHue2(float v){
        if( God.wren != null ){ God.wren.state.SetHue2(v); }
    }

    public void SetHue3(float v){
        if( God.wren != null ){ God.wren.state.SetHue3(v); }
    }

    public void SetHue4(float v){
        if( God.wren != null ){ God.wren.state.SetHue4(v); }   
    }

}
