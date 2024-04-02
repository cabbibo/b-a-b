using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
//https://patorjk.com/software/taag/#p=display&f=Jacky&t=full%20Game%20Started%0A

public class TutorialStateManager : MonoBehaviour
{

    public FlyingTutorialSequence flyingTutorialSequence;

    public GameObject tutorialIsland;
    public GameObject postCrashTutorialObjects;

    public GameObject tutorialClouds;
    public GameObject tutorialOcean;

    public GameObject portal;

    public GameObject mainOcean;
    public GameObject mainIsland;

    public GameObject windCircle;

    public GameObject theCrossing;


    public PlayCutScene tutorialIslandCutScene;
    public PlayCutScene islandFinishedCutScene;


    // flying tutorial end?
    public Transform tutorialEndPosition;


    public Transform crashEndPosition;
    public Transform tutorialIslandEndPosition;


    public bool inFreeFlight;
    public bool flightFinished;
    public bool hasCrashed;
    public bool tutorialIslandCompleted;

    public bool islandReached;

    public GameObject fadeOrb;
    public GameObject introFull;



    /*

        Bugs
       // Clouds dont move with wren in free flight before you start the directions, start moving with you once you do
        //not spawning close enough to the ground
        //Fade doesn fade out when setting on island
            
      //  When starting on crash, wren doesnt start in correct position
      //Disover a biome when turn island on
      //When to Turn off clouds?


 ???   Phase Shift at end of tutorial sequence is funky and off! ( needs to set before transion out, or maybe on animation start??) 


"Sub State" systems, that can be encorporated into full state system!
add phase shift into onenable, not the set state!

       
       // set in sequence vs set at beginning of load

    */



    public void OnEnable()
    {

        FlyingTutorialSequence.OnTutorialStart += SetCinematicFlightTutorialState;
        FlyingTutorialSequence.OnFreeFlightStarted += SetFreeFlightState;
        FlyingTutorialSequence.OnTutorialDiveFinished += SetTransitionState;

        print(" Which Start Status : ");




        if (God.state.tutorialFinished == false)
        {

            print("start cinematic flight tutorial");
            // start from the beginning
            StartCinematicFlightTutorial();

        }
        else
        {

            if (God.state.tutorialIslandFinished == false)
            {


                print("On first crash start");
                // start from our first crash
                OnFirstCrashEnd();
            }
            else
            {

                if (God.state.islandDiscovered == false)
                {

                    print("on tutorial island finished ");
                    // Start from the reveal of the island
                    OnTutorialIslandFinishedEnd();

                }
                else
                {

                    print("full game started");
                    // Start the full game
                    FullGameStart();
                }
            }

        }

    }


    public void OnDisable()
    {
        FlyingTutorialSequence.OnTutorialStart -= SetCinematicFlightTutorialState;
        FlyingTutorialSequence.OnTutorialDiveFinished -= SetFreeFlightState;
        FlyingTutorialSequence.OnFreeFlightStarted -= SetFreeFlightState;
    }




    /*


          _____   ________     ____     ______     ________         _________   ______      _____    _____         _________   _____        _____      _____    __    __   ________  
         / ____\ (___  ___)   (    )   (   __ \   (___  ___)       (_   _____) (   __ \    / ___/   / ___/        (_   _____) (_   _)      (_   _)    / ___ \  (  \  /  ) (___  ___) 
        ( (___       ) )      / /\ \    ) (__) )      ) )            ) (___     ) (__) )  ( (__    ( (__            ) (___      | |          | |     / /   \_)  \ (__) /      ) )    
         \___ \     ( (      ( (__) )  (    __/      ( (            (   ___)   (    __/    ) __)    ) __)          (   ___)     | |          | |    ( (  ____    ) __ (      ( (     
             ) )     ) )      )    (    ) \ \  _      ) )            ) (        ) \ \  _  ( (      ( (              ) (         | |   __     | |    ( ( (__  )  ( (  ) )      ) )    
         ___/ /     ( (      /  /\  \  ( ( \ \_))    ( (            (   )      ( ( \ \_))  \ \___   \ \___         (   )      __| |___) )   _| |__   \ \__/ /    ) )( (      ( (     
        /____/      /__\    /__(  )__\  )_) \__/     /__\            \_/        )_) \__/    \____\   \____\         \_/       \________/   /_____(    \____/    /_/  \_\     /__\    



    */

