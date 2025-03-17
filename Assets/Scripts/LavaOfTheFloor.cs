using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class LavaOfTheFloor : MonoBehaviour
{

    public FloorIsLava floorIsLava;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCollisionEnter(Collision collision)
    {
        if (God.IsOurWren(collision.collider))
        {
            floorIsLava.OnIsLavaHit();
        }
    }
}
