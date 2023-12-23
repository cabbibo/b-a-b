using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class SetCountFromOtherParticles : Cycle
{

    public Form other;
    public Form form;
    public int multiplier;

    public override void Create()
    {
        base.Create();
        form.count = other.count * multiplier;
    }

}
