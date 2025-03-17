using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class SearchController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Transform resetPosition;
    public void OnEnterCave()
    {

    }

    public void OnCaveHit()
    {
        print("Cave hit");
        God.wren.PhaseShift(resetPosition.position);
    }

}
