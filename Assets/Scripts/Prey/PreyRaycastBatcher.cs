using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;

// Batches every prey's ground (down) + obstacle (forward) raycast into ONE
// RaycastCommand job that runs across worker threads, instead of 2 blocking
// Physics.Raycast calls per bird on the main thread.
//
// Pattern: schedule-early / complete-late.
//   • Each frame we Complete() the job scheduled LAST frame (it had the whole
//     frame to finish on worker threads, so Complete usually returns instantly),
//     scatter its results into each bird, then build + schedule THIS frame's job.
//   • Birds therefore read ground/forward data that is 1 frame old. That is
//     invisible here because PreyController already smooths it with
//     physicsInfoLerpSpeed.
//
// Zero config: the first bird to register lazily creates this object. Runs at
// a very negative execution order so its Update happens before any bird Update.
[DefaultExecutionOrder( -200 )]
public class PreyRaycastBatcher : MonoBehaviour
{
    private static PreyRaycastBatcher _instance;
    private static bool _quitting;

    public static bool HasInstance => _instance != null;

    public static PreyRaycastBatcher Instance
    {
        get {
            if ( _instance == null && !_quitting && Application.isPlaying ) {
                var go = new GameObject( "PreyRaycastBatcher" );
                _instance = go.AddComponent<PreyRaycastBatcher>();
            }
            return _instance;
        }
    }

    // birds currently wanting batched rays
    private readonly List<PreyController> _registered = new List<PreyController>();

    // snapshot of the birds whose rays are in-flight this frame (index → bird)
    private PreyController[] _scheduledBirds = new PreyController[0];
    private int _scheduledCount;

    // persistent native buffers (2 rays per bird: down at 2i, forward at 2i+1)
    private NativeArray<RaycastCommand> _commands;
    private NativeArray<RaycastHit>     _results;
    private int       _capacity;       // in rays
    private JobHandle _handle;
    private bool      _hasPending;

    private static readonly QueryParameters RayParams =
        new QueryParameters( Physics.DefaultRaycastLayers , false , QueryTriggerInteraction.UseGlobal , false );

    public void Register( PreyController bird )
    {
        if ( bird == null || _registered.Contains( bird ) ) return;
        _registered.Add( bird );
        // sensible defaults so the bird's first frame (before any scatter) isn't garbage
        bird.batchedDown    = new Vector4( 0 , 1 , 0 , 0 );
        bird.batchedForward = new Vector4( 0 , 1 , 0 , 0 );
    }

    public void Unregister( PreyController bird )
    {
        if ( bird != null ) _registered.Remove( bird );
    }

    private void Awake()
    {
        if ( _instance != null && _instance != this ) { Destroy( this ); return; }
        _instance = this;
    }

    private void OnApplicationQuit() => _quitting = true;

    private void Update()
    {
        long t0 = PreyProfiler.Now;

        // 1) finish last frame's job and push its hits back onto the birds
        if ( _hasPending ) {
            _handle.Complete();
            Scatter();
            _hasPending = false;
        }

        // 2) build this frame's batch from the live bird set
        int n = _registered.Count;
        if ( n > 0 ) {
            EnsureCapacity( n * 2 );
            BuildCommands( n );
            var cmds = _commands.GetSubArray( 0 , n * 2 );
            var res  = _results.GetSubArray( 0 , n * 2 );
            _handle  = RaycastCommand.ScheduleBatch( cmds , res , 8 );
            _hasPending  = true;
            _scheduledCount = n;
            PreyProfiler.batchedRaycastCount += n * 2;
        }

        PreyProfiler.batchTicks += PreyProfiler.Now - t0;
    }

    private void BuildCommands( int n )
    {
        if ( _scheduledBirds.Length < n ) _scheduledBirds = new PreyController[n];

        for ( int i = 0; i < n; i++ ) {
            var bird = _registered[i];
            _scheduledBirds[i] = bird;

            if ( bird == null || bird.parameters == null ) {
                // dead/uninitialised slot → harmless zero-distance no-op rays
                _commands[2 * i]     = new RaycastCommand( Vector3.zero , Vector3.down , RayParams , 0f );
                _commands[2 * i + 1] = new RaycastCommand( Vector3.zero , Vector3.down , RayParams , 0f );
                continue;
            }

            var t   = bird.transform;
            var pos = t.position;
            float down = bird.parameters.distance.maxDownDistance;
            float fwd  = bird.parameters.distance.maxForwardDistance;

            _commands[2 * i]     = new RaycastCommand( pos , -t.up      , RayParams , down );
            _commands[2 * i + 1] = new RaycastCommand( pos ,  t.forward , RayParams , fwd  );
        }

        // any stale tail slots from a previous larger batch are ignored (we only
        // schedule the first n*2), so no cleanup needed.
    }

    private void Scatter()
    {
        for ( int i = 0; i < _scheduledCount; i++ ) {
            var bird = _scheduledBirds[i];
            if ( bird == null || bird.parameters == null ) continue;

            // ── down ─────────────────────────────────────────────────────────
            var dh   = _results[2 * i];
            float minY = bird.parameters.altitude.minimumTotalY;
            float py   = bird.transform.position.y;
            if ( dh.normal.sqrMagnitude > 1e-6f ) {
                float d = dh.point.y < minY ? py - minY : dh.distance;
                bird.batchedDown = new Vector4( dh.normal.x , dh.normal.y , dh.normal.z , d );
            } else {
                bird.batchedDown = new Vector4( 0 , 1 , 0 , py - minY );
            }

            // ── forward ──────────────────────────────────────────────────────
            var fh = _results[2 * i + 1];
            if ( fh.normal.sqrMagnitude > 1e-6f ) {
                bird.batchedForward = new Vector4( fh.normal.x , fh.normal.y , fh.normal.z , fh.distance );
            } else {
                bird.batchedForward = new Vector4( 0 , 1 , 0 , bird.parameters.distance.maxForwardDistance );
            }
        }
    }

    private void EnsureCapacity( int rays )
    {
        if ( _commands.IsCreated && _capacity >= rays ) return;

        // previous job already Completed in Update step 1, so it's safe to realloc
        if ( _commands.IsCreated ) _commands.Dispose();
        if ( _results.IsCreated  ) _results.Dispose();

        _capacity = Mathf.Max( 64 , Mathf.NextPowerOfTwo( rays ) );
        _commands = new NativeArray<RaycastCommand>( _capacity , Allocator.Persistent );
        _results  = new NativeArray<RaycastHit>(     _capacity , Allocator.Persistent );
    }

    private void OnDisable()  => DisposeAll();
    private void OnDestroy()
    {
        DisposeAll();
        if ( _instance == this ) _instance = null;
    }

    private void DisposeAll()
    {
        if ( _hasPending ) { _handle.Complete(); _hasPending = false; }
        if ( _commands.IsCreated ) _commands.Dispose();
        if ( _results.IsCreated  ) _results.Dispose();
    }
}
