// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader"Islands/Cave/CrystalCollected"
{
    Properties
    {

        _Color ("Color", Color) = (1,1,1,1)
        _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
        _Size ("Size", float) = .01
        _Fade ("Fade", float) = 1
        _FadeLocation ("_FadeLocation",Vector) = (0,0,0)

        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}


        _TriplanarMap ("TriplanarMap", 2D) = "white" {}
        _TriplanarNormalMap ("TriplanarNormal", 2D) = "white" {}
        _TriplanarSharpness ("_TriplanarSharpness", float) = 1


        _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)

        _PaintTexture("_Paint Texture",2D)="white" {}

        _WindDirection ("_WindDirection",Vector) = (1,0,0)
        _WindAmount ("_WindAmount",float) = 1
        _WindChangeSpeed ("_WindChangeSpeed",float) = 1
        _WindChangeSize ("_WindChangeSize",float) = 1


        _ColorMap("_ColorMap",2D)="white"{}



    }


    SubShader
    {

        Pass
        {

            Tags
            {
                "RenderType"="Opaque"
            }
            LOD 100
            Cull Off
            Tags
            {
                "LightMode" = "ForwardBase"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            // make fog work
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"


            #include "Assets/Resources/Shaders/Chunks/hsv.cginc"
            #include "Assets/Resources/Shaders/Chunks/noise.cginc"
            #include "Assets/Resources/Shaders/Chunks/snoise3D.cginc"
            #include "Assets/Resources/Shaders/Chunks/triNoise3D.cginc"

            UNITY_INSTANCING_BUFFER_START( Props )
            UNITY_INSTANCING_BUFFER_END( Props )

            uniform float3 _Color;

            float _Fade;
            float _Multiplier;


            float3 _FadeLocation;

            struct appdata_full2
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                fixed4 color : COLOR;
                uint   id : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //uniform float4x4 worldMat;

            sampler2D _MainTex;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings
            {
                float4 pos : SV_POSITION;
                float3 nor : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye : TEXCOORD2;
                float3 debug : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float2 uv2 : TEXCOORD6;
                float4 color : TEXCOORD11;
                float  id : TEXCOORD5;
                int    feather:TEXCOORD7;
                float4 data1:TEXCOORD9;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.

                UNITY_SHADOW_COORDS( 8 )
                UNITY_FOG_COORDS( 10 )
            };

            uniform float4x4 _Transform;
            uniform int      _NumberMeshes;

            float3 _WindDirection;
            float  _WindAmount;
            float  _WindChangeSpeed;
            float  _WindChangeSize;

            sampler2D _ColorMap;

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert( appdata_full2 vert )
            {
                varyings o;

                UNITY_SETUP_INSTANCE_ID( vert );
                UNITY_TRANSFER_INSTANCE_ID( vert , o ); // necessary only if you want to access instanced properties in the fragment Shader.
                int instanceID = 0;

                #ifdef INSTANCING_ON
    instanceID = vert.instanceID;
                #endif


                float flooredTime = floor( _Time.y * _WindChangeSpeed + float( 0 ) * .4 );

                float3 wPos = mul( unity_ObjectToWorld , float4( vert.vertex.xyz , 1 ) ).xyz;

                //  o.worldPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;

                float3 windDirection = float3( 1 , 0 , 0 );

                float3 noiseVal = snoise( wPos * _WindChangeSize + windDirection * flooredTime );

                o.worldPos = wPos + _WindDirection * noiseVal * _WindAmount; //windAmount;


                o.pos   = mul( UNITY_MATRIX_VP , float4( o.worldPos , 1.0f ) );
                o.eye   = _WorldSpaceCameraPos - o.worldPos;
                o.nor   = normalize( mul( unity_ObjectToWorld , float4( vert.normal , 0 ) ).xyz );
                o.uv    = vert.texcoord.xy;
                o.color = vert.color;
                UNITY_TRANSFER_SHADOW( o , o.worldPos );
                UNITY_TRANSFER_FOG( o , o.pos );


                return o;

            }


            uniform sampler2D _PaintTexture;

            #include "Assets/Resources/Shaders/Chunks/triplanar.cginc"

            float distanceToLine( float3 p , float3 a , float3 b )
            {
                float3 pa      = p - a;
                float3 ba      = b - a;
                float  h       = dot( pa , ba ) / dot( ba , ba );
                float3 closest = a + ba * h;
                return length( p - closest );
            }


            float sdCapsule( float3 p , float3 a , float3 b , float r )
            {
                float3 pa = p - a, ba = b - a;
                float  h  = clamp( dot( pa , ba ) / dot( ba , ba ) , 0.0 , 1.0 );
                return length( pa - ba * h ) - r;
            }


            #include "Assets/Resources/Shaders/Chunks/flashlight.cginc"
            #include  "Assets/Resources/Shaders/Chunks/ShardToggleGroup.cginc"

            //Pixel function returns a solid color for each point.
            float4 frag( varyings v ) : COLOR
            {

                float3 col = 0;



                float flashlightVal = flashlight( v.worldPos );

                if ( flashlightVal < 0 )
                {
                    // discard;

                }

                float3 rd = normalize( v.worldPos - _WorldSpaceCameraPos );



                float4 traceCol = 0;
                for ( int i = 0; i < 10; i++ )
                {

                    float3 wPos = v.worldPos + rd * float( i ) * 1;



                    // float p = snoise( wPos * .03 + _Time.x );

                    float p = triNoise3D( wPos * .04 , 1 , _Time.y );


                    float4 triplanarTex = triplanarSample( wPos * ( .03 + p * .00001 ) , v.nor );

                    traceCol += tex2D( _ColorMap , p + float( i ) / 10 ) * triplanarTex.r * saturate( ( ( sin( p * 11 ) - .8 ) * 1 ) ) * 4;
                }

                col = traceCol * 10;







                return float4( col , 1 );
            }
            ENDCG

        }










    }

    Fallback "Diffuse"


}