

float4x4 GetInverse(float4x4 a)
{
  float  s0 = a[0, 0] * a[1, 1] - a[1, 0] * a[0, 1];
  float  s1 = a[0, 0] * a[1, 2] - a[1, 0] * a[0, 2];
  float  s2 = a[0, 0] * a[1, 3] - a[1, 0] * a[0, 3];
  float  s3 = a[0, 1] * a[1, 2] - a[1, 1] * a[0, 2];
  float  s4 = a[0, 1] * a[1, 3] - a[1, 1] * a[0, 3];
  float  s5 = a[0, 2] * a[1, 3] - a[1, 2] * a[0, 3];

  float  c5 = a[2, 2] * a[3, 3] - a[3, 2] * a[2, 3];
  float  c4 = a[2, 1] * a[3, 3] - a[3, 1] * a[2, 3];
  float  c3 = a[2, 1] * a[3, 2] - a[3, 1] * a[2, 2];
  float  c2 = a[2, 0] * a[3, 3] - a[3, 0] * a[2, 3];
  float  c1 = a[2, 0] * a[3, 2] - a[3, 0] * a[2, 2];
  float  c0 = a[2, 0] * a[3, 1] - a[3, 0] * a[2, 1];

  // Should check for 0 determinant
  float  invdet = 1.0 / (s0 * c5 - s1 * c4 + s2 * c3 + s3 * c2 - s4 * c1 + s5 * c0);

  float4x4 b;

  b[0, 0] = ( a[1, 1] * c5 - a[1, 2] * c4 + a[1, 3] * c3) * invdet;
  b[0, 1] = (-a[0, 1] * c5 + a[0, 2] * c4 - a[0, 3] * c3) * invdet;
  b[0, 2] = ( a[3, 1] * s5 - a[3, 2] * s4 + a[3, 3] * s3) * invdet;
  b[0, 3] = (-a[2, 1] * s5 + a[2, 2] * s4 - a[2, 3] * s3) * invdet;

  b[1, 0] = (-a[1, 0] * c5 + a[1, 2] * c2 - a[1, 3] * c1) * invdet;
  b[1, 1] = ( a[0, 0] * c5 - a[0, 2] * c2 + a[0, 3] * c1) * invdet;
  b[1, 2] = (-a[3, 0] * s5 + a[3, 2] * s2 - a[3, 3] * s1) * invdet;
  b[1, 3] = ( a[2, 0] * s5 - a[2, 2] * s2 + a[2, 3] * s1) * invdet;

  b[2, 0] = ( a[1, 0] * c4 - a[1, 1] * c2 + a[1, 3] * c0) * invdet;
  b[2, 1] = (-a[0, 0] * c4 + a[0, 1] * c2 - a[0, 3] * c0) * invdet;
  b[2, 2] = ( a[3, 0] * s4 - a[3, 1] * s2 + a[3, 3] * s0) * invdet;
  b[2, 3] = (-a[2, 0] * s4 + a[2, 1] * s2 - a[2, 3] * s0) * invdet;

  b[3, 0] = (-a[1, 0] * c3 + a[1, 1] * c1 - a[1, 2] * c0) * invdet;
  b[3, 1] = ( a[0, 0] * c3 - a[0, 1] * c1 + a[0, 2] * c0) * invdet;
  b[3, 2] = (-a[3, 0] * s3 + a[3, 1] * s1 - a[3, 2] * s0) * invdet;
  b[3, 3] = ( a[2, 0] * s3 - a[2, 1] * s1 + a[2, 2] * s0) * invdet;

  return b;
}



