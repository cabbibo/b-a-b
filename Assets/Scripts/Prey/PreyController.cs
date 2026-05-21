using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

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
    public Transform    cage;
    public float        spawnTime;
    public Vector3      spawnPoint;
    public bool         spawning;

    // ── Observable data (shown in inspector for debugging) ────────────────────
    [Header( "State" )]
    public PreyState state;

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

    private LineRenderer focusLine;

    // ── Private physics ───────────────────────────────────────────────────────
    private Vector3 startPosition;
    private Vector4 rayCastData;
    private int     frame;
    private float   noiseOffset;
    private float   circleRuntimeAngle;
    private float   currentBank;
    private float   currentSpeed;

    // ── Runtime state (one per module that needs per-instance state) ──────────
    private class PerchRuntimeState
    {
        public Transform target;
        public float     landDesireTimer;
        public float     perchedTimer;
        public float     currentPerchDuration;

        public void Init( PreyPerchModule cfg )
        {
            target = null;
            landDesireTimer = cfg.landDesireInterval + Random.Range( -cfg.landDesireVariance , cfg.landDesireVariance );
            perchedTimer = 0;
            currentPerchDuration = 0;
        }
    }

    private class TakeOffRuntimeState
    {
        public float   timer;
        public float   circleTimer;
        public float   circleAngle;
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
        public UpdraftZone zone;
    }

    private class ThermalRuntimeState
    {
        public float circleAngle;
        public bool  isThermaling;
    }

    private class RunRuntimeState
    {
        public float   calmTimer;
        public float   jukeTimer;
        public Vector3 jukeDir;
    }

    private PerchRuntimeState   perchState   = new();
    private TakeOffRuntimeState takeOffState = new();
    private FlockRuntimeState   flockState   = new();
    private SplineRuntimeState  splineState  = new();
    private UpdraftRuntimeState updraftState = new();
    private ThermalRuntimeState thermalState = new();
    private RunRuntimeState     runState     = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }

    private void Update()
    {
        UpdateFocusLine();

        if ( stepThrough || (manager != null && manager.stepThrough) ) {
            if ( !stepForward ) return;
            stepForward = false;
        }

        UpdateVectorToWren();
        UpdateData();
        UpdateModuleStates();
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
        cage       = mgr.cage;

        // seed per-instance randomness
        noiseOffset = Random.Range( 0f , 100f );
        circleRuntimeAngle = Random.Range( 0f , Mathf.PI * 2f );
        thermalState.circleAngle = Random.Range( 0f , Mathf.PI * 2f );

        if ( parameters.modules.perch ) {
            perchState.Init( parameters.perch );
        }

        SetHeight();

        force = Vector3.zero;
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

        focusLine = gameObject.AddComponent<LineRenderer>();
        focusLine.positionCount = 2;
        focusLine.startWidth    = 0.15f;
        focusLine.endWidth      = 0.04f;
        focusLine.useWorldSpace = true;
        focusLine.startColor    = new Color( 1f , 1f , 1f , 0.9f );
        focusLine.endColor      = new Color( 1f , 1f , 1f , 0.2f );
        focusLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        focusLine.receiveShadows    = false;

        var focusShader = Shader.Find( "Prey/FocusLine" );
        if ( focusShader != null )
            focusLine.material = new Material( focusShader );

        focusLine.enabled = false;

        StartCoroutine( SpawnCoroutine( config.spawn.spawnSpeed ) );
        OnInitialize();
    }

    protected virtual void OnInitialize()
    {
    }

    public void SetHeight()
    {
        if ( Physics.Raycast( transform.position , -transform.up , out var hit , 100000 ) ) {
            transform.position = hit.point + transform.up *
                Mathf.Lerp( parameters.altitude.minAltitude , parameters.altitude.maxAltitude , Random.value );
            position = transform.position;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Wren / data updates
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateFocusLine()
    {
        if ( focusLine == null || parameters == null ) return;

        var wren = God.wren != null         ? God.wren.transform
                 : manager?.debugWren != null ? manager.debugWren.transform
                 : null;

        if ( wren == null ) { focusLine.enabled = false; return; }

        bool inFocus = Vector3.Distance( transform.position , wren.position ) <= parameters.crystals.focusRadius;
        focusLine.enabled = inFocus;

        if ( inFocus ) {
            focusLine.SetPosition( 0 , transform.position );
            focusLine.SetPosition( 1 , wren.position );

            var mat = focusLine.material;
            mat.SetVector( "_BirdPos"   , transform.position );
            mat.SetFloat(  "_EatRadius" , parameters.crystals.eatRadius );
        }
    }

    private void UpdateVectorToWren()
    {
        var wren = God.wren != null ? God.wren.transform
            : manager.debugWren != null ? manager.debugWren.transform
            : null;
        vectorToWren = wren != null ? wren.position - transform.position : Vector3.one * 9999f;
    }

    public void UpdateData()
    {
        frame++;

        if ( frame % parameters.physics.physicsResolution == 0 ) {
            rayCastData = RaycastDown();
            rawDistanceToGround = rayCastData.w;
            rawGroundNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );

            rayCastData = RaycastForward();
            rawDistanceToForward = rayCastData.w;
            rawForwardNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );
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

    private void UpdateModuleStates()
    {
        if ( parameters.modules.flock ) {
            UpdateFlockState();
        }

        if ( parameters.modules.updraft ) {
            UpdateUpdraftState();
        }

        if ( parameters.modules.spline ) {
            UpdateSplineState();
        }
    }

    private void UpdateFlockState()
    {
        flockState.queryTimer += Time.deltaTime;

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

    private void UpdateUpdraftState()
    {
        updraftState.zone = UpdraftZone.FindNearest( position , parameters.updraft.detectionRadius );
    }

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
        switch (state) {

            case PreyState.Calm:
                if ( parameters.modules.run && vectorToWren.magnitude < parameters.run.startleRadius ) {
                    EnterDisturbed();
                    break;
                }

                if ( parameters.modules.perch ) {
                    perchState.landDesireTimer -= Time.deltaTime;

                    if ( perchState.landDesireTimer <= 0 ) {
                        TryStartLanding();
                    }
                }

                break;

            case PreyState.Landing:
                if ( parameters.modules.run && vectorToWren.magnitude < parameters.run.startleRadius ) {
                    EnterDisturbed();
                    break;
                }

                if ( perchState.target == null ) {
                    EnterCalm();
                    break;
                }

                if ( Vector3.Distance( position , perchState.target.position ) < parameters.perch.snapDistance ) {
                    EnterPerched();
                }

                break;

            case PreyState.Perched:
                bool wrenClose = vectorToWren.magnitude < parameters.perch.startleRadius;
                perchState.perchedTimer += Time.deltaTime;
                bool timerExpired = perchState.perchedTimer >= perchState.currentPerchDuration;

                if ( wrenClose || timerExpired ) {
                    if ( parameters.modules.takeOff ) {
                        EnterTakeOff();
                    } else {
                        EnterCalm();
                    }
                }

                break;

            case PreyState.TakingOff:
                takeOffState.timer += Time.deltaTime;

                if ( takeOffState.timer > parameters.takeOff.duration ) {
                    if ( parameters.modules.circle ) {
                        takeOffState.circleTimer += Time.deltaTime;

                        if ( takeOffState.circleTimer >= parameters.takeOff.circleAfter ) {
                            EnterCalm();
                        }
                    } else {
                        EnterCalm();
                    }
                }

                break;

            case PreyState.Disturbed:
                runState.calmTimer += Time.deltaTime;

                if ( runState.calmTimer > parameters.run.calmDownTime &&
                     vectorToWren.magnitude > parameters.run.calmDownDistance ) {
                    EnterCalm();
                }

                break;
        }
    }

    private void EnterCalm()
    {
        state = PreyState.Calm;
        circleRuntimeAngle = Random.Range( 0f , Mathf.PI * 2f );

        if ( parameters.modules.perch ) {
            perchState.Init( parameters.perch );
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

    private void TryStartLanding()
    {
        var target = FindBestPerchTarget();

        if ( target == null ) {
            // no perch available — back off and try again sooner
            perchState.landDesireTimer = parameters.perch.landDesireInterval * 0.5f;
            return;
        }

        perchState.target = target;
        state = PreyState.Landing;
    }

    private void EnterPerched()
    {
        state = PreyState.Perched;
        velocity = Vector3.zero;
        flapValue = Vector3.zero;
        position = perchState.target.position;
        transform.position = position;
        perchState.perchedTimer = 0;
        perchState.currentPerchDuration = parameters.perch.perchDuration
                                          + Random.Range( -parameters.perch.perchDurationVariance ,
                                              parameters.perch.perchDurationVariance );
    }

    private void EnterTakeOff()
    {
        state = PreyState.TakingOff;
        takeOffState.timer = 0;
        takeOffState.circleTimer = 0;
        takeOffState.circleAngle = Random.Range( 0f , Mathf.PI * 2f );
        takeOffState.runDirection = vectorToWren.sqrMagnitude > 0.01f
            ? -vectorToWren.normalized
            : Random.insideUnitSphere.normalized;
        perchState.target = null;
    }

    private Transform FindBestPerchTarget()
    {
        // manager's explicit list first
        if ( manager.perchPoints != null && manager.perchPoints.Length > 0 ) {
            return PerchPoint.FindNearest( position , 200f , manager.perchPoints );
        }

        // fall back to global registry
        return PerchPoint.FindNearest( position , 100f );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Physics dispatch
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void DoPhysics()
    {
        allForces.Clear();

        switch (state) {
            case PreyState.Calm: DoCalmPhysics(); break;
            case PreyState.Landing: DoLandingPhysics(); break;
            case PreyState.Perched: DoPerchedPhysics(); break;
            case PreyState.TakingOff: DoTakeOffPhysics(); break;
            case PreyState.Disturbed: DoDisturbedPhysics(); break;
        }
    }

    // ── Calm ─────────────────────────────────────────────────────────────────

    private void DoCalmPhysics()
    {
        force = Vector3.zero;

        AddAvoidanceForces();

        if ( parameters.modules.noise )  AddForce( NoiseForce()       , Color.yellow            , "noise" );
        if ( parameters.modules.flock )  AddForce( FlockForce()       , Color.cyan              , "flock" );
        if ( parameters.modules.spline ) AddForce( SplineForce()      , Color.blue              , "spline" );

        if ( parameters.modules.updraft && updraftState.zone != null )
            AddForce( UpdraftForce() , Color.green , "updraft" );

        if ( parameters.modules.thermal )
            AddForce( ThermalForce() , new Color( 1f , 0.5f , 0f ) , "thermal" );

        if ( parameters.modules.circle )
            AddForce( CalmCircleForce() , Color.magenta , "circle" );

        if ( parameters.modules.perch && manager.GetClosestAnchorPoints( position ) != null )
            AddForce( AnchorForce() , Color.white , "anchor" );

        if ( parameters.modules.cage )
            AddForce( CageForce() , new Color( 1f , 0.8f , 0f ) , "cage" );

        ApplyVelocity( parameters.movement.desiredSpeed );

        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }

        transform.position = position + flapValue;
    }

    // ── Landing ──────────────────────────────────────────────────────────────

    private void DoLandingPhysics()
    {
        if ( perchState.target == null ) {
            return;
        }

        force = Vector3.zero;

        var toTarget = perchState.target.position - position;
        float dist = toTarget.magnitude;

        // slow approach speed the closer we get
        float t = Mathf.Clamp01( dist / (parameters.perch.snapDistance * 8f) );
        float approachSpd = Mathf.Lerp( parameters.movement.desiredSpeed * parameters.perch.approachSpeedMult ,
            parameters.movement.desiredSpeed , t );

        force += toTarget.normalized;
        force += MoveAlongGroundAndTurnAwayFromObstacles() * 0.3f;
        if ( parameters.modules.cage ) force += CageForce();

        ApplyVelocity( approachSpd );

        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }

        transform.position = position + flapValue;
    }

    // ── Perched ──────────────────────────────────────────────────────────────

    private void DoPerchedPhysics()
    {
        velocity = Vector3.zero;
        flapValue = Vector3.zero;

        if ( perchState.target != null ) {
            position = perchState.target.position;
        }

        transform.position = position;
    }

    // ── Taking off ───────────────────────────────────────────────────────────

    private void DoTakeOffPhysics()
    {
        force = Vector3.zero;

        if ( takeOffState.timer < parameters.takeOff.duration ) {
            // burst phase
            force += Vector3.up * parameters.takeOff.upForce;
            force += takeOffState.runDirection * parameters.takeOff.runForce;
            force += MoveAlongGroundAndTurnAwayFromObstacles();
        } else if ( parameters.modules.circle ) {
            // circle phase (only reached when circle module is on)
            takeOffState.circleAngle += Time.deltaTime;
            var wrenPos = transform.position + vectorToWren;
            var target = wrenPos
                         + new Vector3( Mathf.Cos( takeOffState.circleAngle ) , 0 , Mathf.Sin( takeOffState.circleAngle ) )
                         * parameters.circle.circleRadius
                         + Vector3.up * parameters.takeOff.circleHeight;
            force += (target - transform.position).normalized * parameters.circle.circleForce;
            force += MoveAlongGroundAndTurnAwayFromObstacles();
        }

        if ( parameters.modules.cage ) force += CageForce();

        ApplyVelocity( parameters.movement.desiredSpeed );

        if ( parameters.modules.flap ) {
            DoFlapInfo();
        } else {
            flapValue = Vector3.zero;
        }

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
        runState.jukeTimer += Time.deltaTime;

        if ( runState.jukeTimer > 1f / Mathf.Max( parameters.run.jukeFrequency , 0.01f ) ) {
            runState.jukeTimer = 0;
            runState.jukeDir = Vector3.Cross( vectorToWren.normalized , Vector3.up ).normalized
                               * (Random.value > 0.5f ? 1f : -1f);
        }

        force += runState.jukeDir * parameters.run.jukeAmount;

        force += MoveAlongGroundAndTurnAwayFromObstacles();
        if ( parameters.modules.cage ) force += CageForce();

        ApplyVelocity( parameters.movement.desiredSpeed * parameters.run.speedMultiplier );

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

    private Vector3 NoiseForce()
    {
        float t = Time.time * parameters.noise.noiseSpeed + noiseOffset;
        return new Vector3(
            Mathf.PerlinNoise( t , 0f ) - 0.5f ,
            Mathf.PerlinNoise( 0f , t ) - 0.5f ,
            Mathf.PerlinNoise( t + 50f , t ) - 0.5f
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
        if ( splineState.spline == null ) {
            return Vector3.zero;
        }

        float t;
        var nearest = splineState.spline.GetNearestPoint( position , out t );
        splineState.currentT = t;
        var toSpline = nearest - position;
        float dist = toSpline.magnitude;

        if ( dist < parameters.spline.pullRadius ) {
            return splineState.spline.GetForwardAt( t ) * parameters.spline.pullForce;
        } else {
            return toSpline.normalized * parameters.spline.returnForce;
        }
    }

    private Vector3 UpdraftForce()
    {
        if ( updraftState.zone == null ) {
            return Vector3.zero;
        }

        var toCenter = updraftState.zone.transform.position - position;
        toCenter.y = 0;
        var tangent = Vector3.Cross( Vector3.up , toCenter.normalized );

        return tangent * parameters.updraft.spiralForce
               + Vector3.up * parameters.updraft.liftForce * updraftState.zone.strength;
    }

    private Vector3 ThermalForce()
    {
        if ( manager == null ) {
            return Vector3.zero;
        }

        var center = manager.GetClosestThermalCenter( position );

        if ( center == null ) {
            return Vector3.zero;
        }

        var t = parameters.thermal;
        bool thermaling = distanceToGround < t.minAltitude;
        thermalState.isThermaling = thermaling;

        thermalState.circleAngle += Time.deltaTime * t.circleSpeed * (thermaling ? 1.5f : 1f);
        float radius = thermaling
            ? parameters.circle.circleRadius * t.thermalTightness
            : parameters.circle.circleRadius;

        var target = center.position
                     + new Vector3( Mathf.Cos( thermalState.circleAngle ) , 0 , Mathf.Sin( thermalState.circleAngle ) ) * radius;

        var f = (target - transform.position).normalized * parameters.circle.circleForce;

        if ( thermaling ) {
            f += Vector3.up * parameters.circle.updraft;
        }

        return f;
    }

    private Vector3 CalmCircleForce()
    {
        circleRuntimeAngle += Time.deltaTime;
        var wrenPos = transform.position + vectorToWren;
        var target = wrenPos
                     + new Vector3( Mathf.Cos( circleRuntimeAngle ) , 0 , Mathf.Sin( circleRuntimeAngle ) )
                     * parameters.circle.circleRadius;
        return (target - transform.position).normalized * parameters.circle.circleForce;
    }

    private Vector3 CageForce()
    {
        if ( cage == null ) return Vector3.zero;

        var half   = cage.lossyScale * 0.5f;
        var center = cage.position;
        var min    = center - half;
        var max    = center + half;
        var cfg    = parameters.cage;
        float d    = cfg.borderTurnDistance;

        var pushDir = Vector3.zero;
        float distMinX = position.x - min.x;
        float distMaxX = max.x - position.x;
        float distMinZ = position.z - min.z;
        float distMaxZ = max.z - position.z;

        if ( distMinX < d ) pushDir.x += 1f - distMinX / d;
        if ( distMaxX < d ) pushDir.x -= 1f - distMaxX / d;
        if ( distMinZ < d ) pushDir.z += 1f - distMinZ / d;
        if ( distMaxZ < d ) pushDir.z -= 1f - distMaxZ / d;

        return pushDir * cfg.borderTurnForce;
    }

    private Vector3 AnchorForce()
    {
        var anchor = manager.GetClosestAnchorPoints( position );

        if ( anchor == null ) {
            return Vector3.zero;
        }

        var toAnchor = anchor.position - position;

        if ( toAnchor.magnitude > parameters.perch.anchorRadius ) {
            return toAnchor.normalized * parameters.perch.anchorPullForce;
        }

        return Vector3.zero;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shared velocity application
    // ─────────────────────────────────────────────────────────────────────────

    private void AddForce( Vector3 f , Color c , string n )
    {
        force += f;
        allForces.Add( new PreyForce( f , c , n ) );
    }

    private void ApplyVelocity( float targetSpeed )
    {
        var m = parameters.movement;

        oldVelocity = velocity;
        velocity += force;
        velocity *= (1f - m.dampening);                               // friction on full vector
        float speed = velocity.magnitude;
        speed += (targetSpeed - speed) * m.dampening;                // drive back toward target
        speed = Mathf.Clamp( speed , m.minSpeed , m.maxSpeed );
        currentSpeed = speed;
        velocity = velocity.sqrMagnitude > 0.0001f
            ? velocity.normalized * speed
            : oldVelocity.normalized * m.minSpeed;

        desiredVelocity = velocity;          // direction forces want to go, before turn-rate limit
        velocity = GetNewVelocity( velocity , oldVelocity );
        velocity = velocity.normalized * currentSpeed;

        ApplyAltitudeCorrection();

        position += velocity;
    }

    private void ApplyAltitudeCorrection()
    {
        if ( !parameters.modules.altitude ) return;

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

        if ( climbRate > 0 ) {
            positionInFlapCycle += parameters.flap.flapSpeed * climbRate * climbRate;
        } else {
            float currentCycle = Mathf.Floor( positionInFlapCycle / (Mathf.PI * 2) );
            float mid = currentCycle * Mathf.PI * 2 + Mathf.PI;
            positionInFlapCycle = Mathf.Lerp( positionInFlapCycle , mid , .1f );
        }

        flapValue = transform.up * Mathf.Sin( positionInFlapCycle ) * parameters.flap.upBounceSize
                    + transform.forward * Mathf.Sin( positionInFlapCycle + parameters.flap.forwardBounceOffset ) *
                    parameters.flap.forwardBounceSize;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Raycasts
    // ─────────────────────────────────────────────────────────────────────────

    public Vector4 RaycastDown()
    {
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
            StartCoroutine( DestroyCoroutine( parameters.spawn.ateDieSpeed ) );
            return;
        }

        if ( state == PreyState.Perched || state == PreyState.Landing || state == PreyState.TakingOff ) {
            return;
        }

        if ( Time.time - spawnTime > parameters.despawn.minimumTimeAlive ) {
            if ( vectorToWren.magnitude > parameters.despawn.distanceBeforeNotCaught ) {
                OnNotCaught();
            }
        }
    }

    private void OnNotCaught()
    {
        if ( !spawning ) {
            spawning = true;
            StartCoroutine( DestroyCoroutine( parameters.spawn.dieSpeed ) );
        }
    }

    public void ForceDespawn()
    {
        if ( !spawning ) {
            spawning = true;
            StartCoroutine( DestroyCoroutine( parameters.spawn.dieSpeed ) );
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spawn / die coroutines
    // ─────────────────────────────────────────────────────────────────────────

    public IEnumerator SpawnCoroutine( float speed )
    {
        spawning = true;
        life = 0;
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
        spawning = true;
        float start = Time.time;

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

        foreach ( var pf in allForces ) {
            if ( pf.force.sqrMagnitude < 0.00001f ) continue;
            Gizmos.color = pf.color;
            var tip = DrawGizmoArrow( origin , pf.force * scale );
#if UNITY_EDITOR
            UnityEditor.Handles.Label( tip , pf.name );
#endif
        }

        // desired velocity — cyan
        Gizmos.color = Color.cyan;
        var desiredTip = DrawGizmoArrow( origin , desiredVelocity * scale );
#if UNITY_EDITOR
        UnityEditor.Handles.Label( desiredTip , "desired vel" );
#endif

        // actual velocity — white
        Gizmos.color = Color.white;
        var velTip = DrawGizmoArrow( origin , velocity * scale );
#if UNITY_EDITOR
        UnityEditor.Handles.Label( velTip , "velocity" );
#endif

        // flap offset — green→blue over cycle
        if ( parameters.modules.flap && flapValue.sqrMagnitude > 0.00001f ) {
            float cycleT  = (positionInFlapCycle % (Mathf.PI * 2f)) / (Mathf.PI * 2f);
            Gizmos.color  = Color.Lerp( new Color( 0.2f , 1f , 0.4f ) , new Color( 0.2f , 0.4f , 1f ) , cycleT );
            var flapTip = DrawGizmoArrow( origin , flapValue );
#if UNITY_EDITOR
            UnityEditor.Handles.Label( flapTip , $"flap  {cycleT:F2}" );
#endif
        }

#if UNITY_EDITOR
        // eat radius ring
        UnityEditor.Handles.color = new Color( 1f , 0.3f , 0.3f , 0.8f );
        UnityEditor.Handles.DrawWireDisc( origin , Vector3.up , parameters.crystals.eatRadius );
#endif
    }

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