    public void StartCinematicFlightTutorial()
    {

        print("starting cinematic flight");

        SetCinematicFlightTutorialState();

        // auto takeoff
        if (God.wren)
        {
            God.wren.state.TakeOff();
        }

        flyingTutorialSequence.StartTutorial();

    }

    public void SetCinematicFlightTutorialState()
    {

        print("setting cinematic flight state");

        hasCrashed = false;
        flightFinished = false;
        inFreeFlight = false;
        tutorialIslandCompleted = false;
        islandReached = true;


        introFull.SetActive(true);
        tutorialIsland.SetActive(false);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(false);
        mainIsland.SetActive(false);
        windCircle.SetActive(false);
        portal.SetActive(false);
        tutorialOcean.SetActive(true);
        tutorialClouds.SetActive(true);
        fadeOrb.SetActive(true);
        theCrossing.SetActive(false);


    }





    /*

      _____   ________     ____     ______     ________         _________   ______      _____    _____         _________   _____        _____      _____    __    __   ________  
     / ____\ (___  ___)   (    )   (   __ \   (___  ___)       (_   _____) (   __ \    / ___/   / ___/        (_   _____) (_   _)      (_   _)    / ___ \  (  \  /  ) (___  ___) 
    ( (___       ) )      / /\ \    ) (__) )      ) )            ) (___     ) (__) )  ( (__    ( (__            ) (___      | |          | |     / /   \_)  \ (__) /      ) )    
     \___ \     ( (      ( (__) )  (    __/      ( (            (   ___)   (    __/    ) __)    ) __)          (   ___)     | |          | |    ( (  ____    ) __ (      ( (     
         ) )     ) )      )    (    ) \ \  _      ) )            ) (        ) \ \  _  ( (      ( (              ) (         | |   __     | |    ( ( (__  )  ( (  ) )      ) )    
     ___/ /     ( (      /  /\  \  ( ( \ \_))    ( (            (   )      ( ( \ \_))  \ \___   \ \___         (   )      __| |___) )   _| |__   \ \__/ /    ) )( (      ( (     
    /____/      /__\    /__(  )__\  )_) \__/     /__\            \_/        )_) \__/    \____\   \____\         \_/       \________/   /_____(    \____/    /_/  \_\     /__\    


    */

    public void StartFreeFlight()
    {
        print("starting freeflight");
        // fade out cards?
        SetFreeFlightState();

    }

    public void SetFreeFlightState()
    {

        print("setting freeflight state");

        hasCrashed = false;
        flightFinished = false;
        inFreeFlight = true;
        tutorialIslandCompleted = false;
        islandReached = true;


        introFull.SetActive(true);
        fadeOrb.SetActive(true);
        tutorialIsland.SetActive(false);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(false);
        mainIsland.SetActive(false);
        windCircle.SetActive(false);
        portal.SetActive(false);
        tutorialOcean.SetActive(true);
        tutorialClouds.SetActive(true);
        theCrossing.SetActive(false);


    }

    /*


      _____   ________     ____     ______     ________         ________   ______       ____        __      _    _____    _____   ________    _____     ____        __      _  
     / ____\ (___  ___)   (    )   (   __ \   (___  ___)       (___  ___) (   __ \     (    )      /  \    / )  / ____\  (_   _) (___  ___)  (_   _)   / __ \      /  \    / ) 
    ( (___       ) )      / /\ \    ) (__) )      ) )              ) )     ) (__) )    / /\ \     / /\ \  / /  ( (___      | |       ) )       | |    / /  \ \    / /\ \  / /  
     \___ \     ( (      ( (__) )  (    __/      ( (              ( (     (    __/    ( (__) )    ) ) ) ) ) )   \___ \     | |      ( (        | |   ( ()  () )   ) ) ) ) ) )  
         ) )     ) )      )    (    ) \ \  _      ) )              ) )     ) \ \  _    )    (    ( ( ( ( ( (        ) )    | |       ) )       | |   ( ()  () )  ( ( ( ( ( (   
     ___/ /     ( (      /  /\  \  ( ( \ \_))    ( (              ( (     ( ( \ \_))  /  /\  \   / /  \ \/ /    ___/ /    _| |__    ( (       _| |__  \ \__/ /   / /  \ \/ /   
    /____/      /__\    /__(  )__\  )_) \__/     /__\             /__\     )_) \__/  /__(  )__\ (_/    \__/    /____/    /_____(    /__\     /_____(   \____/   (_/    \__/    



    */


