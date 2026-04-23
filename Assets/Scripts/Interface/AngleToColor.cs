using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleToColor : MonoBehaviour
{

    public float value;
    Material material;
    // Start is called before the first frame update
    void Start()
    {
      material = GetComponent<MeshRenderer>().material;   
    }

    // Update is called once per frame
    void Update()
    {

        value = ((transform.localRotation.eulerAngles.y )) / 360;
    
        Color color = Color.HSVToRGB( value , 1, 1);
        material.SetColor( "_Color", color);
      
    }


    public void setValue( float v ){
//      print( "setting value : " + v );
      value = v;
      transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, v * 360,transform.localRotation.eulerAngles.z);
    }
}
