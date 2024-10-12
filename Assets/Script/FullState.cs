using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

[ExecuteAlways]
public class FullState : MonoBehaviour

{


    public bool gameStarted;
    public bool tutorialFinished;
    public bool tutorialIslandFinished;
    public bool islandDiscovered;

    public bool[] biomesDiscovered;
    public bool[] biomesStarted;
    public bool[] biomesCompleted;



    public bool[] questsDiscovered;
    public bool[] questsStarted;
    public bool[] questsCompleted;

    public int numQuests = 7;



    public bool[] activitiesDiscovered;
    public bool[] activitiesStarted;
    public bool[] activitiesCompleted;


    public int numActivities = 100;


    public bool gameFinished;


    public int currentSceneID;
    public int currentBiomeID;
    public int currentQuestID;


    public int numBiomes = 7;

    public Vector3 lastPosition;


    public float totalTimeInGame;

    public void OnEnable()
    {
        //   print("full state enabled");
        LoadState();
    }

    public int framesBetweenSaves = 2000;

    public void OnDestroy()
    {
        UpdateLastPosition();
        UpdateState();


    }
    public void Update()
    {

        totalTimeInGame += Time.deltaTime;
        PlayerPrefs.SetInt("_TotalSecondsInGame", (int)totalTimeInGame);

        if (Time.frameCount % framesBetweenSaves == framesBetweenSaves - 1)
        {
            UpdateLastPosition();
        }

    }
    public void UpdateState()
    {

        print("Updating State");
        // TODO save all this to player prefs

        PlayerPrefsX.SetBool("_GameStarted", gameStarted);
        PlayerPrefsX.SetBool("_TutorialFinished", tutorialFinished);
        PlayerPrefsX.SetBool("_TutorialIslandFinished", tutorialIslandFinished);
        PlayerPrefsX.SetBool("_IslandDiscovered", islandDiscovered);

        PlayerPrefsX.SetBool("_GameFinished", gameFinished);

        PlayerPrefsX.SetBoolArray("_BiomesStarted", biomesStarted);
        PlayerPrefsX.SetBoolArray("_BiomesCompleted", biomesCompleted);
        PlayerPrefsX.SetBoolArray("_BiomesDiscovered", biomesDiscovered);


        PlayerPrefsX.SetBoolArray("_QuestsStarted", questsStarted);
        PlayerPrefsX.SetBoolArray("_QuestsCompleted", questsCompleted);
        PlayerPrefsX.SetBoolArray("_QuestsDiscovered", questsDiscovered);



        PlayerPrefs.SetInt("_CurrentScene", currentSceneID);
        PlayerPrefs.SetInt("_CurrentBiome", currentBiomeID);
        PlayerPrefs.SetInt("_CurrentQuest", currentQuestID);

        PlayerPrefs.SetInt("_TotalSecondsInGame", (int)totalTimeInGame);



    }

    public void UpdateLastPosition()
    {

        if (God.wren != null)
        {
            lastPosition = God.wren.transform.position;
            // saving the position of the bird!
            PlayerPrefsX.SetVector3("_LastPosition", lastPosition);
        }
    }

    public void SetLastPosition(Vector3 v)
    {
        lastPosition = v;
        PlayerPrefsX.SetVector3("_LastPosition", lastPosition);
    }



