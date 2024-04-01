using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class MainSceneController : MonoBehaviour
{

    /*

    public IslandData mainIsland;

    public FlyingTutorialSequence tutorialSequence;

    public TutorialEnder tutorialEnder;
    public TutorialIslandEnder tutorialIslandEnder;


    public bool mainIslandShown;
    public bool tutorialIslandShown;
    public bool flyingTutorialActive;
    public bool islandTutorialActive;

    public void OnSceneLoaded()
    {

        print("Loading here");

        if (God.state.tutorialFinished == false)
        {
            SetTutorialStarted();
        }
        else
        {
            SetTutorialEnded();
            if (God.state.tutorialIslandFinished == false)
            {
                SetTutorialIslandStarted();
            }
            else
            {
                SetTutorialIslandEnded();

                if (God.state.islandDiscovered == false)
                {
                    SetIslandPreDiscovered();
                }
                else
                {
                    SetIslandPostDiscovered();
                    SetFullGameStarted();
                }

            }

        }

        // SetStateFromBoolValues();


    }



    public GameObject MainIsland;
    public GameObject TutorialIsland;
    public GameObject TutorialSequence;
    public GameObject TutorialEnder;
    public GameObject TutorialIslandEnder;


    void SetTutorialStarted()
    {

        MainIsland.SetActive(false);
        TutorialIsland.SetActive(false);
        TutorialSequence.SetActive(true);
        TutorialEnder.SetActive(true);
        TutorialIslandEnder.SetActive(false);

        tutorialSequence.SetStart();


    }

    void SetTutorialEnded()
    {

        tutorialSequence.SetEnd();
        mainIslandShown = false;
        tutorialIslandShown = true;
    }

    void SetTutorialIslandStarted()
    {

        tutorialIslandEnder.SetStart();
        mainIslandShown = false;
        tutorialIslandShown = true;
    }

    void SetTutorialIslandEnded()
    {
        tutorialIslandEnder.SetEnd();
        mainIslandShown = true;
        tutorialIslandShown = true;
    }


    void SetIslandPreDiscovered()
    {

        mainIslandShown = true;
        tutorialIslandShown = true;
    }

    void SetIslandPostDiscovered()
    {
        mainIslandShown = true;
        tutorialIslandShown = true;

    }

    void SetFullGameStarted()
    {
        mainIslandShown = true;
        tutorialIslandShown = true;

    }
*/




}
