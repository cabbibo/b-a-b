using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AudioLoopChunk))]
public class AudioLoopChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AudioLoopChunk myScript = (AudioLoopChunk)target;
        if (GUILayout.Button("Start Play"))
        {
            myScript.StartPlay();
        }

        if (GUILayout.Button("Move To Next Section"))
        {
            myScript.moveToNextSection = true;
        }

        if (GUILayout.Button("End Section"))
        {
            myScript.endSection = true;
        }
    }
}

#endif

public class AudioLoopChunk : MonoBehaviour
{

    public AudioClip[] sections;
    public AudioClip[] tails;

    public float[] sectionLengths;

    public int currentSection;

    public bool moveToNextSection = false;
    public bool endSection = false;

    public float nextEndTime;


    public float playStartTime;
    public AudioSource source;

    public void Start()
    {
        source = GetComponent<AudioSource>();
    }


    public void StartPlay()
    {
        StartPlay(0);
    }


    public void StartPlay(int section)
    {
        currentSection = section;
        source.clip = sections[currentSection];
        source.Play();
        source.loop = true;
        nextEndTime = Time.time + sections[currentSection].length;
        playStartTime = Time.time;
    }


    public void FadeIn()
    {

    }



    public void OnLoop()
    {


        if (moveToNextSection == false)
        {
            // do nothing
            SetLoopTime();
        }

        if (moveToNextSection == true)
        {
            moveToNextSection = false;
            currentSection++;
            if (currentSection >= sections.Length)
            {
                currentSection = 0;
            }
            source.clip = sections[currentSection];
            source.Play();
            source.loop = true;
            playStartTime = Time.time;
            nextEndTime = Time.time + sections[currentSection].length;
        }

        if (endSection == true)
        {
            endSection = false;
            source.clip = tails[currentSection];
            source.Play();
            source.loop = false;
            nextEndTime = Time.time + tails[currentSection].length;
        }

    }

    void SetLoopTime()
    {
        playStartTime = Time.time;
        nextEndTime = Time.time + sections[currentSection].length;
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time > nextEndTime - Time.deltaTime)
        {
            OnLoop();
        }


    }
}
