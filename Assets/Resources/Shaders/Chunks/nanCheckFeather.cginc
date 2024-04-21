void nanCheck( inout Feather v  ){

    bool nan;

    nan = isnan(v.vel);
    if( nan ){ v.vel = float3(1,0,0); }

    nan = isnan(v.pos);
    if( nan ){ v.pos = 0; }

    nan = isnan(v.ogPos);
    if( nan ){ v.ogPos = float3(0,0,0); }

    //nan = isnan(v.ogNor);
    //if( nan ){ v.ogNor = float3(1,0,0); }

    //nan = isnan(v.ogNor);
    //if( nan ){ v.ogNor = float3(1,0,0); }

    nan = isnan(v.touchingGround);
    if( nan ){ v.touchingGround = 1; }

    nan = isnan(v.touchingGround);
    if( nan ){ v.touchingGround = 1; }

    float4x4 t = translationMatrix(float3(0,0,0));
    float4x4 r = look_at_matrix(float3(1,0,0),float3(0,1,0));
    float4x4 s = scaleMatrix(float3(1,1,1));


    float4x4 rts =mul(t,mul(r,s));

    nan = isnan(v.ltw);
    if( nan ){ v.ltw = rts; }

}