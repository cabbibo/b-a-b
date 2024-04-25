using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WrenUtils;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;


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
            myScript.ResetParticles();
        }
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load"))
            {
                myScript.TryLoadParticles();
            }
            if (GUILayout.Button("Save"))
            {
                myScript.SaveParticles();
            }
            if (GUILayout.Button("Clear"))
            {
                myScript.ClearSavedParticle();
            }
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

    public bool updateNearBird = true;

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
    [SerializeField] float dampen = 2;
    [SerializeField] bool returnToOriginalShape = true;

    [System.Serializable]
    public struct BrushData
    {
        public enum Type { Player, Light, Hole }
        public Type type;

        [Header("Physics")]
        public float pushForce;
        [Range(0, 1)] public float vortexForce;
        public float forwardAmount;

        [Header("Hole")]
        public bool hole;
        public bool holeConstant;
        [Range(0, 1)] public float holeFalloff;
        [Range(-1, 1)] public float sizeDelta;

        [Header("Light")]
        public bool light;
        public float lightRadius;

        // Dynamic
        internal Vector3 position;
        internal float radius;
        internal Vector3 forward;

        // generate a constructor
        public BrushData(Type type)
        {
            this.type = type;
            pushForce = .4f;
            vortexForce = 1;
            forwardAmount = 0f;
            hole = false;
            holeConstant = false;
            holeFalloff = 1;
            sizeDelta = 0;
            light = false;
            lightRadius = 20;

            position = Vector3.zero;
            radius = 1;
            forward = Vector3.forward;
        }
    }

    public float playerForwardAmount = 1;

    public Vector3 PlayerPosition { get { return player.position; } }
    public Vector3 PlayerForward { get { return player.forward; } }

    [System.Serializable]
    struct BigParticle
    {
        public Vector3 startPosition;
        public Vector3 position;
        public Vector3 velocity;
        public float size;
        public float life;
    }

    [SerializeField] BigParticle[] _savedParticles;

    BigParticle[] bigParticles;
    Matrix4x4[] particleMatrices;

    ComputeBuffer particleBuffer;

    [Range(-1, 1)] public float startSize;

    PushableCloudGPUBrush[] brushes;

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

    internal void ResetParticles()
    {
        InitializeParticles();
        UpdateParticles();
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
        brushes = GetComponentsInChildren<PushableCloudGPUBrush>();

        bigParticles = new BigParticle[particleCount];
        particleMatrices = new Matrix4x4[particleCount];

        if (!TryLoadParticles())
        {
            for (int i = 0; i < particleCount; i++)
            {
                bigParticles[i].position = bigParticles[i].startPosition = transform.position + new Vector3(Random.Range(-areaSize.x, areaSize.x), Random.Range(-areaSize.y, areaSize.y), Random.Range(-areaSize.z, areaSize.z)) * .5f;
                bigParticles[i].velocity = Random.onUnitSphere * .1f;
                bigParticles[i].life = Random.value;
                foreach (var b in brushes)
                {
                    if (b.brushData.hole)
                        HoleParticle(i, b);
                }
            }
        }
    }

    void HoleParticle(int i, PushableCloudGPUBrush brush)
    {
        var d = Vector3.Distance(bigParticles[i].position, brush.transform.position);
        var inside = d < brush.Radius;
        if (!inside) return;
        bigParticles[i].size = Mathf.Lerp(brush.brushData.sizeDelta, 0, ((d / brush.Radius) - brush.brushData.holeFalloff) / (1 - brush.brushData.holeFalloff));
    }

    private void UpdateParticles()
    {

        NativeArray<Matrix4x4> particleMatricesNative = new NativeArray<Matrix4x4>(bigParticles.Length, Allocator.TempJob);
        NativeArray<BigParticle> bigParticlesNative = new NativeArray<BigParticle>(bigParticles, Allocator.TempJob);
        NativeArray<BrushData> brushesNative = new NativeArray<BrushData>(brushes.Length, Allocator.TempJob);

        for (int i = 0; i < brushes.Length; i++)
        {
            brushesNative[i] = brushes[i].brushData;
        }

        for (int i = 0; i < brushes.Length; i++)
        {
            BrushJob j = new BrushJob
            {
                bigParticles = bigParticlesNative,
                brush = brushesNative[i],
                deltaTime = Time.deltaTime,
                numBrushes = brushes.Length
            };
            JobHandle h = j.Schedule(bigParticles.Length, 64);
            h.Complete();
            bigParticlesNative.CopyTo(bigParticles);
        }

        UpdateParticlesJob job = new UpdateParticlesJob
        {
            particleMatrices = particleMatricesNative,
            bigParticles = bigParticlesNative,
            brushes = brushesNative,
            meshSize = meshSize,
            startSize = startSize,
            bigParticleSize = bigParticleSize,
            dampen = dampen,
            returnToOriginalShape = returnToOriginalShape,

            deltaTime = Time.deltaTime,
            numBrushes = brushes.Length
        };
        JobHandle handle = job.Schedule(bigParticles.Length, 64);

        handle.Complete();
        bigParticlesNative.CopyTo(bigParticles);
        particleMatricesNative.CopyTo(particleMatrices);

        particleMatricesNative.Dispose();
        bigParticlesNative.Dispose();
        brushesNative.Dispose();
    }

    void UpdateBuffers()
    {


        particleBuffer.SetData(bigParticles);
        particleMaterial.SetBuffer("particleBuffer", particleBuffer);

        int li = 1;
        foreach (var b in brushes)
        {
            if (b.brushData.light)
            {
                particleMaterial.SetVector("_Light" + li, new Vector4(b.transform.position.x, b.transform.position.y, b.transform.position.z, b.brushData.lightRadius));
                li++;
            }
        }
        // particleMaterial.SetVector("_Light1", new Vector4(light1.position.x, light1.position.y, light1.position.z, light1radius));
    }

    private void RenderParticles()
    {
        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, particleMatrices);
        // RenderParams rp = new RenderParams(particleMaterial);
        // rp.worldBounds = new Bounds(transform.position, areaSize);
        // rp.matProps = new MaterialPropertyBlock();
        // Graphics.RenderMeshPrimitives(rp, particleMesh, 0, particleCount);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, areaSize);

        Gizmos.DrawWireSphere(PlayerPosition, 1);
        Gizmos.DrawWireSphere(PlayerPosition + PlayerForward * playerForwardAmount, 1);
        // Gizmos.DrawWireSphere(PlayerPosition + PlayerForward * playerForwardAmount, playerPushRadius);
    }

    // Editor

    internal void SaveParticles()
    {
        _savedParticles = new BigParticle[bigParticles.Length];
        for (int i = 0; i < bigParticles.Length; i++)
        {
            _savedParticles[i] = bigParticles[i];
            _savedParticles[i].startPosition = bigParticles[i].startPosition;
        }
    }
    internal void ClearSavedParticle()
    {
        _savedParticles = null;
    }
    internal bool TryLoadParticles()
    {
        // copy the array
        if (_savedParticles == null || _savedParticles.Length == 0) return false;
        particleCount = _savedParticles.Length;
        bigParticles = new BigParticle[_savedParticles.Length];
        for (int i = 0; i < bigParticles.Length; i++)
        {
            bigParticles[i] = _savedParticles[i];
            bigParticles[i].startPosition = _savedParticles[i].position;
        }
        return true;
    }

    // Job

    struct BrushJob : IJobParallelFor
    {
        public NativeArray<BigParticle> bigParticles;
        [ReadOnly]
        public BrushData brush;
        [ReadOnly]
        public float deltaTime;
        [ReadOnly]
        public float numBrushes;

        public void Execute(int i)
        {
            BigParticle particle = bigParticles[i];

            // Hole
            if (brush.hole && brush.holeConstant)
            {
                var d = Vector3.Distance(particle.position, brush.position);
                var inside = d < brush.radius;
                if (inside)
                    particle.size = Mathf.Lerp(brush.sizeDelta, 0, ((d / brush.radius) - brush.holeFalloff) / (1 - brush.holeFalloff));
            }

            // Push
            if (brush.pushForce < 0 || brush.pushForce > 0)
            {
                var data = brush;
                var pos = brush.position + brush.forward * data.forwardAmount;
                var dtp = Vector3.Distance(particle.position, pos);
                if (dtp < brush.radius)
                {
                    particle.velocity += (particle.position - pos).normalized * data.pushForce;
                    particle.life = 1;
                }

                // apply torque force
                var d = particle.position - pos;
                var f = Vector3.Cross(d, brush.forward) * data.vortexForce;
                f *= Mathf.Lerp(0, 1, 1 - dtp / brush.radius / 1.5f);
                particle.position += f * deltaTime;
            }

            bigParticles[i] = particle;
        }
    }
    struct UpdateParticlesJob : IJobParallelFor
    {
        public NativeArray<Matrix4x4> particleMatrices;
        public NativeArray<BigParticle> bigParticles;
        public NativeArray<BrushData> brushes;
        [ReadOnly]
        public float meshSize;
        [ReadOnly]
        public float startSize;
        [ReadOnly]
        public float bigParticleSize;
        [ReadOnly]
        public float dampen;
        [ReadOnly]
        public bool returnToOriginalShape;

        [ReadOnly]
        public float deltaTime;
        [ReadOnly]
        public float numBrushes;

        public void Execute(int i)
        {
            BigParticle particle = bigParticles[i];

            particle.life -= deltaTime / 20f;

            particle.velocity *= 1 - dampen * deltaTime;
            particle.position += particle.velocity * deltaTime;

            // GEt back go start position
            if (returnToOriginalShape && particle.life < 0)
            {
                particle.position = Vector3.Lerp(particle.position, particle.startPosition, -particle.life);
                if (particle.life < -1)
                    particle.life = 1;
            }

            var s = bigParticleSize * Mathf.Clamp(startSize + particle.size, 0, float.MaxValue);

            particleMatrices[i] = Matrix4x4.TRS(particle.position, Quaternion.identity, Vector3.one * s * meshSize);

            bigParticles[i] = particle;
        }
    }

}
