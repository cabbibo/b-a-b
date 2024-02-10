using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BadBoyTrailSegment : MonoBehaviour
{

    public BadBoy badBoy;
    public int trailID;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider c)
    {
        if (God.IsOurWren(c))
        {
            badBoy.OnWrenAte(trailID);
        }
    }


}
