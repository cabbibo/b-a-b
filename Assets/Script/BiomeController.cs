using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomeController : MonoBehaviour
{



    public Color[] biomeColors;
    public Texture2D biomeMap;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {
            Vector3 p = God.wren.transform.position;
            float x = (p.x + 2048) / 4096;
            float y = (p.z + 2048) / 4096;

            Color c = biomeMap.GetPixelBilinear(x, y, 0);

            //            print( c.a);


            float h, s, v;

            Color.RGBToHSV(c, out h, out s, out v);




        }

    }

}
