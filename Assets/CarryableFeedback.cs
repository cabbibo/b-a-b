using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CarryableFeedback : MonoBehaviour
{
    public bool      isCarrying = false;
    public bool      canCarry   = false;
    public Carryable carryable;


    public SetParticleValues setParticleValues;


    public GameObject canCarryRepresentation;
    public GameObject cantCarryRepresentation;

    public GameObject isCarryingRepresentation;

    public LineRenderer canCarryLine;
    public LineRenderer isCarryingLine;

    public Color canCarryColor;
    public Color isCarryingColor;
    public Color cantCarryColor;


    public void OnEnable()
    {
        canCarryRepresentation.gameObject.SetActive( false );
        cantCarryRepresentation.gameObject.SetActive( true );
        isCarryingRepresentation.gameObject.SetActive( false );

        if ( setParticleValues != null ) {
            setParticleValues.SetParticlesPassive();
        }

        if ( canCarryLine != null ) {
            canCarryLine.enabled = false;
        }

        if ( isCarryingLine != null ) {
            isCarryingLine.enabled = false;
        }


    }

    public void OnCarry()
    {

        isCarrying = true;

        if ( setParticleValues != null ) {
            setParticleValues.SetParticlesActive();
        }

        if ( isCarryingRepresentation != null ) {
            isCarryingRepresentation.SetActive( true );
        }

    }

    public void OnDrop()
    {
        isCarrying = false;

        if ( setParticleValues != null ) {
            setParticleValues.SetParticlesPassive();
        }

        if ( isCarryingRepresentation != null ) {
            isCarryingRepresentation.SetActive( false );
        }

    }

    public void OnCanCarry()
    {

        canCarry = true;

        if ( canCarryRepresentation != null ) {
            canCarryRepresentation.SetActive( true );
        }

        if ( cantCarryRepresentation != null ) {
            cantCarryRepresentation.SetActive( false );
        }
    }

    public void OnCantCarry()
    {

        canCarry = false;

        if ( canCarryRepresentation != null ) {
            canCarryRepresentation.SetActive( false );
        }

        if ( cantCarryRepresentation != null ) {
            cantCarryRepresentation.SetActive( true );
        }

    }

    public void Update()
    {
        CanCarryLineUpdate();
    }

    public void CanCarryLineUpdate()
    {
        if ( canCarryLine != null ) {

            if ( canCarry ) {

                canCarryLine.enabled = true;

                canCarryLine.SetPosition( 0 , carryable.transform.position );
                canCarryLine.SetPosition( 1 , carryable.canCarrier.transform.position );


            } else {

                canCarryLine.enabled = false;

                canCarryLine.SetPosition( 0 , carryable.transform.position );
                canCarryLine.SetPosition( 0 , carryable.transform.position );

            }

            if ( isCarrying ) {
                canCarryLine.startColor = isCarryingColor;
                canCarryLine.endColor = isCarryingColor;
            } else {
                canCarryLine.startColor = canCarryColor;
                canCarryLine.endColor = canCarryColor;

            }

        }
    }

    /* public void CarryLineUpdate()
     {
         if ( isCarryingLine != null ) {
             if ( isCarrying ) {
                 isCarryingLine.enabled = true;

                 isCarryingLine.SetPosition( 0 , carryable.transform.position );
                 isCarryingLine.SetPosition( 1 , carryable.carrier.transform.position );

             } else {
                 isCarryingLine.enabled = false;

                 isCarryingLine.SetPosition( 0 , carryable.transform.position );
                 isCarryingLine.SetPosition( 0 , carryable.transform.position );


             }
         }
     }*/
}