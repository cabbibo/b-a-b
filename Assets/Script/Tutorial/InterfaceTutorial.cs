using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WrenUtils;

public class InterfaceTutorial : MonoBehaviour
{
    public Renderer fade;

    public CanvasGroup groupContainer;

    public CanvasGroup groupController;
    public CanvasGroup groupText;
    public CanvasGroup groupPoetry;


    public CanvasGroup xToContinue;

    public TextMeshProUGUI controllerText;
    public TextMeshProUGUI poetryText;

    public RectTransform progressBar;

    public bool debug;
    public int  debugCamIdx = 0;

    public Gradient fadeGradient;


    private float _lastSequenceTime;


    private MaterialPropertyBlock bgMpr;

    public void OnEnable()
    {
        groupContainer.alpha = 0;
    }

    public void SetBGFade( float t )
    {
        if ( bgMpr == null ) {
            bgMpr = new MaterialPropertyBlock();
        }

        fade.gameObject.SetActive( t > 0 );

        if ( Mathf.Approximately( t , 0 ) ) {
            return;
        }

        fade.GetPropertyBlock( bgMpr );
        bgMpr.SetColor( "_Color" , fadeGradient.Evaluate( t ) );
        fade.SetPropertyBlock( bgMpr );
    }


    public IEnumerator WaitForXToContinue()
    {
        bool wait = true;
        float t = 0f;
        groupContainer.alpha = 1;
        groupContainer.gameObject.SetActive( true );
        print( "waiting for X" );
        ShowProgress( t );
        ShowContinue( true );


        //yield return WaitWithCheat(0.5f);

        bool lastX = false; // = God.input.x;
        wait = true;

        while (wait) {
            if ( !lastX && God.input.x ) {
                wait = false;
            }

            if ( Application.isEditor && Input.GetKeyDown( KeyCode.Space ) ) {
                wait = false;
            }

            lastX = God.input.x;
            yield return null;
        }

        _lastSequenceTime = Time.unscaledTime;

        //  groupContainer.alpha = 0;
        ShowContinue( false );
    }


    public IEnumerator FadeGroup( CanvasGroup group , float from = 0 , float to = 1 , float delay = 0 ,
        float duration = .5f )
    {

        group.gameObject.SetActive( true );
        float t = 0;
        float _ct = Time.unscaledTime;

        print( "Fade Routine started" );
        print( "From: " + from );
        print( "To: " + to );
        print( "Delay: " + delay );
        print( "Duration: " + duration );
        print( "Group :" + group.gameObject.name );

        while (t < duration) {

            print( "Fadding group :  " + group.gameObject.name + " : " + t / duration );

            if ( delay > 0 && Time.unscaledTime - _ct < delay ) {
                yield return null;
                continue;
            }

            group.alpha = Mathf.Lerp( from , to , t / duration );
            t += Time.unscaledDeltaTime;
            yield return null;
        }


        group.alpha = to;

    }


    public IEnumerator FadeBG( float from = 0 , float to = 1 )
    {
        float t = 0;
        float duration = 1;

        while (t < duration) {

            t += Time.unscaledDeltaTime;
            float nTime = t / duration;

            SetBGFade( Mathf.Lerp( from , to , nTime ) );
            yield return null;

        }
    }


