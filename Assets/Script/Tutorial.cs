using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WrenUtils;

public class Tutorial : MonoBehaviour
{


    public bool canSkip;
    public TutorialState[] tutorialStates;



    public Slider amountComplete;

    public int state;
    public int oState;

    public float timeBetweenSections = 1;

    public float stateFillSpeed = 1;



    public bool betweenStates;

    public float stateComplete;

    public int tutorialComplete;

    public PlayCutScene[] introAnimations;


    public void CheckIfNeeded()
    {



        tutorialComplete = God.state.tutorialFinished ? 1 : 0;// PlayerPrefs.GetInt("_TutorialComplete", 0);


        // If we have completed, 
        // set all the values for what 
        // happens during the tutorial
        if (tutorialComplete > 0)
        {


            for (int i = 0; i < introAnimations.Length; i++)
            {
                introAnimations[i].SetStartValues();
                introAnimations[i].SetEndValues();
                introAnimations[i].enabled = false;
            }

            for (int i = 0; i < tutorialStates.Length; i++)
            {
                tutorialStates[i].OnStartEvent.Invoke();
                tutorialStates[i].OnCompleteEvent.Invoke();
                //tutorialStates
            }

            var p = (ReachLocationTutorialState)tutorialStates[6];
            p.hasFired = true;


            p = (ReachLocationTutorialState)tutorialStates[19];
            p.hasFired = true;

            // Otherwise, 
            // Play the animation!    
        }
        else
        {

            for (int i = 0; i < introAnimations.Length; i++)
            {
                introAnimations[i].SetStartValues();
                introAnimations[i].enabled = false;
            }

            introAnimations[0].enabled = true;
            introAnimations[0].SetStartValues();
            introAnimations[0].Play();
        }


    }

    // Start is called before the first frame update
    void OnEnable()
    {
        state = -1;
        amountComplete.gameObject.SetActive(false);
        amountComplete.value = 0;
    }

    // Update is called once per frame
    void Update()
    {


        if (God.wren != null && !betweenStates && tutorialComplete == 0)
        {

            // Going in from intro animation!
            if (state == -1)
            {
                state = 0;
                God.wren.state.canTakeOff = false;
                God.audio.Play(God.sounds.tutorialSectionStartSound);
                if (!tutorialStates[state].instant)
                {
                    amountComplete.value = 0;
                    amountComplete.gameObject.SetActive(true);
                }
                tutorialStates[0].OnStart();
            }
            else if (state == 0)
            {
                // Ground Move Forward
                if (God.input.left.y > .5f && God.input.right.y > .5f) { UpdatingState(); }
            }
            else if (state == 1)
            {
                // Ground Move Back
                if (God.input.left.y < -.5f && God.input.right.y < -.5f) { UpdatingState(); }
            }
            else if (state == 2)
            {
                // Ground Move Left
                if (God.input.left.x < -.5f && God.input.right.x < -.5f) { UpdatingState(); }
            }
            else if (state == 3)
            {
                // Ground Move Right
                if (God.input.left.x > .5f && God.input.right.x > .5f) { UpdatingState(); }
            }
            else if (state == 4)
            {
                // Ground Turn Left
                if (God.input.left.y < -.5f && God.input.right.y > .5f) { UpdatingState(); }
            }
            else if (state == 5)
            {
                // Ground Turn Right
                if (God.input.left.y > .5f && God.input.right.y < -.5f) { UpdatingState(); }
            }
            else if (state == 6)
            {
                // Get it from collision!
            }
            else if (state == 7)
            {
                // Take Off
                God.wren.state.canTakeOff = true;
                if (God.wren.input.o_ex < .5 && God.wren.input.ex > .5) { stateComplete = 11; UpdatingState(); }
            }
            else if (state == 8)
            {
                // Tilt Up
                if (God.input.left.y < -.5f && God.input.right.y < -.5f && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 9)
            {
                // Tilt Down
                if (God.input.left.y > .5f && God.input.right.y > .5f && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 10)
            {
                // turn left
                if (God.input.left.x > .5f && God.input.right.x > .5f && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 11)
            {
                // turn right
                if (God.input.left.x < -.5f && God.input.right.x < -.5f && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 12)
            {
                // tuck
                if (God.input.l2 > .5f && God.input.r2 > .5f && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 13)
            {
                // brake
                if (God.input.l3 && God.input.r3 && !God.wren.state.onGround) { UpdatingState(); }
            }
            else if (state == 14)
            {

                if (God.input.circle) { stateComplete = 11; UpdatingState(); }
                // Get it from collision!
            }
            else if (state == 15)
            {

                if (God.input.square) { stateComplete = 11; UpdatingState(); }

                // Get it from collision!
            }
            else if (state == 16)
            {
                God.wren.state.canTakeOff = true;
                if (God.wren.input.o_ex < .5 && God.wren.input.ex > .5) { stateComplete = 11; UpdatingState(); }
            }
            else if (state == 17)
            {

                if (God.input.l1 > .5f && God.input.r1 > .5f && !God.wren.state.onGround) { UpdatingState(); }

            }
            else if (state == 18)
            {

                if (God.input.triangle) { stateComplete = 11; UpdatingState(); }
            }
            else if (state == 19)
            {

                // Get it from collision!
            }
            else if (state == 20)
            {
                print("Get to portal");
            }

            if (canSkip && Input.GetKey("space"))
            {
                NextState();
            }
        }

    }

    void UpdatingState()
    {

        float fillSpeed = .01f * stateFillSpeed;
        if (canSkip) { fillSpeed *= 1000; }
        stateComplete += fillSpeed;
        amountComplete.value = stateComplete;
        if (stateComplete > 1)
        {
            NextState();
        }

    }

    public void NextState()
    {

        stateComplete = 0;
        amountComplete.value = 0;
        amountComplete.gameObject.SetActive(false);
        God.audio.Play(God.sounds.tutorialSuccessSound);
        tutorialStates[state].OnComplete();
        betweenStates = true;

        float fTime = timeBetweenSections;
        if (canSkip) { fTime = .01f; }
        StartCoroutine(StartNextSection(fTime));

    }


    void OnFinishTutorial()
    {
        God.state.OnTutorialFinish();
        //PlayerPrefs.SetInt("_TutorialComplete", 1);
    }


    IEnumerator StartNextSection(float time)
    {
        yield return new WaitForSeconds(time);

        StartSection();
        // Code to execute after the delay
    }

    void StartSection()
    {

        betweenStates = false;
        state++;

        print("section started");
        print(state);

        if (state == tutorialStates.Length)
        {
            OnFinishTutorial();
        }
        else
        {
            stateComplete = 0;
            if (!tutorialStates[state].instant)
            {
                amountComplete.value = 0;
                amountComplete.gameObject.SetActive(true);
            }
            God.audio.Play(God.sounds.tutorialSectionStartSound);
            tutorialStates[state].OnStart();
        }

    }


    public void ReachLocationStateHit(TutorialState tutState)
    {

        print("tutorial state reached");

        if (tutState == tutorialStates[state])
        {
            print("tutorial state reached inside");
            stateComplete = 11;
            UpdatingState();
        }
    }




}
