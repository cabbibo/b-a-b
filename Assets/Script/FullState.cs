using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullState : MonoBehaviour
{



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



    /*
        UGC
    */
    public List<race> races;
    public List<plantable> plantables;



    public struct plantable{
        
        
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


    public struct speedTrap{
        public string name;
        public bool unlocked;
        public Color color;
        public string[] highScoreIDs;
        public float[] highScores;  

        public Transform location;

    }



    // Overall Narrative
    public struct collectable{
        public string name;
        public Color color;
        public bool started;
        public bool completed;
        public bool[] pointsCollected;
    }


 
    public void LoadLocalState(){

        started = PlayerPrefsX.GetBool("Started", false);
        completed = PlayerPrefsX.GetBool("Completed", false);

        print("statreted" + started);

        if( !started ){
            startAnimation.Play();
        }else{
            startAnimation.SetEndValues();
        }



    }
    

    public void SetInitialState(){

    }

    public void SetStarted(){
        started = true;
        PlayerPrefsX.SetBool("Started", started);
    }


    public void SetCompleted(){
        completed = true;
        PlayerPrefsX.SetBool("Completed", completed);
    }



    public void UpdateState(){

    }

    public void SaveState(){

    }

    public void LoadState(){

    }



    public void CollectableCollected(Collectable c){
        
    }
 
  
}
