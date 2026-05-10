using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PreyController : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────
    public PreyManager  manager;
    public PreyConfigSO parameters;
    public float        spawnTime;
    public Vector3      spawnPoint;
    public bool         spawning;

    // ── Observable data (shown in inspector for debugging) ────────────────────
    [Header( "State" )]
    public PreyState state;
    public float     life;

    [Header( "Physics Data" )]
    public float   distanceToGround;
    public float   rawDistanceToGround;
    public float   distanceToForward;
    public float   rawDistanceToForward;
    public Vector3 groundNormal;
    public Vector3 rawGroundNormal;
    public Vector3 forwardNormal;
    public Vector3 rawForwardNormal;
    public Vector3 vectorToWren;
    public float   forwardTurnNormalizedValue;
    public float   downTurnNormalizedValue;

    [Header( "Movement" )]
    public float   positionInFlapCycle;
    public Vector3 flapValue;
    public float   climbRate;
    public Vector3 force;
    public Vector3 velocity;
    public Vector3 position;
    public Vector3 oldVelocity;

    // ── Private physics ───────────────────────────────────────────────────────
    private Vector3 startPosition;
    private Vector4 rayCastData;
    private int     frame;
    private float   noiseOffset;
    private float   circleRuntimeAngle;
    private float   currentBank;

    // ── Runtime state (one per module that needs per-instance state) ──────────
    private class PerchRuntimeState
    {
        public Transform target;
        public float     landDesireTimer;
        public float     perchedTimer;
        public float     currentPerchDuration;

        public void Init( PreyPerchModule cfg )
        {
            target               = null;
            landDesireTimer      = cfg.landDesireInterval + Random.Range( -cfg.landDesireVariance , cfg.landDesireVariance );
            perchedTimer         = 0;
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
        public List<PreyController> neighbors  = new List<PreyController>();
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

    private PerchRuntimeState   perchState   = new PerchRuntimeState();
    private TakeOffRuntimeState takeOffState = new TakeOffRuntimeState();
    private FlockRuntimeState   flockState   = new FlockRuntimeState();
    private SplineRuntimeState  splineState  = new SplineRuntimeState();
    private UpdraftRuntimeState updraftState = new UpdraftRuntimeState();
    private ThermalRuntimeState thermalState = new ThermalRuntimeState();
    private RunRuntimeState     runState     = new RunRuntimeState();

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    public void OnEnable()  { }
    public void OnDisable() { }

    private void Update()
    {
        UpdateVectorToWren();
        UpdateData();
        UpdateModuleStates();
        UpdateState();
        DoPhysics();
        CheckForDespawn();

        if ( velocity.sqrMagnitude > 0.0001f ) {
            Vector3 fwd         = velocity.normalized;
            float   targetBank  = Vector3.Cross( oldVelocity.normalized , fwd ).y * parameters.turning.bankStrength;
            currentBank         = Mathf.Lerp( currentBank , targetBank , parameters.turning.bankSmoothing );
            Vector3 right       = Vector3.Cross( Vector3.up , fwd ).normalized;
            Vector3 bankUp      = ( Vector3.up + right * currentBank ).normalized;
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
        noiseOffset        = Random.Range( 0f , 100f );
        circleRuntimeAngle = Random.Range( 0f , Mathf.PI * 2f );
        thermalState.circleAngle = Random.Range( 0f , Mathf.PI * 2f );

        if ( parameters.modules.perch )
            perchState.Init( parameters.perch );

        SetHeight();

        force        = Vector3.zero;
        frame        = Random.Range( 0 , parameters.physics.physicsResolution );
        enabled      = true;
        spawnPoint   = transform.position;
        startPosition = transform.position;
        position     = startPosition;
        velocity     = Random.insideUnitSphere.normalized * parameters.movement.speed;
        oldVelocity  = velocity;
        spawnTime    = Time.time;
        life         = 0;
        state        = PreyState.Calm;

        StartCoroutine( SpawnCoroutine( config.spawn.spawnSpeed ) );
        OnInitialize();
    }

    protected virtual void OnInitialize() { }

    public void SetHeight()
    {
        if ( Physics.Raycast( transform.position , -transform.up , out RaycastHit hit , 100000 ) ) {
            transform.position = hit.point + transform.up * Mathf.Lerp( parameters.altitude.minAltitude , parameters.altitude.maxAltitude , Random.value );
            position = transform.position;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Wren / data updates
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateVectorToWren()
    {
        Transform wren = God.wren != null           ? God.wren.transform
                       : manager.debugWren != null  ? manager.debugWren.transform
                       : null;
        vectorToWren = wren != null ? wren.position - transform.position : Vector3.one * 9999f;
    }

    public void UpdateData()
    {
        frame++;
        if ( frame % parameters.physics.physicsResolution == 0 ) {
            rayCastData         = RaycastDown();
            rawDistanceToGround = rayCastData.w;
            rawGroundNormal     = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );

            rayCastData          = RaycastForward();
            rawDistanceToForward = rayCastData.w;
            rawForwardNormal     = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );
        }

        distanceToGround  = Mathf.Lerp( distanceToGround  , rawDistanceToGround                         , parameters.physics.physicsInfoLerpSpeed );
        distanceToForward = Mathf.Lerp( distanceToForward , rawDistanceToForward - parameters.movement.speed , parameters.physics.physicsInfoLerpSpeed );
        groundNormal      = Vector3.Lerp( groundNormal    , rawGroundNormal      , parameters.physics.physicsInfoLerpSpeed );
        forwardNormal     = Vector3.Lerp( forwardNormal   , rawForwardNormal     , parameters.physics.physicsInfoLerpSpeed );

        float startDist = parameters.turning.distanceForStartTurn;
        float hardDist  = parameters.turning.distanceForHardTurn;
        forwardTurnNormalizedValue = Mathf.Clamp( (startDist - distanceToForward) / (startDist - hardDist) , 0 , 1 );
        downTurnNormalizedValue    = Mathf.Clamp( (startDist - distanceToGround)  / (startDist - hardDist) , 0 , 1 );
    }

    private void UpdateModuleStates()
    {
        if ( parameters.modules.flock )   UpdateFlockState();
        if ( parameters.modules.updraft ) UpdateUpdraftState();
        if ( parameters.modules.spline )  UpdateSplineState();
    }

    private void UpdateFlockState()
    {
        flockState.queryTimer += Time.deltaTime;
        if ( flockState.queryTimer < parameters.flock.neighborQueryInterval ) return;
        flockState.queryTimer = 0;

        flockState.neighbors.Clear();
        manager.GetNearbyBirds( position , parameters.flock.detectionRadius , this , flockState.neighbors );

        if ( flockState.neighbors.Count > 0 ) {
            flockState.flockCenter = Vector3.zero;
            flockState.avgVelocity = Vector3.zero;
            foreach ( var n in flockState.neighbors ) {
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
        if ( splineState.spline == null )
            splineState.spline = PreySpline.FindNearest( position , 1000f );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // State machine
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateState()
    {
        switch ( state ) {

            case PreyState.Calm:
                if ( parameters.modules.run && vectorToWren.magnitude < parameters.run.startleRadius ) {
                    EnterDisturbed();
                    break;
                }
                if ( parameters.modules.perch ) {
                    perchState.landDesireTimer -= Time.deltaTime;
                    if ( perchState.landDesireTimer <= 0 )
                        TryStartLanding();
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
                if ( Vector3.Distance( position , perchState.target.position ) < parameters.perch.snapDistance )
                    EnterPerched();
                break;

            case PreyState.Perched:
                bool wrenClose = vectorToWren.magnitude < parameters.perch.startleRadius;
                perchState.perchedTimer += Time.deltaTime;
                bool timerExpired = perchState.perchedTimer >= perchState.currentPerchDuration;

                if ( wrenClose || timerExpired ) {
                    if ( parameters.modules.takeOff ) EnterTakeOff();
                    else                              EnterCalm();
                }
                break;

            case PreyState.TakingOff:
                takeOffState.timer += Time.deltaTime;
                if ( takeOffState.timer > parameters.takeOff.duration ) {
                    if ( parameters.modules.circle ) {
                        takeOffState.circleTimer += Time.deltaTime;
                        if ( takeOffState.circleTimer >= parameters.takeOff.circleAfter )
                            EnterCalm();
                    } else {
                        EnterCalm();
                    }
                }
                break;

            case PreyState.Disturbed:
                runState.calmTimer += Time.deltaTime;
                if ( runState.calmTimer  > parameters.run.calmDownTime &&
                     vectorToWren.magnitude > parameters.run.calmDownDistance )
                    EnterCalm();
                break;
        }
    }

    private void EnterCalm()
    {
        state              = PreyState.Calm;
        circleRuntimeAngle = Random.Range( 0f , Mathf.PI * 2f );
        if ( parameters.modules.perch )
            perchState.Init( parameters.perch );
    }

    private void EnterDisturbed()
    {
        state              = PreyState.Disturbed;
        runState.calmTimer = 0;
        runState.jukeTimer = 0;
        runState.jukeDir   = Random.insideUnitSphere.normalized;
        runState.jukeDir.y = 0;
    }

    private void TryStartLanding()
    {
        Transform target = FindBestPerchTarget();
        if ( target == null ) {
            // no perch available — back off and try again sooner
            perchState.landDesireTimer = parameters.perch.landDesireInterval * 0.5f;
            return;
        }
        perchState.target = target;
        state             = PreyState.Landing;
    }

    private void EnterPerched()
    {
        state                          = PreyState.Perched;
        velocity                       = Vector3.zero;
        flapValue                      = Vector3.zero;
        position                       = perchState.target.position;
        transform.position             = position;
        perchState.perchedTimer        = 0;
        perchState.currentPerchDuration = parameters.perch.perchDuration
            + Random.Range( -parameters.perch.perchDurationVariance , parameters.perch.perchDurationVariance );
    }

    private void EnterTakeOff()
    {
        state                   = PreyState.TakingOff;
        takeOffState.timer      = 0;
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
        if ( manager.perchPoints != null && manager.perchPoints.Length > 0 )
            return PerchPoint.FindNearest( position , 200f , manager.perchPoints );
        // fall back to global registry
        return PerchPoint.FindNearest( position , 100f );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Physics dispatch
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void DoPhysics()
    {
        switch ( state ) {
            case PreyState.Calm:      DoCalmPhysics();      break;
            case PreyState.Landing:   DoLandingPhysics();   break;
            case PreyState.Perched:   DoPerchedPhysics();   break;
            case PreyState.TakingOff: DoTakeOffPhysics();   break;
            case PreyState.Disturbed: DoDisturbedPhysics(); break;
        }
    }

    // ── Calm ─────────────────────────────────────────────────────────────────

    private void DoCalmPhysics()
    {
        force = Vector3.zero;

        force += MoveAlongGroundAndTurnAwayFromObstacles();

        if ( parameters.modules.noise )   force += NoiseForce();
        if ( parameters.modules.flock )   force += FlockForce();
        if ( parameters.modules.spline )  force += SplineForce();
        if ( parameters.modules.updraft && updraftState.zone != null ) force += UpdraftForce();
        if ( parameters.modules.thermal ) force += ThermalForce();
        if ( parameters.modules.circle )  force += CalmCircleForce();
        if ( parameters.modules.perch && manager.anchorPoint != null ) force += AnchorForce();

        ApplyVelocity( parameters.movement.speed );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ── Landing ──────────────────────────────────────────────────────────────

    private void DoLandingPhysics()
    {
        if ( perchState.target == null ) return;

        force = Vector3.zero;

        Vector3 toTarget = perchState.target.position - position;
        float   dist     = toTarget.magnitude;

        // slow approach speed the closer we get
        float t           = Mathf.Clamp01( dist / (parameters.perch.snapDistance * 8f) );
        float approachSpd = Mathf.Lerp( parameters.movement.speed * parameters.perch.approachSpeedMult ,
                                        parameters.movement.speed , t );

        force += toTarget.normalized;
        force += MoveAlongGroundAndTurnAwayFromObstacles() * 0.3f;

        ApplyVelocity( approachSpd );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ── Perched ──────────────────────────────────────────────────────────────

    private void DoPerchedPhysics()
    {
        velocity   = Vector3.zero;
        flapValue  = Vector3.zero;
        if ( perchState.target != null )
            position = perchState.target.position;
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
            Vector3 wrenPos = transform.position + vectorToWren;
            Vector3 target  = wrenPos
                + new Vector3( Mathf.Cos( takeOffState.circleAngle ) , 0 , Mathf.Sin( takeOffState.circleAngle ) )
                * parameters.circle.circleRadius
                + Vector3.up * parameters.takeOff.circleHeight;
            force += (target - transform.position).normalized * parameters.circle.circleForce;
            force += MoveAlongGroundAndTurnAwayFromObstacles();
        }

        ApplyVelocity( parameters.movement.speed );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ── Disturbed ────────────────────────────────────────────────────────────

    private void DoDisturbedPhysics()
    {
        force = Vector3.zero;

        float distToWren = vectorToWren.magnitude;
        float fleeMult   = Mathf.Clamp01(
            (parameters.run.startleRadius - distToWren) /
            Mathf.Max( parameters.run.startleRadius - parameters.run.fullRunRadius , 0.01f ) );
        Vector3 runDir = parameters.run.chaseInstead ? vectorToWren.normalized : -vectorToWren.normalized;
        force += runDir * parameters.run.fleeForce * fleeMult;

        // juke — random lateral burst to make escape less predictable
        runState.jukeTimer += Time.deltaTime;
        if ( runState.jukeTimer > 1f / Mathf.Max( parameters.run.jukeFrequency , 0.01f ) ) {
            runState.jukeTimer = 0;
            runState.jukeDir   = Vector3.Cross( vectorToWren.normalized , Vector3.up ).normalized
                               * (Random.value > 0.5f ? 1f : -1f);
        }
        force += runState.jukeDir * parameters.run.jukeAmount;

        force += MoveAlongGroundAndTurnAwayFromObstacles();

        ApplyVelocity( parameters.movement.speed * parameters.run.speedMultiplier );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        transform.position = position + flapValue;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Force helpers
    // ─────────────────────────────────────────────────────────────────────────

    public virtual Vector3 MoveAlongGroundAndTurnAwayFromObstacles()
    {
        var f = Vector3.zero;

        if ( parameters.modules.altitude )
            f -= Vector3.down * (parameters.altitude.desiredAltitude - distanceToGround) * parameters.altitude.strengthTowardsDesiredAltitude;

        if ( parameters.modules.turning ) {
            f += groundNormal  * downTurnNormalizedValue    * parameters.turning.groundTurnForce;
            if ( downTurnNormalizedValue > .99f ) f = Vector3.up;

            f += forwardNormal * forwardTurnNormalizedValue * parameters.turning.forwardTurnForce;
            if ( forwardTurnNormalizedValue > .99f ) f = forwardNormal - velocity.normalized;
        }

        return f;
    }

    private Vector3 NoiseForce()
    {
        float t = Time.time * parameters.noise.noiseSpeed + noiseOffset;
        return new Vector3(
            Mathf.PerlinNoise( t       , 0f ) - 0.5f ,
            Mathf.PerlinNoise( 0f      , t  ) - 0.5f ,
            Mathf.PerlinNoise( t + 50f , t  ) - 0.5f
        ) * parameters.noise.noiseForce;
    }

    private Vector3 FlockForce()
    {
        if ( flockState.neighbors.Count == 0 ) return Vector3.zero;

        var     f   = parameters.flock;
        Vector3 sep = Vector3.zero;

        foreach ( var n in flockState.neighbors ) {
            Vector3 away = position - n.position;
            if ( away.magnitude < f.separationRadius )
                sep += away.normalized / Mathf.Max( away.magnitude , 0.01f );
        }

        Vector3 cohesion  = flockState.flockCenter - position;
        Vector3 alignment = flockState.avgVelocity;

        return sep.normalized       * f.separationForce
             + cohesion.normalized  * f.cohesionForce
             + alignment.normalized * f.alignmentForce;
    }

    private Vector3 SplineForce()
    {
        if ( splineState.spline == null ) return Vector3.zero;

        float   t;
        Vector3 nearest  = splineState.spline.GetNearestPoint( position , out t );
        splineState.currentT = t;
        Vector3 toSpline = nearest - position;
        float   dist     = toSpline.magnitude;

        if ( dist < parameters.spline.pullRadius )
            return splineState.spline.GetForwardAt( t ) * parameters.spline.pullForce;
        else
            return toSpline.normalized * parameters.spline.returnForce;
    }

    private Vector3 UpdraftForce()
    {
        if ( updraftState.zone == null ) return Vector3.zero;

        Vector3 toCenter = updraftState.zone.transform.position - position;
        toCenter.y = 0;
        Vector3 tangent = Vector3.Cross( Vector3.up , toCenter.normalized );

        return tangent  * parameters.updraft.spiralForce
             + Vector3.up * parameters.updraft.liftForce * updraftState.zone.strength;
    }

    private Vector3 ThermalForce()
    {
        if ( manager == null || manager.thermalCenter == null ) return Vector3.zero;

        var  t            = parameters.thermal;
        bool thermaling   = distanceToGround < t.minAltitude;
        thermalState.isThermaling = thermaling;

        thermalState.circleAngle += Time.deltaTime * t.circleSpeed * (thermaling ? 1.5f : 1f);
        float radius = thermaling
            ? parameters.circle.circleRadius * t.thermalTightness
            : parameters.circle.circleRadius;

        Vector3 target = manager.thermalCenter.position
            + new Vector3( Mathf.Cos( thermalState.circleAngle ) , 0 , Mathf.Sin( thermalState.circleAngle ) ) * radius;

        Vector3 f = (target - transform.position).normalized * parameters.circle.circleForce;
        if ( thermaling ) f += Vector3.up * parameters.circle.updraft;
        return f;
    }

    private Vector3 CalmCircleForce()
    {
        circleRuntimeAngle += Time.deltaTime;
        Vector3 wrenPos = transform.position + vectorToWren;
        Vector3 target  = wrenPos
            + new Vector3( Mathf.Cos( circleRuntimeAngle ) , 0 , Mathf.Sin( circleRuntimeAngle ) )
            * parameters.circle.circleRadius;
        return (target - transform.position).normalized * parameters.circle.circleForce;
    }

    private Vector3 AnchorForce()
    {
        Vector3 toAnchor = manager.anchorPoint.position - position;
        if ( toAnchor.magnitude > parameters.perch.anchorRadius )
            return toAnchor.normalized * parameters.perch.anchorPullForce;
        return Vector3.zero;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shared velocity application
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplyVelocity( float speed )
    {
        oldVelocity  = velocity;
        velocity    += force;
        velocity     = velocity.normalized * speed;
        velocity     = GetNewVelocity( velocity , oldVelocity );
        velocity     = velocity.normalized * speed;
        position    += velocity;
    }

    public Vector3 GetNewVelocity( Vector3 desired , Vector3 current )
    {
        float angle = Vector3.Angle( current , desired );
        if ( angle < parameters.movement.maxAngleTurnBetweenFrames ) return desired;

        Vector3 axis = Vector3.Cross( current , desired ).normalized;
        if ( axis == Vector3.zero )
            axis = Vector3.Cross( Random.insideUnitSphere.normalized , current ).normalized;

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
            float currentCycle  = Mathf.Floor( positionInFlapCycle / (Mathf.PI * 2) );
            float mid           = currentCycle * Mathf.PI * 2 + Mathf.PI;
            positionInFlapCycle = Mathf.Lerp( positionInFlapCycle , mid , .1f );
        }

        flapValue = transform.up      * Mathf.Sin( positionInFlapCycle )                                   * parameters.flap.upBounceSize
                  + transform.forward * Mathf.Sin( positionInFlapCycle + parameters.flap.forwardBounceOffset ) * parameters.flap.forwardBounceSize;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Raycasts
    // ─────────────────────────────────────────────────────────────────────────

    public Vector4 RaycastDown()
    {
        if ( Physics.Raycast( transform.position , -transform.up , out RaycastHit hit , parameters.distance.maxDownDistance ) ) {
            float d = hit.point.y < parameters.altitude.minimumTotalY
                ? transform.position.y - parameters.altitude.minimumTotalY
                : hit.distance;
            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , d );
        }
        return new Vector4( 0 , 1 , 0 , transform.position.y - parameters.altitude.minimumTotalY );
    }

    public Vector4 RaycastForward()
    {
        if ( Physics.Raycast( transform.position , transform.forward , out RaycastHit hit , parameters.distance.maxForwardDistance ) )
            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , hit.distance );
        return new Vector4( 0 , 1 , 0 , parameters.distance.maxForwardDistance );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Despawn / eat
    // ─────────────────────────────────────────────────────────────────────────

    public virtual void CheckForDespawn()
    {
        if ( !parameters.modules.despawn ) return;
        if ( state == PreyState.Perched || state == PreyState.Landing || state == PreyState.TakingOff ) return;

        if ( Time.time - spawnTime > parameters.despawn.minimumTimeAlive )
            if ( vectorToWren.magnitude > parameters.despawn.distanceBeforeNotCaught )
                OnNotCaught();
    }

    private void OnTriggerEnter( Collider c )
    {
        if ( God.IsOurWren( c ) && !spawning ) {
            manager.PreyGotAte( this );
            Destroy( gameObject );
            StartCoroutine( DestroyCoroutine( parameters.spawn.ateDieSpeed ) );
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
        life     = 0;
        float start = Time.time;
        float end   = start + speed;

        while ( Time.time < end ) {
            life = Mathf.InverseLerp( start , end , Time.time );
            WhileSpawning( life );
            yield return null;
        }

        life     = 1;
        spawning = false;
    }

    public IEnumerator DestroyCoroutine( float speed )
    {
        spawning    = true;
        float start = Time.time;

        while ( Time.time < start + speed ) {
            life = Mathf.Lerp( 1 , 0 , (Time.time - start) / speed );
            WhileSpawning( life );
            yield return null;
        }

        DestroyImmediate( gameObject );
    }

    public virtual void WhileSpawning( float t )
    {
        if ( !parameters.modules.scale ) return;
        float s = Mathf.Clamp( 1 - (t - parameters.scale.maxScaleStartLife) , 0 , 1 );
        s = Mathf.Min( Mathf.Clamp( 1 - (parameters.scale.maxScaleEndLife - t) , 0 , 1 ) , s );
        transform.localScale = Vector3.one * parameters.scale.maxScale * s;
    }

    public void QuickKill() => DestroyImmediate( gameObject );
}
