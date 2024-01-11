using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullState : MonoBehaviour
{


    public bool gameStarted;
    public bool tutorialStarted;
    public bool tutorialFinished;

    public bool[] biomesDiscovered;
    public bool[] biomesStarted;
    public bool[] biomesCompleted;

    public bool gameFinished;


    public int currentSceneID;
    public int currentBiomeID;


    public int numBiomes = 7;

    public void UpdateState()
    {

        // TODO save all this to player prefs

        PlayerPrefsX.SetBool("_TutorialStarted", tutorialStarted);
        PlayerPrefsX.SetBool("_TutorialFinished", tutorialFinished);

        PlayerPrefsX.SetBool("_GameStarted", gameStarted);
        PlayerPrefsX.SetBool("_GameFinished", gameFinished);

        PlayerPrefsX.SetBoolArray("_BiomesStarted", biomesStarted);
        PlayerPrefsX.SetBoolArray("_BiomesCompleted", biomesCompleted);
        PlayerPrefsX.SetBoolArray("_BiomesDiscovered", biomesDiscovered);

        PlayerPrefs.SetInt("_CurrentScene", currentSceneID);
        PlayerPrefs.SetInt("_CurrentBiome", currentBiomeID);

    }


    public void LoadState()
    {

        // TODO load all this from player prefs

        tutorialStarted = PlayerPrefsX.GetBool("_TutorialStarted", false);
        tutorialFinished = PlayerPrefsX.GetBool("_TutorialFinished", false);

        gameStarted = PlayerPrefsX.GetBool("_GameStarted", false);
        gameFinished = PlayerPrefsX.GetBool("_GameFinished", false);

        currentSceneID = PlayerPrefs.GetInt("_CurrentScene", 0);
        currentBiomeID = PlayerPrefs.GetInt("_CurrentBiome", -1);



        biomesStarted = PlayerPrefsX.GetBoolArray("_BiomesStarted");
        biomesCompleted = PlayerPrefsX.GetBoolArray("_BiomesCompleted");
        biomesDiscovered = PlayerPrefsX.GetBoolArray("_BiomesDiscovered");

        if (biomesStarted.Length != numBiomes)
        {
            biomesStarted = new bool[numBiomes];
        }

        if (biomesCompleted.Length != numBiomes)
        {
            biomesCompleted = new bool[numBiomes];
        }

        if (biomesDiscovered.Length != numBiomes)
        {
            biomesDiscovered = new bool[numBiomes];
        }





    }


    public void ResetAll()
    {


        PlayerPrefs.DeleteAll();


        currentSceneID = 0;
        currentBiomeID = -1;

        gameStarted = false;
        tutorialStarted = false;
        tutorialFinished = false;
        gameFinished = false;

        biomesDiscovered = new bool[numBiomes];
        biomesStarted = new bool[numBiomes];
        biomesCompleted = new bool[numBiomes];



        UpdateState();


    }

    public void SetCurrentScene(int i)
    {
        currentSceneID = i;
        UpdateState();
    }

    public void SetCurrentBiome(int i)
    {
        currentBiomeID = i;
        UpdateState();
    }

    public void OnBiomeDiscovered(int i)
    {
        biomesDiscovered[i] = true;
        UpdateState();
    }

    public void OnBiomeStarted(int i)
    {
        biomesStarted[i] = true;
        UpdateState();
    }

    public void OnBiomeCompleted(int i)
    {
        biomesCompleted[i] = true;
        UpdateState();
    }

    public void OnGameStart()
    {
        gameStarted = true;
        UpdateState();
    }

    public void OnTutorialStart()
    {
        tutorialStarted = true;
        UpdateState();
    }

    public void OnTutorialFinish()
    {
        tutorialFinished = true;
        UpdateState();
    }

    public void OnGameFinish()
    {
        gameFinished = true;
        UpdateState();
    }



    public void LoadLocalState()
    {

        LoadState();
    }


    /*


        //stores if the intro has started
        public bool started;

        // stores
        public bool completed;

        public PlayCutScene startAnimation;
        public PlayCutScene completeAnimation;


        public CollectableController collectableController;

        //TODO:
        //public RaceController  raceController;
        //public SpeedTrapController speedTrapController; 
        //public PlantableController plantableController;


        // Narrative collectables
        public List<collectable> collectables;



        // UGC
        public List<race> races;
        public List<plantable> plantables;



        public struct plantable
        {


            public Transform location;
            public float waterAmount;
            public int seedType;

            public string[] peopleWatered;

        }


        public struct race
        {
            public string name;
            public bool unlocked;
            public Color color;
            public string[] highScoreIDs;
            public float[] highScores;

            public Transform[] gates;
            public Transform startGate;
            public Transform endGate;


        }


        public struct speedTrap
        {
            public string name;
            public bool unlocked;
            public Color color;
            public string[] highScoreIDs;
            public float[] highScores;

            public Transform location;

        }



        // Overall Narrative
        public struct collectable
        {
            public string name;
            public Color color;
            public bool started;
            public bool completed;
            public bool[] pointsCollected;
        }



        public void LoadLocalState()
        {

            started = PlayerPrefsX.GetBool("Started", false);
            completed = PlayerPrefsX.GetBool("Completed", false);

            print("statreted" + started);

            if (!started)
            {
                startAnimation.Play();
            }
            else
            {
                startAnimation.SetEndValues();
            }



        }


        public void SetInitialState()
        {

        }

        public void SetStarted()
        {
            started = true;
            PlayerPrefsX.SetBool("Started", started);
        }


        public void SetCompleted()
        {
            completed = true;
            PlayerPrefsX.SetBool("Completed", completed);
        }



        public void UpdateState()
        {

        }

        public void SaveState()
        {

        }

        public void LoadState()
        {

        }



        public void CollectableCollected(Collectable c)
        {

        }
    */

}
