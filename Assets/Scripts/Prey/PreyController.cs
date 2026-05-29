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

    // ── Observable data (shown in inspector for debugging) ────────────────────
    [Header( "State" )]
    public PreyState state;

    public Transform CurrentPerchTarget => perchState.target;

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

    private LineRenderer focusLine;

    // ── Private physics ───────────────────────────────────────────────────────
    private Vector3 startPosition;
    private Vector4 rayCastData;
    private int     frame;
    private float   noiseOffset;
    private float   circleRuntimeAngle;
    private float   currentBank;
    private float   currentSpeed;
    private float   timeOutsideRegion;
    private bool    isOutsideRegion;
    private bool    isDespawning;

    private int   ambientFlapsInBurst = 0;
    private float ambientGlideTimer   = 0f;
    private float simTimeAccum        = 0f;
    private float _flapSpeedMult      = 1f;

    // ── Runtime state (one per module that needs per-instance state) ──────────
    private class PerchRuntimeState
    {
        public Transform target;
        public float     landDesireTimer;
        public float     perchedTimer;
        public float     currentPerchDuration;
        public float     landingBlend;       // 0 = full calm forces, 1 = full landing forces

        public void Init( PreyPerchModule cfg )
        {
            target               = null;
            landDesireTimer      = cfg.landDesireInterval + Random.Range( -cfg.landDesireVariance , cfg.landDesireVariance );
            perchedTimer         = 0;
            currentPerchDuration = 0;
            landingBlend         = 0f;
        }
    }

    private class SocialPressureState
    {
        public float desireToTakeOff;
        public float desireToDisturb;
        public float desireToCalmDown;
        public float desireToLand;
        public float sampleTimer;

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
        public UpdraftZone       zone;
        public PreyInterestPoint interestPoint;
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

    private class SearchRuntimeState
    {
        public PreyInterestPoint currentTarget;
        public float             calmTimer;
        public float             nextSearchTime;
        public float             searchTimer;
        public float             arrivedTimer;
        public float             searchCooldown; // blocks all searching after takeoff

        public void Init( PreySearchModule cfg )
        {
            currentTarget  = null;
            calmTimer      = 0f;
            searchTimer    = 0f;
            arrivedTimer   = 0f;
            searchCooldown = 0f;
            nextSearchTime = cfg.calmBeforeSearch
                             + Random.Range( -cfg.calmBeforeSearchVariance , cfg.calmBeforeSearchVariance );
        }
    }

    private PerchRuntimeState   perchState   = new();
    private TakeOffRuntimeState takeOffState = new();
    private FlockRuntimeState   flockState   = new();
    private SplineRuntimeState  splineState  = new();
    private UpdraftRuntimeState updraftState = new();
    private ThermalRuntimeState thermalState = new();
    private RunRuntimeState     runState     = new();
    private SearchRuntimeState  searchState  = new();

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
        } else {
            float simSpeed = manager != null ? manager.simulationSpeed : 1f;
            if ( simSpeed < 1f ) {
                simTimeAccum += Time.deltaTime;
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
        circleRuntimeAngle = Random.Range( 0f , Mathf.PI * 2f );
        thermalState.circleAngle = Random.Range( 0f , Mathf.PI * 2f );

        if ( parameters.modules.perch ) {
            perchState.Init( parameters.perch );
        }

        if ( parameters.modules.search ) {
            searchState.Init( parameters.search );
        }

        SetHeight();

        stamina           = parameters.modules.sprint ? parameters.sprint.maxStamina : 0f;
        timeOutsideRegion = 0f;
        isOutsideRegion   = false;
        isDespawning      = false;
        ambientFlapsInBurst = 0;
        ambientGlideTimer   = Random.Range( parameters.flap.glideTimeMin , parameters.flap.glideTimeMax );
        force             = Vector3.zero;
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

    private void UpdateStamina()
    {
        if ( !parameters.modules.sprint ) return;

        var s = parameters.sprint;
        stamina = Mathf.Min( stamina + s.staminaRefillRate * Time.deltaTime , s.maxStamina );

        float speedRange = Mathf.Max( s.maxSprintSpeed - parameters.movement.maxSpeed , 0.001f );
        float excess     = Mathf.Max( 0f , currentSpeed - parameters.movement.maxSpeed );
        stamina -= s.staminaDrainRate * (excess / speedRange) * Time.deltaTime;
        stamina  = Mathf.Max( stamina , 0f );
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

        updraftState.interestPoint = null;
        if ( manager?.interestPoints != null ) {
            float bestSqr = float.MaxValue;
            foreach ( var ip in manager.interestPoints ) {
                if ( ip == null || ip.type != InterestPointType.Updraft ) continue;
                float sqr = (ip.transform.position - position).sqrMagnitude;
                if ( sqr < ip.noticeRadius * ip.noticeRadius && sqr < bestSqr ) {
                    bestSqr = sqr;
                    updraftState.interestPoint = ip;
                }
            }
        }
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
                    if ( perchState.landDesireTimer <= 0 ) TryStartLanding();
                }

                if ( parameters.modules.search ) {
                    searchState.calmTimer += Time.deltaTime;

                    if ( searchState.searchCooldown > 0f ) {
                        searchState.searchCooldown -= Time.deltaTime;
                    } else {
                        // proximity notice — one chance-roll per frame; picks from in-range by priority
                        if ( Random.value < parameters.search.noticeChance * Time.deltaTime ) {
                            var noticed = PickSearchTarget( false );
                            if ( noticed != null ) { EnterSearching( noticed ); break; }
                        }

                        // forced scan after being calm long enough; includes alwaysInteresting far targets
                        if ( searchState.calmTimer >= searchState.nextSearchTime ) {
                            var target = PickSearchTarget( true );
                            if ( target != null ) { EnterSearching( target ); break; }
                            searchState.Init( parameters.search ); // nothing found, reset timer
                        }
                    }
                }

                break;

            case PreyState.Searching:
                if ( parameters.modules.run && vectorToWren.magnitude < parameters.run.startleRadius ) {
                    EnterDisturbed();
                    break;
                }

                if ( searchState.currentTarget?.transform == null ) { EnterCalm(); break; }

                searchState.searchTimer += Time.deltaTime;

                if ( Vector3.Distance( position , searchState.currentTarget.transform.position )
                     <= parameters.search.arrivalRadius ) {
                    searchState.arrivedTimer += Time.deltaTime;
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
                perchState.perchedTimer += Time.deltaTime;

                if ( vectorToWren.magnitude < parameters.perch.startleRadius ) {
                    if ( parameters.modules.takeOff ) EnterTakeOff();
                    else                              EnterCalm();
                } else if ( perchState.perchedTimer >= perchState.currentPerchDuration ) {
                    EnterCalm();
                    searchState.searchCooldown = searchState.nextSearchTime;
                }

                break;

            case PreyState.TakingOff:
                takeOffState.timer += Time.deltaTime;

                if ( takeOffState.timer > parameters.takeOff.duration ) {
                    if ( parameters.modules.circle ) {
                        takeOffState.circleTimer += Time.deltaTime;

                        if ( takeOffState.circleTimer >= parameters.takeOff.circleAfter ) {
                            EnterCalm();
                            searchState.searchCooldown = searchState.nextSearchTime;
                        }
                    } else {
                        EnterCalm();
                        searchState.searchCooldown = searchState.nextSearchTime;
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

        if ( parameters.modules.perch ) perchState.Init( parameters.perch );
        if ( parameters.modules.search ) searchState.Init( parameters.search );
    }

    private void EnterSearching( PreyInterestPoint target )
    {
        state = PreyState.Searching;
        searchState.currentTarget = target;
        searchState.searchTimer   = 0f;
        searchState.calmTimer     = 0f;
    }

    private void ArriveAtSearchTarget( PreyInterestPoint target )
    {
        searchState.currentTarget = null;

        switch ( target.type ) {
            case InterestPointType.Perch:
                var perchSpot = FindBestPerchTarget();
                if ( perchSpot != null ) {
                    perchState.target = perchSpot;
                    state = PreyState.Landing;
                } else {
                    EnterCalm();
                }
                break;

            case InterestPointType.Updraft:
            case InterestPointType.NewCalm:
                EnterCalm();
                break;

            case InterestPointType.NewInterest:
                // immediately search for a different point, never returning to this one
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

    private void TryStartLanding()
    {
        var target = FindBestPerchTarget();

        if ( target == null ) {
            perchState.landDesireTimer = parameters.perch.landDesireInterval * 0.5f;
            return;
        }

        perchState.target      = target;
        perchState.landingBlend = 0f;
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
        perchState.currentPerchDuration = parameters.perch.getBored
                                          + Random.Range( -parameters.perch.getBoredVariance ,
                                              parameters.perch.getBoredVariance );
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

    // forcedScan = true  → include alwaysInteresting targets regardless of distance
    // forcedScan = false → only targets within noticeRadius
    // exclude           → skip this specific point (used by NewInterest to avoid revisiting)
    private PreyInterestPoint PickSearchTarget( bool forcedScan , PreyInterestPoint exclude = null )
    {
        if ( manager?.interestPoints == null || manager.interestPoints.Length == 0 ) return null;

        var   candidates  = new System.Collections.Generic.List<PreyInterestPoint>();
        float totalWeight = 0f;

        foreach ( var ip in manager.interestPoints ) {
            if ( ip == null || ip == exclude ) continue;
            float dist    = Vector3.Distance( position , ip.transform.position );
            bool  inRange = dist <= ip.noticeRadius;

            if ( inRange || ( forcedScan && ip.alwaysInteresting ) ) {
                candidates.Add( ip );
                totalWeight += Mathf.Max( ip.priority , 0.001f );
            }
        }

        if ( candidates.Count == 0 ) return null;

        float r     = Random.Range( 0f , totalWeight );
        float accum = 0f;

        foreach ( var c in candidates ) {
            accum += Mathf.Max( c.priority , 0.001f );
            if ( r <= accum ) return c;
        }

        return candidates[ candidates.Count - 1 ];
    }

    private static readonly List<Transform>      _perchCandidates = new();
    private static readonly HashSet<Transform>   _occupiedPerches = new();
    private static readonly List<PreyController> _socialNeighbors = new();

    private Transform FindBestPerchTarget()
    {
        // collect perches already claimed by landing or perched birds
        _occupiedPerches.Clear();
        if ( manager?.preyHolder != null ) {
            for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
                var other = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
                if ( other == null || other == this ) continue;
                if ( other.state == PreyState.Landing || other.state == PreyState.Perched ) {
                    var t = other.CurrentPerchTarget;
                    if ( t != null ) _occupiedPerches.Add( t );
                }
            }
        }

        _perchCandidates.Clear();
        float searchSqr = 200f * 200f;

        if ( manager?.interestPoints != null ) {
            foreach ( var ip in manager.interestPoints ) {
                if ( ip == null || ip.type != InterestPointType.Perch ) continue;

                if ( ip.transform.childCount > 0 ) {
                    for ( int i = 0; i < ip.transform.childCount; i++ ) {
                        var child = ip.transform.GetChild( i );
                        if ( !child.name.StartsWith( "_perch_" ) ) continue;
                        if ( _occupiedPerches.Contains( child ) ) continue;
                        if ( (child.position - position).sqrMagnitude < searchSqr )
                            _perchCandidates.Add( child );
                    }
                } else {
                    if ( _occupiedPerches.Contains( ip.transform ) ) continue;
                    if ( (ip.transform.position - position).sqrMagnitude < searchSqr )
                        _perchCandidates.Add( ip.transform );
                }
            }
        }

        if ( _perchCandidates.Count > 0 )
            return _perchCandidates[ Random.Range( 0 , _perchCandidates.Count ) ];

        return PerchPoint.FindNearest( position , 100f );
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
            case PreyState.Perched:   DoPerchedPhysics();   break;
            case PreyState.TakingOff: DoTakeOffPhysics();   break;
            case PreyState.Disturbed: DoDisturbedPhysics(); break;
        }
    }

    // ── Calm ─────────────────────────────────────────────────────────────────

    private void DoCalmPhysics()
    {
        _flapSpeedMult = 1f;
        force = Vector3.zero;

        AddAvoidanceForces();

        if ( parameters.modules.drive )  AddForce( DriveForce()       , new Color( 0.6f , 1f , 0f ) , "drive" );
        if ( parameters.modules.noise )  AddForce( NoiseForce()       , Color.yellow            , "noise" );
        if ( parameters.modules.flock )  AddForce( FlockForce()       , Color.cyan              , "flock" );
        if ( parameters.modules.spline ) AddForce( SplineForce() , Color.blue , "spline" );

        if ( parameters.modules.updraft && (updraftState.zone != null || updraftState.interestPoint != null) )
            AddForce( UpdraftForce() , Color.green , "updraft" );

        if ( parameters.modules.thermal )
            AddForce( ThermalForce() , new Color( 1f , 0.5f , 0f ) , "thermal" );

        if ( parameters.modules.circle )
            AddForce( CalmCircleForce() , Color.magenta , "circle" );

        if ( parameters.modules.perch && manager.GetClosestAnchorPoints( position ) != null )
            AddForce( AnchorForce() , Color.white , "anchor" );

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

    // ── Searching ────────────────────────────────────────────────────────────

    private void DoSearchingPhysics()
    {
        force = Vector3.zero;

        if ( searchState.currentTarget?.transform != null ) {
            var toTarget = (searchState.currentTarget.transform.position - position).normalized;
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

    private void DoLandingPhysics()
    {
        if ( perchState.target == null ) { EnterCalm(); return; }

        var   p         = parameters.perch;
        var   toTarget  = perchState.target.position - position;
        float dist      = toTarget.magnitude;

        // ramp from calm forces (0) to full landing forces (1) over landingBlendDuration
        perchState.landingBlend = Mathf.MoveTowards(
            perchState.landingBlend , 1f , Time.deltaTime / p.landingBlendDuration );

        // ── aim point: above target when far, direct when close (dive approach) ──
        bool   diving  = dist < p.approachRadius;
        var    aimPos  = diving
            ? perchState.target.position
            : perchState.target.position + Vector3.up * p.approachHeight;
        var    toAim   = aimPos - position;
        var    aimDir  = toAim.sqrMagnitude > 0.001f ? toAim.normalized : Vector3.down;

        // ── collect calm forces ───────────────────────────────────────────────
        force = Vector3.zero;
        var calmF = Vector3.zero;
        if ( parameters.modules.drive )  calmF += DriveForce();
        if ( parameters.modules.noise )  calmF += NoiseForce();
        if ( parameters.modules.flock )  calmF += FlockForce();
        if ( parameters.modules.spline ) calmF += SplineForce();
        if ( parameters.modules.cage   ) calmF += CageForce();

        // blend calm → landing
        force = Vector3.Lerp( calmF , aimDir , perchState.landingBlend );
        AddForce( MoveAlongGroundAndTurnAwayFromObstacles() * 0.3f , new Color( 1f , 0.4f , 0.1f ) , "avoidance" );

        // ── speed: slow progressively as we get close ─────────────────────────
        float distT      = Mathf.Clamp01( dist / (p.snapDistance * 8f) );
        float approachSpd = Mathf.Lerp( parameters.movement.desiredSpeed * p.approachSpeedMult ,
                                        parameters.movement.desiredSpeed , distT );

        ApplyVelocity( approachSpd );

        // ── flap: rapid when close to surface, normal when far ────────────────
        float closeT    = 1f - Mathf.Clamp01( dist / p.approachRadius );
        _flapSpeedMult  = Mathf.Lerp( 1f , p.landingFlapMult , closeT * perchState.landingBlend );

        if ( parameters.modules.flap ) DoFlapInfo();
        else flapValue = Vector3.zero;

        _flapSpeedMult = 1f; // reset so it doesn't bleed into other states

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
            AddForce( MoveAlongGroundAndTurnAwayFromObstacles() , new Color( 1f , 0.4f , 0.1f ) , "avoidance" );
        } else if ( parameters.modules.circle ) {
            // circle phase (only reached when circle module is on)
            takeOffState.circleAngle += Time.deltaTime;
            var wrenPos = transform.position + vectorToWren;
            var target = wrenPos
                         + new Vector3( Mathf.Cos( takeOffState.circleAngle ) , 0 , Mathf.Sin( takeOffState.circleAngle ) )
                         * parameters.circle.circleRadius
                         + Vector3.up * parameters.takeOff.circleHeight;
            force += (target - transform.position).normalized * parameters.circle.circleForce;
            AddForce( MoveAlongGroundAndTurnAwayFromObstacles() , new Color( 1f , 0.4f , 0.1f ) , "avoidance" );
        }

        if ( parameters.modules.cage )  force += CageForce();
        if ( parameters.modules.drive ) AddForce( DriveForce() , new Color( 0.6f , 1f , 0f ) , "drive" );

        ApplyVelocity( parameters.movement.desiredSpeed , true );

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
            ? parameters.sprint.maxSprintSpeed
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

        var sp    = curve.Spline;
        var xform = curve.transform;

        var localPos = new float3( xform.InverseTransformPoint( position ) );
        SplineUtility.GetNearestPoint( sp , localPos , out float3 nearestLocal , out float t );

        var nearestWorld = xform.TransformPoint( new Vector3( nearestLocal.x , nearestLocal.y , nearestLocal.z ) );
        var tangentWorld = xform.TransformDirection( (Vector3)SplineUtility.EvaluateTangent( sp , t ) );

        var toSpline = nearestWorld - position;
        var forward  = tangentWorld;

        var pullF    = toSpline.sqrMagnitude > 0.0001f ? toSpline.normalized * parameters.spline.pullForce          : Vector3.zero;
        var forwardF = forward.sqrMagnitude  > 0.0001f ? forward.normalized  * parameters.spline.splineForwardForce : Vector3.zero;

        Debug.DrawRay( position , pullF    , Color.cyan );
        Debug.DrawRay( position , forwardF , Color.blue );

        return pullF + forwardF;
    }

    private Vector3 UpdraftForce()
    {
        if ( updraftState.zone != null ) {
            var toCenter = updraftState.zone.transform.position - position;
            toCenter.y = 0;
            var tangent = Vector3.Cross( Vector3.up , toCenter.normalized );
            return tangent * parameters.updraft.spiralForce
                   + Vector3.up * parameters.updraft.liftForce * updraftState.zone.strength;
        }

        if ( updraftState.interestPoint != null ) {
            var  us       = updraftState.interestPoint.updraftSettings;
            var  toCenter = updraftState.interestPoint.transform.position - position;
            toCenter.y = 0;
            int  curl     = us.curlDirection == CurlDirection.CounterClockwise ? 1 : -1;
            var  tangent  = toCenter.sqrMagnitude > 0.01f
                ? Vector3.Cross( Vector3.up , toCenter.normalized ) * curl
                : Vector3.zero;
            var inward = toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Vector3.zero;
            return Vector3.up * us.forceUp
                   + tangent   * us.curlForce
                   + inward    * us.forceIn;
        }

        return Vector3.zero;
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
        if ( manager == null ) return Vector3.zero;

        Vector3 min, max;
        if ( manager.regionType == RegionType.Box && manager.boxRegion != null ) {
            var half = manager.boxRegion.lossyScale * 0.5f;
            min = manager.boxRegion.position - half;
            max = manager.boxRegion.position + half;
        } else if ( manager.regionType == RegionType.Collider && manager.regionCollider != null ) {
            min = manager.regionCollider.bounds.min;
            max = manager.regionCollider.bounds.max;
        } else {
            return Vector3.zero;
        }

        var cfg = parameters.cage;
        float d = cfg.borderTurnDistance;

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

    private void ApplyVelocity( float targetSpeed , bool allowSprint = false )
    {
        var m = parameters.movement;

        if ( allowSprint && parameters.modules.sprint && stamina > 0f ) {
            targetSpeed = parameters.sprint.maxSprintSpeed;
        }

        float effectiveMax = (allowSprint && parameters.modules.sprint && stamina > 0f)
            ? parameters.sprint.maxSprintSpeed
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
                ambientGlideTimer -= Time.deltaTime;
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
            bool outsideNow = vectorToWren.magnitude > parameters.despawn.distanceBeforeNotCaught;

            if ( outsideNow && !isOutsideRegion ) OnLeaveRegion();
            else if ( !outsideNow && isOutsideRegion ) OnEnterRegion();
        }

        if ( isOutsideRegion ) {
            timeOutsideRegion += Time.deltaTime;
            if ( timeOutsideRegion >= parameters.despawn.timeOutsideDistanceBeforeNotCaughtTriggered ) {
                OnNotCaught();
            }
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
            }
            UnityEditor.Handles.Label( origin + Vector3.up * 2f , stateLabel , GizmoLabel( stateColor ) );
        }

        // ── Calm search-readiness debug ───────────────────────────────────────
        if ( dbg.showCalmDebug && parameters.modules.search && state == PreyState.Calm ) {
            string inRangeLabel = "none in range";
            if ( manager?.interestPoints != null ) {
                foreach ( var ip in manager.interestPoints ) {
                    if ( ip == null ) continue;
                    float dist = Vector3.Distance( position , ip.transform.position );
                    if ( dist <= ip.noticeRadius ) {
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
            var targetPos = searchState.currentTarget.transform.position;
            Gizmos.color = new Color( 0.6f , 0.2f , 1f , 0.8f );
            Gizmos.DrawLine( origin , targetPos );
            UnityEditor.Handles.color = new Color( 0.6f , 0.2f , 1f , 0.4f );
            UnityEditor.Handles.DrawWireDisc( targetPos , Vector3.up , parameters.search.arrivalRadius );
            float remaining = parameters.search.giveUpTime - searchState.searchTimer;
            UnityEditor.Handles.Label(
                origin + Vector3.up * 3.5f ,
                $"→ {searchState.currentTarget.type}  give up in {remaining:F1}s" ,
                GizmoLabel( new Color( 0.6f , 0.2f , 1f ) ) );
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

        // countdown when outside not-caught region
        if ( timeOutsideRegion > 0f ) {
            float remaining   = parameters.despawn.timeOutsideDistanceBeforeNotCaughtTriggered - timeOutsideRegion;
            var   leaveColor  = Color.Lerp( Color.yellow , Color.red , timeOutsideRegion / parameters.despawn.timeOutsideDistanceBeforeNotCaughtTriggered );
            UnityEditor.Handles.Label( origin + Vector3.up * 2f , $"Leaving in  {remaining:F1}s" , GizmoLabel( leaveColor ) );
        }
#endif
    }

#if UNITY_EDITOR
    private static GUIStyle GizmoLabel( Color col )
    {
        return new GUIStyle( UnityEditor.EditorStyles.label ) {
            normal  = { textColor = col },
            fontSize = 7
        };
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