using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class ButterflyWingFromIndex : IndexForm
{
    public override void SetCount()
    {

        //print( toIndex );
        if (toIndex == null) { toIndex = GetComponent<Form>(); }
        count = (toIndex.count / 4) * 3 * 2;
    }

    public override void Embody()
    {

        int[] values = new int[count];
        int index = 0;

        int count2 = toIndex.count / 4;
        for (int i = 0; i < count2; i++)
        {
            values[index++] = i * 4 + 1;
            values[index++] = i * 4 + 2;
            values[index++] = i * 4 + 0;

            values[index++] = i * 4 + 2;
            values[index++] = i * 4 + 1;
            values[index++] = i * 4 + 3;

        }

        SetData(values);

    }

}