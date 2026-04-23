using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TutorialEnder : MonoBehaviour
{

    public Transform[] tutorialObjects;

    public GameObject island;
    public GameObject island2;
    public GameObject tutorialOcean;
    public GameObject mainOcean;
    public GameObject portal;

    public float timeBetweenSpawns = 10;

    float lastSpawnTime;

    public FlyingTutorial flyingTutorial;
    public void OnEnable()
    {

        flyingTutorial.OnTutorialStart += StartTutorial;
        flyingTutorial.OnTutorialDiveFinished += EndTutorial;
        //ended = false;
        //lastSpawnTime = Time.time;
        // StartTutorial();

    }
    public void OnDisable()
    {

        flyingTutorial.OnTutorialStart -= StartTutorial;
        flyingTutorial.OnTutorialDiveFinished -= EndTutorial;
    }


    public void EndTutorial()
    {

        if (God.wren)
        {

            Vector3 shift = transform.position - God.wren.transform.position;
            foreach (Transform t in tutorialObjects)
            {
                t.position += shift;
            }

            God.wren.PhaseShift(transform.position);
        }

        island.SetActive(true);
        tutorialOcean.SetActive(false);
        mainOcean.SetActive(true);
        portal.SetActive(true);


    }


    public void StartTutorial()
    {
        if (God.wren)
        {
            God.wren.PhaseShift(transform.position);
        }

        island.SetActive(false);
        tutorialOcean.SetActive(true);
        island2.SetActive(false);
        portal.SetActive(false);
        mainOcean.SetActive(false);
    }


}
