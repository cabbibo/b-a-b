using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WrenUtils;


[ExecuteAlways]
public class SuperflightScoreManager : MonoBehaviour
{


    public TMP_Text scoreText;
    public int score;


    public int ringFlyThroughScore;
    public int closestToSurfaceScoreAdd;

    public int tooFarAwayScoreSubtract;






    // Start is called before the first frame update
    void Start()
    {

        if (God.wren)
        {
            God.wren.PhaseShift(transform.position);
        }
        score = 0;

    }

    // Update is called once per frame
    void Update()
    {

        scoreText.text = "Score : " + score.ToString();

        if (God.wren)
        {

            if (God.wren.physics.rawDistToGround < God.wren.physics.furthestHeight)
            {


                float scoreAddValue = 1 - (God.wren.physics.rawDistToGround / God.wren.physics.furthestHeight);

                scoreAddValue *= (float)closestToSurfaceScoreAdd;

                score += (int)scoreAddValue;

            }
            else
            {

                score -= tooFarAwayScoreSubtract;
            }
        }

    }

    void OnHitGround()
    {

        God.wren.PhaseShift(transform.position);
    }
}
