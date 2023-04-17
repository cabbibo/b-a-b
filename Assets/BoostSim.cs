using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;



public class BoostSim : Binder
{

    public float lifeBoostFalloff = .9f;
    public float lifeBoostMultiplier = 1;


    public void OnBoost(Booster b)
    {

        print(whichTransform.gameObject.name);
        print("boost");//
        print(b.transform.gameObject.name);
        whichTransform = b.transform;
        lifeBoostVal = b.lifeBoostVal * lifeBoostMultiplier;

        transformMatrix = whichTransform.localToWorldMatrix;
        lifeBoostVal *= lifeBoostFalloff;

    }

    public Vector3 lifeBoostVal;

    public Transform whichTransform;

    public Matrix4x4 transformMatrix;
    public override void Bind()
    {
        if (whichTransform == null)
        {
            whichTransform = transform;
        }

        toBind.BindMatrix("_Transform", () => this.transformMatrix);
        toBind.BindVector3("_BoostVal", () => this.lifeBoostVal);

    }


    public override void WhileLiving(float v)
    {

        transformMatrix = whichTransform.localToWorldMatrix;
        lifeBoostVal *= lifeBoostFalloff;
        WrenUtils.God.instance.SetWrenCompute(0, toBind.shader);
    }

}
