// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/PointerInterfaceProcShader1"
{
    Properties
    {

        _Size("_Size", Float) = 1
        _DiscardTexture("Discard Texture", 2D) = "white" {}


        _MinSizeMultiplier("_MinSizeMultiplier", Float) = 1
        _MaxSizeMultiplier("_MaxSizeMultiplier", Float) = 1

        _MinSizeDistance("_MinSizeDistance", Float) = 1
        _MaxSizeDistance("_MaxSizeDistance", Float) = 1
        _TypeSizeMultiplier("_TypeSizeMultiplier", vector) = (2,1,1,1)

        _OffsetFromBirdCenter("_OffsetFromBirdCenter", Float) = 0.1
        _PointerWidth("_PointerWidth", Float) = 0.1

        _ExtraViewMultiplier("_ExtraViewMultiplier", Float) = 1
        _AboveBirdOffset("_AboveBirdOffset", Float) = 0.4
    }


    SubShader
    {

        // Tags {"Queue"="Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent"}
        // Tags {"Queue"="Background" "IgnoreProjector"="True" "RenderType"="Background"}

        // Tags { "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000" }
        //	Blend SrcAlpha One
        //	AlphaTest Greater .01
        //ColorMask RGB
        // Cull Off
        //ZWrite Off 
        // ZTest Always

        Tags
        {
            "Queue"="Overlay+1000" "RenderType"="Transparent+100000"
        }
        Blend One One
        //	AlphaTest Greater .01
        //ColorMask RGB
        Cull Off
        ZWrite Off
        ZTest Always


        Pass
        {
            Tags
            {
                "Queue"="Overlay+1000" "IgnoreProjector"="True" "RenderType"="Transparent+100000"
            }


            CGPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
            #include "Assets/Resources/Shaders/Chunks/noise.cginc"


            uniform int    _Count;
            uniform float  _Size;
            uniform float3 _WrenPos;
            uniform float  _Fade;


            uniform float _MinSizeMultiplier;
            uniform float _MaxSizeMultiplier;

            uniform float  _MinSizeDistance;
            uniform float  _MaxSizeDistance;
            uniform float4 _TypeSizeMultiplier;

            float _ExtraViewMultiplier;

            uniform sampler2D _DiscardTexture;

            float _OffsetFromBirdCenter;

            float _PointerWidth;
            float _AboveBirdOffset;


            StructuredBuffer<float3> _PositionBuffer;
            StructuredBuffer<float>  _FadeBuffer;
            StructuredBuffer<float>  _TypeBuffer;
            StructuredBuffer<float4> _ExtraDataBuffer;

            //uniform float4x4 worldMat;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float2 uv : TEXCOORD4;
                float  id : TEXCOORD5;
                float  value : TEXCOORD6;
                float  fade : TEXCOORD7;
                float  type : TEXCOORD8;
                float4 extra : TEXCOORD9;
                float3 centerPos : TEXCOORD10;
            };


            uniform float4x4 _Transform;
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( uint id : SV_VertexID )
            {

                varyings o;

                int base      = id / 6;
                int alternate = id % 6;

                if ( base < _Count * 6 )
                {


                    float3 center = _PositionBuffer[ base ];

                    float3 pos = _WrenPos;
                    float3 fwd = pos - _WorldSpaceCameraPos;


                    //centerDir = (center - pos);
                    //up = float3(0,1,0);

                    float midPointerValue = .3;

                    // float size= 10;
                    float3 centerDif  = center - pos;
                    float3 centerDir  = normalize( centerDif );
                    float  centerDist = length( centerDif );

                    float3 up = normalize( cross( centerDir , fwd ) );

                    float3 camForward = UNITY_MATRIX_V[ 2 ].xyz;
                    float3 camUp      = UNITY_MATRIX_V[ 1 ].xyz;
                    float3 camLeft    = UNITY_MATRIX_V[ 0 ].xyz;

                    float3 basePos = pos + centerDir * _OffsetFromBirdCenter;
                    /* basePos        = _WorldSpaceCameraPos;
                     basePos -= camForward * 8;
                     basePos -= camUp * 3;
                     basePos -= camLeft * 5;*/



                    int intType = _TypeBuffer[ base ];


                    float sizeMultiplier = _TypeSizeMultiplier[ intType ]; //+ 1;

                    if ( intType == 10 )
                    {
                        sizeMultiplier = 10;
                    }


                    // if we are close to the place we are going, connect completely ( longer )
                    // otherwise make it shorter and fatter ( depending on the type )




                    float dist = length( center - pos );

                    float distMultiplier = lerp( _MinSizeMultiplier , _MaxSizeMultiplier ,
                    saturate(
                        ( dist - _MinSizeDistance ) / ( _MaxSizeDistance -
                            _MinSizeDistance ) ) );


                    float fSize = _Size * sizeMultiplier * distMultiplier;


                    /* diamond
                    float3 p1 = basePos;
                    float3 p2 = basePos+ centerDir * (_Size *midPointerValue ) - up * (_Size * .1);
                    float3 p3 = basePos+ centerDir * (_Size *midPointerValue ) + up * (_Size * .1);
                    float3 p4 = basePos+ centerDir * (_Size );
                    */

                    float3 p1 = -up * fSize * _PointerWidth;
                    float3 p2 = +up * ( fSize * _PointerWidth );
                    float3 p3 = +centerDir * ( fSize ) - up * ( fSize * _PointerWidth );
                    float3 p4 = +centerDir * ( fSize ) + up * ( fSize * _PointerWidth );


                    float4 wrenViewPos = mul( UNITY_MATRIX_V , float4( _WrenPos , 1 ) );
                    float3 dirViewPos  = mul( UNITY_MATRIX_V , float4( centerDir , 0 ) ).xyz;
                    float3 upViewPos   = camUp;
                    float3 upInViewPos = normalize( cross( dirViewPos , camUp ) );
                    upInViewPos        = float3( upInViewPos.x , upInViewPos.y , 0 );
                    upInViewPos        = normalize( upInViewPos );

                    float forwardMatch    = dot( dirViewPos , camForward );
                    float forwardFattener = abs( forwardMatch ) * 3 + 1;


                    float3 flattenedViewDir = normalize( float3( dirViewPos.x , dirViewPos.y , 0 ) );

                    float3 vBasePos = wrenViewPos.xyz + upViewPos * _AboveBirdOffset;

                    float3 outAmount = dirViewPos * _OffsetFromBirdCenter;

                    float3 vUp = upInViewPos * fSize * _PointerWidth * forwardFattener;

                    float3 vP1 = outAmount + wrenViewPos - vUp;
                    float3 vP2 = outAmount + wrenViewPos + vUp;
                    float3 vP3 = outAmount + wrenViewPos + dirViewPos * fSize - vUp;
                    float3 vP4 = outAmount + wrenViewPos + dirViewPos * fSize + vUp;




                    //float3 vP1 = mul()


                    float3 truCenter = basePos + centerDir * ( fSize * midPointerValue );



                    /*float3 p1 = center - up *_Size;
                    float3 p2 =  pos  - up *_Size;
                    float3 p3 = center + up *_Size;
                    float3 p4 = pos + up *_Size;*/


                    float3 extra     = 0;
                    float3 viewExtra = 0;
                    float2 uv        = 0;

                    float value = 0;

                    /*
              
                    Diamond
                    if( alternate == 0 ){ extra = p1; uv = float2(.5,0); value = 0; }
                    if( alternate == 1 ){ extra = p2; uv = float2(1,midPointerValue); value = midPointerValue;}
                    if( alternate == 2 ){ extra = p4; uv = float2(.5,1); value = 1;}
                    if( alternate == 3 ){ extra = p1; uv = float2(.5,0); value = 0;}
                    if( alternate == 4 ){ extra = p4; uv = float2(.5,1); value = 1;}
                    if( alternate == 5 ){ extra = p3; uv = float2(0,midPointerValue); value = midPointerValue;}
              
                    */


                    if ( alternate == 0 )
                    {
                        extra     = p1;
                        viewExtra = vP1;
                        uv        = float2( 0 , 0 );
                        value     = 0;
                    }
                    if ( alternate == 1 )
                    {
                        extra     = p2;
                        viewExtra = vP2;
                        uv        = float2( 1 , 0 );
                        value     = 0;
                    }
                    if ( alternate == 2 )
                    {
                        extra     = p4;
                        viewExtra = vP4;
                        uv        = float2( 1 , 1 );
                        value     = 1;
                    }
                    if ( alternate == 3 )
                    {
                        extra     = p1;
                        viewExtra = vP1;
                        uv        = float2( 0 , 0 );
                        value     = 0;
                    }
                    if ( alternate == 4 )
                    {
                        extra     = p4;
                        viewExtra = vP4;
                        uv        = float2( 1 , 1 );
                        value     = 1;
                    }
                    if ( alternate == 5 )
                    {
                        extra     = p3;
                        viewExtra = vP3;
                        uv        = float2( 0 , 1 );
                        value     = 0;
                    }




                    o.worldPos = basePos + extra;

                    float3 basePosViewPos = mul( UNITY_MATRIX_V , float4( basePos , 1 ) ).xyz;
                    float3 extraViewPos   = mul( UNITY_MATRIX_V , float4( extra , 1 ) ).xyz;

                    // multiply by view depth for constant view size scaling
                    extraViewPos *= -basePosViewPos.z;

                    // divide by perspective projection matrix [1][1] if you don't want camera FOV to displayed size
                    // the * 0.5 is to make a default quad with a scale of 1 be exactly the height of the view
                    //  extraViewPos /= UNITY_MATRIX_P._m11 * 0.5;

                    // along with the perspective projection divide by screen height if you want the scale to be in screen pixels
                    // vertex /= _ScreenParams.y;


                    basePosViewPos += viewExtra * _ExtraViewMultiplier;
                    //basePosViewPos += float3( uv.x , uv.y , 0 ) * 1;

                    //viewExtra *= max( -basePosViewPos.z , 0 );
                    // vBasePos + viewExtra * _ExtraViewMultiplier;
                    float3 fBasePos = vBasePos + viewExtra; // * _ExtraViewMultiplier;

                    float4 final = mul( UNITY_MATRIX_P , float4( fBasePos , wrenViewPos.w ) );


                    //final = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1 ) );




                    // mul(_Transform, float4((v.pos) ,1));
                    ///o.worldPos +=  extra * _Size;

                    o.eye   = _WorldSpaceCameraPos - o.worldPos;
                    o.nor   = normalize( UNITY_MATRIX_V[ 2 ].xyz ); //v.nor;
                    o.uv    = uv;
                    o.id    = base;
                    o.fade  = _FadeBuffer[ base ];
                    o.type  = _TypeBuffer[ base ];
                    o.extra = _ExtraDataBuffer[ base ];


                    o.pos       = final; //mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                    o.centerPos = truCenter;

                }

                return o;

            }


            float sdfDiamond( float2 p , float2 center , float size )
            {
                // Translate point relative to the center
                p -= center;


                if ( p.y < 0 )
                {
                    p.y = 2 * p.y;
                }
                else
                {
                    p.y = p.y;
                }

                // Scale by size
                p /= size;

                // Create the diamond shape using an SDF
                float d = abs( p.x ) + abs( p.y ) - 1.0;

                // Scale back to the original size
                return d * size;
            }


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {

                float center = length( v.worldPos - v.centerPos ) / _Size;



                float diamond = sdfDiamond( v.uv , float2( .5 , .3 ) , .5 );

                float n = noise( float3( v.uv.x , v.uv.y , _Time.y ) * 30 );

                if ( diamond > -.1 * n )
                {
                    discard;
                }


                float3 c1 = hsv( v.extra.x * .1 , 1 , 1 );

                float3 fCol = 0;



                fCol = -diamond * 4; // -1 * noise( float3(v.uv.x,v.uv.y,10) * 30);


                fCol *= hsv( v.type * .1 , 1 , 1 );

                fCol *= v.fade;




                if ( v.extra.x < .5 )
                {
                    // discard the inside; 




                    if ( diamond < -.2 + .1 * n )
                    {
                        discard;
                    }


                    /*
                    if( abs(v.uv.y-.5) < .4 && abs(v.uv.x-.5) < .4){
                      //discard;
                    }*/

                }
                else
                {

                    for ( int i = 0; i < v.extra.x; i++ )
                    {

                        if ( abs( ( 1 - v.uv.y ) - abs( v.uv.x - .5 ) * .1 - ( .2 + .6 * ( 1 - ( (float)i / 10 ) ) ) ) <
                            .02 + .01 * n )
                        {
                            fCol *= 3;
                        }
                    }
                }




                // POINT OF INTEREST
                if ( v.type > 9.5 && v.type < 10.5 )
                {
                    fCol *= 10 * sin( _Time.y * 100 );
                }


                fCol = 1;




                return float4( fCol , 1 );

            }
            ENDCG

        }
    }

    Fallback Off


}