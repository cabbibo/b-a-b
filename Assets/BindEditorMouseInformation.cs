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
    public float paintDirectionImportance;


    public Vector3 paintPosition;
    public Vector3 oPaintPosition;
    public Vector3 paintNormal;
    public Vector3 paintDirection;

    public Transform paintRep;
    public bool painting;


    public bool paintOnDown = false;
    public bool paintOnMove = true;


    public int particleID;

    public float paintTime;
    public float minPaintTime;

    public float spread;

    public bool alwaysUp;


    public float DPI;

    public override void Bind()
    {

        toBind.BindVector3("_PlacePosition", () => paintPosition);
        toBind.BindVector3("_PlaceDirection", () => paintDirection);
        toBind.BindVector3("_PlaceNormal", () => paintNormal);
        toBind.BindInt("_IsPainting", () => painting ? 1 : 0);
        toBind.BindInt("_ParticleID", () => particleID);

    }



    public override void WhileLiving(float v)
    {


        if (mouseDown == false)
        {
            painting = false;
        }


        paintRep.position = paintPosition; ;


    }


    public void MouseDown(Ray ray)
    {

        RaycastHit hit;

        ray.direction = (ray.direction + Random.insideUnitSphere * spread).normalized;

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // painting = true;
            oPaintPosition = hit.point;
            paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);

            /* if( Time.time - paintTime > minPaintTime ){

             }*/

            // particleID ++; 
            // particleID %= particlesToPlace.count;

            if (paintOnDown)
            {
                DoPaintPlace(ray);
            }

        }
        else
        {
            painting = false;
            paintPosition = ray.origin;
        }



    }


    public void OnMouseUp()
    {

    }

    public void WhileDown(Ray ray)
    {

        if (paintOnMove)
        {
            DoPaintPlace(ray);
        }


    }
    public void Save()
    {
        Saveable.Save(particlesToPlace);
    }


    public void DoPaintPlace(Ray ray)
    {
        RaycastHit hit;


        ray.direction = (ray.direction + Random.insideUnitSphere * spread).normalized;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            painting = true;
            oPaintPosition = paintPosition;
            paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);
            paintNormal = hit.normal;

            if (alwaysUp)
            {
                paintNormal = Vector3.up;
            }

            paintDirection = paintPosition - oPaintPosition;


            particleID++;
            particleID %= particlesToPlace.count;

        }
        else
        {
            painting = false;
            paintPosition = ray.origin;
        }

    }


}
