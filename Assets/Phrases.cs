using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringArrayWrapper
{
    public string[] data;
}

public class Phrases : MonoBehaviour
{
    public StringArrayWrapper[] phrases;
    public StringArrayWrapper[] alreadyCompletedPhrases;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }
}