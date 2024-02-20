using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WrenUtils;


// make an editor script
#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(PushableCloudGPU))]
public class PushableCloudGPUEditor : Editor
{

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PushableCloudGPU myScript = (PushableCloudGPU)target;
        if (GUILayout.Button("Reset"))
        {
            myScript.InitializeParticles();
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
        PushableCloudGPU myScript = (PushableCloudGPU)target;
        Handles.color = Color.red;
        // Handles.DrawWireDisc(myScript.PlayerPosition, Vector3.up, myScript.playerPushRadius);
    }
}
#endif

[ExecuteAlways()]
public class PushableCloudGPU : MonoBehaviour
{
    public bool debug = false;
    public bool renderTinyParticles = false;

    [SerializeField] int particleCount = 1000;
    [SerializeField] Mesh particleMesh;
    [SerializeField] float meshSize = 1;
    [SerializeField] Material particleMaterial;
    [SerializeField] Vector3 areaSize = new Vector3(1, 1, 1);
    [SerializeField] float lifetime = 5;
    [SerializeField] float bigParticleSize = 1;
    [SerializeField] float tinyParticleSize = .4f;
    [SerializeField] AnimationCurve sizeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [Header("Transforms")]
    [SerializeField] Transform player;
    [SerializeField] Transform light1;
    [SerializeField] float light1radius = 43;


    public float playerPushRadius = 5;
    public float playerForwardAmount = 1;
    
    [Range(0,1)] public float playerFwdVortexForce = 1;

    // public Vector3 PlayerPosition { get { return debug || !God.wren ? transform.position + Vector3.forward * Mathf.Sin(Time.time * .5f) * 20 : God.wren.physics.transform.position; }}
    // public Vector3 PlayerForward { get { return debug || !God.wren ? Vector3.forward : God.wren.physics.transform.forward; }}

    public Vector3 PlayerPosition { get { return player.position; }}
    public Vector3 PlayerForward { get { return player.forward; }}

    struct BigParticle {
        public Vector3 startPosition;
        public Vector3 position;
        public Vector3 velocity;
        public float size;
        public float life;
    }
    
    struct TinyParticle {
        public Vector3 position;
        public Vector3 velocity;
        public float size;
        public float life;
    }

    BigParticle[] bigParticles;
    TinyParticle[] tinyParticles;
    Matrix4x4[] particleMatrices;
    Matrix4x4[] tinyParticlesMatrices;

    ComputeBuffer particleBuffer;

    const int PARTICLES_PER_CLOUD = 10;

    void OnEnable()
    {
        InitializeParticles();
        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * 11);
    }
    void OnDisable()
    {
        particleBuffer.Release();
    }

    void Start()
    {
        InitializeParticles();
        UpdateBuffers();
    }

    private void Update()
    {
        // light1.position = transform.position + new Vector3(
        //     Mathf.Sin(Time.time * .5f) * 20,
        //     Mathf.Cos(Time.time * .5f) * 20,
        //     0
        // );

        UpdateParticles();
        UpdateBuffers();
        RenderParticles();
    }

    public void InitializeParticles()
    {
        bigParticles = new BigParticle[particleCount];
        particleMatrices = new Matrix4x4[particleCount];
        tinyParticlesMatrices = new Matrix4x4[particleCount * PARTICLES_PER_CLOUD];

        for (int i = 0; i < particleCount; i++)
        {
            bigParticles[i].position = bigParticles[i].startPosition = transform.position + new Vector3(Random.Range(-areaSize.x, areaSize.x), Random.Range(-areaSize.y, areaSize.y), Random.Range(-areaSize.z, areaSize.z)) * .5f;
            bigParticles[i].velocity = Random.onUnitSphere * .1f;
            bigParticles[i].size = 1;
            bigParticles[i].life = Random.value;

        }
    }

    void PushParticle(int i, Vector3 pp)
    {
        var dtp = Vector3.Distance(bigParticles[i].position, pp);
        if (dtp < playerPushRadius)
        {
            float pushForce = .4f;
            bigParticles[i].velocity += (bigParticles[i].position - pp).normalized * pushForce;
            bigParticles[i].life = 1;
        }

        // apply torque force
        var d = bigParticles[i].position - pp;
        var f = Vector3.Cross(d, PlayerForward) * playerFwdVortexForce;
        f *= Mathf.Lerp(0, 1, 1 - dtp / playerPushRadius / 1.5f);
        bigParticles[i].position += f * Time.deltaTime;
    }

    private void UpdateParticles()
    {
        var pp = PlayerPosition + PlayerForward * playerForwardAmount;
        var lp = light1.position;
        for (int i = 0; i < particleCount; i++)
        {
            bigParticles[i].life -= Time.deltaTime / 20f;

            PushParticle(i, pp);
            PushParticle(i, lp);

            float dampen = 2.0f;
            bigParticles[i].velocity *= 1 - dampen * Time.deltaTime;

            // vortex

            


            bigParticles[i].position += bigParticles[i].velocity * Time.deltaTime;
            var s = bigParticleSize * bigParticles[i].size;
            
            particleMatrices[i].SetTRS(bigParticles[i].position, Quaternion.identity, Vector3.one * s * meshSize);

            if (renderTinyParticles)
            {
                for (int j = 0; j < PARTICLES_PER_CLOUD; j++)
                {
                    Random.InitState(i + j * 1000);
                    var pos = bigParticles[i].position + Random.onUnitSphere * s *.5f;
                    var t = (Random.value + Time.time * ( 1 / lifetime)) % 1;
                    pos += Random.onUnitSphere * t * 2.5f;
                    var ns = sizeCurve.Evaluate(t) * tinyParticleSize;
                    tinyParticlesMatrices[i * PARTICLES_PER_CLOUD + j].SetTRS(pos, Quaternion.identity, Vector3.one * ns * 50);
                }
            }


            // GEt back go start position
            // if (bigParticles[i].life < 0)
            // {
            //     bigParticles[i].position = Vector3.Lerp(bigParticles[i].position, bigParticles[i].startPosition, -bigParticles[i].life);
            //     if (bigParticles[i].life < -1)
            //         bigParticles[i].life = 1;
            // }
        }
    }

    void UpdateBuffers()
    {
        particleBuffer.SetData(bigParticles);
        particleMaterial.SetBuffer("particleBuffer", particleBuffer);
        particleMaterial.SetVector("_Light1", new Vector4(light1.position.x, light1.position.y, light1.position.z, light1radius));
    }

    private void RenderParticles()
    {
        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, particleMatrices);
        if (renderTinyParticles)
            Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, tinyParticlesMatrices);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, areaSize);
        
        Gizmos.DrawWireSphere(PlayerPosition, 1);
        Gizmos.DrawWireSphere(PlayerPosition + PlayerForward * playerForwardAmount, 1);
        Gizmos.DrawWireSphere(PlayerPosition + PlayerForward * playerForwardAmount, playerPushRadius);
        
        Gizmos.DrawWireSphere(light1.position, light1radius);
    }
    
}
