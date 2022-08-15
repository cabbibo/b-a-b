using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


[ExecuteAlways]
public class MenuSelector : OptionsMenu
{


    public MenuOption continueGame;
    public MenuOption newGame;
    public MenuOption preferences;
    public MenuOption credits;
    public MenuOption quit;



    public override void Create(){      
        
   
        
        options = new List<MenuOption>();

        int gameStarted = PlayerPrefs.GetInt("_GameStarted");

     
        if( gameStarted == 1 ){
//            print("GAME HAS BEEN STARTED");
            options.Add(continueGame);            
            continueGame.gameObject.SetActive(true);

        }else{
            continueGame.gameObject.SetActive(false);
        }

        options.Add( newGame     );
       // options.Add( preferences );
        options.Add( credits     );
        options.Add( quit        );



        for( int i = 0; i < options.Count; i++ ){
            DeactivateOption(i);
        }

        ActivateOption(currentOption);
     
        
    }




   

 

}
