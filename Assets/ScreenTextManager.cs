using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScreenTextManager : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float opacity;

    public void SetText( string t )
    {
        text.text = t;
    }


    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }
}