using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteAlways]
public class UpdateInEditMode : MonoBehaviour
{

    public bool pause;
    // Start is called before the first frame update

    public void OnEnable()
    {

        EditorApplication.update += Always;
    }

    public void OnDisable()
    {
        EditorApplication.update -= Always;
    }

    public void Always()
    {
        if (pause) { return; }
        EditorApplication.QueuePlayerLoopUpdate();
    }


}
