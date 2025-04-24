using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using Unity.VisualScripting;
using UnityEngine.UI;
using WrenUtils;

public class WrenCarrying : MonoBehaviour
{
    public Wren            wren;
    public float           upDistCarrying   = .1f;
    public float           backDistCarrying = .4f;
    public List<Carryable> CarriedItems     = new();

    public List<int> FeetCarriedItems = new();

    public LineRenderer lineRendererL;
    public LineRenderer lineRendererR;


    public List<GameObject> carryableObjects = new();


    public LineRenderer canCarryLineRenderer;


    public void OnEnable()
    {
        carryableObjects.Clear();
    }

    // TODO: don't use God, use info from Wren
    public int GetNormalClientId()
    {
        return God.wrenMaker.GetNormalClientId();
    }

    public bool PickUpItem( GameObject g , int footID )
    {
        Carryable carryable;

        if ( g.TryGetComponent( out carryable ) ) {
            return PickUpItem( carryable , footID );
        } else {
            Debug.LogWarning(
                $"Trying to pick up Object {g.name}, but it doesn't have a Carryable component attached." );
        }

        return false;
    }

    public bool PickUpItem( Carryable c , int footID )
    {
        var targetPosition = transform.position - transform.up * upDistCarrying - transform.forward * backDistCarrying;

        if ( c.TryToCarry( this , targetPosition ) ) {

            c.footID = footID;
            c.carryTransform = footID == 0 ? wren.bird.leftFoot : wren.bird.rightFoot;
            God.audio.Play( God.sounds.collectablePickedUpSounds );
            CarriedItems.Add( c );
            FeetCarriedItems.Add( footID );

            print( CarriedItems.Count );


        } else {

            print( "no picky" );

        }


        //var rb = g.GetComponent<Rigidbody>();
        //rb.position = transform.position - transform.up * 1- transform.forward * 3;
        //g.transform.localScale = new Vector3(4,4,4);
        //rb.drag = 3f;

        return true;
    }


    public void DropAllCarriedItems( Carryable.DropSettings dropSettings = null )
    {
        for ( int i = 0; i < CarriedItems.Count; i++ ) {
            if ( DropCarriedItemAtIndex( i , dropSettings ) ) {
                i--;
            }
        }
    }


    public void DropLeftFootItems()
    {
        for ( int i = 0; i < CarriedItems.Count; i++ ) {
            if ( FeetCarriedItems[i] == 0 ) {
                if ( DropCarriedItemAtIndex( i ) ) {
                    i--;
                }
            }
        }
    }

    public void DropRightFootItems()
    {
        for ( int i = 0; i < CarriedItems.Count; i++ ) {
            if ( FeetCarriedItems[i] == 1 ) {
                if ( DropCarriedItemAtIndex( i ) ) {
                    i--;
                }
            }
        }
    }

    public bool DropFirstCarriedItem( Carryable.DropSettings dropSettings = null )
    {
        return DropCarriedItemAtIndex( 0 , dropSettings );
    }

    public bool DropLastCarriedItem( Carryable.DropSettings dropSettings = null )
    {
        return DropCarriedItemAtIndex( CarriedItems.Count - 1 , dropSettings );
    }

    public bool DropCarriedItemAtIndex( int index , Carryable.DropSettings dropSettings = null )
    {
        if ( CarriedItems.IsIndexValid( index ) && CarriedItems[index].TryToDrop( this , dropSettings ) ) {
            God.audio.Play( God.sounds.collectableDroppedSounds );
            CarriedItems.RemoveAt( index );
            FeetCarriedItems.RemoveAt( index );
            return true;
        }

        return false;
    }


    public void UpdateCarriedItems()
    {
        var targetPosition = transform.position;
        int index = 0;

        foreach (var c in CarriedItems) {

            int id = FeetCarriedItems[index];

            if ( id == 0 ) {
                targetPosition = wren.bird.leftFoot.position;
            } else {
                targetPosition = wren.bird.rightFoot.position;
            }


            // targetPosition -= transform.up * c.carryUpDistance - transform.forward * c.carryBackDistance;
            c.UpdateCarriedPosition( this , targetPosition );


        }

    }

    public void LateUpdate()
    {
        UpdateLineRenderers();
    }

    public int carryingLineResolution = 30;


