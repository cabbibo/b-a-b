//A simple input struct for our pixel shader step containing a position.
struct FullVaryingData
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

    float3 tangent : TEXCOORD12;
    float3 tspace0 : TEXCOORD13;
    float3 tspace1 : TEXCOORD14;
    float3 tspace2 : TEXCOORD15;
    float  offsetAmount : TEXCOORD16;
    uint   instanceID : SV_InstanceID;

    // UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.

    UNITY_SHADOW_COORDS( 8 )
    UNITY_FOG_COORDS( 10 )
};
