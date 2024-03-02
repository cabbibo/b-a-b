void primaryFeathersCubic( float lerpVal , out float3 fPos , out float3 fFwd , out float3 fUp ){

    float3 pos1[4] = {   
      shoulder,
      elbow, 
      hand, 
      finger 
    };
    
    
    fPos = cubicFromValue4( lerpVal , pos1);
  
    float3 pos2[4] = {   
      shoulderU,
      elbowU, 
      handU, 
      fingerU 
    };
    
    fUp =normalize(cubicFromValue4( lerpVal , pos2)); //normalize(lerp(  handU , fingerU , lerpVal ));//float3(1,0,0);// lerp(  handF , fingerF , lerpVal );
    
    float3 pos3[4] = {   
      shoulderF,
      elbowF-elbowR * .2, 
      handF+ handR * .4, 
      fingerR 
    };

    fFwd = normalize(cubicFromValue4( lerpVal , pos3));//-cross(handF,handU);//lerp( float3( 0 , 0 , 1 ) , float3(0,0,1), lerpVal);
    
}

void primaryFeathers( uint id , out float3 fPos , out float3 fFwd , out float3 fUp , out float3 fScale ){
    float lerpVal = float(id) / float(_NumPrimaryFeathers);

    v1 = hand;
    v2 = hand - finger;
    fPos = v1 - v2 * lerpVal;

    primaryFeathersCubic( lerpVal , fPos , fFwd , fUp );

    // flipping to right side
    //fUp = -normalize(cross(fFwd,fUp));
    fScale  = (6  * lerpVal + 6 ) ;

    fFwd += cross( fUp , fFwd ) * _BaseDirectionLeftRightNoise * (hash(float(id * 10))-.5);
    fFwd += fUp  * _BaseDirectionUpNoise * ((hash(float(id * 30)))-.5);
    fFwd += fUp  * _BackAmountOverlapping;

    // flipping to right side
    //fUp = -normalize(cross(fFwd,fUp));
    fScale  = (.5-abs(lerpVal-.5)) * _MiddlePrimaryFeatherScaleMultiplier    + _BasePrimaryFeatherScale;// (1  * (1-lerpVal) + 4 ) ;
    fScale  += _BaseNoiseScale * (hash(float(int(id)* 50)) - .5);// snoise(fPos * _BaseNoiseSize);


}

void primaryCovertsCubic(float lerpVal, out float3 fPos , out float3 fFwd , out float3 fUp){
   float3 pos1[3] = {   
      shoulder+ shoulderU * .2 * 0,
      elbow+ elbowU * .2 * 0, 
      hand+ handU * .2 * 0
      //finger+ fingerU * .2 
    };
    
    
    fPos = cubicFromValue3( lerpVal , pos1);
  
    float3 pos2[3] = {   
      shoulderU,
      elbowU, 
      handU 
      //fingerU 
    };
    
    fUp =normalize(cubicFromValue3( lerpVal , pos2)); //normalize(lerp(  handU , fingerU , lerpVal ));//float3(1,0,0);// lerp(  handF , fingerF , lerpVal );
    
    float3 pos3[3] = {   
      shoulderF - shoulderU * .2 ,
      elbowF+elbowR * .5- elbowU * .2, 
      handR- handU * .1, 
      //fingerR 
    };

    fFwd = normalize(cubicFromValue3( lerpVal , pos3));//-cross(handF,handU);//lerp( float3( 0 , 0 , 1 ) , float3(0,0,1), lerpVal);
    
}

void primaryCoverts( uint id , out float3 fPos , out float3 fFwd , out float3 fUp , out float3 fScale ){
 
   float lerpVal = float(id-_NumPrimaryFeathers) / float(_NumPrimaryCoverts);

  primaryCovertsCubic( lerpVal , fPos , fFwd , fUp );

  float3 fRight = normalize(cross(fUp,fFwd));
   
    fFwd += cross( fUp , fFwd ) * _BaseDirectionLeftRightNoise * (hash(float(id * 10))-.5);
    fFwd += fUp  * _BaseDirectionUpNoise * ((hash(float(id * 30)))-.5);
   // fFwd += fRight;
    fFwd += fUp  * _BackAmountOverlapping;



    // flipping to right side
    //fUp = -normalize(cross(fFwd,fUp));
    fScale  = (.5-abs(lerpVal-.5)) * _MiddleSecondaryFeatherScaleMultiplier + _BaseSecondaryFeatherScale;// (1  * (1-lerpVal) + 4 ) ;
    fScale  += _BaseNoiseScale * (hash(float(int(id)* 50)) - .5);// snoise(fPos * _BaseNoiseSize);


}


