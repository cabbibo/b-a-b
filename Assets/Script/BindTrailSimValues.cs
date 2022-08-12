using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindTrailSimValues : Binder
{

    public float explodeTime;
    public float ribbonWidth;
    public float someFollow;
    public float tailFlowForce;
    public Transform following;

    public Life[] extraBinds;

    public ExplodeSimulation explodeSimulation;


    public Matrix4x4 transformMatrix;
  public string sdfTextureName;
    public Form3D sdfForm;



    public override void Bind(){

    
      toBind.BindMatrix("_Transform", () => this.transformMatrix );
      toBind.BindTexture(sdfTextureName, () => sdfForm._texture );
      toBind.BindVector3("_Dimensions", ()=>sdfForm.dimensions );
      toBind.BindVector3("_Extents", ()=>sdfForm.extents );
      toBind.BindVector3("_Center",()=> sdfForm.center );
      toBind.BindMatrix("_SDFTransform", () => sdfForm.transform.localToWorldMatrix );
      toBind.BindMatrix("_SDFInverseTransform", () => sdfForm.transform.worldToLocalMatrix );

      toBind.BindFloat( "_ExplodeTime", () => explodeTime );
      toBind.BindFloat("_RibbonWidth", () => ribbonWidth);

      toBind.BindVector3("_FollowPosition", () => this.following.position );
      toBind.BindFloat("_SomeFollow" , () => someFollow );
      toBind.BindFloat("_TailFlowForce" , () => tailFlowForce);

      for( int i = 0; i < extraBinds.Length; i++ ){
        
        extraBinds[i].BindMatrix("_Transform", () => this.transformMatrix );
        extraBinds[i].BindTexture(sdfTextureName, () => sdfForm._texture );
        extraBinds[i].BindFloat( "_ExplodeTime", () => explodeTime );
        extraBinds[i].BindFloat("_RibbonWidth", () => ribbonWidth);

        
        extraBinds[i].BindVector3("_Dimensions", ()=>sdfForm.dimensions );
        extraBinds[i].BindVector3("_Extents", ()=>sdfForm.extents );
        extraBinds[i].BindVector3("_Center",()=> sdfForm.center );
        extraBinds[i].BindMatrix("_SDFTransform", () => sdfForm.transform.localToWorldMatrix );
        extraBinds[i].BindMatrix("_SDFInverseTransform", () => sdfForm.transform.worldToLocalMatrix );

        extraBinds[i].BindVector3("_FollowPosition", () => this.following.position );
        extraBinds[i].BindFloat("_SomeFollow" , () => someFollow );

        
        extraBinds[i].BindFloat("_TailFlowForce" , () => tailFlowForce);

      }


    }



    public override void WhileLiving(float v){


      
      if( God.wren != null ){ following = God.wren.transform; }
      transformMatrix = transform.localToWorldMatrix;
      explodeTime = explodeSimulation.explodeTime;

    }



}
