using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class DebugHierarchy : MonoBehaviour
{

    // debug buffer for a hierachy
    public ComputeBuffer _buffer;
    public ComputeBuffer _connectionsBuffer;


    public Transform[] transforms;
    public Matrix4x4[] matrices;
    public Vector2[] connections;

    public MaterialPropertyBlock mpb;
    public MaterialPropertyBlock matrixMPB;

    public Material connectionsMaterial;
    public Material matrixMaterial;


    public LineRenderer lineRenderer;


    public bool debugConnections;
    public bool debugBasis;
    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public void SetUpConnections()
    {

        if (debugConnections || debugBasis)
        {

            lineRenderer.positionCount = transforms.Length;
            for (int i = 0; i < transforms.Length; i++)
            {
                lineRenderer.SetPosition(i, transforms[i].position);
            }

            UpdateMatrices();

            if (_buffer == null)
            {
                return;
            }




            _connectionsBuffer.SetData(connections);



        }

    }

    void LateUpdate()
    {


        SetUpConnections();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }


        if (matrixMPB == null)
        {
            matrixMPB = new MaterialPropertyBlock();
        }

        if (debugConnections)
        {

            mpb.SetInt("_Count", connections.Length);
            mpb.SetBuffer("_TransformBuffer", _buffer);
            mpb.SetBuffer("_ConnectionBuffer", _connectionsBuffer);
            Graphics.DrawProcedural(connectionsMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, connections.Length * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
        }

        if (debugBasis)
        {
            matrixMPB.SetInt("_Count", transforms.Length);
            matrixMPB.SetBuffer("_TransformBuffer", _buffer);
            Graphics.DrawProcedural(matrixMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, transforms.Length * 3 * 2 * 3, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
        }
    }




    void OnEnable()
    {

        List<Transform> transformsList = new List<Transform>();
        List<Vector2> relationsList = new List<Vector2>();

        ExtractTransforms(transform, transformsList, relationsList);

        transforms = transformsList.ToArray();
        connections = relationsList.ToArray();

        matrices = new Matrix4x4[transforms.Length];


        _buffer = new ComputeBuffer(transforms.Length, sizeof(float) * 16);
        _connectionsBuffer = new ComputeBuffer(connections.Length, sizeof(float) * 2);

        UpdateMatrices();
        _connectionsBuffer.SetData(connections);


    }

    public void UpdateMatrices()
    {

        for (int i = 0; i < transforms.Length; i++)
        {
            matrices[i] = transforms[i].localToWorldMatrix;
        }

        _buffer.SetData(matrices);

    }

    void ExtractTransforms(Transform parent, List<Transform> transformsList, List<Vector2> relationsList)
    {
        int parentIndex = AddTransform(parent, transformsList);
        foreach (Transform child in parent)
        {
            int childIndex = AddTransform(child, transformsList);
            relationsList.Add(new Vector2(parentIndex, childIndex));
            ExtractTransforms(child, transformsList, relationsList);
        }
    }

    int AddTransform(Transform transform, List<Transform> transformsList)
    {
        if (!transformsList.Contains(transform))
        {
            transformsList.Add(transform);
        }
        return transformsList.IndexOf(transform);
    }
}
