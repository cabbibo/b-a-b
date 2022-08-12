float3 cubicFromValue4(float val  , float3 points[4]  ){
  float vPP = 4;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicFromValue3(float val  , float3 points[3]  ){
  float vPP = 3;
  #include "../Chunks/CubicInclude.cginc"
}
float3 cubicFromValue6(float val  , float3 points[6]  ){
  float vPP = 6;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicFromValue7(float val  , float3 points[7]  ){
  float vPP = 7;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicFromValue8(float val  , float3 points[8]  ){
  float vPP = 8;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicFromValue9(float val  , float3 points[9]  ){
  float vPP = 9;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicFromValue5(float val  , float3 points[5]  ){
  float vPP = 5;
  #include "../Chunks/CubicInclude.cginc"
}

float3 cubicLoop8( float val ,float3 points[8]){
    float vPP = 8;

    float baseVal = val * (vPP);
    int baseUp   = floor( baseVal );
    int baseDown = ceil(baseVal );
    float amount = baseVal - float(baseUp);

    int id1 = baseUp;
    int id3 = baseUp-1;
    if( id3 < 0 ){ id3 += int(vPP);}
    if( id1 < 0 ){ id1 += int(vPP);}


  int id2 = baseDown;
  int id4 = (baseDown +1);

    id2 %= int(vPP);
    id4 %= int(vPP);
    //

    float3 p0 = points[ id1];
    float3 pMinus = points[id3];

    float3 p1 = points[ id2];
    float3 p2 = points[id4];

    float3 v1 = .5 * ( p2 - p0 );
    float3 v0 = .5 * ( p1 - pMinus );

  float3 c0 = p0;
  float3 c1 = p0 + v0/3;
  float3 c2 = p1 - v1/3;
  float3 c3 = p1;

  float3 pos = cubicCurve( amount , c0 , c1 , c2 , c3 );
  
  if( baseUp == baseDown ){
     pos = points[ baseUp];
  }
  return pos;


}