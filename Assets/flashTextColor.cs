using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteAlways]
public class flashTextColor : MonoBehaviour
{


    public float speed;
    public TextMeshPro text;
    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float v = (Mathf.Sin(Time.time * 10 * speed));
        text.color = color * v * 11;//new Color(v,v,v,v);
    }
}
