using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WrenUtils;

// make an editor script
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof(PushableCloudGPUCompute) )]
public class PushableCloudGPUComputeEditor : Editor
{
    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (PushableCloudGPUCompute)target;

        if ( GUILayout.Button( "Reset" ) ) {
            myScript.ResetParticles();
        }

        var s = SceneView.lastActiveSceneView.sceneViewState;
        s.alwaysRefresh = true;
        SceneView.lastActiveSceneView.sceneViewState = s;
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI( SceneView sceneView )
    {
        var myScript = (PushableCloudGPUCompute)target;
        Handles.color = Color.red;
        // Handles.DrawWireDisc(myScript.PlayerPosition, Vector3.up, myScript.playerPushRadius);
    }
}
#endif

[ExecuteAlways()]
public class PushableCloudGPUCompute : MonoBehaviour
{
    public bool debug = false;

    [SerializeField]
    private int particleCount = 1000;

    [SerializeField]
    private Mesh particleMesh;

    [SerializeField]
    private float meshSize = 1;

    [SerializeField]
    private Material particleMaterial;

    [SerializeField]
    private Vector3 areaSize = new(1 , 1 , 1);

    [SerializeField]
    private float lifetime = 5;

    [SerializeField]
    private float bigParticleSize = 1;

    [SerializeField]
    private float dampen = 2;

    [System.Serializable]
    public class BrushData
    {
        public enum Type
        {
            Player ,
            Light ,
            Hole
        }

        public Type type;

        [Header( "Physics" )]
        public float pushForce = .4f;

        [Range( 0 , 1 )]
        public float vortexForce = 1;

        public float forwardAmount = 0f;

        [Header( "Hole" )]
        public bool hole;

        public bool holeConstant = false;

        [Range( 0 , 1 )]
        public float holeFalloff = 1;

        [Range( -1 , 1 )]
        public float sizeDelta = 0;

        [Header( "Light" )]
        public bool light;

        public float lightRadius = 20;
    }

    private struct BigParticle
    {
        public Vector3 startPosition;
        public Vector3 position;
        public Vector3 velocity;
        public float   size;
        public float   life;
    }


    private BigParticle[] bigParticles;
    private Matrix4x4[]   particleMatrices;

    private ComputeBuffer particleBuffer;
    private ComputeBuffer brushesBuffer;
    private ComputeBuffer particleMatricesBuffer;
    private ComputeBuffer argsBuffer;

    [Range( -1 , 1 )]
    public float startSize;

    public ComputeShader computeShader;

    private const int PARTICLES_PER_CLOUD = 10;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        particleBuffer.Release();
    }

    private void Start()
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

        particleBuffer = new ComputeBuffer( particleCount , sizeof(float) * 11 );
        brushesBuffer = new ComputeBuffer( particleCount , sizeof(float) * 14 );
        particleMatricesBuffer = new ComputeBuffer( particleCount , 16 * 4 );

        // Initialize argsBuffer
        uint[] args = new uint[5] { 0 , 0 , 0 , 0 , 0 };
        args[0] = (uint)particleMesh.GetIndexCount( 0 );
        args[1] = (uint)particleMatrices.Length;
        args[2] = (uint)particleMesh.GetIndexStart( 0 );
        args[3] = (uint)particleMesh.GetBaseVertex( 0 );
        argsBuffer = new ComputeBuffer( 1 , args.Length * sizeof(uint) , ComputeBufferType.IndirectArguments );
        argsBuffer.SetData( args );

        DispatchInitializeCompute();
    }

    #region Compute

    private void DispatchInitializeCompute()
    {
        int kernelID = computeShader.FindKernel( "InitializeParticle" );

        computeShader.SetBuffer( kernelID , "bigParticles" , particleBuffer );
        computeShader.SetBuffer( kernelID , "brushes" , brushesBuffer );
        computeShader.SetBuffer( kernelID , "particleMatrices" , particleMatricesBuffer );

        computeShader.Dispatch( kernelID , (particleMatrices.Length + 255) / 256 , 1 , 1 );
    }

    private void DispatchUpdateCompute()
    {
        computeShader.SetFloat( "_Time" , Time.time );

        int kernelID = computeShader.FindKernel( "UpdateParticle" );

        computeShader.SetBuffer( kernelID , "bigParticles" , particleBuffer );
        computeShader.SetBuffer( kernelID , "brushes" , brushesBuffer );
        computeShader.SetBuffer( kernelID , "particleMatrices" , particleMatricesBuffer );

        computeShader.Dispatch( kernelID , (particleMatrices.Length + 255) / 256 , 1 , 1 );

        particleMatricesBuffer.GetData( particleMatrices );
        particleBuffer.GetData( bigParticles );
    }

    #endregion

    private void UpdateBuffers()
    {
        particleBuffer.SetData( bigParticles );
        particleMaterial.SetBuffer( "particleBuffer" , particleBuffer );
    }

    private void RenderParticles()
    {
        particleMaterial.SetBuffer( "particleBuffer" , particleBuffer );
        particleMaterial.SetBuffer( "particleMatrices" , particleMatricesBuffer );
        Graphics.DrawMeshInstancedIndirect( particleMesh , 0 , particleMaterial , new Bounds( transform.position , areaSize ) ,
            argsBuffer );
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube( transform.position , areaSize );
    }
}