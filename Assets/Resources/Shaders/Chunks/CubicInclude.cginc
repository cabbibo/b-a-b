 float3 p0; float3 v0; float3 p1; float3 v1; float3 p2;


  float baseVal = val * (vPP-1);
  int baseUp   = floor( baseVal );
  int baseDown = ceil(baseVal );
  float amount = baseVal - float(baseUp);

  v0 = 0;
  v1 = 0;


  if( baseUp == 0 ){

    p0 = points[ baseUp       ];
    p1 = points[ baseDown     ];
    p2 = points[ baseDown + 1 ];

    v1 = .5 * ( p2 - p0 );

  }else if( baseDown == vPP-1 ){

    p0 = points[ baseUp     ];
    p1 = points[ baseDown   ];
    p2 = points[ baseUp - 1 ];

    v0 = .5 * ( p1 - p2 );

  }else{

    p0 = points[ baseUp   ];
    p1 = points[ baseDown ];


    float3 pMinus;

    pMinus = points[ baseUp -1  ];
    p2 =     points[ baseDown+1 ];

    v1 = .5 * ( p2 - p0 );
    v0 = .5 * ( p1 - pMinus );

  }

  float3 c0 = p0;
  float3 c1 = p0 + v0/3;
  float3 c2 = p1 - v1/3;
  float3 c3 = p1;


  float3 pos = cubicCurve( amount , c0 , c1 , c2 , c3 );
  
  if( baseUp == baseDown ){
     pos = points[ baseUp];
  }
  return pos;