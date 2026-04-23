using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class SetParticleGradientFromWrenHues : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {



    if( God.wren ){
        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
       GradientColorKey[] colorKey = new GradientColorKey[8];
        colorKey[0].color =  Color.HSVToRGB( God.wren.state.hue1 , 1,1);
        colorKey[0].time = 0.0f;
        colorKey[1].color =  Color.HSVToRGB( God.wren.state.hue1 , 1,1);
        colorKey[1].time = 0.24f;
        colorKey[2].color =  Color.HSVToRGB( God.wren.state.hue2 , 1,1);
        colorKey[2].time = .25f;
        colorKey[3].color =  Color.HSVToRGB( God.wren.state.hue2 , 1,1);
        colorKey[3].time = .49f;
        colorKey[4].color =  Color.HSVToRGB( God.wren.state.hue3 , 1,1);
        colorKey[4].time = .5f;
        colorKey[5].color =  Color.HSVToRGB( God.wren.state.hue3 , 1,1);
        colorKey[5].time = .74f;
        colorKey[6].color =  Color.HSVToRGB( God.wren.state.hue4 , 1,1);
        colorKey[6].time = .75f;
        colorKey[7].color =  Color.HSVToRGB( God.wren.state.hue4 , 1,1);
        colorKey[7].time = 1;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        var randomColors = new ParticleSystem.MinMaxGradient(gradient);
        randomColors.mode = ParticleSystemGradientMode.RandomColor;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        var col = ps.colorOverLifetime;
       // col.enabled = true;
        col.color = Color.white;//gradient;

          var main = ps.main;
        main.startColor = randomColors;/// new Color(hSliderValueR, hSliderValueG, hSliderValueB, hSliderValueA);
    
    }     
    }

   
}
