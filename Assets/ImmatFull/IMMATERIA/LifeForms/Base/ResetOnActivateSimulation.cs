﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMMATERIA {
public class ResetOnActivateSimulation : Cycle
{

  public string nameInBuffer = "_VertBuffer";
  public Form form;
  public Life life;
  public Life resetLife;

  public bool skipFormBind;
  

  public Binder[] binders;

  // Use this for initialization
  public override void _Create(){

    if( form == null ){ form = GetComponent<Form>();}
    if( life == null ){ life = GetComponent<Life>();}

    if( !skipFormBind) SafeInsert(form);
    SafeInsert(life);
    SafeInsert(resetLife);


    binders = GetComponents<Binder>();
    for( int i = 0 ; i < binders.Length; i++ ){
      SafeInsert( binders[i] );
    }
    
    DoCreate();


  }

  public override void _Bind(){

    life.BindPrimaryForm(nameInBuffer, form); 
    resetLife.BindPrimaryForm(nameInBuffer, form); 
    
    Bind();

  }


  public override void Activate(){

    //resetLife.YOLO();

  }



  


}
}
