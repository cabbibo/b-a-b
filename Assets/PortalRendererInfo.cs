using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PortalRendererInfo : MonoBehaviour
{
 

        public float portalShownAmount;
    public MeshRenderer portalRenderer;
    MaterialPropertyBlock portalMPB;
        public void Update()
    {

        if (portalMPB == null)
        {
            portalMPB = new MaterialPropertyBlock();
        }


        portalRenderer.GetPropertyBlock(portalMPB);
        portalMPB.SetFloat("_OpenAmount", portalShownAmount);
        portalRenderer.SetPropertyBlock(portalMPB);
    }

}