void lesserCovertsCubic(float rowVal , float colVal, out float3 fPos , out float3 fFwd , out float3 fUp){
   float3 pos1[4] = {   
      shoulder + shoulderF * 1.4,
      shoulder + shoulderF * 1.2 - shoulderR * .4,
      elbow + elbowF * .9,
      hand + handF*.1
    };

    
    
    float3 pos2[4] = {   
      shoulder + shoulderF * .2 + shoulderU * .4,
      shoulder + shoulderF * .1 - shoulderR * .5 + shoulderU * .4,
      elbow + elbowF * .3 +  elbowU * .5,
      hand + handF*.1  - handR * .2
    };

    float3 pos3[4] = {   
      
      shoulder - shoulderF * .8 + shoulderU * .4,
      shoulder - shoulderF * .8 - shoulderR * .6 + shoulderU * .3,
      elbow - elbowF * .3+  elbowU * .5 - elbowR * .3, 
      hand - handF*.1 - handR * .6
    };

    float3 pos4[4] = {   
      shoulder - shoulderF * 1.5+ shoulderU * .3,
      shoulder - shoulderF  * 1.3- shoulderR * .6 + shoulderU * .2,
      elbow - elbowF * .8 + elbowU * .4- elbowR * .5,
       hand - handF*.2  - handR *1
    };

    float3 cubicPos[4];
    

    cubicPos[0] = cubicFromValue4( rowVal ,pos1 );
    cubicPos[1] = cubicFromValue4( rowVal ,pos2 );
    cubicPos[2] = cubicFromValue4( rowVal ,pos3 );
    cubicPos[3] = cubicFromValue4( rowVal ,pos4 );

    float3 p1 = cubicFromValue4( colVal , cubicPos);
    float3 p2 = cubicFromValue4( colVal -.01 , cubicPos);

    cubicPos[0] = cubicFromValue4( rowVal + .01 ,pos1 );
    cubicPos[1] = cubicFromValue4( rowVal + .01 ,pos2 );
    cubicPos[2] = cubicFromValue4( rowVal + .01 ,pos3 );
    cubicPos[3] = cubicFromValue4( rowVal + .01 ,pos4 );

    float3 p3 = cubicFromValue4( colVal , cubicPos);

    cubicPos[0] = cubicFromValue4( rowVal - .01 ,pos1 );
    cubicPos[1] = cubicFromValue4( rowVal - .01 ,pos2 );
    cubicPos[2] = cubicFromValue4( rowVal - .01 ,pos3 );
    cubicPos[3] = cubicFromValue4( rowVal - .01 ,pos4 );
    float3 p4 = cubicFromValue4( colVal -.01 , cubicPos);
        



    fPos = p1;
    fFwd = -normalize(p1 -p2);//float3(0,0,1);
    fUp = -normalize(cross(p4-p3, fFwd)) * (_LeftOrRight);

}

void lesserCoverts( uint id , out float3 fPos , out float3 fFwd , out float3 fUp , out float3 fScale ){
   
    int bID = id - _NumPrimaryFeathers - _NumPrimaryCoverts;

    int row = bID % _NumLesserCovertRows;
    int col = bID / _NumLesserCovertRows;

   float rowVal = float(row)/float(_NumLesserCovertRows);
   float colVal = float(col)/float(_NumLesserCovertCols);

  // offset!
  rowVal += colVal * .5;
  rowVal %= 1;

   rowVal *= .95;
   rowVal += .025;

   
   colVal *= .5;
   colVal += .025;

   lesserCovertsCubic(rowVal, colVal, fPos , fFwd , fUp);

   //rowVal += colVal * .5;
   //rowVal %= 1;

  
    fScale = ( colVal * 2 + 3 ) * .5;// float3(1,1 ,1);//float3(1,1,1);
    fScale  += _BaseNoiseScale * snoise(fPos * _BaseNoiseSize);


       
    fFwd += cross( fUp , fFwd ) * _BaseDirectionLeftRightNoise * (hash(float(id * 10))-.5);
    fFwd += fUp  * _BaseDirectionUpNoise * ((hash(float(id * 30)))-.5);
    fFwd += fUp  * _BackAmountOverlapping;



    // flipping to right side
    //fUp = -normalize(cross(fFwd,fUp));
    fScale  = (.5-abs(rowVal-.5)) * _MiddleCovertsFeatherScaleMultiplier + _BaseCovertsFeatherScale;// (1  * (1-lerpVal) + 4 ) ;
    fScale  += _BaseNoiseScale * (hash(float(int(id)* 50)) - .5);// snoise(fPos * _BaseNoiseSize);

}

