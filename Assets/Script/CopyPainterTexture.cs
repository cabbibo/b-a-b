using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CopyPainterTexture : MonoBehaviour
{


    public IslandData islandData;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Renderer>().sharedMaterial.mainTexture = islandData.biomeMap2;
    }


}
