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


    void OnEnable()
    {
        //Delete all children
        foreach (Transform child in ringParent)
        {
            cycle.JumpDeath(child.GetComponent<Cycle>());
            DestroyImmediate(child.gameObject);

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
            cycle.JumpStart(gate.GetComponent<Cycle>());

        }




    }
}
