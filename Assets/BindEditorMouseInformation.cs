using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
public class BindEditorMouseInformation : Binder
{


    public bool mouseDown;
    public Vector3 mousePosition;
    public Vector3 oMousePosition;

    public Vector3 screenDirection;

    public int currentParticle;
    public Form particlesToPlace;
    public float spread;
    public float paintDirectionImportance;


    public Vector3 paintPosition;
    public Vector3 paintNormal;

    public Transform paintRep;
    public bool painting;


    public int particleID;

    public float paintTime;
    public float minPaintTime;

    public override void Bind(){

        toBind.BindVector3( "_PlacePosition" , () => paintPosition );
        toBind.BindVector3( "_PlaceNormal"   , () => paintNormal   );
        toBind.BindInt("_IsPainting" , () => painting ? 1:0  );
        toBind.BindInt("_ParticleID", () => particleID );

    }



    public override void WhileLiving( float v ){


        if( mouseDown == false){
            painting = false;
        }


        paintRep.position = paintPosition;;


    }


    public void MouseDown( Ray ray){

        RaycastHit hit;
        
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast( ray , out hit, Mathf.Infinity))
        {
            painting = true;
            paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);

           /* if( Time.time - paintTime > minPaintTime ){
                
            }*/

            particleID ++; 
            particleID %= particlesToPlace.count;

        }else{
            painting = false;
            paintPosition = ray.origin;
        }

    

    }
    
    public void WhileDown( Ray ray){

            RaycastHit hit;
    // Does the ray intersect any objects excluding the player layer
    if (Physics.Raycast( ray , out hit, Mathf.Infinity))
    { 
        painting = true;
      paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);

      
            particleID ++; 
            particleID %= particlesToPlace.count;

    }else{
        painting = false;
      paintPosition = ray.origin;
    }

    


    }
    public void Save(){
        Saveable.Save(particlesToPlace);
    }

    
}
