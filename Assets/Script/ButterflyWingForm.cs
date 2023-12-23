using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class ButterflyWingForm : Form
{

    public Form wing;
    public override void SetCount()
    {
        structSize = 16;
        count = wing.count * 4;
    }



}
