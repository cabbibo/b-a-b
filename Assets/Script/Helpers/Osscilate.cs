using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class Osscilate : MonoBehaviour
{

    public float size;

    public float speed;
    // Start is called before the first frame update    

    Vector3 originalPosition;
    void OnEnable()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.Sin(speed * Time.time ) * 20 * size ;
        float y = Mathf.Sin(speed * Time.time * 1.3f ) * 20 * size ;
        float z = Mathf.Sin(speed * Time.time * .7f ) * 20 * size ;
        
        transform.position = originalPosition + new Vector3( x , y , z);
    }
}
