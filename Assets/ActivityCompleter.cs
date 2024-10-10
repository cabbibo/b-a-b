using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityCompleter : MonoBehaviour
{

    public Activity activity;
    public float amountToComplete = 1;


    public void AmountCompleteAdd(float v)
    {
        amountToComplete += v;

    }
}
