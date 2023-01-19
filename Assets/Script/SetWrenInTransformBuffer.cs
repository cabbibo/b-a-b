using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

using WrenUtils;
public class SetWrenInTransformBuffer : Cycle
{
    public TransformBuffer buffer;

    public override void WhileLiving(float v){

        if(  WrenUtils.God.wren != null ){
            buffer.transforms[0] =  WrenUtils.God.wren.transform;
        }
    }
}
