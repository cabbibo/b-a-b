using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TutorialIslandEnder : MonoBehaviour
{


    public bool hasCrashed;
    public PlayCutScene cutScene;

    public GameObject island;
    public bool completed;


    public WindCircle windCircle;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (God.wren)
        {
            if (hasCrashed && God.wren.shards.GetBodyShardPercentage() >= 1 && completed == false)
            {


                print("WE DID IT");
                OnEnoughCollected();

            }
        }
    }


    void OnEnoughCollected()
    {

        completed = true;

        island.SetActive(true);
        cutScene.Play();
    }


    public void SetStart()
    {
        windCircle.enabled = true;
    }

    public void SetEnd()
    {
        windCircle.enabled = false;
        enabled = false; // dont need to care about self anymore
    }

    public void OnCutSceneFinished()
    {


        print("tutorialIsland Finsihed");
        God.state.OnTutorialIslandFinish();

        SetEnd();
        // Set tutorial finished, turn off wind, set us to look at the island

    }


}
