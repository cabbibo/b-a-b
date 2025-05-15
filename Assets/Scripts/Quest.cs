using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


/*
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Quest))]
public class QuestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Quest myScript = (Quest)target;


        if(GUILayout.Button("Set State")){

        }


        if (GUILayout.Button("Discover Quest"))
        {
            myScript.DiscoverQuest();
        }

        if (GUILayout.Button("Start Quest"))
        {
            myScript.StartQuest();
        }

        if (GUILayout.Button("Complete Quest"))
        {
            myScript.CompleteQuest();
        }

        if (GUILayout.Button("Reset Quest"))
        {
            myScript.Reset();
        }

        if(GUILayout.Button("Initialize Quest"))
        {
            myScript.Initialize();
        }

        if(GUILayout.Button("Enter Quest"))
        {
            myScript.OnEnterQuest();
        }

        if(GUILayout.Button("Exit Quest"))
        {
            myScript.OnExitQuest();
        }

        if(GUILayout.Button("Complete Animation Finished"))
        {
            myScript.OnCompletedAnimationFinished();
        }

        if(GUILayout.Button("Add to Completion"))
        {
            myScript.AddToCompletion(0.1f);
        }

        if(GUILayout.Button("Set Completion"))
        {
            myScript.SetCompletion(0.5f);
        }



    }
}

#endif

*/


public class Quest : MonoBehaviour
{
    public Activity activity;
    public Portal   portal;
    public float    amountComplete;

    public Slide discoveredSlide;
    public Slide startedSlide;
    public Slide completedSlide;


    public void AddToCompletion( float amount )
    {
        amountComplete += amount;
        activity.SetCompleteAmount( amountComplete );
    }

    public void SetCompletion( float amount )
    {
        amountComplete = amount;
        activity.SetCompleteAmount( amountComplete );
    }


    public void SetStartValues( Slide slide )
    {
        God.playableDirector.playableAsset = slide.timeline;
        God.playableDirector.time = 0;
        God.playableDirector.Evaluate();
    }

    public void SetEndValues( Slide slide )
    {
        God.playableDirector.playableAsset = slide.timeline;
        God.playableDirector.time = slide.timeline.duration;
        God.playableDirector.Evaluate();
    }

    public void DiscoverQuest()
    {


    }

    public void CompleteQuest()
    {

        portal.OpenPortal();


    }

    public void StartQuest()
    {


    }

    public void SetVoidState()
    {

        if ( completedSlide != null ) {
            SetStartValues( completedSlide );
        }

        if ( startedSlide != null ) {
            SetStartValues( startedSlide );
        }

        if ( discoveredSlide != null ) {
            SetStartValues( discoveredSlide );
        }

    }


    public void SetState()
    {

        print( "setting state in quest" );
        print( activity.completed );
        SetVoidState();

        if ( activity.discovered ) {
            SetDiscoveredState();
        }

        if ( activity.started ) {
            SetStartedState();
        }

        if ( activity.completed ) {
            SetCompletedState();
        }


    }

    public void SetDiscoveredState()
    {
        portal.SetPortalOff();

        print( "Set Discover State" );

        if ( discoveredSlide != null ) {
            SetEndValues( discoveredSlide );
        }
    }

    public void SetStartedState()
    {
        portal.SetPortalOff();

        print( "Set Start State" );

        if ( startedSlide != null ) {
            SetEndValues( startedSlide );
        }
    }

    public void SetCompletedState()
    {

        print( "Set Complete State" );
        portal.SetPortalFull();

        if ( completedSlide != null ) {
            SetEndValues( completedSlide );
        }

    }
}