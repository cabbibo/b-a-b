using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class InstanceMeshBody : Cycle
{
    public Material material;
    public Form     form;

    public override void WhileLiving( float v )
    {
        /*RenderParams rp = new RenderParams(material);
        MyInstanceData[] instData = new MyInstanceData[numInstances];
        for(int i=0; i<numInstances; ++i)
        {
           instData[i].objectToWorld = Matrix4x4.Translate(new Vector3(-4.5f+i, 0.0f, 5.0f));
           instData[i].renderingLayerMask = (i & 1) == 0 ? 1u : 2u;
        }
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);*/
    }
}