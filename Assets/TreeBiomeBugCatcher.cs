using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TreeBiomeBugCatcher : MonoBehaviour
{


    public ButterflySpawner[] butterflySpawners;
    public Quest quest;


    // Start is called before the first frame update
    void Start()
    {

    }


    public void OnTriggerEnter(Collider c)
    {


        print("LETS GO");

        if (God.IsOurWren(c))
        {
            if (!quest.started)
            {
                quest.StartQuest();
            }
        }
    }


    public void OnBugAte(float v)
    {
        print("BUG ATE");
        print(v);
        quest.AddToCompletion(v);

    }




}
