using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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

    public void OnSetCarryable( bool isCarryable , Carryable carryable )
    {

        if ( isCarryable ) {

            canCarryRepresentation.gameObject.SetActive( false );
            cantCarryRepresentation.gameObject.SetActive( true );
            isCarryingRepresentation.gameObject.SetActive( false );
        } else {
            canCarryRepresentation.gameObject.SetActive( false );
            cantCarryRepresentation.gameObject.SetActive( false );
            isCarryingRepresentation.gameObject.SetActive( false );

            if ( setParticleValues != null ) {
                setParticleValues.SetParticlesOff();
            }
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

        if ( carryable.isCarryable ) {
            canCarry = true;

            if ( canCarryRepresentation != null ) {
                canCarryRepresentation.SetActive( true );
            }

            if ( cantCarryRepresentation != null ) {
                cantCarryRepresentation.SetActive( false );
            }
        } else {
            if ( canCarryRepresentation != null ) {
                canCarryRepresentation.SetActive( false );
            }

            if ( cantCarryRepresentation != null ) {
                cantCarryRepresentation.SetActive( false );
            }
        }
    }

    public void OnCantCarry()
    {

        if ( carryable.isCarryable ) {
            canCarry = false;

            if ( canCarryRepresentation != null ) {
                canCarryRepresentation.SetActive( false );
            }

            if ( cantCarryRepresentation != null ) {
                cantCarryRepresentation.SetActive( true );
            }
        } else {
            if ( canCarryRepresentation != null ) {
                canCarryRepresentation.SetActive( false );
            }

            if ( cantCarryRepresentation != null ) {
                cantCarryRepresentation.SetActive( false );
            }
        }

    }

    public void Update()
    {
        CanCarryLineUpdate();
    }

    public int carryLinePositionCount = 20;

    public void CanCarryLineUpdate()
    {
        if ( canCarryLine != null ) {

            if ( carryable.isCarryable ) {
                if ( canCarry ) {

                    canCarryLine.enabled = true;
                    canCarryLine.positionCount = 20;

                    for ( int i = 0; i < carryLinePositionCount; i++ ) {
                        var end = carryable.transform.position;
                        var start = carryable.canCarrier.transform.position;
                        var right = Vector3.Cross( (start - end).normalized , Vector3.up ).normalized;
                        float t = (float)i / (float)(carryLinePositionCount - 1);
                        var pos = Vector3.Lerp( start , end , t );

                        float c = 1 - Mathf.Abs( t - .5f ) * 2;
                        pos += Mathf.Sin( t * 20 + Time.time * 5 ) * right * c * 1;
                        canCarryLine.SetPosition( i , pos );
                    }


                } else {

                    canCarryLine.enabled = false;
                    canCarryLine.positionCount = 2;

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
            } else {

                canCarryLine.enabled = false;
                canCarryLine.SetPosition( 0 , carryable.transform.position );
                canCarryLine.SetPosition( 0 , carryable.transform.position );
            }

        }
    }

    /* public void CarryLineUpdate()
     {
         if ( isCarryingLine != null ) {
             if ( isCarrying ) {
                 isCarryingLine.enabled = true;

                 isCarryingLine.SetPosition( 0 , carryable.transform.position );
                 isCarryingLine.SetPosition( 1 , carryable.carryable.transform.position );

             } else {
                 isCarryingLine.enabled = false;

                 isCarryingLine.SetPosition( 0 , carryable.transform.position );
                 isCarryingLine.SetPosition( 0 , carryable.transform.position );


             }
         }
     }*/
}