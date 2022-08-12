using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableController : MonoBehaviour
{
    public Collectable[] collectables;

    public int[] collectablesCollected;
    public int totalCollectablesCollected;

    public void FullCollect(Collectable c){

        for( int i = 0; i < collectables.Length; i++ ){

            if( collectables[i] == c ){
                collectablesCollected[i] = 2;
            }


        }


        SetCollectedValues();

        PlayerPrefsX.SetIntArray( "_CollectablesCollected",collectablesCollected);

        UpdateWrenInfo();
        


    }

    public void FullStart( Collectable c ){
        
        for( int i = 0; i < collectables.Length; i++ ){

            if( collectables[i] == c ){
                collectablesCollected[i] = 1;
            }


        }


        SetCollectedValues();

        PlayerPrefsX.SetIntArray( "_CollectablesCollected",collectablesCollected);

        UpdateWrenInfo();

    }
    public void Uncollect( Collectable c ){
        
       for( int i = 0; i < collectables.Length; i++ ){

            if( collectables[i] == c ){
                collectablesCollected[i] = 0;
            }

        }


        PlayerPrefsX.SetIntArray( "_CollectablesCollected",collectablesCollected);

        UpdateWrenInfo();

    }
    

    public void UpdateWrenInfo(){

        if( God.wren ){
            God.wren.collection.OnCollect();
        }

    }

    void Start(){


        collectablesCollected =  PlayerPrefsX.GetIntArray( "_CollectablesCollected",0,collectables.Length);

        if( collectablesCollected.Length  != collectables.Length ){
            collectablesCollected = new int[ collectables.Length];
        }



        totalCollectablesCollected = collectablesCollected.Length;

        for( int i = 0;  i < collectablesCollected.Length; i++ ){
            
            if( collectablesCollected[i] == 0 ) collectables[i].ResetState();
            if( collectablesCollected[i] == 1 ) collectables[i].SetStarted();
            if( collectablesCollected[i] == 2 )  collectables[i].SetCollected();
            
        }
    


    }


    public void SetCollectedValues(){
        
        for( int i = 0;  i < collectablesCollected.Length; i++ ){
            
            if( collectablesCollected[i] == 0 ) collectables[i].ResetState();
            if( collectablesCollected[i] == 1 ) collectables[i].SetStarted();
            if( collectablesCollected[i] == 2 )  collectables[i].SetCollected();
            
        }
    }



}