    public void StartTransition()
    {

        print("starting transtion");

        // moves our bird and all our freeflight objects to the correct position
        if (God.wren)
        {

            Vector3 shift = tutorialEndPosition.position - God.wren.transform.position;
            tutorialClouds.transform.position += shift;

            God.wren.PhaseShift(tutorialEndPosition.position);

        }




        SetTransitionState();

    }

    public void SetTransitionState()
    {

        print("setting transition state");
        hasCrashed = false;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = false;
        islandReached = true;


        introFull.SetActive(true);
        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(true);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(true);
        mainIsland.SetActive(false);
        windCircle.SetActive(true);
        portal.SetActive(true);
        tutorialOcean.SetActive(false);
        tutorialClouds.SetActive(true);
        theCrossing.SetActive(false);

        God.state.OnTutorialFinish();

    }



    /*

       ____        __      _      _________    _____   ______      _____   ________        ____   ______       ____      _____   __    __  
      / __ \      /  \    / )    (_   _____)  (_   _) (   __ \    / ____\ (___  ___)      / ___) (   __ \     (    )    / ____\ (  \  /  ) 
     / /  \ \    / /\ \  / /       ) (___       | |    ) (__) )  ( (___       ) )        / /      ) (__) )    / /\ \   ( (___    \ (__) /  
    ( ()  () )   ) ) ) ) ) )      (   ___)      | |   (    __/    \___ \     ( (        ( (      (    __/    ( (__) )   \___ \    ) __ (   
    ( ()  () )  ( ( ( ( ( (        ) (          | |    ) \ \  _       ) )     ) )       ( (       ) \ \  _    )    (        ) )  ( (  ) )  
     \ \__/ /   / /  \ \/ /       (   )        _| |__ ( ( \ \_))  ___/ /     ( (         \ \___  ( ( \ \_))  /  /\  \   ___/ /    ) )( (   
      \____/   (_/    \__/         \_/        /_____(  )_) \__/  /____/      /__\         \____)  )_) \__/  /__(  )__\ /____/    /_/  \_\


    */

    public void OnFirstCrashStart()
    {
        print("first crash");

        hasCrashed = true;

        God.wren.shards.SpendAllShards();

        //TODO loose all shards
        SetFirstCrashStartState();

        // play our first cut scene
        tutorialIslandCutScene.Play();

    }

    public void OnFirstCrashEnd()
    {

        God.wren.shards.SpendAllShards();
        SetFirstCrashEndState();

    }


    public void SetFirstCrashStartState()
    {
        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = false;
        islandReached = true;

        introFull.SetActive(true);
        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(true);
        postCrashTutorialObjects.SetActive(true);
        mainOcean.SetActive(true);
        mainIsland.SetActive(false);
        windCircle.SetActive(true);

        // turn off portal here?
        portal.SetActive(true);

        tutorialOcean.SetActive(false);

        /// hmmmmmm
        tutorialClouds.SetActive(false);

        theCrossing.SetActive(false);

    }

    public void SetFirstCrashEndState()
    {

        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = false;
        islandReached = true;

        // Set Wren position to last position loaded in state;
        if (God.wren)
        {
            God.wren.PhaseShift(crashEndPosition.position);
        }

        introFull.SetActive(true);
        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(true);
        postCrashTutorialObjects.SetActive(true);
        mainOcean.SetActive(true);
        mainIsland.SetActive(false);
        windCircle.SetActive(true);
        portal.SetActive(false);
        tutorialOcean.SetActive(false);
        theCrossing.SetActive(false);
        /// hmmmmmm
        tutorialClouds.SetActive(false);


    }




