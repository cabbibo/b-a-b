using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class EnterQuest : MonoBehaviour
{
    public Quest quest;

    public void OnTriggerEnter( Collider other )
    {

        Debug.Log( "Enter Quest " + other.gameObject.name );
        Debug.LogError( "DEPRECATED, use ACtivity ARea instead" );

        if ( God.IsOurWren( other ) ) {
            //quest.activity.OnEnterQuest();
        }

    }
}