// Function to compute the inverse of a 4x4 matrix
float4x4 InverseMatrix(float4x4 m)
{
  float4x4 inv;

  // Calculate the determinant
  float det = 
  m._m00 * (m._m11 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m12 * (m._m21 * m._m33 - m._m23 * m._m31) + m._m13 * (m._m21 * m._m32 - m._m22 * m._m31)) -
  m._m01 * (m._m10 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m12 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m13 * (m._m20 * m._m32 - m._m22 * m._m30)) +
  m._m02 * (m._m10 * (m._m21 * m._m33 - m._m23 * m._m31) - m._m11 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m13 * (m._m20 * m._m31 - m._m21 * m._m30)) -
  m._m03 * (m._m10 * (m._m21 * m._m32 - m._m22 * m._m31) - m._m11 * (m._m20 * m._m32 - m._m22 * m._m30) + m._m12 * (m._m20 * m._m31 - m._m21 * m._m30));

  // Check if determinant is 0 (matrix is singular)
  if (det == 0.0)
  {
    // Return a zero matrix or identity matrix in case of a non-invertible matrix
    return float4x4(0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0);
  }

  // Compute the inverse (using adjugate and determinant)
  float invDet = 1.0 / det;

  inv._m00 = (m._m11 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m12 * (m._m21 * m._m33 - m._m23 * m._m31) + m._m13 * (m._m21 * m._m32 - m._m22 * m._m31)) * invDet;
  inv._m01 = -(m._m01 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m02 * (m._m21 * m._m33 - m._m23 * m._m31) + m._m03 * (m._m21 * m._m32 - m._m22 * m._m31)) * invDet;
  inv._m02 = (m._m01 * (m._m12 * m._m33 - m._m13 * m._m32) - m._m02 * (m._m11 * m._m33 - m._m13 * m._m31) + m._m03 * (m._m11 * m._m32 - m._m12 * m._m31)) * invDet;
  inv._m03 = -(m._m01 * (m._m12 * m._m23 - m._m13 * m._m22) - m._m02 * (m._m11 * m._m23 - m._m13 * m._m21) + m._m03 * (m._m11 * m._m22 - m._m12 * m._m21)) * invDet;

  inv._m10 = -(m._m10 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m12 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m13 * (m._m20 * m._m32 - m._m22 * m._m30)) * invDet;
  inv._m11 = (m._m00 * (m._m22 * m._m33 - m._m23 * m._m32) - m._m02 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m03 * (m._m20 * m._m32 - m._m22 * m._m30)) * invDet;
  inv._m12 = -(m._m00 * (m._m12 * m._m33 - m._m13 * m._m32) - m._m02 * (m._m10 * m._m33 - m._m13 * m._m30) + m._m03 * (m._m10 * m._m32 - m._m12 * m._m30)) * invDet;
  inv._m13 = (m._m00 * (m._m12 * m._m23 - m._m13 * m._m22) - m._m02 * (m._m10 * m._m23 - m._m13 * m._m20) + m._m03 * (m._m10 * m._m22 - m._m12 * m._m20)) * invDet;

  inv._m20 = (m._m10 * (m._m21 * m._m33 - m._m23 * m._m31) - m._m11 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m13 * (m._m20 * m._m31 - m._m21 * m._m30)) * invDet;
  inv._m21 = -(m._m00 * (m._m21 * m._m33 - m._m23 * m._m31) - m._m01 * (m._m20 * m._m33 - m._m23 * m._m30) + m._m03 * (m._m20 * m._m31 - m._m21 * m._m30)) * invDet;
  inv._m22 = (m._m00 * (m._m11 * m._m33 - m._m13 * m._m31) - m._m01 * (m._m10 * m._m33 - m._m13 * m._m30) + m._m03 * (m._m10 * m._m31 - m._m11 * m._m30)) * invDet;
  inv._m23 = -(m._m00 * (m._m11 * m._m23 - m._m13 * m._m21) - m._m01 * (m._m10 * m._m23 - m._m13 * m._m20) + m._m03 * (m._m10 * m._m21 - m._m11 * m._m20)) * invDet;

  inv._m30 = -(m._m10 * (m._m21 * m._m32 - m._m22 * m._m31) - m._m11 * (m._m20 * m._m32 - m._m22 * m._m30) + m._m12 * (m._m20 * m._m31 - m._m21 * m._m30)) * invDet;
  inv._m31 = (m._m00 * (m._m21 * m._m32 - m._m22 * m._m31) - m._m01 * (m._m20 * m._m32 - m._m22 * m._m30) + m._m02 * (m._m20 * m._m31 - m._m21 * m._m30)) * invDet;
  inv._m32 = -(m._m00 * (m._m11 * m._m32 - m._m12 * m._m31) - m._m01 * (m._m10 * m._m32 - m._m12 * m._m30) + m._m02 * (m._m10 * m._m31 - m._m11 * m._m30)) * invDet;
  inv._m33 = (m._m00 * (m._m11 * m._m22 - m._m12 * m._m21) - m._m01 * (m._m10 * m._m22 - m._m12 * m._m20) + m._m02 * (m._m10 * m._m21 - m._m11 * m._m20))  * invDet;



  return inv;
}