    /*


       ____        __      _      ________   __    __   ________     ____     ______      _____     ____     _____           _____    _____   _____         ____        __      _   ______        _________    _____      __      _    _____    _____   __    __    _____   ______    
      / __ \      /  \    / )    (___  ___)  ) )  ( (  (___  ___)   / __ \   (   __ \    (_   _)   (    )   (_   _)         (_   _)  / ____\ (_   _)       (    )      /  \    / ) (_  __ \      (_   _____)  (_   _)    /  \    / )  (_   _)  / ____\ (  \  /  )  / ___/  (_  __ \   
     / /  \ \    / /\ \  / /         ) )    ( (    ) )     ) )     / /  \ \   ) (__) )     | |     / /\ \     | |             | |   ( (___     | |         / /\ \     / /\ \  / /    ) ) \ \       ) (___       | |     / /\ \  / /     | |   ( (___    \ (__) /  ( (__      ) ) \ \  
    ( ()  () )   ) ) ) ) ) )        ( (      ) )  ( (     ( (     ( ()  () ) (    __/      | |    ( (__) )    | |             | |    \___ \    | |        ( (__) )    ) ) ) ) ) )   ( (   ) )     (   ___)      | |     ) ) ) ) ) )     | |    \___ \    ) __ (    ) __)    ( (   ) ) 
    ( ()  () )  ( ( ( ( ( (          ) )    ( (    ) )     ) )    ( ()  () )  ) \ \  _     | |     )    (     | |   __        | |        ) )   | |   __    )    (    ( ( ( ( ( (     ) )  ) )      ) (          | |    ( ( ( ( ( (      | |        ) )  ( (  ) )  ( (        ) )  ) ) 
     \ \__/ /   / /  \ \/ /         ( (      ) \__/ (     ( (      \ \__/ /  ( ( \ \_))   _| |__  /  /\  \  __| |___) )      _| |__  ___/ /  __| |___) )  /  /\  \   / /  \ \/ /    / /__/ /      (   )        _| |__  / /  \ \/ /     _| |__  ___/ /    ) )( (    \ \___   / /__/ /  
      \____/   (_/    \__/          /__\     \______/     /__\      \____/    )_) \__/   /_____( /__(  )__\ \________/      /_____( /____/   \________/  /__(  )__\ (_/    \__/    (______/        \_/        /_____( (_/    \__/     /_____( /____/    /_/  \_\    \____\ (______/   



    */



    public void OnTutorialIslandFinishedStart()
    {

        SetTutorialIslandFinishedStartState();
        islandFinishedCutScene.Play();

    }



    public void OnTutorialIslandFinishedEnd()
    {
        SetTutorialIslandFinishedEndState();
    }


    // TODO do we always have tutorial island show?
    public void SetTutorialIslandFinishedStartState()
    {

        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = true;
        islandReached = false;

        // Set Wren position to last position loaded in state;
        if (God.wren)
        {
            God.wren.PhaseShift(tutorialIslandEndPosition.position);
        }

        introFull.SetActive(true);
        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(true);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(true);
        mainIsland.SetActive(true);
        windCircle.SetActive(true);
        portal.SetActive(false);
        tutorialOcean.SetActive(false);
        theCrossing.SetActive(false);

        /// hmmmmmm
        tutorialClouds.SetActive(false);



    }

    // TODO do we always have tutorial island show?
    public void SetTutorialIslandFinishedEndState()
    {


        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = true;
        islandReached = false;
        // Set Wren position to last position loaded in state;
        if (God.wren)
        {
            God.wren.PhaseShift(tutorialIslandEndPosition.position);
        }

        introFull.SetActive(true);
        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(true);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(true);
        mainIsland.SetActive(true);
        windCircle.SetActive(false);
        portal.SetActive(false);
        tutorialOcean.SetActive(false);
        theCrossing.SetActive(true);

        /// hmmmmmm
        tutorialClouds.SetActive(false);

        God.state.OnTutorialIslandFinish();

    }



    /*


       ____        __      _       _____    _____   _____         ____        __      _   ______        ______      _____     ____       ____   __    __    _____   ______    
      / __ \      /  \    / )     (_   _)  / ____\ (_   _)       (    )      /  \    / ) (_  __ \      (   __ \    / ___/    (    )     / ___) (  \  /  )  / ___/  (_  __ \   
     / /  \ \    / /\ \  / /        | |   ( (___     | |         / /\ \     / /\ \  / /    ) ) \ \      ) (__) )  ( (__      / /\ \    / /      \ (__) /  ( (__      ) ) \ \  
    ( ()  () )   ) ) ) ) ) )        | |    \___ \    | |        ( (__) )    ) ) ) ) ) )   ( (   ) )    (    __/    ) __)    ( (__) )  ( (        ) __ (    ) __)    ( (   ) ) 
    ( ()  () )  ( ( ( ( ( (         | |        ) )   | |   __    )    (    ( ( ( ( ( (     ) )  ) )     ) \ \  _  ( (        )    (   ( (       ( (  ) )  ( (        ) )  ) ) 
     \ \__/ /   / /  \ \/ /        _| |__  ___/ /  __| |___) )  /  /\  \   / /  \ \/ /    / /__/ /     ( ( \ \_))  \ \___   /  /\  \   \ \___    ) )( (    \ \___   / /__/ /  
      \____/   (_/    \__/        /_____( /____/   \________/  /__(  )__\ (_/    \__/    (______/       )_) \__/    \____\ /__(  )__\   \____)  /_/  \_\    \____\ (______/   


    */






