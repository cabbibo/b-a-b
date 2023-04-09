using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class SetBodyColor : Cycle
{

    public IMMATERIA.Body body;
    public string name;
    public Color color;

    public override void WhileLiving(float v)
    {

        body.mpb.SetColor(name, color);

    }

}
