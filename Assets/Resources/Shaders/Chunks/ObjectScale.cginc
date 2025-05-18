half3 ObjectScale()
{
    return half3(
        length( unity_ObjectToWorld._m00_m10_m20 ) ,
        length( unity_ObjectToWorld._m01_m11_m21 ) ,
        length( unity_ObjectToWorld._m02_m12_m22 )
    );
}
