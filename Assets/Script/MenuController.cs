﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class MenuController : MonoBehaviour
{

    public AudioClip selectSound;
    public AudioClip deselectSound;
    public AudioClip startSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    public AudioClip highlightSound;


    public MenuSelector mainMenu;
    public OptionsMenu optionsMenu;
    public GameObject credits;
    public OptionsMenu exitMenu;
    public OptionsMenu startMenu;

    public Canvas canvas;

    public bool menuOn;

    // public bool autoLoad = true;

    public string currentMenu = "";

    // Start is called before the first frame update

    void OnEnable()
    {
        God.audio.Play(menuOpenSound);
        TurnMenuOn();
    }

    // Update is called once per frame
    void Update()
    {


        // can only do this if we aren't in the middle of fading!
        if (God.input.menuPressed && God.wren != null)
        {


            // if our menu is off, we turn it on!
            if (!menuOn)
            {

                TurnMenuOn();

            }
            else
            {

                // If we are in main menu, going 'back'
                // will take us out of the menu
                if (currentMenu == "main")
                {


                    OnContinue();
                }
                else
                {
                    // just show the main menu
                    ActivateMainMenu();
                }

            }
        }


        if (God.input.circlePressed && menuOn)
        {
            // If we are in main menu, going 'back'
            // will take us out of the menu
            if (currentMenu == "main")
            {

                OnContinue();

            }
            else
            {
                // just show the main menu
                ActivateMainMenu();
            }

        }

    }


    public void Continue()
    {

    }



    public void TurnMenuOff()
    {

        God.audio.Play(menuCloseSound);
        God.postController.FadeIn();
        if (God.wren != null)
        {
            God.wren.canMove = true;
        }

    }



    public void TurnMenuOff(float l)
    {


        //        print("menu turning off");
        God.audio.Play(menuCloseSound);
        God.postController.FadeIn();
        // DeactivateMenu();
        if (God.wren != null)
        {
            God.wren.canMove = true;
        }

        // God.fade.FadeIn(.5f);
    }




    public void TurnMenuOn()
    {

        //        print("somewhere the menu is turning on");
        God.postController.FadeOut();
        ActivateMainMenu();
        God.audio.Play(menuOpenSound);
        if (God.wren != null)
        {
            God.wren.canMove = false;
            God.SetWrenSavedPosition(God.wren.transform.position);
        }

    }

    public void DeactivateMenu()
    {
        Time.timeScale = 1;
        canvas.enabled = false;
        mainMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        credits.SetActive(false);
        exitMenu.gameObject.SetActive(false);
        menuOn = false;
        currentMenu = "";
    }


    public void ActivateMainMenu()
    {
        Time.timeScale = 0;
        canvas.enabled = true;
        mainMenu.gameObject.SetActive(true);
        optionsMenu.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        credits.SetActive(false);
        exitMenu.gameObject.SetActive(false);
        menuOn = true;
        currentMenu = "main";
    }


    public void ActivateOptionsMenu()
    {
        canvas.enabled = true;
        mainMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(true);
        startMenu.gameObject.SetActive(false);
        credits.SetActive(false);
        exitMenu.gameObject.SetActive(false);
        menuOn = true;
        currentMenu = "options";
    }

    public void ActivateCredits()
    {

        print("hello");
        canvas.enabled = true;
        mainMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        credits.SetActive(true);
        exitMenu.gameObject.SetActive(false);
        menuOn = true;
        currentMenu = "credits";
    }

    public void ActivateExitMenu()
    {
        canvas.enabled = true;
        mainMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        credits.SetActive(false);
        exitMenu.gameObject.SetActive(true);
        menuOn = true;
        currentMenu = "exit";
    }


    public void ActivateStartMenu()
    {

        canvas.enabled = true;
        mainMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(true);
        credits.SetActive(false);
        exitMenu.gameObject.SetActive(false);
        menuOn = true;
        currentMenu = "start";

        // TODO : Skips and deactivates if we dont have a new game yet!

    }


    public void QuitGame()
    {
        Application.Quit();
    }


    public void OnContinue()
    {
        // DeactivateMenu();

        if (God.sceneController.sceneLoaded == true)
        {
            DeactivateMenu();
            TurnMenuOff();
        }
        else
        {
            God.sceneController.OnSceneLoadEvent.AddListener(OnSceneLoaded);//m_MyEvent.AddListener(MyAction);
            God.sceneController.HardStart();

        }

    }


    public void OnNewGameSelected()
    {

        God.sceneController.ResetSave();
        God.sceneController.OnSceneLoadEvent.AddListener(OnSceneLoaded);//m_MyEvent.AddListener(MyAction);
        God.sceneController.HardStart();



        // print("need to start a new game here");

    }


    // Turn off the menu but only once scene is loaded
    public void OnSceneLoaded()
    {
        God.sceneController.OnSceneLoadEvent.RemoveListener(OnSceneLoaded);
        DeactivateMenu();
        TurnMenuOff(.5f);

    }


}
