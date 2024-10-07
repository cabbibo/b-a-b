using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenReverser : MonoBehaviour
{

    public Wren wren;

    public List<Vector3> previousPositions;
    public int maxNumPositions = 100;

    public float spaceBetweenPositions = 3;

    public Vector3 lastPosition;


    // Todo:
    // Can move forward, but only until you overwrite the position with enough movement

    public void MoveToPrevious()
    {
        if (previousPositions.Count > 0)
        {
            Vector3 previousPosition = previousPositions[0];
            previousPositions.RemoveAt(0);
            //previousPositions.Clear();
            // previousPositions.Insert(0, previousPosition);

            wren.shards.DoReverse(wren.transform.position - previousPosition);
            wren.PhaseShift(previousPosition);
            wren.physics.vel = Vector3.zero;
            lastPosition = previousPosition;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(lastPosition, wren.transform.position) > spaceBetweenPositions && previousPositions.Count < maxNumPositions)
        {
            previousPositions.Insert(0, wren.transform.position);
            lastPosition = wren.transform.position;
        }

    }
}
