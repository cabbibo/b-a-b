struct BaseInputData
{
    float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    fixed4 color : COLOR;

    uint id : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
