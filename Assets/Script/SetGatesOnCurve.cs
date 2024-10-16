using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;
using IMMATERIA;


[ExecuteAlways]
public class SetGatesOnCurve : MonoBehaviour
{


    public Curve curve;

    public GameObject gatePrefab;

    public int numGates;

    public Transform ringParent;

    public float widthMultiplier;

    public Cycle cycle;

    public bool regenerate;


    void OnEnable()
    {


        if (regenerate)
        {
            //Delete all children

            while (ringParent.childCount > 0)
            {
                //cycle.JumpDeath(ringParent.GetChild(0).GetComponent<Cycle>());
                DestroyImmediate(ringParent.GetChild(0).gameObject);

            }

            //Create gates
            curve = GetComponent<Curve>();
            for (int i = 0; i < numGates; i++)
            {

                float v = (float)i / (float)numGates;

                GameObject gate = Instantiate(gatePrefab);
                gate.transform.parent = ringParent;
                curve.SetTransformFromValueAlongCurve(v, gate.transform, widthMultiplier);
                gate.SetActive(true);
                // cycle.JumpStart(gate.GetComponent<Cycle>());

            }


        }



    }
}
