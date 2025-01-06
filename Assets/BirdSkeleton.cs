using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BirdSkeleton : MonoBehaviour
{

    public FullBird fullBird;
    public DebugHierarchy debugHierarchy;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private int instanceCount = 0;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer argsBuffer;

    void OnEnable()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        instanceCount = debugHierarchy.connections.Length;
        UpdateBuffers();
    }
    void UpdateBuffers()
    {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);



        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }



    // Update is called once per frame
    void LateUpdate()
    {


        if (debugHierarchy.enabled == false)
        {
            debugHierarchy.UpdateMatrices();
        }
        else
        {

        }
        instanceCount = debugHierarchy.connections.Length;

        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        instanceMaterial.SetInt("_Count", debugHierarchy.connections.Length);
        instanceMaterial.SetBuffer("_TransformBuffer", debugHierarchy._buffer);
        instanceMaterial.SetBuffer("_ConnectionBuffer", debugHierarchy._connectionsBuffer);

        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f)), argsBuffer);

    }
}
