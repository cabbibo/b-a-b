using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseInterface : MonoBehaviour
{


    public WrenInterface[] interfaces;
    
    public int activeInterface;

    public float scale;

    public int totalInterfaces;



    public void Toggle( bool onOff ){

        gameObject.SetActive( onOff );

        activeInterface = 1;
        if( onOff ){
            NextInterface(1);
        }


    }

    bool odRight;
    bool odLeft;
    

    // Update is called once per frame
    void Update()
    {


        if( odRight == false && God.input.dRight == true ){
            NextInterface(-1);
        }

        if( odLeft == false  && God.input.dLeft == true ){
           NextInterface(1);
        }

        odRight = God.input.dRight;    
        odLeft = God.input.dLeft;    

           
    }


    void NextInterface(int addition){
        
        activeInterface += addition;
        if( activeInterface == -1 ){ activeInterface += interfaces.Length; }
        if( activeInterface == interfaces.Length ){ activeInterface = 0; }


        for( int i = 0; i < interfaces.Length; i ++ ){
            if( i != activeInterface){
                interfaces[i].gameObject.SetActive(false);//Deactivate();
            }else{
                interfaces[i].gameObject.SetActive(true);
            }
        }

      

    }







}
