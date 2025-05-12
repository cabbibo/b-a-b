float2 rotateUV(float2 uv, float rotation)
{
  float mid = 0.5;
  return float2(
      cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
      cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
  );
}

float2 rotateUV(float2 uv, float rotation, float2 mid)
{
  return float2(
    cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
    cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
  );
}

float2 rotateUV(float2 uv, float rotation, float mid)
{
  return float2(
    cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
    cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
  );
}