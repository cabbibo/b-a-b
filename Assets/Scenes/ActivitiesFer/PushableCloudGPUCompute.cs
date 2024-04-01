using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WrenUtils;

// make an editor script
#if UNITY_EDITOR

using UnityEditor.Experimental.GraphView;
using UnityEditor;
[CustomEditor(typeof(PushableCloudGPUCompute))]
public class PushableCloudGPUComputeEditor : Editor
{

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PushableCloudGPUCompute myScript = (PushableCloudGPUCompute)target;
        if (GUILayout.Button("Reset"))
        {
            myScript.ResetParticles();
        }
        var s = SceneView.lastActiveSceneView.sceneViewState;
        s.alwaysRefresh = true;
        SceneView.lastActiveSceneView.sceneViewState = s;
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        PushableCloudGPUCompute myScript = (PushableCloudGPUCompute)target;
        Handles.color = Color.red;
        // Handles.DrawWireDisc(myScript.PlayerPosition, Vector3.up, myScript.playerPushRadius);
    }
}
#endif

[ExecuteAlways()]
public class PushableCloudGPUCompute : MonoBehaviour
{
    public bool debug = false;

    [SerializeField] int particleCount = 1000;
    [SerializeField] Mesh particleMesh;
    [SerializeField] float meshSize = 1;
    [SerializeField] Material particleMaterial;
    [SerializeField] Vector3 areaSize = new Vector3(1, 1, 1);
    [SerializeField] float lifetime = 5;
    [SerializeField] float bigParticleSize = 1;
    [SerializeField] float dampen = 2;

    [System.Serializable]
    public class BrushData
    {
        public enum Type { Player, Light, Hole }
        public Type type;

        [Header("Physics")]
        public float pushForce = .4f;
        [Range(0,1)] public float vortexForce = 1;
        public float forwardAmount = 0f;

        [Header("Hole")]
        public bool hole;
        public bool holeConstant = false;
        [Range(0,1)] public float holeFalloff = 1;
        [Range(-1,1)] public float sizeDelta = 0;

        [Header("Light")]
        public bool light;
        public float lightRadius = 20;
    }

    struct BigParticle {
        public Vector3 startPosition;
        public Vector3 position;
        public Vector3 velocity;
        public float size;
        public float life;
    }
    

    BigParticle[] bigParticles;
    Matrix4x4[] particleMatrices;

    ComputeBuffer particleBuffer;
    ComputeBuffer brushesBuffer;
    ComputeBuffer particleMatricesBuffer;
    private ComputeBuffer argsBuffer;

    [Range(-1,1)] public float startSize;

    public ComputeShader computeShader;

    const int PARTICLES_PER_CLOUD = 10;

    void OnEnable()
    {
        Initialize();
    }
    void OnDisable()
    {
        particleBuffer.Release();
    }

    void Start()
    {
        Initialize();
        UpdateBuffers();
    }

    internal void ResetParticles()
    {
        Initialize();
    }

    private void Update()
    {
        DispatchUpdateCompute();
        UpdateBuffers();
        RenderParticles();
    }

    public void Initialize()
    {
        bigParticles = new BigParticle[particleCount];
        particleMatrices = new Matrix4x4[particleCount];

        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * 11);
        brushesBuffer = new ComputeBuffer(particleCount, sizeof(float) * 14);
        particleMatricesBuffer = new ComputeBuffer(particleCount, 16 * 4);

        // Initialize argsBuffer
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)particleMesh.GetIndexCount(0);
        args[1] = (uint)particleMatrices.Length;
        args[2] = (uint)particleMesh.GetIndexStart(0);
        args[3] = (uint)particleMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        DispatchInitializeCompute();
    }

    #region Compute

    void DispatchInitializeCompute()
    {
        int kernelID = computeShader.FindKernel("InitializeParticle");

        computeShader.SetBuffer(kernelID, "bigParticles", particleBuffer);
        computeShader.SetBuffer(kernelID, "brushes", brushesBuffer);
        computeShader.SetBuffer(kernelID, "particleMatrices", particleMatricesBuffer);

        computeShader.Dispatch(kernelID, (particleMatrices.Length + 255) / 256, 1, 1);
    }

    void DispatchUpdateCompute()
    {
        computeShader.SetFloat("_Time", Time.time);

        int kernelID = computeShader.FindKernel("UpdateParticle");

        computeShader.SetBuffer(kernelID, "bigParticles", particleBuffer);
        computeShader.SetBuffer(kernelID, "brushes", brushesBuffer);
        computeShader.SetBuffer(kernelID, "particleMatrices", particleMatricesBuffer);

        computeShader.Dispatch(kernelID, (particleMatrices.Length + 255) / 256, 1, 1);

        particleMatricesBuffer.GetData(particleMatrices);
        particleBuffer.GetData(bigParticles);
    }

    #endregion

    void UpdateBuffers()
    {
        particleBuffer.SetData(bigParticles);
        particleMaterial.SetBuffer("particleBuffer", particleBuffer);
    }

    private void RenderParticles()
    {
        particleMaterial.SetBuffer("particleBuffer", particleBuffer);
        particleMaterial.SetBuffer("particleMatrices", particleMatricesBuffer);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMaterial, new Bounds(transform.position, areaSize), argsBuffer);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
    
}
