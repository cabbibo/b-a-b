 Shader "Custom/TerrainShader" {
     Properties {
         _Color ("Color", Color) = (1,1,1,1)
         _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
         _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
         _DetailTex ("Detail (RGB)", 2D) = "white" {}
     }
     SubShader {
         Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
         LOD 200
         cull off
         
         CGPROGRAM        
         #pragma surface surf Standard  alphatest:_Cutoff 
                 
         #pragma target 4.0
 
         sampler2D _MainTex;
         sampler2D _DetailTex;        
         fixed4 _Color;
 
         struct Input {
             float2 uv_MainTex;
             float2 uv2_DetailTex;
         };
 
         void surf (Input IN, inout SurfaceOutputStandard o) {            
             fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * tex2D(_DetailTex, IN.uv2_DetailTex);
             o.Albedo = c.rgb;
             o.Alpha = c.a;
         }
         ENDCG
     }
     FallBack "Transparent/Cutout/Diffuse"
 }
