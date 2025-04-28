using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class Crysplosion : MonoBehaviour
{
    public bool exploding = false;
    public bool reseting  = false;

    public float explosionSpeed = 1;
    public float resetSpeed     = 1;


    [Range( 0 , 1f )]
    public float explosionValue;


    public float explositionSize = 10;
    public int   explosionType;

    public MaterialPropertyBlock mpb;

    public MeshRenderer meshRenderer;


    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        if ( exploding ) {


            explosionValue = Mathf.Lerp( explosionValue , 1 , explosionSpeed );

            if ( explosionValue > .95f ) {
                explosionValue = 1;
                exploding = false;
            }
        }

        if ( reseting ) {

            explosionValue = Mathf.Lerp( explosionValue , 0 , resetSpeed );

            if ( explosionValue < 0.01f ) {
                explosionValue = 0;
                reseting = false;
            }

        }


        if ( mpb == null ) {
            mpb = new MaterialPropertyBlock();
        }


        meshRenderer.GetPropertyBlock( mpb );
        mpb.SetFloat( "_ExplosionValue" , explosionValue );
        mpb.SetFloat( "_ExplosionSize" , explositionSize );
        mpb.SetInt( "_ExplosionType" , explosionType );
        meshRenderer.SetPropertyBlock( mpb );


    }

    public void Explode( float speed )
    {

        exploding = true;

    }


    public void Reset( float startExplosionValue )
    {

        reseting = true;
        explosionValue = startExplosionValue;
        // play come together sound!

    }

    public void Reset()
    {
        Reset( 1 );

    }
}