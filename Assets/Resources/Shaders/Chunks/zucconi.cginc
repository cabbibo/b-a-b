 float3 bump3y (float3 x, float3 yoffset)
 {
     float3 y = 1 - x * x;
     y = saturate(y-yoffset);
     return y;
 }


float3 zucconi(float x)
 {
     x %=1;
     const float3 cs = float3(3.54541723, 2.86670055, 2.29421995);
     const float3 xs = float3(0.69548916, 0.49416934, 0.28269708);
     const float3 ys = float3(0.02320775, 0.15936245, 0.53520021);
     return bump3y (    cs * (x - xs), ys);
 }

