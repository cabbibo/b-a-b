using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CaveHit : MonoBehaviour
{

    public SearchController searchController;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision c)
    {
        if (God.IsOurWren(c.collider))
        {
            searchController.OnCaveHit();
        }
    }


}
