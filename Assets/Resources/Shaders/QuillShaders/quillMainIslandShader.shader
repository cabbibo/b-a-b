// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Quill/quillMainIslandShader" {
    Properties {

        _Color ("Color", Color) = (1,1,1,1)
        _BackfaceColor("BackfaceColor", Color )= (1,1,1,1)
        _Size ("Size", float) = .01
        _Fade ("Fade", float) = 1
        _FadeLocation ("_FadeLocation",Vector) = (0,0,0)
        _WindDirection ("_WindDirection",Vector) = (1,0,0)
        _WindAmount ("_WindAmount",float) = 1
        _WindChangeSpeed ("_WindChangeSpeed",float) = 1
        _WindChangeSize ("_WindChangeSize",float) = 1

        
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

        _NormalMap ("Normal Map", 2D) = "bump" {}
        _TriplanarMultiplier ("TriplanarMultiplier", Vector) = (1,1,1)
        _TriplanarSharpness ("TriplanarSharpness", float) = 1
        _TriplanarNormalWeight ("TriplanarNormalWeight", float) = 1
        
        _TextureShadingWeights( "Texture Shading Weight" , Vector ) = ( 0,1,2,3)

        _PainterlyLightMap("PainterlyLightMap", 2D) = "white" {}


        _OutlineAmount("OutlineAmount", float) = .1
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)

        _ShadowStrength("ShadowStrength", float) = 1

        _PainterlyLightImportance("PainterlyLightImportance", float) = 1

        



    }






















    CGINCLUDE

    #include "Lighting.cginc"
    #include "../Chunks/hsv.cginc"
    #include "../Chunks/noise.cginc"
    #include "../Chunks/snoise3D.cginc"

    
    float3 hash33_float3(float3 p)
    {
        float3 q = float3(dot(p, float3(127.1, 311.7, 74.7)),
        dot(p, float3(269.5, 183.3, 246.1)),
        dot(p, float3(113.5, 271.9, 124.6)));
        return frac(sin(q) * 43758.5453);
    }
    
    uniform float4x4 _Transform;
    uniform int _NumberMeshes;
    float3 _WindDirection;
    float _WindAmount;
    float _WindChangeSpeed;
    float _WindChangeSize;
    float3 _SafeCameraPosition;

    float3 GetWindOffset( int id , float3 pos ){
        
        float3 windDirection = float3(1,0,0);
        float flooredTime = floor(_Time.y *_WindChangeSpeed + float(id) * .4);
        float3 noiseVal = hash33_float3( pos * _WindChangeSize + windDirection * flooredTime);

        float distanceMultiplier = length(pos - _WorldSpaceCameraPos) / 1000;
        return _WindDirection * noiseVal * _WindAmount * distanceMultiplier;//windAmount;
        
    }


    struct LightingData{
        float3 flatNormal;
        float flatNormalMatch;
        float normalMatch;
        float lightMatch;
        float flatLightMatch;
        float eyeMatch;
        float reflectionMatch;

    };

    void GetLightingData( float3 worldPos , float3 eye , float3 nor , float3 lightDir , out LightingData lightingData ){

        float3 ddxW = ddx(worldPos);
        float3 ddyW = ddy(worldPos);
        lightingData.flatNormal = -normalize(cross(ddxW,ddyW));

        lightingData.flatNormalMatch = saturate(dot(normalize(eye), lightingData.flatNormal ));
        lightingData.normalMatch = saturate(dot( normalize(eye), nor));

        lightingData.lightMatch = saturate(dot( normalize(lightDir), nor));
        lightingData.flatLightMatch = saturate(dot( normalize(lightDir),  lightingData.flatNormal));

        lightingData.eyeMatch = saturate(dot( normalize(eye), nor));
        lightingData.reflectionMatch = saturate(dot( normalize(-lightDir), reflect( normalize(eye), nor)));


        
    }


    void DoEdgeDiscard(LightingData lightingData, float3 worldPos, float3 eye){
        
        
        float discardValue = lightingData.normalMatch - (snoise(worldPos * 4.4)+1) * .5;
        discardValue =lerp( discardValue , 1 , saturate( length(eye) * .003));
        
        if( discardValue < 0 ){
            discard;
        }

    }

    sampler2D _NormalMap;
    float3 _TriplanarMultiplier;
    float _TriplanarSharpness;
    float _TriplanarNormalWeight;


    float3 triplanarNormal(float3 p, float3 n, float3 tspace0, float3 tspace1, float3 tspace2 , float offset){

        // UDN blend
        // Triplanar uvs
        float2 uvX = p.zy * _TriplanarMultiplier + offset; // x facing plane
        float2 uvY = p.xz * _TriplanarMultiplier + offset; // y facing plane
        float2 uvZ = p.xy * _TriplanarMultiplier + offset; // z facing plane

        // Tangent space normal maps
        half3 tnormalX = UnpackNormal(tex2D(_NormalMap, uvX%1));
        half3 tnormalY = UnpackNormal(tex2D(_NormalMap, uvY%1));
        half3 tnormalZ = UnpackNormal(tex2D(_NormalMap, uvZ%1));

        // Swizzle world normals into tangent space and apply UDN blend.
        // These should get normalized, but it's very a minor visual
        // difference to skip it until after the blend.
        tnormalX = normalize(half3(tnormalX.xy * _TriplanarNormalWeight + n.zy, n.x));
        tnormalY = normalize(half3(tnormalY.xy* _TriplanarNormalWeight  + n.xz, n.y));
        tnormalZ = normalize(half3(tnormalZ.xy* _TriplanarNormalWeight  + n.xy, n.z));

        half3 blend =  pow(abs(n),_TriplanarSharpness) ;
        // make sure the weights sum up to 1 (divide by sum of x+y+z)
        blend /= dot(blend,1.0);

        // Swizzle tangent normals to match world orientation and triblend
        half3 worldNormal = normalize(
        tnormalX.zyx * blend.x +
        tnormalY.xzy * blend.y +
        tnormalZ.xyz * blend.z
        );

        return worldNormal;
        
    }



    sampler2D _PainterlyLightMap;
    
    float4 triplanarSample(float3 p , float3 n){
        
        half3 blend = pow(abs(n),_TriplanarSharpness) ;;
        
        // make sure the weights sum up to 1 (divide by sum of x+y+z)
        blend /= dot(blend,1.0);

        
        float4 cx = tex2D(_PainterlyLightMap,(p.zy * _TriplanarMultiplier *.3));
        float4 cy = tex2D(_PainterlyLightMap,(p.xz * _TriplanarMultiplier *.3));
        float4 cz = tex2D(_PainterlyLightMap,(p.xy * _TriplanarMultiplier *.3));

        
        // blend the textures based on weights
        fixed4 c = 0;
        c= cx * blend.x + cy * blend.y + cz * blend.z;
        return c;

    }




    // painterly
    float4 _TextureShadingWeights;

    // m = match
    float4 PainterlyColor(float3 pos , float3 nor  , float m , float2 uv){


        // float4 p = triplanarSample( pos , nor );
        float4 p = tex2D(_PainterlyLightMap,uv * 3);


        float4 weights = 0;
        if( m < _TextureShadingWeights.x){
            weights = float4(1 , 0 , 0, 0);
            }else if( m >= _TextureShadingWeights.x && m < _TextureShadingWeights.y){
            weights = float4(1-(m-_TextureShadingWeights.x)/(_TextureShadingWeights.y-_TextureShadingWeights.x) ,(m-_TextureShadingWeights.x)/(_TextureShadingWeights.y-_TextureShadingWeights.x) , 0, 0);//lerp( p.x , p.y , m );
            }else if( m >= _TextureShadingWeights.y && m < _TextureShadingWeights.z){
            weights = float4(0,1-(m-_TextureShadingWeights.y)/(_TextureShadingWeights.z-_TextureShadingWeights.y) , (m-_TextureShadingWeights.y)/(_TextureShadingWeights.z-_TextureShadingWeights.y) ,  0);
            }else if( m >= _TextureShadingWeights.z && m < _TextureShadingWeights.w){
            weights = float4(0,0,1-(m-_TextureShadingWeights.z)/(_TextureShadingWeights.w-_TextureShadingWeights.z) , (m-_TextureShadingWeights.z)/(_TextureShadingWeights.w-_TextureShadingWeights.z) );
            }else{
            weights = float4(0,0,0 , 1);
        }

        float4 fLCol  = p.x * weights.x;
        fLCol += p.y * weights.y;
        fLCol += p.z * weights.z;
        fLCol += p.w * weights.w;
        //fLCol = 1-fLCol;

        // fLCol = lerp( float4(1,.9,.6,1) * .7 + .1 , (float4(.4,.5,.8,1) * .8 + .2) * .3, 1-fLCol);
        fLCol = lerp( float4(1,.9,.8,1) * 1 , (float4(.2,.3,.4,1) * .3), 1-pow(fLCol,4));

        return fLCol;
    }
    


    ENDCG





















    SubShader{

        Pass{

            
            Tags { "RenderType"="Opaque" }
            Tags{ "LightMode" = "ForwardBase" }
            LOD 100 
            Cull Off
            // Giving our selves stencil info 
            // for our outline shader to use
            Stencil
            {
                Ref 10
                Comp always
                Pass replace
                ZFail keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_fogV
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            


            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)


            uniform float3 _Color;

            float _Fade;

            float3 _FadeLocation;
            
            sampler2D _MainTex;
            
            
            struct inputData {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                fixed4 color : COLOR;
                
                uint   id                : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };


            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 nor      : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
                float2 uv2       : TEXCOORD6;
                float4 color : TEXCOORD11;
                float id        : TEXCOORD5;
                int feather:TEXCOORD7;
                float4 data1:TEXCOORD9;   

                float3 tangent : TEXCOORD12;
                float3 tspace0 : TEXCOORD13;
                float3 tspace1 : TEXCOORD14;
                float3 tspace2 : TEXCOORD15;
                float offsetAmount : TEXCOORD16;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
                
                UNITY_SHADOW_COORDS(8)
                UNITY_FOG_COORDS(10)
            };


            float _PainterlyLightImportance;



            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (inputData vert){
                
                varyings o;

                UNITY_SETUP_INSTANCE_ID(vert);
                UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                
                float3 wPos = mul( unity_ObjectToWorld,  float4(vert.vertex.xyz,1)).xyz;


                int instanceID = 0;
                #ifdef INSTANCING_ON
                    instanceID = vert.instanceID;
                #endif

                float3 windOffset = GetWindOffset( instanceID , wPos );
                o.worldPos = wPos + windOffset;//windAmount;
                
                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));
                o.eye = _WorldSpaceCameraPos - o.worldPos;
                o.nor = normalize(mul( unity_ObjectToWorld,  float4(vert.normal,0)).xyz);
                o.uv = vert.texcoord.xy;
                o.color = vert.color;
                o.tangent = vert.tangent.xyz * vert.tangent.w;
                o.offsetAmount = length(windOffset);

                half3 wNormal = o.nor;
                half3 wTangent = mul( unity_ObjectToWorld,float4(vert.tangent.xyz,0) ).xyz* vert.tangent.w;
                // compute bitangent from cross product of normal and tangent
                //half tangentSign = tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent);// * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);


                UNITY_TRANSFER_SHADOW(o,o.worldPos);
                UNITY_TRANSFER_FOG(o,o.pos);
                

                return o;

            }



            uniform sampler2D _PaintTexture;


            float sdCapsule( float3 p, float3 a, float3 b, float r )
            {
                float3 pa = p - a, ba = b - a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                return length( pa - ba*h ) - r;
            }

            float3 _WrenPos;



            void DoWrenDiscard(float3 worldPos ){
                
                // Discards around bird!

                float capDistance =sdCapsule(worldPos , _WorldSpaceCameraPos , _WrenPos , 1 );
                capDistance -= snoise(worldPos) * .2;

                if( capDistance < 0 ){
                    discard;
                    }else{
                    //col *= saturate(capDistance * 10);
                }
                

            }
            
            half3 ObjectScale() {
                return half3(
                length(unity_ObjectToWorld._m00_m10_m20),
                length(unity_ObjectToWorld._m01_m11_m21),
                length(unity_ObjectToWorld._m02_m12_m22)
                );
            }

            float2 GetXYInLightSpace(float3 worldPos ){

                // this is our x value
                float distTowardLight = dot( worldPos  , normalize(_WorldSpaceLightPos0));

                
                float distTowardUp = dot( worldPos  , normalize(cross( cross(_WorldSpaceLightPos0, float3(0,1,0)), _WorldSpaceLightPos0)));



                //float


                // get a perpentdicular value
                float3 perp = cross( worldPos , _WorldSpaceLightPos0);
                float distTowardCamera = dot( perp , float3(0,1,0));



                return float2( distTowardLight , distTowardUp);
            }


            float _ShadowStrength;

            //Pixel function returns a solid color for each point.
            float4 frag (varyings v) : COLOR {


                LightingData lightingData;
                float3 col;

                fixed shadow = UNITY_SHADOW_ATTENUATION(v,v.worldPos);// * .5 + .5;

                shadow = shadow * _ShadowStrength + (1-_ShadowStrength);
                
                GetLightingData( v.worldPos , v.eye , v.nor , _WorldSpaceLightPos0.xyz , lightingData );

                float3 fNor = normalize( lerp(lightingData.flatNormal , v.nor , 1) );
                float3 triplanarNor = triplanarNormal(v.worldPos,fNor ,v.tspace0,v.tspace1,v.tspace2,v.offsetAmount * .1);
                
                //float 

                float3 traceCol = 0;
                float3 eye = v.eye;
                for( int i = 0; i < 3; i++){
                    float ni = (float)i/3;
                    float3 fPos = v.worldPos - normalize(eye) * float(i) * 1.3;
                    float v = snoise(fPos * (ni * 10+2) );
                    traceCol += v;
                    
                }//



                
                // Add bird fade

                float3 shadowCol = pow( traceCol ,6) * .1;//* traceCol * traceCol * .1;
                shadowCol = length(shadowCol) * v.color;

                shadowCol = lerp( shadowCol , 0 , saturate( length(v.eye) * .01));

                col = v.color;//lerp( v.color * (shadowCol * .5+.5) ,shadowCol,1-shadow);

                float lightMatch = saturate(dot(_WorldSpaceLightPos0,triplanarNor));
                float normalMatch = saturate(dot(v.eye,triplanarNor));
                float reflectionMatch = saturate(dot( reflect( -_WorldSpaceLightPos0 , triplanarNor),normalize(v.eye)));
                //col += lerp(v.color*floor(pow(1-normalMatch,3) * 5)/5,0, saturate( length(v.eye) * .001)) * 1;
                //col += lerp( (v.color )*floor(pow(reflectionMatch ,4) * 2)/2,0, saturate( length(v.eye) * .001));
                float scale = length(ObjectScale());
                float4 painterlyColor = PainterlyColor(
                v.worldPos , 
                triplanarNor , 
                
                (lightMatch* shadow+ reflectionMatch *3+ pow(normalMatch,10) * .4)  ,
                (GetXYInLightSpace(v.worldPos) * _TriplanarMultiplier.xy) 
                );

                float4 painterlyColor2 = PainterlyColor(
                v.worldPos , 
                triplanarNor , 
                (lightMatch* shadow+ reflectionMatch *3* shadow+ pow(normalMatch,10) * .4)    ,
                (GetXYInLightSpace(v.worldPos) * _TriplanarMultiplier.xy).yx 
                );


                
                col = lerp( 1 , lerp(painterlyColor, painterlyColor2,0) ,_PainterlyLightImportance);

                col += shadowCol * .2 * (1-shadow);
                col *= v.color  * 2.;

                // col += lightMatch;

                //col =v.color * (painterlyColor )* .8;
                //col += shadowCol * .2 * (1-shadow);
                //col += floor(pow( reflectionMatch,20) * 4)/4;

                // col.xy = sin(GetXYInLightSpace(v.worldPos) * float2(1,1));
                //col.z = 0;

                //                col.xy = sin(GetXYInLightSpace(v.worldPos));

                DoEdgeDiscard(lightingData,v.worldPos,v.eye);
                DoWrenDiscard(v.worldPos);

                return float4(col,1);
            }

            ENDCG

        }


        














        
        /*


        // Outline Pass
        Pass
        {

            Cull OFF
            ZWrite ON
            ZTest ON

            // Here is where we set the values 
            // so the outline will only show *outside* 
            // the object
            Stencil
            {
                Ref 10
                Comp notequal
                Fail keep
                Pass replace
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #pragma multi_compile_instancing



            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 nor      : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
                float2 uv2       : TEXCOORD6;
                float4 color : TEXCOORD11;
                float id        : TEXCOORD5;
                int feather:TEXCOORD7;
                float4 data1:TEXCOORD9;   
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
                
                UNITY_FOG_COORDS(10)
            };



            
            struct inputData {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                fixed4 color : COLOR;
                uint   id                : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };


            float4 _OutlineColor;
            float _OutlineAmount;

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            


            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            
            varyings vert ( inputData v )
            {
                varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                //  UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                // int instanceID = UNITY_GET_INSTANCE_ID(vert);
                float3 wPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
                float3 wNor = normalize(mul( unity_ObjectToWorld,  float4(v.normal.xyz,0)).xyz) ;
                //UNITY_VERTEX_INPUT_INSTANCE_ID

                
                int instanceID = 0;
                #ifdef INSTANCING_ON
                    instanceID = v.instanceID;
                #endif

                float3 windOffset = GetWindOffset( instanceID , wPos );
                o.worldPos = wPos + windOffset;//windAmount;
                
                o.worldPos += wNor * _OutlineAmount;
                
                o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


                return o;
            }

            fixed4 frag (varyings v) : SV_Target
            {
                fixed4 col = _OutlineColor;
                return col;
            }

            ENDCG
        }

        


        */


        

        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"



            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 nor      : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
                float2 uv2       : TEXCOORD6;
                float4 color : TEXCOORD11;
                float id        : TEXCOORD5;
                int feather:TEXCOORD7;
                float4 data1:TEXCOORD9;   
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
                
                UNITY_FOG_COORDS(10)
            };



            
            struct inputData {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                fixed4 color : COLOR;
                uint   id                : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };

            #include "../Chunks/ShadowCasterPos.cginc"
            float4 CustomShadowPos(float4 vertex, float3 normal)
            {
                float4 wPos = vertex;
                float3 wNormal = normal;

                if (unity_LightShadowBias.z != 0.0)
                {
                    
                    float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

                    // apply normal offset bias (inset position along the normal)
                    // bias needs to be scaled by sine between normal and light direction
                    // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
                    //
                    // unity_LightShadowBias.z contains user-specified normal offset amount
                    // scaled by world space texel size.

                    float shadowCos = dot(wNormal, wLight);
                    float shadowSine = sqrt(1-shadowCos*shadowCos);
                    float normalBias = unity_LightShadowBias.z * shadowSine;

                    wPos.xyz -= wNormal * normalBias;
                }

                return mul(UNITY_MATRIX_VP, wPos);
            }

            

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)
            
            

            varyings vert(inputData v)
            {
                varyings o;

                //float4 p = UnityObjectToClipPos(v.vertex);
                float4 p = float4( v.vertex.xyz, 1);
                // TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos);

                
                UNITY_SETUP_INSTANCE_ID(vert);
                //  UNITY_TRANSFER_INSTANCE_ID(vert, o); // necessary only if you want to access instanced properties in the fragment Shader.
                // int instanceID = UNITY_GET_INSTANCE_ID(vert);
                float3 wPos = mul( unity_ObjectToWorld,  float4(v.vertex.xyz,1)).xyz;
                //UNITY_VERTEX_INPUT_INSTANCE_ID

                
                int instanceID = 0;
                #ifdef INSTANCING_ON
                    instanceID = vert.instanceID;
                #endif

                float windOffset = GetWindOffset( instanceID , wPos );



                float4 worldPos = float4(wPos + windOffset,1);//windAmount;
                

                // o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
                o.eye = _WorldSpaceCameraPos - worldPos.xyz;
                o.nor = normalize(mul( unity_ObjectToWorld,  float4(v.normal,0)).xyz);
                o.worldPos = worldPos.xyz;



                //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal  =  UnityObjectToWorldNormal(v.normal);

                float4 opos = 0;
                opos = CustomShadowPos(worldPos, worldNormal);
                opos = UnityApplyLinearShadowBias(opos);
                o.pos = opos;


                //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }



            float4 frag(varyings i) : SV_Target
            {
                LightingData lightingData;
                GetLightingData( i.worldPos , i.eye , i.nor , _WorldSpaceLightPos0.xyz , lightingData );
                // discard;
                

                //  DoEdgeDiscard(lightingData,i.worldPos,i.eye);
                

                SHADOW_CASTER_FRAGMENT(i);
                

            }
            ENDCG
        }


    }

    //Fallback "Diffuse"


}









