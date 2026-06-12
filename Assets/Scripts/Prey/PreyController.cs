using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using WrenUtils;
using Random = UnityEngine.Random;

public struct PreyForce
{
    public Vector3 force;
    public Color   color;
    public string  name;

    public PreyForce( Vector3 f , Color c , string n ) { force = f; color = c; name = n; }
}

public class PreyController : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────
    public PreyManager  manager;
    public PreyConfigSO parameters;
    public float        spawnTime;
    public Vector3      spawnPoint;
    public bool         spawning;

    // true while the bird is fading out (DestroyCoroutine running) — used by PreyManager's WhenFull logic
    public bool IsDespawning => isDespawning;

    // ── Observable data (shown in inspector for debugging) ────────────────────
    [Header( "State" )]
    public PreyState state;

    public Transform CurrentPerchTarget => perchState.target;

    // Land-claim queries (used by Field perches for spacing against other birds)
    public bool IsClaimingLand => state == PreyState.Landing || state == PreyState.Perched
        || (state == PreyState.Searching && (perchState.fieldLanding || perchState.target != null));
    public Vector3 ClaimedLandPosition => perchState.fieldLanding ? perchState.landPos
        : (perchState.target != null ? perchState.target.position : position);

    public float life;

    [Header( "Physics Data" )]
    public float distanceToGround;

    public float   rawDistanceToGround;
    public float   distanceToForward;
    public float   rawDistanceToForward;
    public Vector3 groundNormal;
    public Vector3 rawGroundNormal;
    public Vector3 forwardNormal;
    public Vector3 rawForwardNormal;
    public Vector3 vectorToWren;

    [System.NonSerialized]
    public float forwardTurnNormalizedValue;

    [System.NonSerialized]
    public float downTurnNormalizedValue;

    [Header( "Movement" )]
    public float stamina;
    public float positionInFlapCycle;

    public Vector3 flapValue;
    public float   climbRate;
    public Vector3 force;
    public Vector3 velocity;
    public Vector3 desiredVelocity;
    public Vector3 position;
    public Vector3 oldVelocity;

    // ── Debug ─────────────────────────────────────────────────────────────────
    [Header( "Debug" )]
    public bool stepThrough = false;
    public bool stepForward = false;

    public List<PreyForce> allForces = new List<PreyForce>();

    // Borrowed from PreyFocusLinePool only while this bird is within focusRadius
    // of the wren; null otherwise. No per-bird LineRenderer/Material allocation.
    private LineRenderer focusLine;
    // Lazily created on first use — must NOT be a field initializer: MaterialPropertyBlock's
    // ctor calls Unity native code, which Unity forbids during MonoBehaviour construction.
    private static MaterialPropertyBlock _focusMPB;

    // ── Private physics ───────────────────────────────────────────────────────
    private Vector3 startPosition;
    private Vector4 rayCastData;

    // Filled by PreyRaycastBatcher when batched raycasting is active. Same layout
    // as RaycastDown()/RaycastForward() returns: (normal.xyz, distance).
    [HideInInspector] public Vector4 batchedDown;
    [HideInInspector] public Vector4 batchedForward;
    private bool _batchRegistered;

    // Spline-force cache: the expensive SplineUtility.GetNearestPoint search is
    // refreshed at most every SplineQueryInterval (or after moving far enough),
    // not every frame. The pull is still recomputed each frame from the cached
    // nearest point against the live position, so it stays responsive.
    private const float SplineQueryInterval = 0.15f;
    private float   _splineQueryTimer;
    private Vector3 _splineNearestWorld;
    private Vector3 _splineTangentWorld;
    private Vector3 _splineQueryPos;
    private bool    _splineCached;

    private int     frame;

    // ── LOD (distance tick-striding) ──────────────────────────────────────────
    // PreyManager sets lodStride each frame from this bird's distance to the player.
    // stride 1 = full simulation every frame (unchanged behavior). stride > 1 = run
    // the expensive sim (forces, state, raycasts, collision) only every Nth frame and
    // cheaply dead-reckon position on the frames in between. simDt carries the real
    // elapsed time for the current full tick so all timers stay correct regardless of
    // stride (it replaces Time.deltaTime everywhere in the per-tick code).
    public  int     lodStride = 1;
    private float   simDt;
    private float   _simDtAccum;
    private int     _framesSinceFull;

    private float   noiseOffset;
    private float   currentBank;
    private float   currentSpeed;
    private float   timeOutsideRegion;
    private bool    isOutsideRegion;
    private bool    isDespawning;

    // ── Bounce calm behavior: settle-in-place state (modules.bounce) ──────────
    private bool    bounceSettled;        // pinned where it landed, waiting to relaunch
    private float   bounceSettleTimer;    // seconds elapsed since settling
    private float   bounceSettleDuration; // rolled timeToRemainSettled (+variance) for this settle

    private int   ambientFlapsInBurst = 0;
    private float ambientGlideTimer   = 0f;
    private float simTimeAccum        = 0f;
    private float _flapSpeedMult      = 1f;
    private Vector3 smoothedForce;     // force eased toward the active state's force (state-change blend)

    // ── Runtime state (one per module that needs per-instance state) ──────────
    private class PerchRuntimeState
    {
        public Transform target;        // discrete perch spot (OnCollider / InArea)
        public bool      fieldLanding;  // true → use landPos/landNormal (Field subtype, runtime-computed)
        public Vector3   landPos;
        public Vector3   landNormal;
        public float     perchedTimer;
        public float     currentPerchDuration;
        public float     landingBlend;   // 0 = full calm forces, 1 = full aim forces
        public bool      isDiving;       // true once we've reached approach height — no more upward correction
        public float     spiralAngle;    // accumulated angle for the Spiral approach style

        public void Init( PreyPerchModule cfg )
        {
            target               = null;
            fieldLanding         = false;
            landPos              = Vector3.zero;
            landNormal           = Vector3.up;
            perchedTimer         = 0;
            currentPerchDuration = 0;
            landingBlend         = 0f;
            isDiving             = false;
            spiralAngle          = 0f;
        }
    }

    private class SocialPressureState
    {
        public float desireToTakeOff;
        public float desireToDisturb;
        public float desireToCalmDown;
        public float desireToLand;
        public float sampleTimer;

        public string lastTriggerLabel = "";
        public float  lastTriggerTime  = -999f;

        public struct Influence {
            public PreyController bird;
            public float          weight;
            public PreyState      sourceState;
        }
        public readonly List<Influence> influences = new();

        public void Decay( float rate , float dt ) {
            float d = rate * dt;
            desireToTakeOff  = Mathf.Max( 0 , desireToTakeOff  - d );
            desireToDisturb  = Mathf.Max( 0 , desireToDisturb  - d );
            desireToCalmDown = Mathf.Max( 0 , desireToCalmDown - d );
            desireToLand     = Mathf.Max( 0 , desireToLand     - d );
        }
    }
    private SocialPressureState socialState = new();

    private class TakeOffRuntimeState
    {
        public Vector3 origin;        // where the bird took off from (forces end once far enough from here)
        public Vector3 runDirection;
    }

    private class FlockRuntimeState
    {
        public List<PreyController> neighbors = new();
        public Vector3              flockCenter;
        public Vector3              avgVelocity;
        public float                queryTimer;
    }

    private class SplineRuntimeState
    {
        public PreySpline spline;
        public float      currentT;
    }

    private class UpdraftRuntimeState
    {
        public PreyInterestPoint activeTarget;  // the searched-to point we are currently riding
        public float             remainTimer;   // how long we've ridden activeTarget
    }

    // removed — thermal soaring is now handled via interest points
    // private class ThermalRuntimeState
    // {
    //     public float circleAngle;
    //     public bool  isThermaling;
    // }

    private class RunRuntimeState
    {
        public float   calmTimer;
        public float   jukeTimer;
        public Vector3 jukeDir;
    }

    private class SearchRuntimeState
    {
        public PreyInterestPoint currentTarget;
        public Vector3           targetOffset;     // stable random scatter for this search
        public float             calmTimer;
        public float             nextSearchTime;
        public float             searchTimer;
        public float             arrivedTimer;
        public float             searchCooldown;   // blocks all searching after takeoff / forced calm
        public int               newInterestCount; // chained NewInterest hops since the last full calm

        public void Init( PreySearchModule cfg )
        {
            currentTarget    = null;
            calmTimer        = 0f;
            searchTimer      = 0f;
            arrivedTimer     = 0f;
            searchCooldown   = 0f;
            newInterestCount = 0;
            nextSearchTime = cfg.calmBeforeSearch
                             + Random.Range( -cfg.calmBeforeSearchVariance , cfg.calmBeforeSearchVariance );
        }
    }

    private PerchRuntimeState   perchState   = new();
    private TakeOffRuntimeState takeOffState = new();
    private FlockRuntimeState   flockState   = new();
    private SplineRuntimeState  splineState  = new();
    private UpdraftRuntimeState updraftState = new();
    // private ThermalRuntimeState thermalState = new();   // removed (thermal via interest points)
    private RunRuntimeState     runState     = new();
    private SearchRuntimeState  searchState  = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    public void OnEnable()
    {
        // Re-join the manager's tick loop if this bird was toggled off then on.
        // (manager is null on the very first enable, before Initialize runs — that
        // first registration is handled in Initialize. RegisterBird dedupes.)
        if ( manager != null ) manager.RegisterBird( this );
    }

    public void OnDisable()
    {
        if ( _batchRegistered ) {
            if ( PreyRaycastBatcher.HasInstance ) PreyRaycastBatcher.Instance.Unregister( this );
            _batchRegistered = false;
        }

        if ( manager != null ) manager.UnregisterBird( this );

        ReleaseFocusLine();
    }

    // Driven once per frame by PreyManager.TickBirds() — a single loop over all of
    // a manager's birds, instead of every bird carrying its own MonoBehaviour.Update
    // (which costs a managed→native call per object before any work happens).
    // FrameGate + profiler timing are handled by the manager around the loop.
    public void Tick()
    {
        bool stepMode = stepThrough || (manager != null && manager.stepThrough);
        bool slowMo   = manager != null && manager.simulationSpeed < 1f;

        // Debug stepping, slow-mo and the spawn/despawn fade always run at full rate
        // so nothing visible to the player is ever dead-reckoned.
        int stride = (spawning || stepMode || slowMo) ? 1 : Mathf.Max( 1 , lodStride );

        if ( stride <= 1 ) {
            simDt            = Time.deltaTime;   // identical to pre-LOD behavior
            _simDtAccum      = 0f;
            _framesSinceFull = 0;
            UpdateBird();
            return;
        }

        // Far bird: bank the elapsed time and only run the full sim every `stride`
        // frames; advance position cheaply on the frames in between.
        _simDtAccum += Time.deltaTime;
        _framesSinceFull++;
        if ( _framesSinceFull < stride ) {
            DeadReckon();
            return;
        }

        simDt            = _simDtAccum;
        _simDtAccum      = 0f;
        _framesSinceFull = 0;
        UpdateBird();
    }

    // Cheap between-tick step for strided (far) birds: keep gliding along the last
    // computed velocity so motion stays smooth, without recomputing forces, state,
    // raycasts or collision. flapValue is reused from the last full tick.
    private void DeadReckon()
    {
        position          += velocity;
        transform.position = position + flapValue;
    }

    private void UpdateBird()
    {
        UpdateFocusLine();

        if ( stepThrough || (manager != null && manager.stepThrough) ) {
            if ( !stepForward ) return;
            stepForward = false;
        } else {
            float simSpeed = manager != null ? manager.simulationSpeed : 1f;
            if ( simSpeed < 1f ) {
                simTimeAccum += simDt;
                float interval = (1f / 60f) / simSpeed;
                if ( simTimeAccum < interval ) return;
                simTimeAccum = 0f;
            }
        }

        UpdateVectorToWren();
        UpdateData();
        UpdateModuleStates();
        UpdateStamina();
        UpdateState();
        DoPhysics();
        CheckForDespawn();

        if ( velocity.sqrMagnitude > 0.0001f ) {
            var fwd = velocity.normalized;
            float targetBank = Vector3.Cross( oldVelocity.normalized , fwd ).y * parameters.turning.bankStrength;
            currentBank = Mathf.Lerp( currentBank , targetBank , parameters.turning.bankSmoothing );
            var right = Vector3.Cross( Vector3.up , fwd ).normalized;
            var bankUp = (Vector3.up + right * currentBank).normalized;
            transform.LookAt( position + flapValue + fwd * 10 , bankUp );
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────────────────────────────────────────

    public void Initialize( PreyConfigSO config , PreyManager mgr )
    {
        parameters = config;
        manager    = mgr;

        // seed per-instance randomness
        noiseOffset = Random.Range( 0f , 100f );

        if ( parameters.modules.perch ) {
            perchState.Init( parameters.perch );
        }

        if ( parameters.modules.search ) {
            searchState.Init( parameters.search );
        }

        if ( parameters.modules.social )
            socialState.sampleTimer = Random.Range( 0f , parameters.social.sampleInterval );

        // NOTE: spawn position (incl. altitude) is decided by PreyManager's spawn type.
        // Do NOT override Y here — SetHeight() used to re-snap to a random altitude and
        // throw away spline / desired-altitude / in-distance placement.

        stamina           = parameters.modules.sprint ? parameters.sprint.maxStamina : 0f;
        timeOutsideRegion = 0f;
        isOutsideRegion   = false;
        isDespawning      = false;
        ambientFlapsInBurst = 0;
        ambientGlideTimer   = Random.Range( parameters.flap.glideTimeMin , parameters.flap.glideTimeMax );
        force             = Vector3.zero;
        smoothedForce     = Vector3.zero;
        frame = Random.Range( 0 , parameters.physics.physicsResolution );
        enabled = true;
        spawnPoint = transform.position;
        startPosition = transform.position;
        position = startPosition;
        currentSpeed = parameters.movement.desiredSpeed;
        velocity = Random.insideUnitSphere.normalized * currentSpeed;
        oldVelocity = velocity;
        spawnTime = Time.time;
        life = 0;
        state = PreyState.Calm;

        // join the batched-raycast system (lazily creates the batcher if needed)
        if ( !_batchRegistered ) {
            PreyRaycastBatcher.Instance.Register( this );
            _batchRegistered = true;
        }

        // join the manager's tick loop (the manager drives Tick() for all its birds)
        if ( manager != null ) manager.RegisterBird( this );

        // focus line is now borrowed on demand from PreyFocusLinePool (see UpdateFocusLine) —
        // no per-bird LineRenderer/Material allocated here anymore.

        StartCoroutine( SpawnCoroutine( config.animation.spawnSpeed ) );
        OnInitialize();
    }

    protected virtual void OnInitialize()
    {
    }

    // unused — spawn position (incl. altitude) is decided by PreyManager's spawn type now.
    // public void SetHeight()
    // {
    //     if ( Physics.Raycast( transform.position , -transform.up , out var hit , 100000 ) ) {
    //         transform.position = hit.point + transform.up *
    //             Mathf.Lerp( parameters.altitude.minAltitude , parameters.altitude.maxAltitude , Random.value );
    //         position = transform.position;
    //     }
    // }

    // ─────────────────────────────────────────────────────────────────────────
    // Wren / data updates
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateFocusLine()
    {
        if ( parameters == null ) return;

        var wren = God.wren != null         ? God.wren.transform
                 : manager?.debugWren != null ? manager.debugWren.transform
                 : null;

        bool inFocus = wren != null &&
            Vector3.Distance( transform.position , wren.position ) <= parameters.crystals.focusRadius;

        if ( !inFocus ) { ReleaseFocusLine(); return; }

        // borrow a pooled line on the frame we enter focus (lazily creates the pool)
        if ( focusLine == null ) focusLine = PreyFocusLinePool.Instance.Acquire();
        if ( focusLine == null ) return; // focus shader missing

        focusLine.SetPosition( 0 , transform.position );
        focusLine.SetPosition( 1 , wren.position );

        // per-bird shader values via a shared property block — no material instance
        if ( _focusMPB == null ) _focusMPB = new MaterialPropertyBlock();
        focusLine.GetPropertyBlock( _focusMPB );
        _focusMPB.SetVector( "_BirdPos"   , transform.position );
        _focusMPB.SetFloat(  "_EatRadius" , parameters.crystals.eatRadius );
        focusLine.SetPropertyBlock( _focusMPB );
    }

    private void ReleaseFocusLine()
    {
        if ( focusLine == null ) return;
        if ( PreyFocusLinePool.HasInstance ) PreyFocusLinePool.Instance.Release( focusLine );
        focusLine = null;
    }

    private void UpdateVectorToWren()
    {
        var wren = God.wren != null ? God.wren.transform
            : manager.debugWren != null ? manager.debugWren.transform
            : null;
        vectorToWren = wren != null ? wren.position - transform.position : Vector3.one * 9999f;
    }

    // ── Predictive startle ─────────────────────────────────────────────────────
    // Effective startle reach = base + how far the wren travels toward us in `leadTime` seconds.
    // A fast head-on approach spooks earlier; perpendicular/receding stays at the base radius.
    private Vector3 WrenVelocity()
    {
        if ( God.wren != null && God.wren.physics != null && God.wren.physics.rb != null )
            return God.wren.physics.rb.velocity;
        return Vector3.zero;   // debug wren has no rigidbody → falls back to the plain ring
    }

    private float EffectiveStartle( float baseRadius , float leadTime )
    {
        var v      = WrenVelocity();
        var toPrey = -vectorToWren;                          // vectorToWren is prey → wren
        if ( leadTime <= 0f || v.sqrMagnitude < 1e-4f || toPrey.sqrMagnitude < 1e-4f ) return baseRadius;
        float closing = Vector3.Dot( v , toPrey.normalized );   // + when the wren heads toward us
        return baseRadius + Mathf.Max( 0f , closing ) * leadTime;
    }

    private bool WrenWithinStartle( float baseRadius , float leadTime )
        => vectorToWren.magnitude < EffectiveStartle( baseRadius , leadTime );

    public void UpdateData()
    {
        frame++;

        if ( frame % parameters.physics.physicsResolution == 0 ) {
            if ( _batchRegistered && PreyRaycastBatcher.HasInstance ) {
                // batched path: PreyRaycastBatcher filled these from a worker-thread job
                rawDistanceToGround  = batchedDown.w;
                rawGroundNormal      = new Vector3( batchedDown.x , batchedDown.y , batchedDown.z );
                rawDistanceToForward = batchedForward.w;
                rawForwardNormal     = new Vector3( batchedForward.x , batchedForward.y , batchedForward.z );
            } else {
                // fallback: synchronous main-thread raycasts (no batcher in scene)
                rayCastData = RaycastDown();
                rawDistanceToGround = rayCastData.w;
                rawGroundNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );

                rayCastData = RaycastForward();
                rawDistanceToForward = rayCastData.w;
                rawForwardNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );
            }
        }

        distanceToGround = Mathf.Lerp( distanceToGround , rawDistanceToGround , parameters.physics.physicsInfoLerpSpeed );
        distanceToForward = Mathf.Lerp( distanceToForward , rawDistanceToForward - currentSpeed ,
            parameters.physics.physicsInfoLerpSpeed );
        groundNormal = Vector3.Lerp( groundNormal , rawGroundNormal , parameters.physics.physicsInfoLerpSpeed );
        forwardNormal = Vector3.Lerp( forwardNormal , rawForwardNormal , parameters.physics.physicsInfoLerpSpeed );

        if ( parameters.modules.avoidance ) {
            float avoidDist = parameters.avoidance.avoidanceStartDistance;
            forwardTurnNormalizedValue = avoidDist > 0 ? Mathf.Clamp01( 1f - distanceToForward / avoidDist ) : 0f;
            downTurnNormalizedValue = avoidDist > 0 ? Mathf.Clamp01( 1f - distanceToGround / avoidDist ) : 0f;
        } else {
            forwardTurnNormalizedValue = 0f;
            downTurnNormalizedValue = 0f;
        }
    }

    // Top sprint speed = configured multiple of normal max speed.
    private float MaxSprintSpeed() => parameters.movement.maxSpeed * parameters.sprint.maxSprintSpeedMultiplier;

    private void UpdateStamina()
    {
        if ( !parameters.modules.sprint ) return;

        var s = parameters.sprint;
        stamina = Mathf.Min( stamina + s.staminaRefillRate * simDt , s.maxStamina );

        float speedRange = Mathf.Max( MaxSprintSpeed() - parameters.movement.maxSpeed , 0.001f );
        float excess     = Mathf.Max( 0f , currentSpeed - parameters.movement.maxSpeed );
        stamina -= s.staminaDrainRate * (excess / speedRange) * simDt;
        stamina  = Mathf.Max( stamina , 0f );
    }

    private void UpdateModuleStates()
    {
        if ( parameters.modules.flock ) {
            UpdateFlockState();
        }

        if ( parameters.modules.spline ) {
            UpdateSplineState();
        }
    }

    private void UpdateFlockState()
    {
        flockState.queryTimer += simDt;

        if ( flockState.queryTimer < parameters.flock.neighborQueryInterval ) {
            return;
        }

        flockState.queryTimer = 0;

        flockState.neighbors.Clear();
        manager.GetNearbyBirds( position , parameters.flock.detectionRadius , this , flockState.neighbors );

        if ( flockState.neighbors.Count > 0 ) {
            flockState.flockCenter = Vector3.zero;
            flockState.avgVelocity = Vector3.zero;

            foreach (var n in flockState.neighbors) {
                flockState.flockCenter += n.position;
                flockState.avgVelocity += n.velocity;
            }

            flockState.flockCenter /= flockState.neighbors.Count;
            flockState.avgVelocity /= flockState.neighbors.Count;
        }
    }

    // removed — passive updraft soaring; updraft is now entered via Searching → Updrafting.
    // private void UpdateUpdraftState()
    // {
    //     updraftState.zone = UpdraftZone.FindNearest( position , parameters.updraft.detectionRadius );
    //
    //     updraftState.interestPoint = null;
    //     if ( manager?.interestPoints != null ) {
    //         float bestSqr = float.MaxValue;
    //         foreach ( var ip in manager.interestPoints ) {
    //             if ( ip == null || ip.type != InterestPointType.Updraft ) continue;
    //             float sqr = (ip.transform.position - position).sqrMagnitude;
    //             if ( sqr < ip.noticeRadius * ip.noticeRadius && sqr < bestSqr ) {
    //                 bestSqr = sqr;
    //                 updraftState.interestPoint = ip;
    //             }
    //         }
    //     }
    // }

    private void UpdateSplineState()
    {
        if ( splineState.spline == null ) {
            splineState.spline = PreySpline.FindNearest( position , 1000f );
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // State machine
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateState()
    {
        if ( parameters.modules.social ) {
            socialState.sampleTimer -= simDt;
            if ( socialState.sampleTimer <= 0f ) ScanSocialPressure();
        }

        switch (state) {

            case PreyState.Calm:
                if ( parameters.modules.run && WrenWithinStartle( parameters.run.startleRadius , parameters.run.startleLeadTime ) ) {
                    EnterDisturbed();
                    break;
                }

                if ( parameters.modules.social && socialState.desireToDisturb >= parameters.social.disturbThreshold ) {
                    socialState.desireToDisturb    = 0f;
                    socialState.lastTriggerLabel   = $"social → Disturbed  ({socialState.desireToDisturb:F2})";
                    socialState.lastTriggerTime    = Time.time;
                    EnterDisturbed();
                    break;
                }

                // social pressure to land → search toward a perch point (landing is search-driven)
                if ( parameters.modules.social && parameters.modules.perch && socialState.desireToLand >= parameters.social.landThreshold ) {
                    var perchPoint = PickSearchTarget( true , onlyType: InterestPointType.Perch );
                    if ( perchPoint != null ) {
                        socialState.lastTriggerLabel = $"social → search perch  ({socialState.desireToLand:F2})";
                        socialState.lastTriggerTime  = Time.time;
                        socialState.desireToLand     = 0f;
                        EnterSearching( perchPoint );
                        break;
                    }
                }

                if ( parameters.modules.search ) {
                    // being inside a point's notice range speeds the calm countdown by its urgency
                    searchState.calmTimer += simDt * (1f + InRangeUrgency());

                    if ( searchState.searchCooldown > 0f ) {
                        searchState.searchCooldown -= simDt;
                    } else if ( searchState.calmTimer >= searchState.nextSearchTime ) {
                        // calm period elapsed — commit to a point of interest and head for it
                        var target = PickSearchTarget( true );
                        if ( target != null ) { EnterSearching( target ); break; }
                        searchState.Init( parameters.search ); // no points exist at all — wait & retry
                    }
                }

                break;

            case PreyState.Searching:
                if ( parameters.modules.run && WrenWithinStartle( parameters.run.startleRadius , parameters.run.startleLeadTime ) ) {
                    EnterDisturbed();
                    break;
                }

                if ( searchState.currentTarget?.transform == null ) { EnterCalm(); break; }

                searchState.searchTimer += simDt;

                if ( ArrivedAtTarget() ) {
                    // Perch/Updraft/Despawn use timeToRemainInterested as their action duration
                    // (perch sit, updraft ride), so commit on contact instead of lingering here.
                    // NewCalm/NewInterest have no duration, so they linger timeToRemainInterested first.
                    if ( searchState.currentTarget.type == InterestPointType.Perch
                         || searchState.currentTarget.type == InterestPointType.Updraft
                         || searchState.currentTarget.type == InterestPointType.Despawn ) {
                        ArriveAtSearchTarget( searchState.currentTarget );
                        break;
                    }

                    searchState.arrivedTimer += simDt;
                    if ( searchState.arrivedTimer >= searchState.currentTarget.timeToRemainInterested )
                        ArriveAtSearchTarget( searchState.currentTarget );
                    break;
                }

                // drifted back out — reset the linger timer
                searchState.arrivedTimer = 0f;

                if ( searchState.searchTimer >= parameters.search.giveUpTime ) {
                    EnterCalm();
                    break;
                }

                break;

            case PreyState.Landing:
                if ( parameters.modules.run && WrenWithinStartle( parameters.run.startleRadius , parameters.run.startleLeadTime ) ) {
                    EnterDisturbed();
                    break;
                }

                if ( !HasPerchTarget() ) {
                    EnterCalm();
                    break;
                }

                if ( Vector3.Distance( position , PerchSurface() ) < parameters.perch.snapDistance ) {
                    EnterPerched();
                }

                break;

            case PreyState.Updrafting:
                if ( parameters.modules.run && WrenWithinStartle( parameters.run.startleRadius , parameters.run.startleLeadTime ) ) {
                    EnterDisturbed();
                    break;
                }

                if ( updraftState.activeTarget == null || updraftState.activeTarget.transform == null ) {
                    ExitUpdraft();
                    break;
                }

                updraftState.remainTimer += simDt;
                if ( updraftState.remainTimer >= updraftState.activeTarget.timeToRemainInterested ) {
                    ExitUpdraft();
                }

                break;

            case PreyState.Perched:
                perchState.perchedTimer += simDt;

                if ( WrenWithinStartle( parameters.perch.startleRadius , parameters.perch.startleLeadTime ) ) {
                    if ( parameters.modules.takeOff ) EnterTakeOff();
                    else                              EnterCalm();
                    break;
                }

                if ( parameters.modules.social && socialState.desireToTakeOff >= parameters.social.takeOffThreshold ) {
                    socialState.lastTriggerLabel   = $"social → TakeOff  ({socialState.desireToTakeOff:F2})";
                    socialState.lastTriggerTime    = Time.time;
                    socialState.desireToTakeOff    = 0f;
                    if ( parameters.modules.takeOff ) EnterTakeOff();
                    else                              EnterCalm();
                    searchState.searchCooldown = searchState.nextSearchTime;
                    break;
                }

                // perched long enough — take off (if able), otherwise just go calm
                if ( perchState.perchedTimer >= perchState.currentPerchDuration ) {
                    if ( parameters.modules.takeOff ) {
                        EnterTakeOff();
                    } else {
                        EnterCalm();
                        searchState.searchCooldown = searchState.nextSearchTime;
                    }
                }

                break;

            case PreyState.TakingOff:
                // pop up and away; once we've travelled far enough from the takeoff point, drop the forces
                if ( Vector3.Distance( position , takeOffState.origin ) >= parameters.takeOff.takeOffDistance ) {
                    EnterCalm();
                    searchState.searchCooldown = searchState.nextSearchTime;
                }

                break;

            case PreyState.Disturbed:
                runState.calmTimer += simDt;

                if ( runState.calmTimer > parameters.run.calmDownTime &&
                     vectorToWren.magnitude > parameters.run.calmDownDistance ) {
                    EnterCalm();
                    break;
                }

                if ( parameters.modules.social && socialState.desireToCalmDown >= parameters.social.calmThreshold &&
                     vectorToWren.magnitude > parameters.run.calmDownDistance ) {
                    socialState.lastTriggerLabel   = $"social → Calm  ({socialState.desireToCalmDown:F2})";
                    socialState.lastTriggerTime    = Time.time;
                    socialState.desireToCalmDown   = 0f;
                    EnterCalm();
                }

                break;
        }
    }

    private void EnterCalm()
    {
        state = PreyState.Calm;

        bounceSettled = false;   // bounce calm behavior restarts falling, not mid-settle

        if ( parameters.modules.perch ) perchState.Init( parameters.perch );
        if ( parameters.modules.search ) searchState.Init( parameters.search );
    }

    private void EnterSearching( PreyInterestPoint target )
    {
        state = PreyState.Searching;
        searchState.currentTarget = target;
        searchState.searchTimer   = 0f;
        searchState.calmTimer     = 0f;
        searchState.arrivedTimer  = 0f;

        // LandPoint: reserve the actual landing spot now and aim straight at it. Otherwise clear any
        // previous reservation (a spot is acquired on arrival instead).
        perchState.target       = null;
        perchState.fieldLanding = false;
        if ( target.type == InterestPointType.Perch && target.searchTargetType == SearchTargetType.LandPoint )
            AcquirePerchTarget( target );

        float scatter = target.SearchScatterRadius();
        searchState.targetOffset = scatter > 0f ? target.RandomOffset( scatter ) : Vector3.zero;
    }

    // World position the bird is currently flying toward (respects the point's search type).
    private Vector3 CurrentSearchTargetPos()
    {
        // LandPoint: fly precisely to the reserved landing spot (no scatter)
        if ( searchState.currentTarget.searchTargetType == SearchTargetType.LandPoint ) {
            if ( perchState.fieldLanding )      return perchState.landPos;
            if ( perchState.target != null )    return perchState.target.position;
        }

        return searchState.currentTarget.GetSearchTarget( position , searchState.targetOffset );
    }

    // Has the bird reached its target's enter radius, respecting the point's entrance shape?
    private bool ArrivedAtTarget()
    {
        var ct = searchState.currentTarget;

        // Collider entrance: arrived the moment we're inside the assigned collider (enterRadius ignored).
        if ( ct.entranceShape == EntranceShape.Collider )
            return ct.ContainsPoint( position );

        var   target = CurrentSearchTargetPos();
        var   d      = position - target;
        if ( ct.entranceShape == EntranceShape.Cylinder ) d.y = 0f;
        float r = ct.enterRadius;
        return d.sqrMagnitude <= r * r;
    }

    // Highest noticeUrgency among interest points whose notice radius currently contains us (0 if none).
    private float InRangeUrgency()
    {
        if ( manager?.interestPoints == null ) return 0f;

        float best = 0f;
        foreach ( var ip in manager.interestPoints ) {
            if ( ip == null || ip.noticeUrgency <= best ) continue;
            if ( ip.IsWithin( position , ip.noticeRadius ) ) best = ip.noticeUrgency;
        }
        return best;
    }

    private void ArriveAtSearchTarget( PreyInterestPoint target )
    {
        searchState.currentTarget = null;

        switch ( target.type ) {
            case InterestPointType.Perch:
                // LandPoint already reserved a spot in EnterSearching; otherwise acquire one now
                bool haveSpot = HasPerchTarget() || AcquirePerchTarget( target );
                if ( haveSpot ) {
                    // perch duration comes from the point's Time To Remain (± variance)
                    perchState.currentPerchDuration = Mathf.Max( 0.01f , target.timeToRemainInterested
                        + Random.Range( -target.timeToRemainVariance , target.timeToRemainVariance ) );
                    state = PreyState.Landing;
                } else {
                    EnterCalm();
                }
                break;

            case InterestPointType.Updraft:
                updraftState.activeTarget = target;
                updraftState.remainTimer  = 0f;
                state = PreyState.Updrafting;
                break;

            case InterestPointType.NewCalm:
                EnterCalm();
                break;

            case InterestPointType.Despawn:
                ForceDespawn();
                break;

            case InterestPointType.NewInterest:
                searchState.newInterestCount++;

                // too many search→search hops without a real calm — force a full calm session
                int maxHops = parameters.search.maxNewInterestsBeforeForcedCalm;
                if ( maxHops > 0 && searchState.newInterestCount >= maxHops ) {
                    EnterCalm();                                            // resets newInterestCount via Init
                    searchState.searchCooldown = searchState.nextSearchTime; // no searching until it completes
                    break;
                }

                // otherwise immediately search for a different point, never returning to this one
                var next = PickSearchTarget( true , exclude: target );
                if ( next != null ) EnterSearching( next );
                else                EnterCalm();
                break;
        }
    }

    private void EnterDisturbed()
    {
        state = PreyState.Disturbed;
        runState.calmTimer = 0;
        runState.jukeTimer = 0;
        runState.jukeDir = Random.insideUnitSphere.normalized;
        runState.jukeDir.y = 0;
    }

    private void EnterPerched()
    {
        state = PreyState.Perched;
        velocity = Vector3.zero;
        flapValue = Vector3.zero;
        position = PerchSurface();
        transform.position = position;
        perchState.perchedTimer = 0;

        // duration is set when landing is decided, from the Perch point's Time To Remain (± variance).
        // Guard against a zero value so the bird doesn't take off instantly.
        if ( perchState.currentPerchDuration <= 0f )
            perchState.currentPerchDuration = 5f;
    }

    private void EnterTakeOff()
    {
        state = PreyState.TakingOff;
        takeOffState.origin = position;
        takeOffState.runDirection = vectorToWren.sqrMagnitude > 0.01f
            ? -vectorToWren.normalized
            : Random.insideUnitSphere.normalized;
        perchState.target       = null;
        perchState.fieldLanding = false;
    }

    // Ride is over: drop the updraft forces and return to Calm. EnterCalm resets calmTimer,
    // and proximity notice is now gated by calmBeforeSearch, so the bird naturally rests the
    // full calm period before it can re-pick this same updraft (we're still inside its volume).
    private void ExitUpdraft()
    {
        updraftState.activeTarget = null;
        updraftState.remainTimer  = 0f;
        EnterCalm();
    }

    // ── Spawn-state entry points (used by SpawnType.OnPointOfInterest) ─────────
    // Spawn this bird already perched at a Perch interest point. Returns false if no
    // free perch spot could be acquired (caller can leave it calm where it spawned).
    public bool SpawnPerchedAt( PreyInterestPoint poi )
    {
        if ( poi == null ) return false;

        perchState.Init( parameters.perch );
        if ( !AcquirePerchTarget( poi ) ) return false;

        perchState.currentPerchDuration = Mathf.Max( 0.01f , poi.timeToRemainInterested
            + Random.Range( -poi.timeToRemainVariance , poi.timeToRemainVariance ) );
        EnterPerched();   // snaps to the perch surface, zeroes velocity, sets state = Perched
        return true;
    }

    // Spawn this bird already riding (circling) an Updraft interest point.
    public void SpawnUpdraftingAt( PreyInterestPoint poi )
    {
        if ( poi == null ) return;
        updraftState.activeTarget = poi;
        updraftState.remainTimer  = 0f;
        state = PreyState.Updrafting;
    }

    private void ScanSocialPressure()
    {
        var s = parameters.social;
        socialState.sampleTimer = s.sampleInterval;
        socialState.Decay( s.decayRate , s.sampleInterval );
        socialState.influences.Clear();

        if ( manager == null ) return;

        _socialNeighbors.Clear();
        manager.GetNearbyBirds( position , s.neighborRadius , this , _socialNeighbors );

        foreach ( var neighbor in _socialNeighbors ) {
            float dist   = (neighbor.position - position).magnitude;
            float weight = (1f - Mathf.Clamp01( dist / s.neighborRadius )) * s.socialWeight;
            if ( weight <= 0f ) continue;

            switch ( neighbor.state ) {
                case PreyState.TakingOff:
                case PreyState.Disturbed:
                    socialState.desireToTakeOff += weight;
                    socialState.desireToDisturb += weight;
                    break;
                case PreyState.Perched:
                case PreyState.Landing:
                    socialState.desireToLand += weight;
                    break;
                case PreyState.Calm:
                case PreyState.Searching:
                    socialState.desireToCalmDown += weight;
                    break;
            }

            socialState.influences.Add( new SocialPressureState.Influence {
                bird        = neighbor,
                weight      = weight,
                sourceState = neighbor.state
            } );
        }

        _socialNeighbors.Clear();
    }

    // forcedScan = true  → every point is reachable (a deliberate search), weighted toward
    //                      nearby / urgent / alwaysInteresting points.
    // forcedScan = false → only points within noticeRadius (or alwaysInteresting).
    // exclude            → skip this specific point (used by NewInterest to avoid revisiting).
    // onlyType           → if set, only consider points of this type (used by social-land → Perch).
    private PreyInterestPoint PickSearchTarget( bool forcedScan , PreyInterestPoint exclude = null ,
                                                InterestPointType? onlyType = null )
    {
        if ( manager?.interestPoints == null || manager.interestPoints.Length == 0 ) return null;

        _searchCandidates.Clear();
        _searchWeights.Clear();

        float             closeness   = Mathf.Clamp01( parameters.search.closenessImportance );
        float             totalWeight = 0f;
        float             bestDist    = float.MaxValue;
        PreyInterestPoint nearest     = null;

        foreach ( var ip in manager.interestPoints ) {
            if ( ip == null || ip == exclude ) continue;
            if ( onlyType.HasValue && ip.type != onlyType.Value ) continue;

            bool inRange = ip.IsWithin( position , ip.noticeRadius );
            if ( !inRange && !ip.alwaysInteresting && !forcedScan ) continue; // proximity scan: in-range/always only

            float dist = Vector3.Distance( position , ip.transform.position );

            // base desirability, then bias toward nearer points by closenessImportance
            float weight = Mathf.Max( ip.priority , 0.001f ) * (inRange ? (1f + ip.noticeUrgency) : 1f);
            if ( closeness > 0f )
                weight *= Mathf.Pow( 1f / Mathf.Max( dist , 1f ) , closeness * 3f );

            _searchCandidates.Add( ip );
            _searchWeights.Add( weight );
            totalWeight += weight;

            if ( dist < bestDist ) { bestDist = dist; nearest = ip; }
        }

        if ( _searchCandidates.Count == 0 ) return null;

        // at full closeness, deterministically take the nearest
        if ( closeness >= 1f ) return nearest;

        float r     = Random.Range( 0f , totalWeight );
        float accum = 0f;

        for ( int i = 0; i < _searchCandidates.Count; i++ ) {
            accum += _searchWeights[i];
            if ( r <= accum ) return _searchCandidates[i];
        }

        return _searchCandidates[ _searchCandidates.Count - 1 ];
    }

    private static readonly List<Transform>          _perchCandidates  = new();
    private static readonly HashSet<Transform>       _occupiedPerches  = new();
    private static readonly List<PreyController>     _socialNeighbors  = new();
    private static readonly List<PreyInterestPoint>  _searchCandidates = new();
    private static readonly List<float>              _searchWeights    = new();
    private static readonly List<Vector3>            _occupiedLand     = new();

    // Acquire a landing target on a Perch point: a discrete _perch_ spot (OnCollider/InArea) or a
    // runtime-computed Field spot. Returns true if a spot was secured.
    private bool AcquirePerchTarget( PreyInterestPoint point )
    {
        if ( point.perchSubType == PerchSubType.Field ) {
            perchState.target = null;
            perchState.fieldLanding = ComputeFieldLandSpot( point );
            return perchState.fieldLanding;
        }

        perchState.fieldLanding = false;
        perchState.target = FindBestPerchTarget( point );
        return perchState.target != null;
    }

    // Field landing: sample ground spots in the field area (biased forward along velocity), respect
    // spacing from other landing birds, and prefer spots close to us (and to others if desireToBeClose).
    private bool ComputeFieldLandSpot( PreyInterestPoint point )
    {
        var f = point.perchField;

        // gather where other birds are landing / perched (for spacing)
        _occupiedLand.Clear();
        if ( manager?.preyHolder != null ) {
            for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
                var other = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
                if ( other == null || other == this || !other.IsClaimingLand ) continue;
                _occupiedLand.Add( other.ClaimedLandPosition );
            }
        }

        // sample area, biased forward along our flattened velocity
        Vector3 fwd    = velocity; fwd.y = 0f;
        fwd            = fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector3.zero;
        Vector3 center = point.transform.position + fwd * f.forwardFromVelocity;

        // cast from a bit past the prey along the cast direction (offset avoids self-intersection),
        // down by default or up if toggled — follows the bird's current altitude
        Vector3 castDir = f.castUp ? Vector3.up : Vector3.down;
        float   startY  = position.y + (f.castUp ? f.castHeightOffset : -f.castHeightOffset);

        float   spacingSqr   = f.spacing * f.spacing;
        bool    found        = false;
        float   bestScore    = float.MaxValue;
        Vector3 bestPos      = Vector3.zero, bestNormal = Vector3.up;
        // least-crowded fallback when the field is full
        bool    spreadFound  = false;
        float   bestSpread   = -1f;
        Vector3 spreadPos    = Vector3.zero, spreadNormal = Vector3.up;

        const int samples = 16;
        for ( int i = 0; i < samples; i++ ) {
            var xz     = Random.insideUnitCircle * f.radius;
            var origin = new Vector3( center.x + xz.x , startY , center.z + xz.y );
            PreyProfiler.raycastCount++;
            if ( !Physics.Raycast( origin , castDir , out var hit , 10000f , f.groundLayers ) ) continue;

            var cand = hit.point;

            float nearestSqr = float.MaxValue;
            for ( int j = 0; j < _occupiedLand.Count; j++ ) {
                float d = (cand - _occupiedLand[j]).sqrMagnitude;
                if ( d < nearestSqr ) nearestSqr = d;
            }
            float nearest = _occupiedLand.Count > 0 ? Mathf.Sqrt( nearestSqr ) : f.radius;

            if ( nearest > bestSpread ) { bestSpread = nearest; spreadPos = cand; spreadNormal = hit.normal; spreadFound = true; }

            if ( _occupiedLand.Count > 0 && nearestSqr < spacingSqr ) continue; // too close to another bird

            // lower = better: prefer close to us; desireToBeClose also prefers being near others (clump)
            float score = Vector3.Distance( position , cand ) + f.desireToBeClose * nearest;
            if ( score < bestScore ) { bestScore = score; bestPos = cand; bestNormal = hit.normal; found = true; }
        }

        if ( found )       { perchState.landPos = bestPos;   perchState.landNormal = bestNormal;   return true; }
        if ( spreadFound ) { perchState.landPos = spreadPos; perchState.landNormal = spreadNormal; return true; }
        return false; // no ground found in the field at all
    }

    // Pick a free perch spot on the arrived-at Perch interest point: one of its generated
    // _perch_ children, or the point itself if it has none.
    private Transform FindBestPerchTarget( PreyInterestPoint point )
    {
        if ( point == null ) return null;

        // collect perches already claimed by landing or perched birds
        _occupiedPerches.Clear();
        if ( manager?.preyHolder != null ) {
            for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
                var other = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
                if ( other == null || other == this ) continue;
                // Searching covers LandPoint birds that have reserved a spot but haven't landed yet
                if ( other.state == PreyState.Searching || other.state == PreyState.Landing
                     || other.state == PreyState.Perched ) {
                    var t = other.CurrentPerchTarget;
                    if ( t != null ) _occupiedPerches.Add( t );
                }
            }
        }

        _perchCandidates.Clear();
        for ( int i = 0; i < point.transform.childCount; i++ ) {
            var child = point.transform.GetChild( i );
            if ( !child.name.StartsWith( "_perch_" ) ) continue;
            if ( _occupiedPerches.Contains( child ) ) continue;
            _perchCandidates.Add( child );
        }

        if ( _perchCandidates.Count > 0 )
            return _perchCandidates[ Random.Range( 0 , _perchCandidates.Count ) ];

        // no free generated perch points — land at the point itself if it's not taken
        return _occupiedPerches.Contains( point.transform ) ? null : point.transform;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Physics dispatch
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void DoPhysics()
    {
        allForces.Clear();

        switch (state) {
            case PreyState.Calm:      DoCalmPhysics();      break;
            case PreyState.Searching: DoSearchingPhysics(); break;
            case PreyState.Landing:   DoLandingPhysics();   break;
            case PreyState.Updrafting:DoUpdraftingPhysics();break;
            case PreyState.Perched:   DoPerchedPhysics();   break;
            case PreyState.TakingOff: DoTakeOffPhysics();   break;
            case PreyState.Disturbed: DoDisturbedPhysics(); break;
        }
    }

    // ── Calm ─────────────────────────────────────────────────────────────────

    private void DoCalmPhysics()
    {
        if ( parameters.modules.bounce ) { DoBouncePhysics(); return; }

        _flapSpeedMult = 1f;
        force = Vector3.zero;

        AddAvoidanceForces();

        if ( parameters.modules.drive )  AddForce( DriveForce()       , new Color( 0.6f , 1f , 0f ) , "drive" );
        if ( parameters.modules.noise )  AddForce( NoiseForce()       , Color.yellow            , "noise" );
        if ( parameters.modules.flock )  AddForce( FlockForce()       , Color.cyan              , "flock" );
        if ( parameters.modules.spline ) AddForce( SplineForce() , Color.blue , "spline" );

        if ( parameters.modules.circle )
            AddForce( CalmCircleForce() , Color.magenta , "circle" );

        if ( parameters.modules.cage )
            AddForce( CageForce() , new Color( 1f , 0.8f , 0f ) , "cage" );

        ApplyVelocity( parameters.movement.desiredSpeed , true );

        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }

        transform.position = position + flapValue;
    }

    // Ballistic "drop and bounce" calm behavior (modules.bounce). Vertical is pure gravity + a
    // reflect-on-ground bounce; horizontal still comes from the usual calm forces if they're toggled.
    private void DoBouncePhysics()
    {
        var bnc = parameters.bounce;
        var m   = parameters.movement;

        oldVelocity = velocity;

        // Settled: pinned where it landed. Hold still until the settle time elapses, then relaunch
        // (up + a forward kick along the current heading) back into the bounce loop.
        if ( bounceSettled ) {
            velocity         = Vector3.zero;
            currentSpeed     = 0f;
            flapValue        = Vector3.zero;
            bounceSettleTimer += simDt;
            if ( bounceSettleTimer >= bounceSettleDuration ) Relaunch( bnc );
            transform.position = position;
            return;
        }

        // optional horizontal steering from the usual calm modules (only the toggled ones)
        force = Vector3.zero;
        if ( parameters.modules.drive )  AddForce( DriveForce()      , new Color( 0.6f , 1f , 0f ) , "drive" );
        if ( parameters.modules.noise )  AddForce( NoiseForce()      , Color.yellow , "noise" );
        if ( parameters.modules.flock )  AddForce( FlockForce()      , Color.cyan , "flock" );
        if ( parameters.modules.circle ) AddForce( CalmCircleForce() , Color.magenta , "circle" );
        if ( parameters.modules.cage )   AddForce( CageForce()       , new Color( 1f , 0.8f , 0f ) , "cage" );
        force.y = 0f;   // bounce owns the vertical axis

        // horizontal velocity: steer toward desiredSpeed when there's input, otherwise just damp
        Vector3 hVel = new Vector3( velocity.x , 0f , velocity.z ) + force;
        hVel *= ( 1f - m.dampening );
        if ( force.sqrMagnitude > 1e-8f ) {
            float hSpeed = hVel.magnitude;
            hSpeed += ( m.desiredSpeed - hSpeed ) * m.dampening;
            hSpeed  = Mathf.Min( hSpeed , m.maxSpeed );
            if ( hVel.sqrMagnitude > 1e-8f ) hVel = hVel.normalized * hSpeed;
        }
        velocity.x = hVel.x;
        velocity.z = hVel.z;

        // vertical: gravity (per-frame, matching the rest of the sim's integration)
        velocity.y -= bnc.gravity;

        // integrate, then resolve the ground bounce
        position += velocity;
        BounceOnGround( bnc );

        currentSpeed = velocity.magnitude;
        flapValue    = Vector3.zero;
        transform.position = position;
    }

    // Reflect off the ground once the bird has fallen onto it.
    private void BounceOnGround( PreyBounceModule bnc )
    {
        const float castStart = 5f;
        PreyProfiler.raycastCount++;
        if ( !Physics.Raycast( position + Vector3.up * castStart , Vector3.down , out var hit ,
                               castStart + 10000f , bnc.groundLayers , QueryTriggerInteraction.Ignore ) )
            return;

        float groundY = hit.point.y + bnc.radius;
        if ( position.y > groundY || velocity.y >= 0f ) return;   // above ground or already rising

        position.y = groundY;
        float vy = -velocity.y * bnc.restitution;

        if ( vy < bnc.settleSpeed ) {
            if ( bnc.settle ) {
                // bounce has decayed below the cutoff → settle in place where it landed (just a state:
                // it perches right here). It holds for bounceSettleDuration, then relaunches.
                velocity             = Vector3.zero;
                bounceSettled        = true;
                bounceSettleTimer    = 0f;
                bounceSettleDuration = Mathf.Max( 0f , bnc.timeToRemainSettled
                    + Random.Range( -bnc.timeToRemainSettledVariance , bnc.timeToRemainSettledVariance ) );
                return;
            }
            vy = bnc.settleSpeed;   // Settle off → keep it bouncing forever
        }

        velocity.y  = vy;
        velocity.x *= bnc.bounceFriction;
        velocity.z *= bnc.bounceFriction;
    }

    // Pop out of a settle: upward kick + a horizontal kick along the current heading (scattered by
    // relaunchForwardRandomness), then fall back into the bounce loop.
    private void Relaunch( PreyBounceModule bnc )
    {
        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if ( fwd.sqrMagnitude < 1e-6f ) fwd = Vector3.forward;
        fwd.Normalize();

        // randomness: rotate the heading by up to ±180° around Y (0 = dead ahead, 1 = any direction)
        float ang = Random.Range( -1f , 1f ) * bnc.relaunchForwardRandomness * 180f;
        Vector3 dir = Quaternion.AngleAxis( ang , Vector3.up ) * fwd;

        velocity   = dir * bnc.relaunchForwardVelocity;
        velocity.y = bnc.relaunchForce;

        bounceSettled = false;
    }

    // ── Searching ────────────────────────────────────────────────────────────

    private void DoSearchingPhysics()
    {
        force = Vector3.zero;

        if ( searchState.currentTarget?.transform != null ) {
            var toTarget = (CurrentSearchTargetPos() - position).normalized;
            AddForce( toTarget * parameters.search.moveForce , new Color( 0.6f , 0.2f , 1f ) , "search" );
        }

        AddAvoidanceForces();

        if ( parameters.modules.drive ) AddForce( DriveForce()   , new Color( 0.6f , 1f , 0f ) , "drive" );
        if ( parameters.modules.cage  ) AddForce( CageForce()    , new Color( 1f , 0.8f , 0f ) , "cage"  );
        if ( parameters.modules.noise ) AddForce( NoiseForce()   , Color.yellow               , "noise" );

        ApplyVelocity( parameters.movement.desiredSpeed , true );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ── Landing ──────────────────────────────────────────────────────────────

    private bool    HasPerchTarget() => perchState.fieldLanding || perchState.target != null;
    private Vector3 PerchNormal()    => perchState.fieldLanding ? perchState.landNormal
                                      : (perchState.target != null ? perchState.target.up : Vector3.up);

    private Vector3 PerchSurface()
    {
        if ( perchState.fieldLanding )
            return perchState.landPos + perchState.landNormal * parameters.perch.landingOffset;
        if ( perchState.target == null ) return Vector3.zero;
        return perchState.target.position + perchState.target.up * parameters.perch.landingOffset;
    }

    private void DoLandingPhysics()
    {
        if ( !HasPerchTarget() ) { EnterCalm(); return; }

        var   p        = parameters.perch;
        var   surface  = PerchSurface();
        var   normal   = PerchNormal();
        // approach waypoint: directly above the surface along the surface normal
        var   approach = surface + normal * p.approachHeight;

        // ramp calm→aim blend
        perchState.landingBlend = Mathf.MoveTowards(
            perchState.landingBlend , 1f , simDt / p.landingBlendDuration );

        // ── Phase 1: approach — fly to the waypoint above the surface ─────────
        // ── Phase 2: dive    — once at waypoint, drop straight down; no return ─
        // (Spiral commits to the dive from inside its own branch once it's wound in tight.)
        if ( !perchState.isDiving && p.approachStyle == PerchApproachStyle.Dive ) {
            float distToApproach = Vector3.Distance( position , approach );
            if ( distToApproach < p.approachRadius )
                perchState.isDiving = true;
        }

        force = Vector3.zero;
        float distToSurface;

        if ( perchState.isDiving ) {
            // dive phase: aim straight at surface, ignore calm forces
            var toSurface = surface - position;
            distToSurface = toSurface.magnitude;
            var aimDir = toSurface.sqrMagnitude > 0.001f ? toSurface.normalized : -normal;
            force = aimDir;
        } else if ( p.approachStyle == PerchApproachStyle.Spiral ) {
            // spiral phase: a logarithmic spiral that winds inward and descends onto the surface
            distToSurface = (surface - position).magnitude;

            // basis perpendicular to the surface normal
            var right = Vector3.Cross( normal , Vector3.up );
            if ( right.sqrMagnitude < 1e-4f ) right = Vector3.Cross( normal , Vector3.forward );
            right.Normalize();
            var fwd = Vector3.Cross( right , normal ).normalized;

            perchState.spiralAngle += p.spiralSpeed * simDt;
            float a      = perchState.spiralAngle;
            float radius = p.spiralRadius * Mathf.Exp( -p.spiralTightness * a );          // shrinks each turn
            float height = Mathf.Max( 0f , p.approachHeight - p.spiralDescentRate * a );  // descends each turn
            var   target = surface + ( right * Mathf.Cos( a ) + fwd * Mathf.Sin( a ) ) * radius + normal * height;

            var toTarget = target - position;
            force = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : -normal;

            // wound in tight and low → commit to the final straight dive (UpdateState snaps on contact)
            if ( radius < p.snapDistance && height < p.snapDistance ) perchState.isDiving = true;
        } else {
            // dive approach: blend calm forces → aim at the waypoint above the surface
            var toApproach = approach - position;
            distToSurface  = (surface - position).magnitude;
            var aimDir     = toApproach.sqrMagnitude > 0.001f ? toApproach.normalized : normal;

            var calmF = Vector3.zero;
            if ( parameters.modules.drive )  calmF += DriveForce();
            if ( parameters.modules.noise )  calmF += NoiseForce();
            if ( parameters.modules.flock )  calmF += FlockForce();
            if ( parameters.modules.spline ) calmF += SplineForce();
            if ( parameters.modules.cage   ) calmF += CageForce();

            force = Vector3.Lerp( calmF , aimDir , perchState.landingBlend );
        }

        // fade out avoidance as we close on the landing surface: avoid obstacles on the way in,
        // but stop fighting the landing point itself once we're nearly there
        float avoidFade = p.landingAvoidanceFalloff > 0.01f
            ? Mathf.Clamp01( distToSurface / p.landingAvoidanceFalloff )
            : 1f;
        AddForce( MoveAlongGroundAndTurnAwayFromObstacles() * 0.3f * avoidFade , new Color( 1f , 0.4f , 0.1f ) , "avoidance" );

        // speed: slow as we close in on the surface
        float distT      = Mathf.Clamp01( distToSurface / (p.snapDistance * 8f) );
        float approachSpd = Mathf.Lerp( parameters.movement.desiredSpeed * p.approachSpeedMult ,
                                        parameters.movement.desiredSpeed , distT );
        ApplyVelocity( approachSpd );

        // flap: rapid when close to surface during dive
        float closeT   = perchState.isDiving ? 1f - Mathf.Clamp01( distToSurface / p.approachHeight ) : 0f;
        _flapSpeedMult = Mathf.Lerp( 1f , p.landingFlapMult , closeT );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        _flapSpeedMult = 1f;

        transform.position = position + flapValue;
    }

    // ── Updrafting ─────────────────────────────────────────────────────────────

    private void DoUpdraftingPhysics()
    {
        _flapSpeedMult = 1f;
        force = Vector3.zero;

        AddAvoidanceForces();

        if ( updraftState.activeTarget != null && updraftState.activeTarget.transform != null )
            AddForce( UpdraftForceFromPoint( updraftState.activeTarget ) , Color.green , "updraft" );

        if ( parameters.modules.drive ) AddForce( DriveForce() , new Color( 0.6f , 1f , 0f ) , "drive" );
        if ( parameters.modules.noise ) AddForce( NoiseForce() , Color.yellow               , "noise" );
        if ( parameters.modules.cage  ) AddForce( CageForce()  , new Color( 1f , 0.8f , 0f ) , "cage"  );

        ApplyVelocity( parameters.movement.desiredSpeed , true );

        if ( parameters.modules.flap ) DoFlapInfo();
        else                           flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ── Perched ──────────────────────────────────────────────────────────────

    private void DoPerchedPhysics()
    {
        velocity = Vector3.zero;
        flapValue = Vector3.zero;

        if ( HasPerchTarget() ) {
            position = PerchSurface();
        }

        transform.position = position;
    }

    // ── Taking off ───────────────────────────────────────────────────────────

    private void DoTakeOffPhysics()
    {
        force = Vector3.zero;

        // pop up and push away from the wren; runs until far enough from the takeoff point (UpdateState)
        force += Vector3.up * parameters.takeOff.upForce;
        force += takeOffState.runDirection * parameters.takeOff.runForce;
        AddForce( MoveAlongGroundAndTurnAwayFromObstacles() , new Color( 1f , 0.4f , 0.1f ) , "avoidance" );

        if ( parameters.modules.cage )  force += CageForce();
        if ( parameters.modules.drive ) AddForce( DriveForce() , new Color( 0.6f , 1f , 0f ) , "drive" );

        ApplyVelocity( parameters.movement.desiredSpeed , true );

        _flapSpeedMult = parameters.takeOff.takeOffFlapMult;   // boosted flapping during the pop
        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }
        _flapSpeedMult = 1f;

        transform.position = position + flapValue;
    }

    // ── Disturbed ────────────────────────────────────────────────────────────

    private void DoDisturbedPhysics()
    {
        force = Vector3.zero;

        float distToWren = vectorToWren.magnitude;
        float fleeMult = Mathf.Clamp01(
            (parameters.run.startleRadius - distToWren) /
            Mathf.Max( parameters.run.startleRadius - parameters.run.fullRunRadius , 0.01f ) );
        var runDir = parameters.run.chaseInstead ? vectorToWren.normalized : -vectorToWren.normalized;
        force += runDir * parameters.run.fleeForce * fleeMult;

        // juke — random lateral burst to make escape less predictable
        runState.jukeTimer += simDt;

        if ( runState.jukeTimer > 1f / Mathf.Max( parameters.run.jukeFrequency , 0.01f ) ) {
            runState.jukeTimer = 0;
            runState.jukeDir = Vector3.Cross( vectorToWren.normalized , Vector3.up ).normalized
                               * (Random.value > 0.5f ? 1f : -1f);
        }

        force += runState.jukeDir * parameters.run.jukeAmount;

        force += MoveAlongGroundAndTurnAwayFromObstacles();
        if ( parameters.modules.cage )  force += CageForce();
        if ( parameters.modules.drive ) AddForce( DriveForce() , new Color( 0.6f , 1f , 0f ) , "drive" );

        ApplyVelocity( parameters.movement.desiredSpeed * parameters.run.speedMultiplier , true );

        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }

        transform.position = position + flapValue;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Force helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void AddAvoidanceForces()
    {
        if ( !parameters.modules.avoidance ) return;

        var avoid = parameters.avoidance;

        if ( avoid.avoidGround ) {
            var gf = downTurnNormalizedValue > .99f
                ? Vector3.up
                : groundNormal * downTurnNormalizedValue * avoid.maxForce;
            AddForce( gf , new Color( 1f , 0.4f , 0.1f ) , "avoid ground" );
        }

        if ( avoid.avoidObjects ) {
            var of = forwardTurnNormalizedValue > .99f
                ? forwardNormal - velocity.normalized
                : forwardNormal * forwardTurnNormalizedValue * avoid.maxForce;
            AddForce( of , new Color( 1f , 0.1f , 0.1f ) , "avoid objects" );
        }
    }

    public virtual Vector3 MoveAlongGroundAndTurnAwayFromObstacles()
    {
        var f = Vector3.zero;

        if ( parameters.modules.avoidance ) {
            var avoid = parameters.avoidance;

            if ( avoid.avoidGround ) {
                f += groundNormal * downTurnNormalizedValue * avoid.maxForce;

                if ( downTurnNormalizedValue > .99f ) {
                    f = Vector3.up;
                }
            }

            if ( avoid.avoidObjects ) {
                f += forwardNormal * forwardTurnNormalizedValue * avoid.maxForce;

                if ( forwardTurnNormalizedValue > .99f ) {
                    f = forwardNormal - velocity.normalized;
                }
            }
        }

        return f;
    }

    private Vector3 DriveForce()
    {
        if ( velocity.sqrMagnitude < 0.0001f ) return Vector3.zero;

        float effectiveMax = (parameters.modules.sprint && stamina > 0f)
            ? MaxSprintSpeed()
            : parameters.movement.maxSpeed;

        float t = Mathf.Clamp01( currentSpeed / Mathf.Max( effectiveMax , 0.0001f ) );
        return velocity.normalized * parameters.drive.driveForce * (1f - t);
    }

    private Vector3 NoiseForce()
    {
        float t = Time.time * parameters.noise.noiseSpeed;
        return new Vector3(
            Mathf.PerlinNoise( t + noiseOffset          , noiseOffset + 31.41f ) * 2f - 1f ,
            Mathf.PerlinNoise( noiseOffset + 17.32f     , t + noiseOffset       ) * 2f - 1f ,
            Mathf.PerlinNoise( t + noiseOffset + 53.58f , noiseOffset + 89.79f  ) * 2f - 1f
        ) * parameters.noise.noiseForce;
    }

    private Vector3 FlockForce()
    {
        if ( flockState.neighbors.Count == 0 ) {
            return Vector3.zero;
        }

        var f = parameters.flock;
        var sep = Vector3.zero;

        foreach (var n in flockState.neighbors) {
            var away = position - n.position;

            if ( away.magnitude < f.separationRadius ) {
                sep += away.normalized / Mathf.Max( away.magnitude , 0.01f );
            }
        }

        var cohesion = flockState.flockCenter - position;
        var alignment = flockState.avgVelocity;

        return sep.normalized * f.separationForce
               + cohesion.normalized * f.cohesionForce
               + alignment.normalized * f.alignmentForce;
    }

    private Vector3 SplineForce()
    {
        var curve = manager?.regionSpline;

        if ( curve == null || curve.Splines.Count == 0 ) {
            Debug.LogWarning( "[PreyController] SplineForce: no spline found — assign regionSpline on PreyManager" , manager );
            return Vector3.zero;
        }

        // Throttle the lookup and reuse the cached result otherwise. The lookup
        // itself now hits the manager's baked spline LUT (a flat array scan), not
        // SplineUtility's live subdivision search.
        _splineQueryTimer -= simDt;
        bool movedFar = ( position - _splineQueryPos ).sqrMagnitude > 4f;   // > 2m since last query
        if ( !_splineCached || _splineQueryTimer <= 0f || movedFar ) {
            if ( manager.TryGetNearestOnRegionSpline( position , out var nearW , out var tanW ) ) {
                _splineNearestWorld = nearW;
                _splineTangentWorld = tanW;
                _splineCached       = true;
                _splineQueryTimer   = SplineQueryInterval;
                _splineQueryPos     = position;
                PreyProfiler.splineQueryCount++;
            }
        }

        var toSpline = _splineNearestWorld - position;
        var forward  = _splineTangentWorld;

        var pullF    = toSpline.sqrMagnitude > 0.0001f ? toSpline.normalized * parameters.spline.pullForce          : Vector3.zero;
        var forwardF = forward.sqrMagnitude  > 0.0001f ? forward.normalized  * parameters.spline.splineForwardForce : Vector3.zero;

        Debug.DrawRay( position , pullF    , Color.cyan );
        Debug.DrawRay( position , forwardF , Color.blue );

        return pullF + forwardF;
    }

    // removed — passive updraft soaring. The searched-to updraft uses UpdraftForceFromPoint (below).
    // private Vector3 UpdraftForce()
    // {
    //     if ( updraftState.zone != null ) {
    //         var toCenter = updraftState.zone.transform.position - position;
    //         toCenter.y = 0;
    //         var tangent = Vector3.Cross( Vector3.up , toCenter.normalized );
    //         return tangent * parameters.updraft.spiralForce
    //                + Vector3.up * parameters.updraft.liftForce * updraftState.zone.strength;
    //     }
    //
    //     if ( updraftState.interestPoint != null ) {
    //         return UpdraftForceFromPoint( updraftState.interestPoint );
    //     }
    //
    //     return Vector3.zero;
    // }

    // Spiral/lift force toward and around an updraft interest point's center.
    private Vector3 UpdraftForceFromPoint( PreyInterestPoint ip )
    {
        var us = ip.updraftSettings;
        return SoarForce( ip.transform.position , us.forceUp , us.forceIn , us.curlForce ,
                          us.curlDirection , us.desiredAltitude , us.altitudeRange , us.altitudeHoldStrength );
    }

    // Shared soaring force: curl (tangent) + inward pull around `center`, plus a climb that eases
    // across the altitude band [desiredAltitude - altitudeRange .. desiredAltitude] above center.y.
    // forceUp 0 = no vertical (pure horizontal circling). Used by both updraft and the calm circle.
    private Vector3 SoarForce( Vector3 center , float forceUp , float forceIn , float curlForce ,
                               CurlDirection curlDir , float desiredAltitude , float altitudeRange ,
                               float altitudeHoldStrength )
    {
        var toCenter = center - position;
        toCenter.y = 0;
        int curl    = curlDir == CurlDirection.CounterClockwise ? 1 : -1;
        var tangent = toCenter.sqrMagnitude > 0.01f
            ? Vector3.Cross( Vector3.up , toCenter.normalized ) * curl
            : Vector3.zero;
        var inward  = toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Vector3.zero;

        // vertical: full climb until within altitudeRange of the top, then ease the lift to 0
        // across the band so birds settle gently anywhere in it instead of snapping to one plane.
        float topY = center.y + desiredAltitude;
        float dy   = topY - position.y;   // > 0 = below the top of the band
        float up;
        if ( dy <= 0f ) {
            up = Mathf.Max( dy * altitudeHoldStrength , -forceUp );        // above the band: ease down
        } else {
            up = forceUp * Mathf.Clamp01( dy / Mathf.Max( altitudeRange , 0.01f ) ); // 1 far below → 0 at top
        }

        return Vector3.up * up
               + tangent  * curlForce
               + inward   * forceIn;
    }

    // removed — thermal soaring is now handled via interest points.
    // private Vector3 ThermalForce()
    // {
    //     if ( manager == null ) return Vector3.zero;
    //     var center = manager.GetClosestThermalCenter( position );
    //     if ( center == null ) return Vector3.zero;
    //
    //     var t = parameters.thermal;
    //     bool thermaling = distanceToGround < t.minAltitude;
    //     thermalState.isThermaling = thermaling;
    //
    //     thermalState.circleAngle += simDt * t.circleSpeed * (thermaling ? 1.5f : 1f);
    //     float radius = thermaling
    //         ? parameters.circle.circleRadius * t.thermalTightness
    //         : parameters.circle.circleRadius;
    //
    //     var target = center.position
    //                  + new Vector3( Mathf.Cos( thermalState.circleAngle ) , 0 , Mathf.Sin( thermalState.circleAngle ) ) * radius;
    //
    //     var f = (target - transform.position).normalized * parameters.circle.circleForce;
    //     if ( thermaling ) f += Vector3.up * parameters.circle.updraft;
    //     return f;
    // }

    // Soaring circle around this bird's PreyManager — same shape as an updraft.
    private Vector3 CalmCircleForce()
    {
        if ( manager == null ) return Vector3.zero;
        var c = parameters.circle;
        return SoarForce( manager.transform.position , c.forceUp , c.forceIn , c.curlForce ,
                          c.curlDirection , c.desiredAltitude , c.altitudeRange , c.altitudeHoldStrength );
    }

    private Vector3 CageForce()
    {
        if ( manager == null ) return Vector3.zero;

        var   cfg = parameters.cage;
        float d   = cfg.borderTurnDistance;

        // Box region: use the cage transform fully (position + rotation + scale). Push along the
        // box's own local axes so a rotated cage turns birds along its faces, not world X/Z.
        if ( manager.regionType == RegionType.Box && manager.boxRegion != null ) {
            var t      = manager.boxRegion;
            var toBird = position - t.position;
            float alongX = Vector3.Dot( toBird , t.right );    // signed world distance along local X
            float alongZ = Vector3.Dot( toBird , t.forward );  //                          along local Z
            float halfX  = t.lossyScale.x * 0.5f;
            float halfZ  = t.lossyScale.z * 0.5f;

            var push = Vector3.zero;
            float distMinX = alongX + halfX, distMaxX = halfX - alongX;
            float distMinZ = alongZ + halfZ, distMaxZ = halfZ - alongZ;
            if ( distMinX < d ) push += t.right   * (1f - distMinX / d);
            if ( distMaxX < d ) push -= t.right   * (1f - distMaxX / d);
            if ( distMinZ < d ) push += t.forward * (1f - distMinZ / d);
            if ( distMaxZ < d ) push -= t.forward * (1f - distMaxZ / d);

            return push * cfg.borderTurnForce;
        }

        // Collider region: an arbitrary collider has no single orientation — use its world AABB.
        if ( manager.regionType == RegionType.Collider && manager.regionCollider != null ) {
            var b    = manager.regionCollider.bounds;
            var push = Vector3.zero;
            if ( position.x - b.min.x < d ) push.x += 1f - (position.x - b.min.x) / d;
            if ( b.max.x - position.x < d ) push.x -= 1f - (b.max.x - position.x) / d;
            if ( position.z - b.min.z < d ) push.z += 1f - (position.z - b.min.z) / d;
            if ( b.max.z - position.z < d ) push.z -= 1f - (b.max.z - position.z) / d;
            return push * cfg.borderTurnForce;
        }

        return Vector3.zero;
    }

    // unused — anchor was removed from the interest-point system (GetClosestAnchorPoints returns null).
    // private Vector3 AnchorForce()
    // {
    //     var anchor = manager.GetClosestAnchorPoints( position );
    //
    //     if ( anchor == null ) {
    //         return Vector3.zero;
    //     }
    //
    //     var toAnchor = anchor.position - position;
    //
    //     if ( toAnchor.magnitude > parameters.perch.anchorRadius ) {
    //         return toAnchor.normalized * parameters.perch.anchorPullForce;
    //     }
    //
    //     return Vector3.zero;
    // }

    // ─────────────────────────────────────────────────────────────────────────
    // Shared velocity application
    // ─────────────────────────────────────────────────────────────────────────

    private void AddForce( Vector3 f , Color c , string n )
    {
        force += f;
        allForces.Add( new PreyForce( f , c , n ) );
    }

    private void ApplyVelocity( float targetSpeed , bool allowSprint = false )
    {
        var m = parameters.movement;

        // ease the total force toward the active state's force so transitions aren't rigid.
        // 1 = instant (no smoothing); lower lags the force, blending across state changes.
        float forceLerp = Mathf.Clamp01( m.newStateForceLerpSpeed );
        smoothedForce = forceLerp >= 1f ? force : Vector3.Lerp( smoothedForce , force , forceLerp );
        force = smoothedForce;

        if ( allowSprint && parameters.modules.sprint && stamina > 0f ) {
            targetSpeed = MaxSprintSpeed();
        }

        float effectiveMax = (allowSprint && parameters.modules.sprint && stamina > 0f)
            ? MaxSprintSpeed()
            : m.maxSpeed;

        oldVelocity = velocity;
        velocity += force;
        velocity *= (1f - m.dampening);                               // friction on full vector
        float speed = velocity.magnitude;
        speed += (targetSpeed - speed) * m.dampening;                // drive back toward target
        speed = Mathf.Clamp( speed , m.minSpeed , effectiveMax );
        currentSpeed = speed;
        velocity = velocity.sqrMagnitude > 0.0001f
            ? velocity.normalized * speed
            : oldVelocity.normalized * m.minSpeed;

        desiredVelocity = velocity;          // direction forces want to go, before turn-rate limit
        velocity = GetNewVelocity( velocity , oldVelocity );
        velocity = velocity.normalized * currentSpeed;

        ApplyAltitudeCorrection();

        // Landing is intentionally moving onto a surface — don't let collision stop it short.
        if ( parameters.modules.collision && state != PreyState.Landing ) position = CollideMove( position , velocity );
        else                                                              position += velocity;

        // Hard floor: a bird must NEVER end a frame below the ground (safety net beyond the sweep —
        // e.g. fleeing straight down off a perch when the wren is above it). Landing is exempt so the
        // clamp doesn't stop the bird a radius above its perch target.
        if ( parameters.modules.collision && state != PreyState.Landing ) ClampAboveGround();
    }

    // Raycast straight down and keep the body at least `radius` above the ground.
    private void ClampAboveGround()
    {
        var c = parameters.collision;
        const float castStart = 5f;   // start above the bird so we catch it even if it dipped slightly under
        PreyProfiler.raycastCount++;
        if ( Physics.Raycast( position + Vector3.up * castStart , Vector3.down , out var hit ,
                              castStart + 10000f , c.layers , QueryTriggerInteraction.Ignore ) ) {
            float minY = hit.point.y + c.radius;
            if ( position.y < minY ) {
                position.y = minY;
                if ( velocity.y < 0f ) velocity.y = 0f;   // stop driving further into the floor
            }
        }
    }

    // Swept collide-and-slide: sweep a sphere from `from` along `move`; stop at the first surface
    // (minus skin) and slide the remaining motion along it so the bird can't pass through geometry.
    private Vector3 CollideMove( Vector3 from , Vector3 move )
    {
        var   c    = parameters.collision;
        float dist = move.magnitude;
        if ( dist < 1e-5f ) return from + move;
        var dir = move / dist;

        PreyProfiler.spherecastCount++;
        if ( !Physics.SphereCast( from , c.radius , dir , out var hit , dist + c.skin , c.layers , QueryTriggerInteraction.Ignore ) )
            return from + move;   // clear path

        float allowed = Mathf.Max( 0f , hit.distance - c.skin );
        var   stopped = from + dir * allowed;

        if ( !c.slide ) { velocity = Vector3.ProjectOnPlane( velocity , hit.normal ); return stopped; }

        // slide the leftover motion along the contact plane; kill the into-surface velocity
        var slid = Vector3.ProjectOnPlane( ( dist - allowed ) * dir , hit.normal );
        velocity = Vector3.ProjectOnPlane( velocity , hit.normal );

        // one extra sweep so sliding into a second surface (a corner) doesn't tunnel
        if ( slid.sqrMagnitude > 1e-6f ) {
            float sd = slid.magnitude;
            PreyProfiler.spherecastCount++;
            if ( Physics.SphereCast( stopped , c.radius , slid / sd , out var hit2 , sd + c.skin , c.layers , QueryTriggerInteraction.Ignore ) )
                slid = slid.normalized * Mathf.Max( 0f , hit2.distance - c.skin );
        }

        return stopped + slid;
    }

    private void ApplyAltitudeCorrection()
    {
        if ( !parameters.modules.altitude ) return;
        if ( state == PreyState.Updrafting ) return; // updraft owns altitude while riding
        // searching/landing own their own Y (e.g. diving to a perch) — desired-altitude must not
        // fight the descent, otherwise the bird never reaches the ground to land.
        if ( state == PreyState.Searching || state == PreyState.Landing ) return;

        var alt = parameters.altitude;
        float targetVY;

        if ( distanceToGround < alt.desiredAltitudeMin ) {
            targetVY = (alt.desiredAltitudeMin - distanceToGround) * alt.strengthTowardsDesiredAltitude;
        } else if ( distanceToGround > alt.desiredAltitudeMax ) {
            targetVY = -(distanceToGround - alt.desiredAltitudeMax) * alt.strengthTowardsDesiredAltitude;
        } else {
            targetVY = 0f;
        }

        targetVY = Mathf.Clamp( targetVY , -currentSpeed * 0.8f , currentSpeed * 0.8f );

        allForces.Add( new PreyForce( Vector3.up * targetVY , new Color( 0.4f , 0.8f , 1f ) , "altitude" ) );

        // cancel any opposing vertical momentum before lerping so forces and velocity always agree
        if ( targetVY > 0f && velocity.y < 0f ) velocity.y = 0f;
        if ( targetVY < 0f && velocity.y > 0f ) velocity.y = 0f;

        velocity.y = Mathf.Lerp( velocity.y , targetVY , 0.2f );

        // recompute horizontal components to maintain currentSpeed
        float hSpeed = Mathf.Sqrt( Mathf.Max( 0f , currentSpeed * currentSpeed - velocity.y * velocity.y ) );
        var h = new Vector2( velocity.x , velocity.z );

        if ( h.sqrMagnitude > 0.0001f ) {
            h = h.normalized * hSpeed;
        }

        velocity.x = h.x;
        velocity.z = h.y;
    }

    public Vector3 GetNewVelocity( Vector3 desired , Vector3 current )
    {
        float angle = Vector3.Angle( current , desired );

        if ( angle < parameters.movement.maxAngleTurnBetweenFrames ) {
            return desired;
        }

        var axis = Vector3.Cross( current , desired ).normalized;

        if ( axis == Vector3.zero ) {
            axis = Vector3.Cross( Random.insideUnitSphere.normalized , current ).normalized;
        }

        return Quaternion.AngleAxis( parameters.movement.maxAngleTurnBetweenFrames , axis ) * current;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Flapping
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void DoFlapInfo()
    {
        climbRate = Mathf.Clamp( velocity.normalized.y , 0 , 1 );

        if ( climbRate > 0.01f ) {
            // power-flap while climbing
            positionInFlapCycle += parameters.flap.flapSpeed * _flapSpeedMult * climbRate * climbRate;
            // reset so we glide briefly after levelling out before the next burst
            ambientFlapsInBurst = 0;
            ambientGlideTimer   = Random.Range( parameters.flap.glideTimeMin , parameters.flap.glideTimeMax );
        } else if ( parameters.flap.defaultFlapRate > 0f ) {
            // ambient burst/glide state machine
            if ( ambientFlapsInBurst > 0 ) {
                float prev = positionInFlapCycle;
                positionInFlapCycle += parameters.flap.defaultFlapRate * _flapSpeedMult;
                int completed = (int)(positionInFlapCycle / (Mathf.PI * 2f)) - (int)(prev / (Mathf.PI * 2f));
                if ( completed > 0 ) {
                    ambientFlapsInBurst = Mathf.Max( 0 , ambientFlapsInBurst - completed );
                    if ( ambientFlapsInBurst == 0 )
                        ambientGlideTimer = Random.Range( parameters.flap.glideTimeMin , parameters.flap.glideTimeMax );
                }
            } else {
                // gliding — wings settle to mid-cycle rest pose
                float cycleFloor = Mathf.Floor( positionInFlapCycle / (Mathf.PI * 2f) );
                positionInFlapCycle = Mathf.Lerp( positionInFlapCycle , cycleFloor * Mathf.PI * 2f + Mathf.PI , 0.1f );
                ambientGlideTimer -= simDt;
                if ( ambientGlideTimer <= 0f )
                    ambientFlapsInBurst = DrawFlapClusterSize( parameters.flap.medianFlapCluster );
            }
        } else {
            // defaultFlapRate = 0: original behavior — wings settle when not climbing
            float currentCycle = Mathf.Floor( positionInFlapCycle / (Mathf.PI * 2f) );
            float mid = currentCycle * Mathf.PI * 2f + Mathf.PI;
            positionInFlapCycle = Mathf.Lerp( positionInFlapCycle , mid , .1f );
        }

        flapValue = transform.up * Mathf.Sin( positionInFlapCycle ) * parameters.flap.upBounceSize
                    + transform.forward * Mathf.Sin( positionInFlapCycle + parameters.flap.forwardBounceOffset ) *
                    parameters.flap.forwardBounceSize;
    }

    private static int DrawFlapClusterSize( float median )
    {
        // Geometric distribution: each extra flap continues with prob (median-1)/median
        // mean = 1/(1-p) = median  →  p = (median-1)/median
        int   count = 1;
        float p     = Mathf.Clamp01( (median - 1f) / Mathf.Max( median , 1f ) );
        while ( count < 20 && Random.value < p ) count++;
        return count;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Raycasts
    // ─────────────────────────────────────────────────────────────────────────

    public Vector4 RaycastDown()
    {
        PreyProfiler.raycastCount++;
        if ( Physics.Raycast( transform.position , -transform.up , out var hit , parameters.distance.maxDownDistance ) ) {
            float d = hit.point.y < parameters.altitude.minimumTotalY
                ? transform.position.y - parameters.altitude.minimumTotalY
                : hit.distance;
            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , d );
        }

        return new Vector4( 0 , 1 , 0 , transform.position.y - parameters.altitude.minimumTotalY );
    }

    public Vector4 RaycastForward()
    {
        PreyProfiler.raycastCount++;
        if ( Physics.Raycast( transform.position , transform.forward , out var hit , parameters.distance.maxForwardDistance ) ) {
            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , hit.distance );
        }

        return new Vector4( 0 , 1 , 0 , parameters.distance.maxForwardDistance );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Despawn / eat
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void CheckForDespawn()
    {
        if ( spawning ) return;

        if ( vectorToWren.magnitude <= parameters.crystals.eatRadius ) {
            manager.PreyGotAte( this );
            spawning = true;
            StartCoroutine( DestroyCoroutine( parameters.animation.ateDieSpeed ) );
            return;
        }

        // Perched birds CAN despawn (e.g. the wren left the cage). Only the transient/active
        // states are exempt so we don't kill a bird mid-landing / mid-takeoff / mid-updraft.
        if ( state == PreyState.Landing
             || state == PreyState.Updrafting || state == PreyState.TakingOff ) {
            return;
        }

        if ( Time.time - spawnTime > manager.minimumTimeAlive ) {
            bool outsideNow = IsOutsideDespawnRegion();

            if ( outsideNow && !isOutsideRegion ) OnLeaveRegion();
            else if ( !outsideNow && isOutsideRegion ) OnEnterRegion();
        }

        if ( isOutsideRegion ) {
            timeOutsideRegion += simDt;
            if ( timeOutsideRegion >= manager.timeOutsideBeforeDespawn ) {
                OnNotCaught();
            }
        }
    }

    // "Outside" test per the manager's despawn type (distance to wren / despawn collider / cage).
    private bool IsOutsideDespawnRegion()
    {
        bool useWren = manager.despawnSubject == DespawnSubject.Wren;
        switch ( manager.despawnType ) {
            case DespawnType.Collider:
                return useWren ? manager.wrenOutsideDespawnCollider
                               : manager.IsOutsideDespawnCollider( position );
            case DespawnType.Region:
                return useWren ? manager.wrenOutsideRegion
                               : manager.IsOutsideRegion( position );
            case DespawnType.Distance:
            default:
                return vectorToWren.magnitude > manager.distanceBeforeNotCaught;   // symmetric (prey↔wren)
        }
    }

    public virtual void OnEnterRegion()
    {
        isOutsideRegion   = false;
        timeOutsideRegion = 0f;
    }

    public virtual void OnLeaveRegion()
    {
        isOutsideRegion   = true;
        timeOutsideRegion = 0f;
    }

    private void OnNotCaught()
    {
        if ( !spawning ) {
            spawning = true;
            StartCoroutine( DestroyCoroutine( parameters.animation.dieSpeed ) );
        }
    }

    public void ForceDespawn()
    {
        if ( !spawning ) {
            spawning = true;
            StartCoroutine( DestroyCoroutine( parameters.animation.dieSpeed ) );
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spawn / die coroutines
    // ─────────────────────────────────────────────────────────────────────────

    public IEnumerator SpawnCoroutine( float speed )
    {
        spawning     = true;
        isDespawning = false;
        life         = 0;
        float start = Time.time;
        float end = start + speed;

        while (Time.time < end) {
            life = Mathf.InverseLerp( start , end , Time.time );
            WhileSpawning( life );
            yield return null;
        }

        life = 1;
        spawning = false;
    }

    public IEnumerator DestroyCoroutine( float speed )
    {
        spawning     = true;
        isDespawning = true;
        float start  = Time.time;

        while (Time.time < start + speed) {
            life = Mathf.Lerp( 1 , 0 , (Time.time - start) / speed );
            WhileSpawning( life );
            yield return null;
        }

        DestroyImmediate( gameObject );
    }

    public virtual void WhileSpawning( float t )
    {
        float s = Mathf.Clamp( 1 - (t - parameters.scale.maxScaleStartLife) , 0 , 1 );
        s = Mathf.Min( Mathf.Clamp( 1 - (parameters.scale.maxScaleEndLife - t) , 0 , 1 ) , s );
        transform.localScale = Vector3.one * parameters.scale.maxScale * s;
    }

    public void QuickKill()
    {
        DestroyImmediate( gameObject );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug gizmos  (redraws from allForces every editor repaint — persists on step)
    // ─────────────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if ( parameters == null ) return;

        float scale  = 20f;
        var   origin = transform.position;

        var dbg = parameters.debug;

        // ── Force arrows ─────────────────────────────────────────────────────
        if ( dbg.showForceArrows ) {
            foreach ( var pf in allForces ) {
                if ( pf.force.sqrMagnitude < 0.00001f ) continue;
                Gizmos.color = pf.color;
                var tip = DrawGizmoArrow( origin , pf.force * scale );
#if UNITY_EDITOR
                UnityEditor.Handles.Label( tip , pf.name , GizmoLabel( pf.color ) );
#endif
            }
        }

        // ── Velocity arrows ──────────────────────────────────────────────────
        if ( dbg.showVelocity ) {
            var desiredColor = new Color( 0.4f , 0.9f , 1f );
            Gizmos.color = desiredColor;
            var desiredTip = DrawGizmoArrow( origin , desiredVelocity * scale );
#if UNITY_EDITOR
            UnityEditor.Handles.Label( desiredTip , "desired vel" , GizmoLabel( desiredColor ) );
#endif
            Gizmos.color = Color.white;
            var velTip = DrawGizmoArrow( origin , velocity * scale );
#if UNITY_EDITOR
            UnityEditor.Handles.Label( velTip , "velocity" , GizmoLabel( Color.white ) );
#endif
        }

        // ── Flap arrow ───────────────────────────────────────────────────────
        if ( dbg.showFlap && parameters.modules.flap && flapValue.sqrMagnitude > 0.00001f ) {
            float cycleT    = (positionInFlapCycle % (Mathf.PI * 2f)) / (Mathf.PI * 2f);
            var   flapColor = Color.Lerp( new Color( 0.2f , 1f , 0.4f ) , new Color( 0.2f , 0.4f , 1f ) , cycleT );
            Gizmos.color    = flapColor;
            var flapTip     = DrawGizmoArrow( origin , flapValue );
#if UNITY_EDITOR
            UnityEditor.Handles.Label( flapTip , $"flap  {cycleT:F2}" , GizmoLabel( flapColor ) );
#endif
        }

#if UNITY_EDITOR
        // ── State label ──────────────────────────────────────────────────────
        if ( dbg.showStateLabel ) {
            Color stateColor;
            if      ( state == PreyState.Calm      ) stateColor = Color.green;
            else if ( state == PreyState.Searching ) stateColor = new Color( 0.6f , 0.2f , 1f );
            else if ( state == PreyState.Disturbed ) stateColor = Color.red;
            else if ( state == PreyState.Landing   ) stateColor = Color.yellow;
            else if ( state == PreyState.Updrafting) stateColor = Color.green;
            else if ( state == PreyState.Perched   ) stateColor = new Color( 0.3f , 0.7f , 1f );
            else if ( state == PreyState.TakingOff ) stateColor = new Color( 1f , 0.6f , 0.1f );
            else                                     stateColor = Color.white;

            string stateLabel = state.ToString();
            if ( state == PreyState.Calm && parameters.modules.search ) {
                if ( searchState.searchCooldown > 0f )
                    stateLabel += $"  (can search in {searchState.searchCooldown:F1}s)";
                else {
                    float untilSearch = searchState.nextSearchTime - searchState.calmTimer;
                    stateLabel += $"  (search in {Mathf.Max( untilSearch , 0f ):F1}s)";
                }
            } else if ( state == PreyState.Perched && parameters.modules.perch ) {
                float remaining = perchState.currentPerchDuration - perchState.perchedTimer;
                stateLabel += $"  {remaining:F1}s";
            } else if ( state == PreyState.Updrafting && updraftState.activeTarget != null ) {
                float remaining = updraftState.activeTarget.timeToRemainInterested - updraftState.remainTimer;
                stateLabel += $"  ride {Mathf.Max( remaining , 0f ):F1}s";
            }

            UnityEditor.Handles.Label( origin + Vector3.up * 2f , stateLabel , GizmoLabel( stateColor ) );
        }

        // ── Calm search-readiness debug ───────────────────────────────────────
        if ( dbg.showCalmDebug && parameters.modules.search && state == PreyState.Calm ) {
            string inRangeLabel = "none in range";
            if ( manager?.interestPoints != null ) {
                foreach ( var ip in manager.interestPoints ) {
                    if ( ip == null ) continue;
                    if ( ip.IsWithin( position , ip.noticeRadius ) ) {
                        inRangeLabel = $"can notice: {ip.type} \"{ip.name}\"";
                        break;
                    }
                }
            }
            float untilForced = searchState.nextSearchTime - searchState.calmTimer;
            string cooldownStr = searchState.searchCooldown > 0f ? $"  [cooldown {searchState.searchCooldown:F1}s]" : "";
            string calmDebug = $"calm {searchState.calmTimer:F1}s  /  forced in {untilForced:F1}s{cooldownStr}\n{inRangeLabel}";
            UnityEditor.Handles.Label( origin + Vector3.up * 3.3f , calmDebug , GizmoLabel( new Color( 0.4f , 0.9f , 0.4f ) ) );
        }

        // ── Search module debug ───────────────────────────────────────────────
        if ( dbg.showSearchDebug && parameters.modules.search && state == PreyState.Searching
             && searchState.currentTarget?.transform != null ) {
            var targetPos = CurrentSearchTargetPos();
            Gizmos.color = new Color( 0.6f , 0.2f , 1f , 0.8f );
            Gizmos.DrawLine( origin , targetPos );
            UnityEditor.Handles.color = new Color( 0.6f , 0.2f , 1f , 0.4f );
            UnityEditor.Handles.DrawWireDisc( targetPos , Vector3.up , searchState.currentTarget.enterRadius );
            float remaining = parameters.search.giveUpTime - searchState.searchTimer;
            UnityEditor.Handles.Label(
                origin + Vector3.up * 3.5f ,
                $"→ {searchState.currentTarget.type}  give up in {remaining:F1}s" ,
                GizmoLabel( new Color( 0.6f , 0.2f , 1f ) ) );
        }

        // ── Landing target line ───────────────────────────────────────────────
        if ( dbg.showLandDebug && state == PreyState.Landing && HasPerchTarget() ) {
            var landSpot = PerchSurface();
            var landCol  = new Color( 1f , 0.85f , 0.1f , 0.9f );
            Gizmos.color = landCol;
            Gizmos.DrawLine( origin , landSpot );
            UnityEditor.Handles.color = new Color( landCol.r , landCol.g , landCol.b , 0.5f );
            UnityEditor.Handles.DrawWireDisc( landSpot , PerchNormal() , parameters.perch.snapDistance );
            UnityEditor.Handles.Label( landSpot , "land here" , GizmoLabel( landCol ) );
        }

        // ── Run module debug ──────────────────────────────────────────────────
        if ( dbg.showRunDebug && parameters.modules.run ) {
            var   run        = parameters.run;
            float wrenDist   = vectorToWren.magnitude;
            bool  inStartle  = wrenDist < run.startleRadius;
            bool  inFullRun  = wrenDist < run.fullRunRadius;

            UnityEditor.Handles.color = inStartle
                ? new Color( 1f , 0.4f , 0.1f , 0.7f )
                : new Color( 1f , 0.9f , 0.1f , 0.25f );
            UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , run.startleRadius );

            // live predictive reach (grows as the wren closes head-on)
            float effStartle = EffectiveStartle( run.startleRadius , run.startleLeadTime );
            if ( effStartle > run.startleRadius + 0.01f ) {
                UnityEditor.Handles.color = new Color( 1f , 0.55f , 0f , 0.5f );
                UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , effStartle );
                UnityEditor.Handles.Label( origin + new Vector3( 0 , 0 , effStartle ) ,
                    $"startle reach {effStartle:F0}" , GizmoLabel( new Color( 1f , 0.55f , 0f ) ) );
            }

            UnityEditor.Handles.Label(
                origin + new Vector3( run.startleRadius , 0 , 0 ) ,
                inStartle ? "IN startle" : "startle" ,
                GizmoLabel( inStartle ? new Color( 1f , 0.4f , 0.1f ) : new Color( 1f , 0.9f , 0.1f ) ) );

            UnityEditor.Handles.color = inFullRun
                ? new Color( 1f , 0.1f , 0.1f , 0.9f )
                : new Color( 1f , 0.5f , 0.1f , 0.4f );
            UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , run.fullRunRadius );
            UnityEditor.Handles.Label(
                origin + new Vector3( run.fullRunRadius , 0 , 0 ) ,
                inFullRun ? "IN full run" : "full run" ,
                GizmoLabel( inFullRun ? Color.red : new Color( 1f , 0.5f , 0.1f ) ) );

            if ( state == PreyState.Disturbed ) {
                bool wrenFarEnough = wrenDist > run.calmDownDistance;
                UnityEditor.Handles.color = wrenFarEnough
                    ? new Color( 0.5f , 1f , 0.5f , 0.15f )
                    : new Color( 0.8f , 0.3f , 1f , 0.15f );
                UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , run.calmDownDistance );
                UnityEditor.Handles.Label(
                    origin + new Vector3( run.calmDownDistance , 0 , 0 ) ,
                    wrenFarEnough ? "wren outside — calming" : "calm dist" ,
                    GizmoLabel( wrenFarEnough ? Color.green : new Color( 0.8f , 0.3f , 1f ) ) );
                float calmRemaining = run.calmDownTime - runState.calmTimer;
                UnityEditor.Handles.Label(
                    origin + Vector3.up * 3.5f ,
                    $"calm in {calmRemaining:F1}s" ,
                    GizmoLabel( Color.Lerp( Color.red , Color.green , runState.calmTimer / run.calmDownTime ) ) );
                if ( vectorToWren.sqrMagnitude > 0.01f ) {
                    var fleeDir = run.chaseInstead ? vectorToWren.normalized : -vectorToWren.normalized;
                    Gizmos.color = Color.red;
                    DrawGizmoArrow( origin , fleeDir * run.fleeForce * scale * 0.5f );
                }
                if ( runState.jukeDir.sqrMagnitude > 0.001f ) {
                    Gizmos.color = new Color( 1f , 0.3f , 0.6f );
                    DrawGizmoArrow( origin , runState.jukeDir * run.jukeAmount * scale * 0.5f );
                    UnityEditor.Handles.Label(
                        origin + runState.jukeDir * run.jukeAmount * scale * 0.5f ,
                        "juke" , GizmoLabel( new Color( 1f , 0.3f , 0.6f ) ) );
                }
            }
        }

        // eat radius ring
        if ( dbg.showEatRadius ) {
            UnityEditor.Handles.color = new Color( 1f , 0.3f , 0.3f , 0.8f );
            UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , parameters.crystals.eatRadius );
        }

        // spawn-in / spawn-out progress
        if ( spawning ) {
            var   spawnColor = isDespawning ? Color.red : Color.green;
            string spawnLabel = isDespawning
                ? $"Despawning  {life * 100f:F0}%"
                : $"Spawning  {life * 100f:F0}%";
            UnityEditor.Handles.Label( origin + Vector3.up * 4f , spawnLabel , GizmoLabel( spawnColor ) );
        }

        // ── Despawn debug: how/when this bird will despawn (per the manager's despawn type) ────
        // (the leaving countdown lives in this block's summary now, below the bird — no overlap with the state label)
        if ( manager != null && manager.showDespawnDebug ) {
            var   green     = new Color( 0.3f , 1f , 0.4f );
            var   red       = new Color( 1f , 0.3f , 0.2f );
            float alive     = Time.time - spawnTime;
            bool  oldEnough = alive > manager.minimumTimeAlive;

            switch ( manager.despawnType ) {
                case DespawnType.Distance: {
                    var   wrenPos = origin + vectorToWren;
                    float dist    = vectorToWren.magnitude;
                    float thresh  = manager.distanceBeforeNotCaught;
                    bool  outside = dist > thresh;

                    Gizmos.color = outside ? red : green;
                    Gizmos.DrawLine( origin , wrenPos );
                    UnityEditor.Handles.color = new Color( red.r , red.g , red.b , 0.5f );
                    UnityEditor.Handles.DrawWireDisc( wrenPos , Vector3.up , thresh );   // catch boundary around the wren
                    UnityEditor.Handles.Label( (origin + wrenPos) * 0.5f ,
                        $"dist {dist:F0} / {thresh:F0}  {(outside ? "OUTSIDE" : "in range")}" ,
                        GizmoLabel( outside ? red : green ) );
                    break;
                }
                case DespawnType.Collider: {
                    if ( manager.despawnCollider != null ) {
                        var  cp      = manager.despawnCollider.ClosestPoint( origin );
                        bool outside = manager.IsOutsideDespawnCollider( origin );
                        Gizmos.color = outside ? red : green;
                        Gizmos.DrawWireCube( manager.despawnCollider.bounds.center , manager.despawnCollider.bounds.size );
                        Gizmos.DrawLine( origin , cp );
                        UnityEditor.Handles.Label( cp ,
                            outside ? "OUTSIDE despawn collider" : "inside despawn collider" ,
                            GizmoLabel( outside ? red : green ) );
                    } else {
                        UnityEditor.Handles.Label( origin + Vector3.up * 2.6f , "no despawn collider set" , GizmoLabel( red ) );
                    }
                    break;
                }
                case DespawnType.Region: {
                    // Region despawn is wren-based (shared) — show the region relative to the WREN, not this prey.
                    bool outside = manager.wrenOutsideRegion;
                    var  col     = outside ? red : green;
                    var  wrenPos = origin + vectorToWren;
                    Gizmos.color = col;
                    if ( manager.regionType == RegionType.Box && manager.boxRegion != null ) {
                        var c    = manager.boxRegion.position;
                        var half = manager.boxRegion.lossyScale * 0.5f;
                        Gizmos.DrawWireCube( c , manager.boxRegion.lossyScale );
                        var cp = new Vector3(
                            Mathf.Clamp( wrenPos.x , c.x - half.x , c.x + half.x ) ,
                            Mathf.Clamp( wrenPos.y , c.y - half.y , c.y + half.y ) ,
                            Mathf.Clamp( wrenPos.z , c.z - half.z , c.z + half.z ) );
                        Gizmos.DrawLine( wrenPos , cp );   // wren → nearest cage edge
                    } else if ( manager.regionType == RegionType.Collider && manager.regionCollider != null ) {
                        Gizmos.DrawWireCube( manager.regionCollider.bounds.center , manager.regionCollider.bounds.size );
                        Gizmos.DrawLine( wrenPos , manager.regionCollider.ClosestPoint( wrenPos ) );
                    }
                    UnityEditor.Handles.Label( wrenPos + Vector3.up * 1f ,
                        outside ? "wren OUTSIDE cage" : "wren inside cage" , GizmoLabel( col ) );
                    break;
                }
            }

            // eligibility (minimumTimeAlive) + grace (timeOutsideBeforeDespawn) summary — placed BELOW
            // the bird so it never stacks on the state ("Perched") / calm / search labels above.
            string graceStr = isOutsideRegion
                ? $"leaving in {Mathf.Max( manager.timeOutsideBeforeDespawn - timeOutsideRegion , 0f ):F1}s"
                : "in region";
            UnityEditor.Handles.Label( origin - Vector3.up * 0.8f ,
                $"despawn[{manager.despawnType}]  alive {alive:F0}s {(oldEnough ? "(eligible)" : $"(min {manager.minimumTimeAlive:F0}s)")}\n{graceStr}" ,
                GizmoLabel( oldEnough ? Color.white : new Color( 0.6f , 0.6f , 0.6f ) ) );
        }

        // ── Social pressure debug ─────────────────────────────────────────────
        if ( dbg.showSocialDebug && parameters.modules.social ) {
            var soc = parameters.social;

            UnityEditor.Handles.color = new Color( 1f , 0.8f , 0.2f , 0.18f );
            UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , soc.neighborRadius );
            UnityEditor.Handles.Label(
                origin + new Vector3( soc.neighborRadius , 0 , 0 ) ,
                "social" , GizmoLabel( new Color( 1f , 0.8f , 0.2f ) ) );

            foreach ( var inf in socialState.influences ) {
                if ( inf.bird == null ) continue;
                Color lc;
                switch ( inf.sourceState ) {
                    case PreyState.TakingOff: lc = new Color( 1f  , 0.6f , 0.1f , Mathf.Clamp01( inf.weight ) ); break;
                    case PreyState.Disturbed: lc = new Color( 1f  , 0.2f , 0.2f , Mathf.Clamp01( inf.weight ) ); break;
                    case PreyState.Perched:
                    case PreyState.Landing:   lc = new Color( 0.3f , 0.7f , 1f  , Mathf.Clamp01( inf.weight ) ); break;
                    default:                  lc = new Color( 0.4f , 1f   , 0.4f , Mathf.Clamp01( inf.weight ) ); break;
                }
                Gizmos.color = lc;
                Gizmos.DrawLine( origin , inf.bird.transform.position );
            }

            string desireLabel = $"↑takeOff {socialState.desireToTakeOff:F2}/{soc.takeOffThreshold:F1}\n" +
                                 $"⚡disturb {socialState.desireToDisturb:F2}/{soc.disturbThreshold:F1}\n" +
                                 $"↓calm    {socialState.desireToCalmDown:F2}/{soc.calmThreshold:F1}\n" +
                                 $"⬤land    {socialState.desireToLand:F2}/{soc.landThreshold:F1}";
            UnityEditor.Handles.Label( origin + Vector3.up * 5.5f , desireLabel , GizmoLabel( new Color( 1f , 0.8f , 0.2f ) ) );

            // flash last trigger for 3 seconds
            float triggerAge = Time.time - socialState.lastTriggerTime;
            if ( triggerAge < 3f && socialState.lastTriggerLabel.Length > 0 ) {
                float alpha = Mathf.Lerp( 1f , 0f , triggerAge / 3f );
                var   col   = new Color( 1f , 1f , 0f , alpha );
                UnityEditor.Handles.Label( origin + Vector3.up * 4.5f , socialState.lastTriggerLabel , GizmoLabel( col ) );
            }
        }
