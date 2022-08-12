﻿using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System;
using UnityEditor;//.EditorGUI;

[ExecuteAlways]
public class FadeToBlack : MonoBehaviour
{
    public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.6f, 0.7f, -1.8f, -1.2f), new Keyframe(1, 0));

    private Material _material;
    public float startFadeSpeed;

    public bool startFadeOut;

    public void OnEnable()
    {

        _material = GetComponent<Renderer>().sharedMaterial;

        _material.SetColor("_Color", new Color(1, 1, 1, 1));

        fadeColor = new Color(1, 1, 1, 1); ;
        currentOpacity = 1;
        startOpacity = 1;
        fadeOpacity = 1;
       
        if( startFadeOut ){ FadeIn(startFadeSpeed); }

    }



    public bool fading;
    public float fadeStartTime;
    public float fadeSpeed;
    public float fadeOpacity;
    public Color fadeColor;

    public float startOpacity;
    public float currentOpacity;


    Func<int> onCompleteFunction;

    public void FadeOut( float time ){
        FadeOut( Color.black , time );
    }

    public void FadeOut(){
        FadeOut( Color.black , 5);
    }

    public void SetBlack(){
         fadeColor = new Color(0, 0, 0, 1); ;
        currentOpacity = 1;
        startOpacity = 1;
        fadeOpacity = 0;
        _material.SetColor("_Color", new Color(0, 0, 0, 1));
    }
    public void FadeOut(Color color, float time)
    {

        fading = true;
        fadeStartTime = Time.unscaledTime;
        fadeSpeed = time;
        fadeColor = color;
        fadeOpacity = 1;
        startOpacity = currentOpacity;

    }

    public void FadeOut(Color color, float time, Func<int> onComplete)
    {

        fading = true;
        fadeStartTime = Time.unscaledTime;
        fadeSpeed = time;
        fadeColor = color;
        fadeOpacity = 1;
        startOpacity = currentOpacity;
        onCompleteFunction = onComplete;

    }


    

    public void FadeIn(float time)
    {

        fading = true;
        fadeStartTime = Time.unscaledTime;
        fadeSpeed = time;
        fadeOpacity = 0;
        startOpacity = currentOpacity;

    }

    public void Update()
    {

        if (fading == true)
        {

            float v = (Time.unscaledTime - fadeStartTime) / fadeSpeed;
            if (v >= 1)
            {
                fading = false;
                if( onCompleteFunction != null ){
                    onCompleteFunction();
                    onCompleteFunction = null;
                }

            }
            else
            {

                currentOpacity = Mathf.Lerp(fadeOpacity, startOpacity, FadeCurve.Evaluate(v));
                _material.SetColor("_Color", new Color(fadeColor.r, fadeColor.g, fadeColor.b, currentOpacity));
            }

        }

    }
}