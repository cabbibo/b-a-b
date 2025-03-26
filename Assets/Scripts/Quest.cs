using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;



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



public class Quest : MonoBehaviour
{

    public bool discovered;
    public bool started;
    public bool completed;

    public int id;

    public Portal portal;
    public float amountComplete;


    public PlayCutScene discoveredAnimation;
    public PlayCutScene startedAnimation;
    public PlayCutScene completedAnimation;

    public List<GameObject> localObjects;


    public void AddToCompletion(float amount)
    {
        amountComplete += amount;

        if (amountComplete >= 1)
        {

            print("Quest complete");
            amountComplete = 1;
            CompleteQuest();
        }
    }

    public void SetCompletion(float amount)
    {
        amountComplete = amount;
        if (amountComplete >= 1)
        {
            print("Quest complete");
            amountComplete = 1;
            CompleteQuest();
        }

    }



    public void DiscoverQuest()
    {


        if (discovered == false)
        {
            print("HELLOOO111");
            discovered = true;
            God.state.OnQuestDiscovered(id);
            if (discoveredAnimation != null) { discoveredAnimation.Play(); }
        }

    }
    public void CompleteQuest()
    {

        if (completed == false)
        {
            print("HELLOOO");

            completed = true;
            portal.OpenPortal();
            if (completedAnimation != null) { completedAnimation.Play(); }
        }
        else
        {
            print("already completed");

        }

    }

    public void StartQuest()
    {

        if (started == false)
        {
            started = true;
            God.state.OnQuestStarted(id);
            if (startedAnimation != null) { startedAnimation.Play(); }
        }
        else
        {
            print("already started");
        }
    }


    public void Initialize()
    {

        discovered = God.state.questsDiscovered[id];
        started = God.state.questsStarted[id];
        completed = God.state.questsCompleted[id];


        if (completedAnimation != null) { completedAnimation.SetStartValues(); }
        if (startedAnimation != null) { startedAnimation.SetStartValues(); }
        if (discoveredAnimation != null) { discoveredAnimation.SetStartValues(); }


        print(gameObject.name + " " + discovered + " " + started + " " + completed);

        // Setting state from animations!
        if (!discovered)
        {
            //            print("HELLO I AM NOT DISCOVERED");
            if (discoveredAnimation != null) { discoveredAnimation.SetStartValues(); }
        }

        if (discovered && !started)
        {
            print("discovered not started");
            if (discoveredAnimation != null) { discoveredAnimation.SetEndValues(); }
            if (startedAnimation != null) { startedAnimation.SetStartValues(); }
        }

        if (discovered && started && !completed)
        {
            print("discovered started not completed");
            if (discoveredAnimation != null) { discoveredAnimation.SetEndValues(); }
            if (startedAnimation != null) { startedAnimation.SetEndValues(); }
            if (completedAnimation != null) { completedAnimation.SetStartValues(); }
        }

        if (discovered && started && completed)
        {
            //print("Setting All End Values");
            if (discoveredAnimation != null) { discoveredAnimation.SetEndValues(); }
            if (startedAnimation != null) { startedAnimation.SetEndValues(); }
            if (completedAnimation != null) { completedAnimation.SetEndValues(); }
        }



        if (!completed)
        {

            print("Turning off portal");
            portal.SetPortalOff();
        }
        else
        {
            print("Turning on portal");
            portal.SetPortalFull();
        }

        // print("HELLO I AM ENABLED");
        //print(gameObject.name);
        //print(discovered);
        //print(started);
        //print(completed);


    }

    public void OnEnterQuest()
    {

        //print("HELLO I AM ENTERED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(true);
        }

        if (!discovered)
        {
            DiscoverQuest();
        }


    }

    public void OnExitQuest()
    {
        //print("HELLO I AM EXITED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(false);
        }

    }

    public void OnCompletedAnimationFinished()
    {
        completed = true;
        God.state.OnQuestCompleted(id);
    }

    public void Reset()
    {
        discovered = false;
        started = false;
        completed = false;
        God.state.ResetQuest(id);
    }

    public void SetState()
    {
        God.state.SetQuestState(id, discovered, started, completed);
    }

}
