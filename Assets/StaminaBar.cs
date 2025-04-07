using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Rendering;


[ExecuteAlways]
public class StaminaBar : MonoBehaviour
{
    private float value      = 0;
    private float brightness = 0;

    public int barResolution = 100;

    public float barHeight = 1;
    public float barWidth  = 1;


    public float verticalOffset = 0.5f;
    public float forwardOffset  = 0.5f;


    public  Material              material;
    private MaterialPropertyBlock mpb;

    public Transform[] transforms;


    public Vector3[] positions = new Vector3[4];

    public ComputeBuffer positionBuffer;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void OnDisable()
    {
        positionBuffer?.Release();
        positionBuffer = null;
    }

    // Update is called once per frame
    private void Update()
    {

        positionBuffer ??= new ComputeBuffer( transforms.Length , sizeof(float) * 3 );

        if ( positions.Length != transforms.Length ) {
            positionBuffer?.Release();
            positionBuffer = new ComputeBuffer( transforms.Length , sizeof(float) * 3 );
            positions = new Vector3[transforms.Length];
        }


        for ( int i = 0; i < transforms.Length; i++ ) {

            positions[i] = transforms[i].position;

        }


        mpb ??= new MaterialPropertyBlock();

        positionBuffer.SetData( positions );

        mpb.SetBuffer( "points" , positionBuffer );

        mpb.SetInt( "_Count" , barResolution );
        mpb.SetInt( "_PositionsCount" , transforms.Length );

        mpb.SetFloat( "_BarWidth" , barWidth );
        mpb.SetFloat( "_BarHeight" , barHeight );
        mpb.SetFloat( "_Value" , value );
        mpb.SetFloat( "_Brightness" , brightness );
        mpb.SetFloat( "_VerticalOffset" , verticalOffset );
        mpb.SetFloat( "_ForwardOffset" , forwardOffset );


        brightness *= .9f;

        // do we draw shadows?
        Graphics.DrawProcedural( material , new Bounds( transform.position , new Vector3( 100 , 100 , 0 ) ) ,
            MeshTopology.Triangles , barResolution * 3 * 2 , 1 , null , mpb , ShadowCastingMode.Off , true );

    }

    public void SetValue( float value , float brightness )
    {
        this.value = value;
        this.brightness = brightness;
    }
}