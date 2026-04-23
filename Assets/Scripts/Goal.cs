using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Goal : MonoBehaviour
{
    public TextMeshPro text;
    public int score;
    public SoccerField field;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "" + score;

        
    }

    public void OnScore(){
        score ++;
        field.OnScore();
    }

    public void OnReset(){
        score = 0;
    }
}
