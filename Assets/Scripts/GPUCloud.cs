using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUCloud : MonoBehaviour
{

    public ComputeBuffer posBuffer;

    public ComputeBuffer cloudBuffer;
    public ComputeShader shader;

    public Mesh mesh;


    public void OnEnable()
    {

        // buffer = new ComputeBuffer(mesh.vertices.Length, sizeof(float) * 3);
        cloudBuffer = new ComputeBuffer(mesh.vertices.Length, sizeof(float) * 16);

        //pos
        //vel
        //nor
        //tan
        //uv
        //debug


        //buffer.SetData(mesh.vertices);




    }


    // Update is called once per frame
    void Update()
    {

    }





}
