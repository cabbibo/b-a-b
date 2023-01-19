using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


public class FullInterface : MonoBehaviour
{

    public Transform selectedInterfacePointer;
    public FourRingInterface fourRingInterface;
    public MapInterface mapInterface;
    public InvertInterface invertInterface;

    public Transform rotator;

    public int activeInterface;
    public float forwardVal;


    public float targetDegrees;


    public int totalInterfaces;

    public Wren wren;


    // Start is called before the first frame update
    void OnEnable()
    {
        //Toggle( true);//
            //mapInterface.Activate();
    }


    public void Toggle( bool onOff ){


        print("now we toglling");
        print( onOff );

        gameObject.SetActive( onOff );

        activeInterface = 1;
        targetDegrees = 0;
        rotator.rotation = transform.localRotation * Quaternion.AngleAxis( targetDegrees , Vector3.up);

        if( onOff ){

            print("turrnign on intreface");
            invertInterface.UpdateValues();;
            mapInterface.SetCenter();    
            mapInterface.Activate();

            NextInterface(1);
        }


    }

    public bool odRight;
    public bool odLeft;
    

    // Update is called once per frame
    void Update()
    {

        if( God.wren != null ){
            transform.position = God.wren.transform.position;
            transform.position += Camera.main.transform.forward * forwardVal;
        }
         transform.LookAt( Camera.main.transform );
        //transform.Rotate( Vector3.up * Mathf.PI );

        if( odRight == false && God.input.dRight == true ){
            NextInterface(-1);
        }

        if( odLeft == false  && God.input.dLeft == true ){
           NextInterface(1);
        }

        odRight = God.input.dRight;    
        odLeft = God.input.dLeft;    

        float angle;
        Vector3 axis; 
        rotator.localRotation.ToAxisAngle( out axis, out angle );

        float closestDegree = targetDegrees;

   


      //  rotator.localRotation = Quaternion.Slerp( rotator.localRotation ,  Quaternion.AngleAxis( targetDegrees , Vector3.up) , .05f);
       
    }


    void NextInterface(int addition){

        print("Trying to change interface");
        
        activeInterface += addition;
        if( activeInterface == -1 ){ activeInterface += 3; }
        if( activeInterface == 3 ){ activeInterface = 0; }

        targetDegrees += (360/3) * (float)addition;


        print( activeInterface );


        if( activeInterface == 1 ){
            fourRingInterface.Deactivate();
            mapInterface.Deactivate();
            invertInterface.Activate();
           // targetDegrees = 0;
        }else if( activeInterface == 2 ){
            activeInterface = 2;
            fourRingInterface.Deactivate();
            mapInterface.Activate();
            invertInterface.Deactivate();
            //targetDegrees = 90;

        }else{
            fourRingInterface.Activate();
            mapInterface.Deactivate();
            invertInterface.Deactivate();
           // targetDegrees = 270;

        }


        //SetPointerAngle();
   
    }



    public void TeleportToLocation( Vector3 v ){
        if( God.wren != null ){

            // If we are carrying anything drop it!
            God.wren.carrying.DropAllCarriedItems();

            // If we are in a race, end it!
            if( God.wren.state.inRace != -1 ){
                print("WREN IN RACE: " + God.wren.state.inRace);
                God.races[God.wren.state.inRace].AbortRace();
            }

            God.wren.state.TransportToPosition( v , Vector3.zero );
            God.wren.state.HitGround();

            // TODO this seems v bugggy        
            mapInterface.SetCenter();
        }

    }

    public void PlaceBeacon( Vector3 pos ){
        if( God.wren != null ){
            God.wren.beacon.PlaceBeacon(pos );
        }
    }

    public void RemoveBeacon(){
        if( God.wren != null ){
            God.wren.state.SetBeaconOn( false );
        }
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
