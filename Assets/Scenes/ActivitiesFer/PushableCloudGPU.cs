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

    public float playerForwardAmount = 1;
    

    // public Vector3 PlayerPosition { get { return debug || !God.wren ? transform.position + Vector3.forward * Mathf.Sin(Time.time * .5f) * 20 : God.wren.physics.transform.position; }}
    // public Vector3 PlayerForward { get { return debug || !God.wren ? Vector3.forward : God.wren.physics.transform.forward; }}

    public Vector3 PlayerPosition { get { return player.position; }}
    public Vector3 PlayerForward { get { return player.forward; }}

    [System.Serializable]
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

    [SerializeField] BigParticle[] _savedParticles;

    BigParticle[] bigParticles;
    TinyParticle[] tinyParticles;
    Matrix4x4[] particleMatrices;
    Matrix4x4[] tinyParticlesMatrices;

    ComputeBuffer particleBuffer;

    [Range(-1,1)] public float startSize;



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
        bigParticles = new BigParticle[particleCount];
        particleMatrices = new Matrix4x4[particleCount];
        tinyParticlesMatrices = new Matrix4x4[particleCount * PARTICLES_PER_CLOUD];

        if (!TryLoadParticles())
        {
            for (int i = 0; i < particleCount; i++)
            {
                bigParticles[i].position = bigParticles[i].startPosition = transform.position + new Vector3(Random.Range(-areaSize.x, areaSize.x), Random.Range(-areaSize.y, areaSize.y), Random.Range(-areaSize.z, areaSize.z)) * .5f;
                bigParticles[i].velocity = Random.onUnitSphere * .1f;
                bigParticles[i].life = Random.value;
                foreach(var b in GetComponentsInChildren<PushableCloudGPUBrush>())
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
    void PushParticle(int i, PushableCloudGPUBrush brush)
    {
        var data = brush.brushData;
        var pos = brush.transform.position + brush.transform.forward * data.forwardAmount;
        var dtp = Vector3.Distance(bigParticles[i].position, pos);
        if (dtp < brush.Radius)
        {
            bigParticles[i].velocity += (bigParticles[i].position - pos).normalized * data.pushForce;
            bigParticles[i].life = 1;
        }

        // apply torque force
        var d = bigParticles[i].position - pos;
        var f = Vector3.Cross(d, brush.transform.forward) * data.vortexForce;
        f *= Mathf.Lerp(0, 1, 1 - dtp / brush.Radius / 1.5f);
        bigParticles[i].position += f * Time.deltaTime;
    }

    private void UpdateParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            bigParticles[i].life -= Time.deltaTime / 20f;

            foreach(var b in GetComponentsInChildren<PushableCloudGPUBrush>())
            {
                if (b.brushData.hole && b.brushData.holeConstant)
                    HoleParticle(i, b);

                if (b.brushData.pushForce < 0 || b.brushData.pushForce > 0)
                    PushParticle(i, b);
            }

            bigParticles[i].velocity *= 1 - dampen * Time.deltaTime;
            bigParticles[i].position += bigParticles[i].velocity * Time.deltaTime;

            var s = bigParticleSize * Mathf.Clamp(startSize + bigParticles[i].size, 0, float.MaxValue);
            
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
            if (returnToOriginalShape && bigParticles[i].life < 0)
            {
                bigParticles[i].position = Vector3.Lerp(bigParticles[i].position, bigParticles[i].startPosition, -bigParticles[i].life);
                if (bigParticles[i].life < -1)
                    bigParticles[i].life = 1;
            }
        }
    }

    void UpdateBuffers()
    {
        particleBuffer.SetData(bigParticles);
        particleMaterial.SetBuffer("particleBuffer", particleBuffer);

        int li = 1;
        foreach(var b in GetComponentsInChildren<PushableCloudGPUBrush>())
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
        if (renderTinyParticles)
            Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, tinyParticlesMatrices);
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
}
