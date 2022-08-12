using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnOffInRuntime : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if( Application.isPlaying ){
            gameObject.SetActive(false);
        }
    }

}
