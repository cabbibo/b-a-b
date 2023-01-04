using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;
using static Unity.Mathematics.math;
using Unity.Mathematics;



[ExecuteInEditMode]

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Curve))]
public class RibbonAlongCurve : MonoBehaviour
{

    public bool flipRighthandedness;

    public bool twoSided;
    public Curve curve;
    public int lengthSegments = 50;
    public int widthSegments = 8;
    public float width = 1;
    public float verticalOffset;


    Vector3[] positions;
    Vector3[] normals;
    Vector4[] tangents;
    Vector2[] uvs;
    int[] triangles;

    public int totalVertCount;
    public int totalTriCount;

    MeshFilter filter;

    public void OnEnable(){
        filter = GetComponent<MeshFilter>();
        curve = GetComponent<Curve>();
        curve.BakeChanged.AddListener(BuildMesh);

    }

    public void OnDisable(){
        curve.BakeChanged.AddListener(BuildMesh);
    }



    void BuildMesh(Curve c){
        
        totalVertCount = lengthSegments * widthSegments;
        totalTriCount = (lengthSegments-1) * (widthSegments-1) * 3 * 2;

        if( twoSided ){
            totalTriCount *= 2;
            totalVertCount *= 2;
        }

        positions = new Vector3[totalVertCount];
        normals = new Vector3[totalVertCount];
        tangents = new Vector4[totalVertCount];
        uvs = new Vector2[totalVertCount];
        triangles = new int[totalTriCount];

        // Building the triangles first
        int index  = 0;
        for( int i = 0; i < lengthSegments-1; i++){
            for( int j = 0; j < widthSegments-1; j++ ){
                

                // Getting indicies to build a tube
                int baseID = widthSegments * i + j;
                int id1 = baseID;
                int id2 = baseID + 1;
                int id3 = baseID + widthSegments;
                int id4 = baseID + widthSegments + 1;


                if(!flipRighthandedness ){
                    triangles[index++] = id1;
                    triangles[index++] = id2;
                    triangles[index++] = id4;
                    triangles[index++] = id1;
                    triangles[index++] = id4;
                    triangles[index++] = id3;
                }else{
                    triangles[index++] = id1;
                    triangles[index++] = id4;
                    triangles[index++] = id2;
                    triangles[index++] = id1;
                    triangles[index++] = id3;
                    triangles[index++] = id4;
                }

                

                if( twoSided ){
                    triangles[index++] = id1 + (totalVertCount/2);
                    triangles[index++] = id4 + (totalVertCount/2);
                    triangles[index++] = id2 + (totalVertCount/2);
                    triangles[index++] = id1 + (totalVertCount/2);
                    triangles[index++] = id3 + (totalVertCount/2);
                    triangles[index++] = id4 + (totalVertCount/2);
                }

            }
        }

        // reset index of array
        index  = 0;

        float3 startPos = transform.InverseTransformPoint(curve.GetPositionFromValueAlongCurve( 0 ));
        for( int i = 0; i < lengthSegments; i++){
            
            float lengthAlongTube = (float)i/(lengthSegments-1);
            float3 centerPos = curve.GetPositionFromValueAlongCurve( lengthAlongTube );
            float3 forward = curve.GetForwardFromValueAlongCurve(lengthAlongTube);

            float3 p1;
            float3 f1;
            float3 u1;
            float3 r1;
            float s1;
           curve.GetDataFromValueAlongCurve( lengthAlongTube  , out p1,out f1,out u1,out  r1,out s1);
            for( int j = 0; j < widthSegments; j++ ){
                float widthness = (float)j / ((float)widthSegments-1);
               
                float w = curve.GetWidthFromValueAlongCurve(lengthAlongTube);
                float3 fPos = curve.GetOffsetPositionFromValueAlongCurve( lengthAlongTube , ((float)widthness-.5f)*w*width, verticalOffset );
                float3 normal = u1;//curve.GetUpFromLengthAlongCurve(lengthAlongTube);

                float4 tangent = float4(normalize(r1),1);//float4(cross(normal,forward),1);
                float2 uv = float2( lengthAlongTube, widthness);
                if( i < 10  && j== 0){
                   // print(normal);
                }
                positions[index] = transform.InverseTransformPoint(fPos);
                tangents[index] = float4(normalize(transform.InverseTransformDirection(tangent.xyz)),1);

                 if( i < 10  && j== 0){
                  //  print(transform.InverseTransformDirection(normal));
                }
                normals[index] = normalize(transform.InverseTransformDirection(normal));


                   if( i < 10 && j== 0 ){
//                    print(normals[index]);
                }
                /*if( i < 10 ){
                    print(tangents[index]);
                }*/
                uvs[ index] = uv;

                if( twoSided ){
                    positions[index + totalVertCount/2] = transform.InverseTransformPoint(fPos);
                    tangents[index + totalVertCount/2] = float4(normalize(transform.InverseTransformDirection(tangent.xyz)),1);
                    normals[index + totalVertCount/2] = normalize(-transform.InverseTransformDirection(normal));
                    uvs[ index + totalVertCount/2] = uv;
                }

                index++;

            }
        }


        Mesh m = new Mesh();

        
        m.Clear();

        m.vertices = positions;
        m.tangents = tangents;
        m.normals = normals;
        m.uv = uvs;
        m.triangles = triangles;

        filter.mesh = m;





    }

}