    public IEnumerator WaitWithCheat( float seconds )
    {
        float t = 0;

        while (t < seconds) {
            if ( Input.GetKeyDown( KeyCode.Space ) ) {
                break;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public IEnumerator LerpCamera( float from , float to , float time )
    {
        float cT = 0;

        while (cT < time) {
            if ( Input.GetKeyDown( KeyCode.Space ) ) {
                break;
            }

            //cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, cT / time));
            cT += Time.unscaledDeltaTime;
            yield return null;
        }
        //cinematicCamera.tutorialCameraIdx = to;
    }


    public IEnumerator LerpCamera( CinematicCamera from , CinematicCamera to , float time )
    {
        float cT = 0;

        while (cT < time) {
            if ( Input.GetKeyDown( KeyCode.Space ) ) {
                break;
            }

            //cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, cT / time));
            God.cameraManager.cinematicManager.LerpCamera( from.info , to.info , cT / time );


            cT += Time.unscaledDeltaTime;
            yield return null;
        }

        God.cameraManager.cinematicManager.SetCamera( to.info , 1 );
    }


    private enum Camera
    {
        Closeup  = 0 ,
        TopClose = 1 ,
        TopFar   = 2 ,
        Front    = 3 ,
        Behind   = 4 ,
        Play     = 5
    }

    public void ShowProgress( float t = 0 )
    {

        //        print("ShowProgress: " + t);
        progressBar.transform.parent.gameObject.SetActive( t > 0 );
        progressBar.localScale = new Vector3( Mathf.Clamp01( t ) , 2 , 1 );
        progressBar.localPosition = new Vector3( -1 + t , 0 , 0 );
    }


    public void ShowText( string text = null )
    {
        // print("ShowText: " + text);
        controllerText.transform.parent.gameObject.SetActive( !string.IsNullOrEmpty( text ) );
        controllerText.text = text;

    }

    public void ShowPoetryText( string text = null )
    {
        // print("ShowText: " + text);
        poetryText.transform.parent.gameObject.SetActive( !string.IsNullOrEmpty( text ) );
        poetryText.text = text;

    }

    public void HidePoetryText()
    {
        poetryText.transform.parent.gameObject.SetActive( false );
    }


    public void ShowContinue( bool bShow )
    {
        // print("ShowContinue: " + bShow);
        xToContinue.gameObject.SetActive( bShow );
    }


    public void TutorialSectionComplete()
    {
        print( "TutorialSectionComplete" );
        StartCoroutine( FadeGroup( groupContainer , 1 , 0 ) );
        ShowProgress( 0 );
        God.audio.Play( God.sounds.texturalHitClips );
        God.audio.Play( God.sounds.tuiCallClips );
        WaitWithCheat( 3 );

    }

    public void SmallSectionComplete()
    {
        //   ShowProgress( 0 );
        God.audio.Play( God.sounds.texturalHitClips );
        God.audio.Play( God.sounds.tuiCallClips );

    }


    /**
    ____ ___ ____   _   _ _____ _     ____  _____ ____  ____
   | __ )_ _/ ___| | | | | ____| |   |  _ \| ____|  _ \/ ___|
   |  _ \| | |  _  | |_| |  _| | |   | |_) |  _| | |_) \___ \
   | |_) | | |_| | |  _  | |___| |___|  __/| |___|  _ < ___) |
   |____/___\____| |_| |_|_____|_____|_|   |_____|_| \_\____/

*/
    public enum ControllerHint
    {
        None ,
        Dive ,
        Left ,
        Right ,
        Forward ,
        Back ,
        Hold ,
        Takeoff ,
        Flap ,
        Swoop ,
        Release ,
        Release2 ,
        Gentle ,
        Boost ,
        Ping ,
        Wiggle ,
        TakeOff ,
        Carry ,
        LeftStick ,
        RightStick ,
        L2 ,
        R2
    }

    [Header( "FeedbackGroups" )]
    public ControllerUIFeedbackGroup groupDive;

    public ControllerUIFeedbackGroup groupLeft;
    public ControllerUIFeedbackGroup groupRight;
    public ControllerUIFeedbackGroup groupUp;
    public ControllerUIFeedbackGroup groupDown;
    public ControllerUIFeedbackGroup groupHold;
    public ControllerUIFeedbackGroup groupFlap;
    public ControllerUIFeedbackGroup groupSwoop;
    public ControllerUIFeedbackGroup groupRelease;
    public ControllerUIFeedbackGroup groupRelease2;

    public ControllerUIFeedbackGroup groupGentle;
    public ControllerUIFeedbackGroup groupBoost;
    public ControllerUIFeedbackGroup groupPing;
    public ControllerUIFeedbackGroup groupWiggle;
    public ControllerUIFeedbackGroup groupTakeOff;

    public ControllerUIFeedbackGroup groupCarry;

    public ControllerUIFeedbackGroup groupLeftStick;
    public ControllerUIFeedbackGroup groupRightStick;

    public ControllerUIFeedbackGroup groupL2;
    public ControllerUIFeedbackGroup groupR2;

    [Header( "Controller" )]
    public GameObject button1;

    public GameObject button2;
    public GameObject button3;
    public GameObject button4;

    public GameObject dUp;
    public GameObject dDown;
    public GameObject dLeft;
    public GameObject dRight;

    public GameObject leftStick;
    public GameObject rightStick;

    public GameObject allDirectionsLeftStick;
    public GameObject allDirectionsRightStick;

    public GameObject upLeftStick;
    public GameObject upRightStick;
    public GameObject downLeftStick;
    public GameObject downRightStick;
    public GameObject leftLeftStick;
    public GameObject leftRightStick;
    public GameObject rightLeftStick;
    public GameObject rightRightStick;

    public GameObject l1;
    public GameObject l2;
    public GameObject l3;

    public GameObject r1;
    public GameObject r2;
    public GameObject r3;

    public GameObject l1Up;
    public GameObject l2Up;

    public GameObject r1Up;
    public GameObject r2Up;

    public GameObject hold;
    public GameObject press;
    public GameObject release;
    public GameObject tap;

    public Image[]           allUIImages;
    public TextMeshProUGUI[] allUIText;


    public ControllerHint currentHint;
    public string         currentHintText;

    public IEnumerator ControllerHintSequence( ControllerHint hint )
    {
        SetControllerHint( hint );
        StartCoroutine( FadeGroup( groupContainer , 0 , 1 ) );
        yield return WaitWithCheat( 0.25f );

        float t = 0;

        while (t < 1) {
            if ( TestControllerHint( hint ) ) {
                t += Time.unscaledDeltaTime * .45f;
            } else {
                t = Mathf.Clamp01( t - Time.unscaledDeltaTime * 1.25f );
            }

            ShowProgress( t );
            yield return null;
        }

        ShowProgress( 0 );
        SetControllerHint( ControllerHint.None );
    }


    public bool TestControllerHint( ControllerHint hint )
    {
        switch (hint) {
            case ControllerHint.Left:
                return God.input.left.x < -.5f && God.input.right.x < -.5f;
            case ControllerHint.Right:
                return God.input.left.x > .5f && God.input.right.x > .5f;
            case ControllerHint.Forward:
                return God.input.left.y > .5f && God.input.right.y > .5f;
            case ControllerHint.Back:
                return God.input.left.y < -.5f && God.input.right.y < -.5f;
            case ControllerHint.Dive:
                return God.input.l2 > .5f && God.input.r2 > .5f;
            case ControllerHint.Hold:
                return God.input.l3 && God.input.r3;
        }

        return false;
    }


    public bool HandleSticksProgress( ref float t , float speed = 2f , bool gravity = true )
    {
        if (
            Mathf.Abs( God.input.left.x ) > .1f ||
            Mathf.Abs( God.input.left.y ) > .1f ||
            Mathf.Abs( God.input.right.x ) > .1f ||
            Mathf.Abs( God.input.right.y ) > .1f ||
            God.input.l2 > 0.1f ||
            God.input.r2 > 0.1f
        ) {
            t += Time.unscaledDeltaTime * .1f * speed;
        } else if ( gravity ) {
            t = Mathf.Clamp01( t - Time.unscaledDeltaTime * .05f );
        }

        ShowProgress( t );
        Debug.Log( t );
        return t < 1;
    }

    public ControllerUIFeedbackGroup currentFeedbackGroup;

    public void SetFeedbackGroup( ControllerUIFeedbackGroup group )
    {

        currentFeedbackGroup = group;

        button1.SetActive( group.button1 );
        button2.SetActive( group.button2 );
        button3.SetActive( group.button3 );
        button4.SetActive( group.button4 );

        dUp.SetActive( group.dUp );
        dDown.SetActive( group.dDown );
        dLeft.SetActive( group.dLeft );
        dRight.SetActive( group.dRight );
        leftStick.SetActive( group.leftStick );
        rightStick.SetActive( group.rightStick );
        allDirectionsLeftStick.SetActive( group.allDirectionsLeftStick );
        allDirectionsRightStick.SetActive( group.allDirectionsRightStick );
        upLeftStick.SetActive( group.upLeftStick );
        upRightStick.SetActive( group.upRightStick );
        downLeftStick.SetActive( group.downLeftStick );
        downRightStick.SetActive( group.downRightStick );
        leftLeftStick.SetActive( group.leftLeftStick );
        leftRightStick.SetActive( group.leftRightStick );
        rightLeftStick.SetActive( group.rightLeftStick );
        rightRightStick.SetActive( group.rightRightStick );

        l1.SetActive( group.l1 );
        l2.SetActive( group.l2 );
        l3.SetActive( group.l3 );

        r1.SetActive( group.r1 );
        r2.SetActive( group.r2 );
        r3.SetActive( group.r3 );

        l1Up.SetActive( group.l1Up );
        l2Up.SetActive( group.l2Up );

        r1Up.SetActive( group.r1Up );
        r2Up.SetActive( group.r2Up );

        hold.SetActive( group.hold );
        press.SetActive( group.press );
        release.SetActive( group.release );
        tap.SetActive( group.tap );


    }


    public void SetControllerHint( ControllerHint hint , string text = null )
    {


        currentHint = hint;
        print( "CONTROLLER HINT SET" );

        Debug.Log( "CONTROLLER HINT SET: " + hint );


        controllerText.transform.parent.gameObject.SetActive( hint !=
                                                              ControllerHint
                                                                  .None ); // turns it off if we arent using any text
        groupController.gameObject.SetActive( hint !=
                                              ControllerHint
                                                  .None ); // turns off the sticks if we are not using any hints

        groupText.gameObject.SetActive( hint != ControllerHint.None );


        if ( hint == ControllerHint.Left ) {
            SetFeedbackGroup( groupLeft );
        }

        if ( hint == ControllerHint.Right ) {
            SetFeedbackGroup( groupRight );
        }

        if ( hint == ControllerHint.Forward ) {
            SetFeedbackGroup( groupUp );
        }

        if ( hint == ControllerHint.Back ) {
            SetFeedbackGroup( groupDown );
        }

        if ( hint == ControllerHint.Dive ) {
            SetFeedbackGroup( groupDive );
        }

        if ( hint == ControllerHint.Hold ) {
            SetFeedbackGroup( groupHold );
        }

        if ( hint == ControllerHint.Flap ) {
            SetFeedbackGroup( groupFlap );
        }

        if ( hint == ControllerHint.Swoop ) {
            SetFeedbackGroup( groupSwoop );
        }

        if ( hint == ControllerHint.Release ) {
            SetFeedbackGroup( groupRelease );
        }

        if ( hint == ControllerHint.Release2 ) {
            SetFeedbackGroup( groupRelease2 );
        }

        if ( hint == ControllerHint.Gentle ) {
            SetFeedbackGroup( groupGentle );
        }

        if ( hint == ControllerHint.Boost ) {
            SetFeedbackGroup( groupBoost );
        }

        if ( hint == ControllerHint.Ping ) {
            SetFeedbackGroup( groupPing );
        }

        if ( hint == ControllerHint.Wiggle ) {
            SetFeedbackGroup( groupWiggle );
        }

        if ( hint == ControllerHint.TakeOff ) {
            SetFeedbackGroup( groupTakeOff );
        }

        if ( hint == ControllerHint.Carry ) {
            SetFeedbackGroup( groupCarry );
        }

        if ( hint == ControllerHint.LeftStick ) {
            SetFeedbackGroup( groupLeftStick );
        }

        if ( hint == ControllerHint.RightStick ) {
            SetFeedbackGroup( groupRightStick );
        }

        if ( hint == ControllerHint.L2 ) {
            SetFeedbackGroup( groupL2 );
        }

        if ( hint == ControllerHint.R2 ) {
            SetFeedbackGroup( groupR2 );
        }


        /*
        groupLeft.SetActive( hint == ControllerHint.Left );
        groupRight.SetActive( hint == ControllerHint.Right );
        groupUp.SetActive( hint == ControllerHint.Forward );
        groupDown.SetActive( hint == ControllerHint.Back );
        groupDive.SetActive( hint == ControllerHint.Dive );
        groupHold.SetActive( hint == ControllerHint.Hold );
        groupFlap.SetActive( hint == ControllerHint.Flap );
        groupSwoop.SetActive( hint == ControllerHint.Swoop );
        groupRelease.SetActive( hint == ControllerHint.Release );
        groupRelease2.SetActive( hint == ControllerHint.Release2 );
        groupGentle.SetActive( hint == ControllerHint.Gentle );
        groupBoost.SetActive( hint == ControllerHint.Boost );
        groupPing.SetActive( hint == ControllerHint.Ping );
        groupWiggle.SetActive( hint == ControllerHint.Wiggle ); // this is the default state when we dont want any hints
        groupTakeOff.SetActive( hint ==
                                ControllerHint.TakeOff ); // this is the default state when we dont want any hints
        groupCarry.SetActive( hint == ControllerHint.Carry ); // this is the default state when we dont want any hints
        groupLeftStick.SetActive( hint == ControllerHint.LeftStick );
        groupRightStick.SetActive( hint == ControllerHint.RightStick );
        groupL2.SetActive( hint == ControllerHint.L2 );
        groupR2.SetActive( hint == ControllerHint.R2 );
*/


        if ( text != null ) {
            controllerText.text = text;
            currentHintText = controllerText.text;

        } else {

            switch (hint) {
                case ControllerHint.Dive:
                    controllerText.text = "DIVE";
                    break;
                case ControllerHint.Left:
                    controllerText.text = "LEFT";
                    break;
                case ControllerHint.Right:
                    controllerText.text = "RIGHT";
                    break;
                case ControllerHint.Forward:
                    controllerText.text = "DOWN";
                    break;
                case ControllerHint.Back:
                    controllerText.text = "UP";
                    break;
                case ControllerHint.Hold:
                    controllerText.text = "BRAKE";
                    break;
                case ControllerHint.Flap:
                    controllerText.text = "FLAP";
                    break;
                case ControllerHint.Swoop:
                    controllerText.text = "SWOOP";
                    break;
                case ControllerHint.Release:
                    controllerText.text = "GLIDE";
                    break;
                case ControllerHint.Release2:
                    controllerText.text = "REST";
                    break;
                case ControllerHint.Gentle:
                    controllerText.text = "WIGGLE";
                    break;
                case ControllerHint.Boost:
                    controllerText.text = "BOOST";
                    break;
                case ControllerHint.Ping:
                    controllerText.text = "PING";
                    break;
                case ControllerHint.Wiggle:
                    controllerText.text = "WIGGLE";
                    break;
                case ControllerHint.TakeOff:
                    controllerText.text = "TAKE OFF";
                    break;
                case ControllerHint.Carry:
                    controllerText.text = "CARRY";
                    break;

                case ControllerHint.LeftStick:
                    controllerText.text = "WIGGLE";
                    break;

                case ControllerHint.RightStick:
                    controllerText.text = "WIGGLE";
                    break;
                case ControllerHint.L2:
                    controllerText.text = "TAP-TAP";
                    break;
                case ControllerHint.R2:
                    controllerText.text = "TAP-TAP";
                    break;
                case ControllerHint.None:
                    controllerText.text = "";
                    break;

                /* case ControllerHint.y:
                     controllerText.text = "PRESS sticks to HOLD";
                     break;*/
                /*            case ControllerHint.TakeOff:
                                controllerText.text = "PRESS X to TAKE OFF";
                                break;*/
            }


        }

        print( "setting text: " + controllerText.text );

        if ( controllerText.text != "" ) {

            print( "starting co routine" );
            groupText.alpha = 1;

            if ( fadeTextCoroutine != null ) {
                Debug.LogWarning( "COROUTINE STOPPING" );
                StopCoroutine( fadeTextCoroutine );
                fadeTextCoroutine = null;
            }

            FadeOutText();

        } else {
            groupText.alpha = 0;
        }

        currentHintText = controllerText.text;


    }


    public void FadeFullGroupCoroutine( float start , float end )
    {
        StartCoroutine( FadeGroup( groupContainer , start , end ) );
    }

    private IEnumerator fadeTextCoroutine;

    public void FadeOutText()
    {

        // Setting Fade Out
        print( "setting Fade Out" );
        fadeTextCoroutine = FadeGroup( groupText , 1 , 0 , 1.3f , 1f );
        StartCoroutine( fadeTextCoroutine );


    }

    public void FadeInIfOff()
    {

        // This function is used to ensure that the groupContainer is faded in if it is currently off. 
        // This can be useful when you want to make sure the tutorial hints are visible to the player.

        if ( groupContainer.alpha <= 0 ) {
            StartCoroutine( FadeGroup( groupContainer , 0 , 1 ) ); // Fading in the groupContainer from 0 to 1.
        }
    }


    public void Update()
    {
        SetUIPulse();
    }

    public void SetUIPulse()
    {

        if ( currentFeedbackGroup != null ) {
            var c = Color.Lerp( currentFeedbackGroup.color1 , currentFeedbackGroup.color2 ,
                (-Mathf.Cos( currentFeedbackGroup.pulseSpeed * Time.time ) + 1) / 2 );
//        print( c );

            for ( int i = 0; i < allUIImages.Length; i++ ) {
                allUIImages[i].color = c;
            }

            for ( int i = 0; i < allUIText.Length; i++ ) {
                allUIText[i].color = c;
            }

        }

    }
}