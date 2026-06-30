// Deforms an assigned mesh so it appears to flap. The mesh is authored with the wings spread along
// local X (X=0 = spine, ±X = wing tips). Each half rotates about the spine by _FlapAngle * _Flap so
// both tips rise/fall together. _Flap (-1..1) and _SpanX (mesh +X extent) are fed in per-instance
// via a MaterialPropertyBlock by PreyVisuals (WingedMesh mode).
Shader "Prey/WingedMesh"
{
    Properties
    {
        _Color     ("Color", Color) = (1,1,1,1)
        _FlapAngle ("Max flap angle (deg)", Float) = 35
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f     { float4 vertex : SV_POSITION; float3 n : TEXCOORD0; };

            fixed4 _Color;
            float  _FlapAngle;   // material default; overridden per-instance below
            // Set per-instance via MaterialPropertyBlock.
            float  _Flap;        // -1..1 current flap phase
            float  _SpanX;       // +X bounding-box extent of the mesh

            void bend( inout float3 p, inout float3 n )
            {
                // angle grows with the flap phase; sign(x) makes both wings lift the same way
                float ang = radians( _FlapAngle ) * _Flap * sign( p.x );
                float s = sin( ang ), c = cos( ang );

                // rotate (x,y) about the spine (z axis), pivoting at x = 0
                float2 r  = float2( p.x * c - p.y * s, p.x * s + p.y * c );
                p.x = r.x; p.y = r.y;

                float2 rn = float2( n.x * c - n.y * s, n.x * s + n.y * c );
                n.x = rn.x; n.y = rn.y;
            }

            v2f vert( appdata v )
            {
                v2f o;
                float3 p = v.vertex.xyz;
                float3 n = v.normal;
                bend( p, n );

                o.vertex = UnityObjectToClipPos( float4( p, 1 ) );
                o.n      = UnityObjectToWorldNormal( n );
                return o;
            }

            fixed4 frag( v2f i ) : SV_Target
            {
                float ndl = saturate( dot( normalize( i.n ), normalize( float3( 0.3, 1, 0.2 ) ) ) );
                return _Color * ( 0.4 + 0.6 * ndl );
            }
            ENDCG
        }
    }
}
