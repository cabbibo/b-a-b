using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[ExecuteAlways]
public class PulseUIColor : MonoBehaviour
{
    public Image[]    images;
    public TMP_Text[] texts;

    public Color color1;
    public Color color2;

    public float speed;


    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        var c = Color.Lerp( color1 , color2 , (Mathf.Sin( speed * Time.time ) + 1) / 2 );
//        print( c );

        for ( int i = 0; i < images.Length; i++ ) {
            images[i].color = c;
        }

        for ( int i = 0; i < texts.Length; i++ ) {
            texts[i].color = c;
        }

    }
}