    public void OnIslandReached()
    {
        SetIslandReachedState();
    }

    public void SetIslandReachedState()
    {

        print("setting island reached state");
        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = true;
        islandReached = true;

        // Set Wren position to last position loaded in state;
        if (God.wren)
        {
            print(God.state.lastPosition);
            God.wren.PhaseShift(God.state.lastPosition);
        }

        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(false);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(true);
        mainIsland.SetActive(true);
        windCircle.SetActive(false);
        portal.SetActive(false);
        tutorialOcean.SetActive(false);
        theCrossing.SetActive(false);
        introFull.SetActive(false);

        /// hmmmmmm
        tutorialClouds.SetActive(false);

        God.state.OnIslandDiscovered();

    }


    /*

     _________   __    __   _____       _____             _____      ____       __    __      _____       _____   ________     ____     ______     ________    _____   ______   
    (_   _____)  ) )  ( (  (_   _)     (_   _)           / ___ \    (    )      \ \  / /     / ___/      / ____\ (___  ___)   (    )   (   __ \   (___  ___)  / ___/  (_  __ \   
      ) (___    ( (    ) )   | |         | |            / /   \_)   / /\ \      () \/ ()    ( (__       ( (___       ) )      / /\ \    ) (__) )      ) )    ( (__      ) ) \ \  
     (   ___)    ) )  ( (    | |         | |           ( (  ____   ( (__) )     / _  _ \     ) __)       \___ \     ( (      ( (__) )  (    __/      ( (      ) __)    ( (   ) ) 
      ) (       ( (    ) )   | |   __    | |   __      ( ( (__  )   )    (     / / \/ \ \   ( (              ) )     ) )      )    (    ) \ \  _      ) )    ( (        ) )  ) ) 
     (   )       ) \__/ (  __| |___) ) __| |___) )      \ \__/ /   /  /\  \   /_/      \_\   \ \___      ___/ /     ( (      /  /\  \  ( ( \ \_))    ( (      \ \___   / /__/ /  
      \_/        \______/  \________/  \________/        \____/   /__(  )__\ (/          \)   \____\    /____/      /__\    /__(  )__\  )_) \__/     /__\      \____\ (______/


    */

    public void FullGameStart()
    {

        // todo what else???
        SetIslandReachedState();

    }

    public void SetFullGameStartedState()
    {

        print("setting island reached state");
        hasCrashed = true;
        flightFinished = true;
        inFreeFlight = false;
        tutorialIslandCompleted = true;
        islandReached = true;

        // Set Wren position to last position loaded in state;
        if (God.wren)
        {
            print(God.state.lastPosition);
            God.wren.PhaseShift(God.state.lastPosition);
        }

        fadeOrb.SetActive(false);
        tutorialIsland.SetActive(false);
        postCrashTutorialObjects.SetActive(false);
        mainOcean.SetActive(true);
        mainIsland.SetActive(true);
        windCircle.SetActive(false);
        portal.SetActive(false);
        tutorialOcean.SetActive(false);
        theCrossing.SetActive(false);
        introFull.SetActive(false);

        /// hmmmmmm
        tutorialClouds.SetActive(false);

    }


    void Update()
    {

        if (inFreeFlight)
        {
            print("while in free flight");
            WhileInFreeFlight();
        }

        if (hasCrashed && tutorialIslandCompleted == false)
        {
            WhileOnTutorialIsland();
        }
    }

    // PhaseShift to correct position as needed
    public void WhileInFreeFlight()
    {


        if (God.wren)
        {
            var p = God.wren.physics.rb.transform.position;

            // always keeping clouds away form us
            tutorialClouds.transform.position = new Vector3(p.x, p.y - 300, p.z);
            tutorialOcean.transform.position = new Vector3(p.x, p.y - 300, p.z);

            if (p.y < 100)
            {
                God.wren.PhaseShift(new Vector3(p.x, p.y + 450, p.z));
            }

        }

    }


    // Check to see if we have crashed
    // Check to see if we have collected enough shards
    public void WhileOnTutorialIsland()
    {

        if (God.wren)
        {
            if (hasCrashed && God.wren.shards.GetBodyShardPercentage() >= 1 && tutorialIslandCompleted == false)
            {


                print("WE DID IT");
                OnEnoughCollected();

            }
        }

    }



    void OnEnoughCollected()
    {

        tutorialIslandCompleted = true;

        OnTutorialIslandFinishedStart();

    }




}
