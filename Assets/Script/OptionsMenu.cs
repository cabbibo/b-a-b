using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


[ExecuteAlways]
public class OptionsMenu : MonoBehaviour
{


    public AudioClip activateOptionSound;
    public AudioClip selectionOptionSound;
    public Canvas canvas;

    public List<MenuOption> options;

    public int currentOption;

    // Start is called before the first frame update
    void OnEnable()
    {
      

        Create();
        
    }

    public virtual void Create(){      
        
         for( int i = 0; i < options.Count; i++ ){
            DeactivateOption(i);
        }

        ActivateOption(currentOption);
        
    }

    // Update is called once per frame
    void Update()
    {

        if( God.input.dUpPressed){
            DeactivateOption(currentOption);
            currentOption --;
            if( currentOption < 0){ currentOption += options.Count; }
            ActivateOption(currentOption);
        }

          if( God.input.dDownPressed ){
            DeactivateOption(currentOption);
            currentOption ++;
            if( currentOption == options.Count){ currentOption = 0; }
            ActivateOption(currentOption);
        }



        if( God.input.xPressed ){
            SelectCurrentOption();
        }

        if( God.input.dLeft ){
            CurrentOptionDLeft();
        }

        
        if( God.input.dRight ){
            CurrentOptionDRight();
        }
        
    }

    public void ActivateOption( int id){
        
        God.audio.Play(activateOptionSound);
        options[id].Activate();

    }

    public void DeactivateOption( int id){
        
        options[id].Deactivate();
    }


    public void SelectCurrentOption(){
        God.audio.Play(selectionOptionSound);
        options[currentOption].Select();
    }

    public void CurrentOptionDLeft(){ 
        options[currentOption].DLeft();
    }

       public void CurrentOptionDRight(){ 
        options[currentOption].DRight();
    }

}
