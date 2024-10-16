Shader "Unlit/BasicDebug"
{
    Properties
    {
      _Size("Size", Range(0.1, 1000)) = 1
      _MainTex("Texture", 2D) = "white" {}
      _ColorMultiplier("Color Multiplier", Range(0, 3)) = 1
      _NormalOffset("Normal Offset", Range(0, 10)) = 0
    }
    SubShader
    {
        
    Cull Off
    Pass{

      CGPROGRAM
      
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag
        #include "UnityCG.cginc"

            struct Vert{
        float3 pos;
        float3 oPos;
        float3 nor;
        float3 color;
        float2 uv;
        float life;
        float debug;
        };



        int _Count;
      float _Size;
      float _ColorMultiplier;
      float _NormalOffset;

      StructuredBuffer<Vert> _VertBuffer;

        //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor : NORMAL;
            float2 uv : TEXCOORD0;
            float debug : TEXCOORD1;
            float3 color : TEXCOORD2;
            float life : TEXCOORD3;
            float id :TEXCOORD4;
            float lifeSize : TEXCOORD5;
      };

        varyings vert (uint id : SV_VertexID){

            varyings o;
            // particle ID in compute buffer
            int base = id / 6;

            // which vert we are in the quad
        int alternate = id %6;


        
            if( base < _Count ){

                Vert vert = _VertBuffer[base];

                // up and right based on normal!
                // a normal that is *straight* up will not get rendered!


                float3 l = cross( vert.nor, float3(0,1,0));

                // if its facing straight up, use a different up floattor
                if( length(l) < .00001 ){
                    l = cross( vert.nor, float3(1,0,0));
                }

                l = normalize(l);

                float3 u = normalize(cross( l, vert.nor));

                // Right and up of camera ( view not project ) matrix
                //float3 l = UNITY_MATRIX_V[0].xyz;
                //float3 u = UNITY_MATRIX_V[1].xyz;

                float3 basePos = vert.pos;
                float3 extra = 0;

                float3 p1 = -l -u;
                float3 p2 = l -u;
                float3 p3 = -l +u;
                float3 p4 = l +u;

                float2 uv = 0;


                if( alternate == 0 ){
                    extra = p1;
                    uv = float2(0,0);
                }else if( alternate == 1 ){
                    extra = p2;
                    uv = float2(1,0);
                }else if( alternate == 2 ){
                    extra = p4;
                    
                    uv = float2(1,1);
                }else if( alternate == 3 ){
                    extra = p1;
                    uv = float2(0,0);
                }else if( alternate == 4 ){
                    extra = p4;
                    uv = float2(1,1);
                }else if( alternate == 5 ){
                    extra = p3;
                    uv = float2(0,1);
                }

                float3 eye = _WorldSpaceCameraPos - basePos;
                float3 eyeDir = normalize(eye);
                float m = dot( eyeDir, vert.nor);
                
                float lightMatch = dot( vert.nor, _WorldSpaceLightPos0.xyz );
            
                // scaling particles in and out based on life
                float dT = saturate(min( (1-vert.life) * 30 , vert.life ));

                // changing the size based on eye match
                float eyeMatchMultiplier =1;//pow( (1-m),1) * .5 + .5;

                // scaling size based on distance from camera ( try to keep them similar size? could be totally based on view if we want!)
                float distanceScale =(length(eye) + 30 )* .003;

                float finalSizeMultiplier = _Size * dT * eyeMatchMultiplier  * distanceScale;

                float scale = length(eye) * .003;// 1.0 + (1.0 / length(eye));
                
                //float scale = 1;
                float3 fPos = basePos + extra * _Size * scale * dT  + vert.nor * _NormalOffset;//*  _VertBuffer[base].debug.y;//saturate(dT * .1);

                o.lifeSize = dT;
                o.nor = vert.nor;
                o.uv = uv;
                o.color = vert.color;
                o.debug = vert.debug;
                o.life = vert.life;
                o.id = float(base);

                if( length(vert.uv.y) < 1 ){
                //  fPos = 0;
                }

              //  o.camPos = 
                o.pos = mul (UNITY_MATRIX_VP, float4(fPos,1.0f));


            }

            return o;

        }

        sampler2D _MainTex;
       float3 hsv(float h, float s, float v)
{
  return lerp( float3( 1.0 , 1, 1 ) , clamp( ( abs( frac(
    h + float3( 3.0, 2.0, 1.0 ) / 3.0 ) * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
} 

float3 rgb2hsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}


// All components are in the range [0…1], including hue.
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float3 ApplyHue(float3 col, float hueAdjust)
{
    const float3 k = float3(0.57735, 0.57735, 0.57735);
    half cosAngle = cos(hueAdjust);
    return col * cosAngle + cross(k, col) * sin(hueAdjust) + k * dot(k, col) * (1.0 - cosAngle);
}
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {      
        float4 col = tex2D(_MainTex, v.uv);

        
        float val = col.x;
        
        if( col.x < .5 ){
            discard;
        }


        float lightMatch = dot( v.nor, _WorldSpaceLightPos0.xyz );

       // col.xyz = lightMatch;

        //col = 1-  saturate((v.debug-.3) * 5);
        col.xyz = col.xyz * (v.nor.xyz * .5 + .5) * _ColorMultiplier;
        col.xyz *= .5;
        col.xyz += .5;

        col =1-saturate((v.debug-.45) * 20);//+1;;
        //col.xyz = hsv(v.life+ val * .4 + v.debug,.5, 1);
        //col.xyz *= hsv(val,.4,1); 
        //col.xyz *= v.color;

        float hue = rgb2hsv(v.color).x;
        col.xyz = ApplyHue(v.color , sin(v.id) *.5);//hsv(hue,.5,1);
        col.xyz *= (sin(v.id * 10)+1) * .5 + .5;
        col.xyz *= v.lifeSize;
        col *= 2;
        col.xyz = ApplyHue( col.xyz , val * .4 );// hsv( val,1,1);
       // col.xyz *= hsv( hue + .3*sin(v.id) ,.4,(sin(v.id * 10)+1) /4 + .5) ;
          return float4(col.xyz,1 );
      }


      ENDCG

    }

}

}
