using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadBoyManager : MonoBehaviour
{

    public Quest quest;

    public List<BadBoy> badBoys;


    // Make it so all the objects are reset!
    public void OnEnable()
    {
        if (quest.completed == false)
        {
            for (int i = 0; i < badBoys.Count; i++)
            {
                badBoys[i].completed = false;
            }
        }
        else
        {
            for (int i = 0; i < badBoys.Count; i++)
            {
                badBoys[i].completed = true;
                badBoys[i].SetComplete();
            }
        }
    }


    public int totalComplete = 0;
    public void OnBadBoyComplete(BadBoy bb)
    {
        totalComplete = 0;
        for (int i = 0; i < badBoys.Count; i++)
        {
            if (badBoys[i].completed)
            {
                totalComplete++;
            }
        }

        print(totalComplete);

        quest.SetCompletion((float)totalComplete / (float)badBoys.Count);
    }
}
