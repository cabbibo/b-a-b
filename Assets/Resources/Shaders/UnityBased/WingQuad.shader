// Aligns a unit quad in world space between a wing root and a wing tip, both fed in per-instance
// via a MaterialPropertyBlock (_RootPos / _TipPos) by PreyVisuals (QuadBasedProcedural mode).
// The mesh's local vertex.x is the root→tip fraction (0..1); vertex.y is the chord (-0.5..0.5).
Shader "Prey/WingQuad"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Chord width", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f     { float4 vertex : SV_POSITION; float3 n : TEXCOORD0; };

            fixed4 _Color;
            float  _Width;
            // Set per-instance via MaterialPropertyBlock (world space).
            float4 _RootPos;
            float4 _TipPos;

            v2f vert( appdata v )
            {
                v2f o;

                float  t    = saturate( v.vertex.x );       // 0 = root, 1 = tip
                float  c    = v.vertex.y;                    // -0.5..0.5 across the chord

                float3 root = _RootPos.xyz;
                float3 tip  = _TipPos.xyz;
                float3 dir  = tip - root;
                float  len  = max( length( dir ), 1e-5 );
                dir        /= len;

                // chord runs perpendicular to the wing, kept roughly horizontal
                float3 side = normalize( cross( dir, float3( 0, 1, 0 ) ) + 1e-5 );

                float3 wp = lerp( root, tip, t ) + side * c * _Width;

                o.vertex = mul( UNITY_MATRIX_VP, float4( wp, 1 ) );
                o.n      = normalize( cross( side, dir ) );  // face normal
                return o;
            }

            fixed4 frag( v2f i ) : SV_Target
            {
                // cheap directional shade so the wings aren't flat
                float ndl = saturate( dot( normalize( i.n ), normalize( float3( 0.3, 1, 0.2 ) ) ) );
                return _Color * ( 0.5 + 0.5 * ndl );
            }
            ENDCG
        }
    }
}
