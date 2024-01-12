using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TutorialEnder : MonoBehaviour
{

    public Transform[] tutorialObjects;

    public GameObject island;
    public GameObject portal;

    public float timeBetweenSpawns = 10;

    float lastSpawnTime;
    public void OnEnable()
    {

        FlyingTutorialSequence.OnTutorialDiveFinished += EndTutorial;
        //ended = false;
        //lastSpawnTime = Time.time;
        // StartTutorial();

    }


    public bool ended = false;

    public void EndTutorial()
    {

        Vector3 shift = transform.position - God.wren.transform.position;
        foreach (Transform t in tutorialObjects)
        {
            t.position += shift;
        }
        ended = true;
        God.wren.PhaseShift(transform.position);
        //island.SetActive(true);
    }


    public void StartTutorial()
    {
        ended = false;
        God.wren.PhaseShift(transform.position);
    }

    public void Update()
    {


        if (ended)
        {
            island.SetActive(true);
            portal.SetActive(true);
        }
        else
        {
            island.SetActive(false);
            portal.SetActive(false);
        }
    }


}