    public void LoadState()
    {


        // TODO load all this from player prefs

        tutorialFinished = PlayerPrefsX.GetBool("_TutorialFinished", false);
        tutorialIslandFinished = PlayerPrefsX.GetBool("_TutorialIslandFinished", false);

        gameStarted = PlayerPrefsX.GetBool("_GameStarted", false);
        gameFinished = PlayerPrefsX.GetBool("_GameFinished", false);
        islandDiscovered = PlayerPrefsX.GetBool("_IslandDiscovered", false);

        currentSceneID = PlayerPrefs.GetInt("_CurrentScene", 0);
        currentBiomeID = PlayerPrefs.GetInt("_CurrentBiome", -1);
        currentQuestID = PlayerPrefs.GetInt("_CurrentQuest", -1);

        ///        print("Loading last Position");
        lastPosition = PlayerPrefsX.GetVector3("_LastPosition", Vector3.zero);


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





        questsStarted = PlayerPrefsX.GetBoolArray("_QuestsStarted");
        questsCompleted = PlayerPrefsX.GetBoolArray("_QuestsCompleted");
        questsDiscovered = PlayerPrefsX.GetBoolArray("_QuestsDiscovered");

        if (questsStarted.Length != numQuests)
        {
            questsStarted = new bool[numQuests];
        }

        if (questsCompleted.Length != numQuests)
        {
            questsCompleted = new bool[numQuests];
        }

        if (questsDiscovered.Length != numQuests)
        {
            questsDiscovered = new bool[numQuests];
        }



        activitiesStarted = PlayerPrefsX.GetBoolArray("_ActivitiesStarted");
        activitiesCompleted = PlayerPrefsX.GetBoolArray("_ActivitiesCompleted");
        activitiesDiscovered = PlayerPrefsX.GetBoolArray("_ActivitiesDiscovered");

        if (activitiesStarted.Length != numActivities)
        {
            activitiesStarted = new bool[numActivities];
        }

        if (activitiesCompleted.Length != numActivities)
        {
            activitiesCompleted = new bool[numActivities];
        }

        if (activitiesDiscovered.Length != numActivities)
        {
            activitiesDiscovered = new bool[numActivities];
        }



        totalTimeInGame = (float)PlayerPrefs.GetInt("_TotalSecondsInGame", 0);




    }


    public void ResetAll()
    {


        PlayerPrefs.DeleteAll();


        currentSceneID = 0;
        currentBiomeID = -1;

        gameStarted = false;
        tutorialFinished = false;
        tutorialIslandFinished = false;
        gameFinished = false;
        islandDiscovered = false;
        lastPosition = Vector3.zero;

        biomesDiscovered = new bool[numBiomes];
        biomesStarted = new bool[numBiomes];
        biomesCompleted = new bool[numBiomes];


        questsDiscovered = new bool[numQuests];
        questsStarted = new bool[numQuests];
        questsCompleted = new bool[numQuests];

        activitiesDiscovered = new bool[numQuests];
        activitiesStarted = new bool[numQuests];
        activitiesCompleted = new bool[numQuests];

        totalTimeInGame = 0;



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

    public void SetCurrentQuest(int i)
    {
        currentQuestID = i;
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


        bool allCompleted = true;
        for (int j = 0; j < biomesCompleted.Length; j++)
        {
            if (!biomesCompleted[j])
            {
                allCompleted = false;
            }
        }

        if (allCompleted)
        {
            OnGameFinish();
        }

        UpdateState();
    }


    public void OnQuestDiscovered(int i)
    {
        questsDiscovered[i] = true;
        UpdateState();
    }

    public void OnQuestStarted(int i)
    {
        questsStarted[i] = true;
        UpdateState();
    }

    public void OnQuestCompleted(int i)
    {
        questsCompleted[i] = true;


        bool allCompleted = true;
        for (int j = 0; j < questsCompleted.Length; j++)
        {
            if (!questsCompleted[j])
            {
                allCompleted = false;
            }
        }

        if (allCompleted)
        {
            OnGameFinish();
        }

        UpdateState();
    }


    public void ResetQuest(int i)
    {
        questsDiscovered[i] = false;
        questsStarted[i] = false;
        questsCompleted[i] = false;
        UpdateState();
    }


    public void OnActivityDiscovered(int i)
    {
        activitiesDiscovered[i] = true;
        UpdateState();
    }

    public void OnActivityStarted(int i)
    {
        activitiesStarted[i] = true;
        UpdateState();
    }

    public void OnActivityCompleted(int i)
    {
        activitiesCompleted[i] = true;


        bool allCompleted = true;
        for (int j = 0; j < activitiesCompleted.Length; j++)
        {
            if (!activitiesCompleted[j])
            {
                allCompleted = false;
            }
        }

        if (allCompleted)
        {
            OnGameFinish();
        }

        UpdateState();
    }


    public void ResetActivity(int i)
    {
        activitiesDiscovered[i] = false;
        activitiesStarted[i] = false;
        activitiesCompleted[i] = false;
        UpdateState();
    }

    public void OnGameStart()
    {
        gameStarted = true;
        UpdateState();
    }


    public void OnTutorialFinish()
    {
        tutorialFinished = true;
        UpdateState();
    }


    public void OnTutorialIslandFinish()
    {
        tutorialIslandFinished = true;
        UpdateState();
    }
    public void OnGameFinish()
    {
        gameFinished = true;
        UpdateState();
    }


    public void OnIslandDiscovered()
    {
        islandDiscovered = true;
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
