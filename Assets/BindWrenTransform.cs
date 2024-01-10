using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using WrenUtils;


public class BindWrenTransform : Binder
{

    public Transform transformToBind;
    public Matrix4x4 transformMatrix;
    public string shaderName = "_Transform";
    public override void Bind()
    {
        if (transformToBind == null) { transformToBind = this.transform; }
        toBind.BindMatrix(shaderName, () => this.transformMatrix);
    }


    public override void WhileLiving(float v)
    {

        if (WrenUtils.God.wren != null)
        {
            transformMatrix = WrenUtils.God.wren.transform.localToWorldMatrix;
        }
        else
        {
            transformMatrix = transformToBind.localToWorldMatrix;
        }
    }


}
