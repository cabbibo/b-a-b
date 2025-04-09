// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Debug/MeshPointerInterface"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _Forwards ("Forwards", float) = 1


        _MinSizeMultiplier("_MinSizeMultiplier", Float) = 1
        _MaxSizeMultiplier("_MaxSizeMultiplier", Float) = 1

        _MinSizeDistance("_MinSizeDistance", Float) = 1
        _MaxSizeDistance("_MaxSizeDistance", Float) = 1
        _TypeSizeMultiplier("_TypeSizeMultiplier", vector) = (2,1,1,1)

        _OffsetFromBirdCenter("_OffsetFromBirdCenter", Float) = 0.1
        _PointerWidth("_PointerWidth", Float) = 0.1

        _ExtraViewMultiplier("_ExtraViewMultiplier", Float) = 1
        _AboveBirdOffset("_AboveBirdOffset", Float) = 0.4

        _ScaleFromWrenToCamera( "_ScaleFromWrenToCamera", Float) = 1


        _WrenToCameraMin("_WrenToCameraMin", Float) = 1
        _WrenToCameraMax("_WrenToCameraMax", Float) = 1
        _WrenToCameraScaleMin("_WrenToCameraScaleMin", Float) = 1
        _WrenToCameraScaleMax("_WrenToCameraScaleMAx", Float) = 1
        _WrenToCameraOffsetMin("_WrenToCameraOffsetMin", Float) = 1
        _WrenToCameraOffsetMax("_WrenToCameraOffsetMax", Float) = 1

        _WrenToCameraUpOffsetMin("_WrenToCameraUpOffsetMin", Float) = 1
        _WrenToCameraUpOffsetMax("_WrenToCameraUpOffsetMax", Float) = 1

        _WrenToCameraForwardOffsetMin("_WrenToCameraForwardOffsetMin", Float) = 1
        _WrenToCameraForwardOffsetMax("_WrenToCameraForwardOffsetMax", Float) = 1


    }


    SubShader
    {

        Tags
        {
            "Queue"="Overlay+1000" "RenderType"="Transparent+100000"
        }
        Blend One One
        //	AlphaTest Greater .01
        //ColorMask RGB
        // Cull Off
        //ZWrite Off
        //ZTest Always

        Pass
        {

            CGPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"


            //#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float2>   _ConnectionBuffer;
            StructuredBuffer<float4x4> _TransformBuffer;
            //#endif


            uniform int    _Count;
            uniform float  _Size;
            uniform float  _Forwards;
            uniform float3 _Color;


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
                float  valAlong : TEXCOORD6;
                float  viewMatch : TEXCOORD7;
                float  fade : TEXCOORD8;
                float  type : TEXCOORD9;
                float4 extra : TEXCOORD10;
            };


            uniform float _MinSizeMultiplier;
            uniform float _MaxSizeMultiplier;

            uniform float  _MinSizeDistance;
            uniform float  _MaxSizeDistance;
            uniform float4 _TypeSizeMultiplier;


            uniform float _WrenToCameraMin;
            uniform float _WrenToCameraMax;
            uniform float _WrenToCameraScaleMin;
            uniform float _WrenToCameraScaleMax;
            uniform float _WrenToCameraOffsetMin;
            uniform float _WrenToCameraOffsetMax;

            uniform float _WrenToCameraUpOffsetMin;
            uniform float _WrenToCameraUpOffsetMax;

            uniform float _WrenToCameraForwardOffsetMin;
            uniform float _WrenToCameraForwardOffsetMax;


            // TYPE INFO
            // x = times complete
            // y = fully completed
            // z = ???
            // w = only show directional


            StructuredBuffer<float3> _PositionBuffer;
            StructuredBuffer<float>  _FadeBuffer;
            StructuredBuffer<float>  _TypeBuffer;
            StructuredBuffer<float4> _ExtraDataBuffer;


            float4x4 createTransformationMatrix( float3 forward , float3 right , float3 up , float3 position ,
                                 float3                 scale )
            {
                // Normalize the direction vectors to ensure they are unit vectors
                forward = normalize( forward );
                right   = normalize( right );
                up      = normalize( up );

                // Return the transformation matrix by filling in the values directly
                return float4x4(
                    scale.x * right.x , scale.x * right.y , scale.x * right.z , 1.0 , // First row: scaled right vector
                    scale.y * up.x , scale.y * up.y , scale.y * up.z , 1.0 , // Second row: scaled up vector
                    scale.z * forward.x , scale.z * forward.y , scale.z * forward.z , 1.0 ,
                    // Third row: scaled negative forward vector
                    position.x , position.y , position.z , 1.0 // Fourth row: position (translation)
                );
            }

            float3 _WrenPos;
            float3 _WrenUp;

            uniform float4x4 _Transform;
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( appdata_full v , uint instanceID : SV_InstanceID )
            {

                varyings o;

                float3 center    = _PositionBuffer[ instanceID ];
                float3 centerPos = _WrenPos;
                float3 fwd       = centerPos - center;
                float  dist      = length( fwd );

                float3 wrenToCam     = normalize( _WorldSpaceCameraPos - _WrenPos );
                float  distWrenToCam = length( _WrenPos - _WorldSpaceCameraPos );

                o.viewMatch = dot( wrenToCam , normalize( fwd ) );

                int intType = _TypeBuffer[ instanceID ];


                float sizeMultiplier = _TypeSizeMultiplier[ intType ]; //+ 1;

                if ( intType == 10 )
                {
                    sizeMultiplier = 10;
                }


                // if we are close to the place we are going, connect completely ( longer )
                // otherwise make it shorter and fatter ( depending on the type )





                float distMultiplier = lerp( _MinSizeMultiplier , _MaxSizeMultiplier ,
                                                                         saturate(
                                                                             ( dist - _MinSizeDistance ) / (
                                                                                 _MaxSizeDistance -
                                                                                 _MinSizeDistance ) ) );



                float wrenToCamMultiplier = lerp( _WrenToCameraScaleMin , _WrenToCameraScaleMax , saturate(
                        ( distWrenToCam - _WrenToCameraMin ) / (
                            _WrenToCameraMax -
                            _WrenToCameraMin ) ) );



                float wrenToCamOffset = lerp( _WrenToCameraOffsetMin , _WrenToCameraOffsetMax ,
                                          saturate(
                                              ( distWrenToCam - _WrenToCameraMin ) / (
                                                  _WrenToCameraMax -
                                                  _WrenToCameraMin ) ) );

                float wrenToCamUpOffset = lerp( _WrenToCameraUpOffsetMin , _WrenToCameraUpOffsetMax ,
                                                                saturate(
                                                                    ( distWrenToCam - _WrenToCameraMin ) / (
                                                                        _WrenToCameraMax -
                                                                        _WrenToCameraMin ) ) );

                float wrenToCamForwardOffset = lerp( _WrenToCameraForwardOffsetMin , _WrenToCameraForwardOffsetMax ,
                    saturate(
                        ( distWrenToCam - _WrenToCameraMin ) / (
                            _WrenToCameraMax -
                            _WrenToCameraMin ) ) );

                float scale = _Size;

                float3 r = normalize( fwd );
                float3 u = float3( 0 , 1 , 0 );
                float3 f = normalize( cross( u , r ) );
                u        = normalize( cross( f , r ) );

                // f = float3( 0 , 0 , 1 );
                // r = float3( 1 , 0 , 0 );
                // u = float3( 0 , 1 , 0 );

                float3 p = centerPos - normalize( fwd ) * 1 * _Forwards * wrenToCamOffset; //.5 * scale;


                p += float3( 0 , 1 , 0 ) * wrenToCamUpOffset;
                p += normalize( wrenToCam * float3( 1 , 0 , 1 ) ) * wrenToCamForwardOffset;


                float scl = _Size * distMultiplier * sizeMultiplier * wrenToCamMultiplier;


                float4x4 worldMat = float4x4(
                    scl * r.x , scl * u.x , scl * f.x , p.x ,
                    scl * r.y , scl * u.y , scl * f.y , p.y ,
                    scl * r.z , scl * u.z , scl * f.z , p.z ,
                    0 , 0 , 0 , 1 );

                //  worldMat = createTransformationMatrix(f, r, u, p, 1);
                float3 changedDir = v.normal.yzx;
                float3 changedPos = v.vertex.yzx;

                float valAlong = changedDir.x + .5;

                o.valAlong = v.vertex.y * 5;






                //changedDir *= valAlong * valAlong;

                float3 worldPos = mul( worldMat , float4( changedPos , 1 ) ).xyz;
                float3 worldNor = normalize( mul( worldMat , float4( changedDir , 0 ) ).xyz );

                o.worldPos = worldPos;

                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.nor = worldNor; //v.nor;
                o.uv  = v.texcoord.xy;
                o.id  = instanceID;

                o.fade  = _FadeBuffer[ instanceID ];
                o.type  = _TypeBuffer[ instanceID ];
                o.extra = _ExtraDataBuffer[ instanceID ];
                o.pos   = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );



                return o;

            }


            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {


                float3 col = float3( 1 , 0 , 0 );


                col = hsv( ( v.viewMatch + 1 ) / 2 , 1 , 1 );
                col = v.nor * .5 + .5;

                col            = dot( v.nor , float3( 0 , 1 , 0 ) );
                float matchVal = pow( saturate( 1 - dot( normalize( v.eye ) , normalize( v.nor ) ) ) * .9 , 10 ) * 10;


                col = hsv( v.type * .1 + ( ( v.viewMatch + 1 ) / 2 ) * .01 + matchVal * .01 - .02 , 1 , 1 );




                if ( v.extra.y < .5 )
                {
                    // discard the inside;

                    col *= 4;

                    if ( matchVal < .2 )
                    {
                        discard;
                    }

                }
                else
                {
                    //col *= matchVal;
                    for ( int i = 0; i < v.extra.x; i++ )
                    {

                        if ( abs( float( i ) - v.valAlong * 100 ) < .2 )
                        {
                            col *= 3;
                        }

                    }
                }


                // POINT OF INTEREST
                if ( v.type > 9.5 && v.type < 10.5 )
                {
                    col *= 10 * sin( _Time.y * 30 + v.valAlong * 4 );
                }


                col *= 3 * v.fade;


                return float4( col , 1 );
            }
            ENDCG

        }
    }

    Fallback Off


}