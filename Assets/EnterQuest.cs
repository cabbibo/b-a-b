using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class EnterQuest : MonoBehaviour
{

    public Quest quest;

    public void OnTriggerEnter(Collider other)
    {

        print("HELLOOO");

        if (God.IsOurWren(other))
        {
            quest.OnEnterQuest();
        }

    }

}