#endif
    }

#if UNITY_EDITOR
    // One reusable style — recolored per call rather than allocated, so repaints don't churn the GC.
    private static GUIStyle _gizmoLabelStyle;
    private static GUIStyle GizmoLabel( Color col )
    {
        if ( _gizmoLabelStyle == null )
            _gizmoLabelStyle = new GUIStyle( UnityEditor.EditorStyles.label ) { fontSize = 7 };
        _gizmoLabelStyle.normal.textColor = col;
        return _gizmoLabelStyle;
    }
#endif

    private static Vector3 DrawGizmoArrow( Vector3 origin , Vector3 vec )
    {
        var tip = origin + vec;
        if ( vec.sqrMagnitude < 0.0001f ) return tip;

        Gizmos.DrawLine( origin , tip );

        var   dir     = vec.normalized;
        var   right   = Vector3.Cross( dir , Vector3.up );
        if ( right.sqrMagnitude < 0.01f ) right = Vector3.Cross( dir , Vector3.forward );
        right = right.normalized;

        float headLen = Mathf.Min( vec.magnitude * 0.25f , 0.5f );
        Gizmos.DrawLine( tip , tip - dir * headLen + right * headLen * 0.5f );
        Gizmos.DrawLine( tip , tip - dir * headLen - right * headLen * 0.5f );
        return tip;
    }
}