
    // Sampling our triplanar texture for our 'sketch' shader
    float3 _TriplanarMultiplier;
    float _TriplanarSharpness;
  sampler2D _TriplanarMap;
  sampler2D _TriplanarNormalMap;

    float4 triplanarSample(float3 p , float3 n){

        half3 blend = pow(abs(n),_TriplanarSharpness) ;;
        
        // make sure the weights sum up to 1 (divide by sum of x+y+z)
        blend /= dot(blend,1.0);
        // read the three texture projections, for x,y,z axes
        fixed4 cx = tex2D(_TriplanarMap,(p.yz * _TriplanarMultiplier.x ) % 1);
        fixed4 cy = tex2D(_TriplanarMap,(p.xz * _TriplanarMultiplier.y ) % 1);
        fixed4 cz = tex2D(_TriplanarMap,(p.xy * _TriplanarMultiplier.z ) % 1);
        // blend the textures based on weights
        fixed4 c = cx * blend.x + cy * blend.y + cz * blend.z;
        return c;

    }



    // Trying to get a normal map triplanr
    float3 triplanarNormal(float3 p , float3 n, float3 t0,float3 t1,float3 t2){


        half3 blend =  pow(abs(n),_TriplanarSharpness) ;
        // make sure the weights sum up to 1 (divide by sum of x+y+z)
        blend /= dot(blend,1.0);
        // read the three texture projections, for x,y,z axes
        half3 cx = UnpackNormal(tex2D(_TriplanarNormalMap,(p.yz * _TriplanarMultiplier.x) % 1));
        half3 cy = UnpackNormal(tex2D(_TriplanarNormalMap,(p.xz * _TriplanarMultiplier.y) % 1));
        half3 cz = UnpackNormal(tex2D(_TriplanarNormalMap,(p.xy * _TriplanarMultiplier.z) % 1));
        // blend the textures based on weights
        half3 c = cx * blend.x + cy * blend.y + cz * blend.z;

        half3 worldNormal;
        worldNormal.x = dot(t0, c);
        worldNormal.y = dot(t1, c);
        worldNormal.z = dot(t2, c);
        return c;
         


    }