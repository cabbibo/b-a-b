using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class SetWrenInTransformBuffer : Cycle
{
    public TransformBuffer buffer;

    public override void WhileLiving(float v){

        if( God.wren != null ){
            buffer.transforms[0] = God.wren.transform;
        }
    }
}
