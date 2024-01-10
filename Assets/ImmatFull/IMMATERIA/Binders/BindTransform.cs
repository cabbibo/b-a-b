using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IMMATERIA
{
  public class BindTransform : Binder
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
      //      print(transform.localToWorldMatrix[0]);
      transformMatrix = transformToBind.localToWorldMatrix;
    }
  }
}