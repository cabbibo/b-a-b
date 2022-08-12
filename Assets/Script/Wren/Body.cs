using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{   

    public bool enabled;

    public FullBird bird;

    
    public Transform feathers;

    public int numberScapularColumns;
    public int numberScapularRows;
    public int numberScapulars;
    public GameObject ScapularObject;   
    public GameObject[] Scapulars;

    public LineRenderer[] lineRenderers;

    public void Create(){
    
    }
    // Start is called before the first frame update
    public void Destroy(){
    }


}
