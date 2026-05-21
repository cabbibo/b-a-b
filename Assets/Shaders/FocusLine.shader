Shader "Prey/FocusLine"
{
    Properties
    {
        _BirdPos   ("Bird World Position", Vector) = (0,0,0,0)
        _EatRadius ("Eat Radius",          Float)  = 1.5
        _EdgeWidth ("Edge Blend Width",    Float)  = 0.5
        _Alpha     ("Alpha",               Float)  = 0.85
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _BirdPos;
            float  _EatRadius;
            float  _EdgeWidth;
            float  _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color    = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.worldPos, _BirdPos.xyz);

                // smooth step: 0 = fully inside eat radius, 1 = fully outside
                float t = smoothstep(_EatRadius - _EdgeWidth, _EatRadius + _EdgeWidth, dist);

                fixed4 red   = fixed4(1.0, 0.15, 0.15, 1.0);
                fixed4 green = fixed4(0.15, 1.0, 0.3,  1.0);
                fixed4 col   = lerp(red, green, t);

                // fade alpha toward the wren end using vertex color alpha baked by LineRenderer
                col.a = i.color.a * _Alpha;
                return col;
            }
            ENDCG
        }
    }
}
