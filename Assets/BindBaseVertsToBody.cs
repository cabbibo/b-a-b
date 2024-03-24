using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindBaseVertsToBody : Cycle
{

    public IMMATERIA.Body body;
    public MeshVerts baseVerts;

    public override void WhileLiving(float v)
    {

        body.mpb.SetBuffer("_BaseVertBuffer", baseVerts._buffer);
        body.mpb.SetInt("_NumVertsPerMesh", baseVerts.count);


    }
}
