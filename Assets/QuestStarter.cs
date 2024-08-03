using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class QuestStarter : MonoBehaviour
{

    public Quest quest;

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

}