    // Draws out carrying line!
    public void UpdateLineRenderers()
    {


        /*canCarryLineRenderer.positionCount = carryableObjects.Count + 1;

        if ( carryableObjects.Count == 0 ) {
            canCarryLineRenderer.enabled = false;
        } else {
            canCarryLineRenderer.enabled = true;
            canCarryLineRenderer.SetPosition( 0 , wren.soul.transform.position );
        }

        for ( int i = 0; i < carryableObjects.Count; i++ ) {

            var g = carryableObjects[i];

            canCarryLineRenderer.SetPosition( i + 1 , g.transform.position );


        }
*/


        var leftFootPositions = new List<Vector3>();
        var rightFootPositions = new List<Vector3>();

        for ( int i = 0; i < CarriedItems.Count; i++ ) {
            if ( FeetCarriedItems[i] == 0 ) {
                leftFootPositions.Add( CarriedItems[i].transform.position );
            } else {
                rightFootPositions.Add( CarriedItems[i].transform.position );
            }
        }


        lineRendererL.positionCount = leftFootPositions.Count + 1;
        lineRendererR.positionCount = rightFootPositions.Count + 1;

        lineRendererL.SetPosition( 0 , wren.bird.leftFoot.position );
        lineRendererR.SetPosition( 0 , wren.bird.rightFoot.position );


        Vector3 t1;
        Vector3 t2;


        if ( leftFootPositions.Count > 0 ) {

            lineRendererL.positionCount = carryingLineResolution + 1;
            t1 = -(wren.bird.leftFoot.position - leftFootPositions[0]); // dir


            for ( int i = 0; i < carryingLineResolution; i++ ) {
                float t = ((float)i + 1) / (carryingLineResolution + 1);
                t2 = wren.bird.leftFoot.position + t * t1;

                float lineOut = HELP.SmoothMin( t * 3 , 1 - t ) + .3f; //.5f - Mathf.Abs( t - .5f );

                //lineOut = 0;
                t2 += -wren.transform.right * lineOut * 1;
                lineRendererL.SetPosition( i + 1 , t2 );
            }
        }

        if ( rightFootPositions.Count > 0 ) {

            lineRendererL.positionCount = carryingLineResolution + 1;
            t1 = -(wren.bird.rightFoot.position - rightFootPositions[0]); // dir


            for ( int i = 0; i < carryingLineResolution; i++ ) {
                float t = ((float)i + 1) / (carryingLineResolution + 1);
                t2 = wren.bird.rightFoot.position + t * t1;

                float lineOut = HELP.SmoothMin( t * 3 , 1 - t ) + .3f; //.5f - Mathf.Abs( t - .5f );

                //lineOut = 0;
                t2 += wren.transform.right * lineOut * 1;
                lineRendererL.SetPosition( i + 1 , t2 );
            }
        }


    }


    public void CheckPickup( Wren wren )
    {
        if ( !wren.state.onGround && wren.state.inInterface == false ) {


            for ( int i = 0; i < carryableObjects.Count; i++ ) {

                var g = carryableObjects[i];


                if ( wren.input.left1 > .4f && wren.input.right1 > .4f ) {

                    // check which its closer to!
                    float leftDist = Vector3.Distance( g.transform.position , wren.bird.leftFoot.position );
                    float rightDist = Vector3.Distance( g.transform.position , wren.bird.rightFoot.position );

                    if ( leftDist < rightDist ) {
                        PickUpItem( g , 0 );
                    } else {
                        PickUpItem( g , 1 );
                    }

                    // otherwise figure out which one is more pressed down
                } else {

                    if ( wren.input.left1 > wren.input.right1 ) {
                        PickUpItem( g , 0 );
                    } else if ( wren.input.right1 > wren.input.left1 ) {
                        PickUpItem( g , 1 );
                    }


                }
            }
        }


    }

    public void OnEnter( GameObject go )
    {
        God.audio.Play( God.sounds.collectableCanCarrySounds );

        if ( !carryableObjects.Contains( go ) ) {
            carryableObjects.Add( go );
            go.GetComponent<Carryable>().CanPickup( this );
        }
    }

    public void OnExit( GameObject go )
    {
        God.audio.Play( God.sounds.collectableCantCarrySounds );

        bool didDrop = false;

        while (carryableObjects.Contains( go )) {
            didDrop = true;
            carryableObjects.Remove( go );
        }

        if ( didDrop ) {
            go.GetComponent<Carryable>().CantPickup( this );
        }


    }

    public int CheckIfCarryingItem( Carryable carryable )
    {
        int id = -1;

        int index = 0;

        foreach (var c in CarriedItems) {
            if ( c == carryable ) {
                id = index;
            }

            index++;
        }

        return id;

    }

    public void DropIfCarrying( Carryable c )
    {
        int id = CheckIfCarryingItem( c );

        print( id );

        if ( id >= 0 ) {
            DropCarriedItemAtIndex( id );
        }
    }

    public void GroundHit( Carryable.DropSettings dropSettings = null )
    {

        /*int index = 0;
        foreach (var c in CarriedItems)
        {
            if (c.dropOnGroundHit)
            {
                DropCarriedItemAtIndex(index, dropSettings);
            }
            index++;
        }*/


        for ( int i = 0; i < CarriedItems.Count; i++ ) {
            if ( CarriedItems[i].dropOnGroundHit ) {
                if ( DropCarriedItemAtIndex( i , dropSettings ) ) {
                    i--;
                }
            }
        }
    }
}