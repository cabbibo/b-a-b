using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using WrenUtils;


public class PreyController : MonoBehaviour
{
    public PreyManager manager;
    public float       spawnTime;

    public PreyConfigSO parameters;


    public Vector3 spawnPoint;
    public bool    spawning;

    [Header( "Data" )]
    public float life;

    public float distanceToGround;

    public float rawDistanceToGround;


    public float distanceToForward;
    public float rawDistanceToForward;


    public Vector3 groundNormal;
    public Vector3 rawGroundNormal;

    public Vector3 forwardNormal;
    public Vector3 rawForwardNormal;
    public Vector3 vectorToWren; // DATA POINT

    public float forwardTurnNormalizedValue;
    public float downTurnNormalizedValue;


    [Header( "Prey Settings" )]

    // DATA 

    // DATA POINT
    public float positionInFlapCycle;

    public Vector3 flapValue;
    public float   climbRate;


    public float spawnSpeed  = 2;
    public float dieSpeed    = 2;
    public float ateDieSpeed = 1;


    /*
        different behaviors


        vultures circling
        butterflies hanging close to center;
        birds flying around a lake
        birds flying in a flock

        //flap when rising up
        //glide when going down

        fade in on spawn ( spawn in front of player )
        fade out if far enough away, ideally when player is looking away

        flying across the landscape ( migrating birds )

        circle around a specific lake
        circle around a specific point



    */


    public Vector3 force;
    public Vector3 velocity;
    public Vector3 position;

    public  Vector3 oldVelocity;
    private Vector3 startPosition;


    public void OnEnable()
    {

    }

    public void OnDisable()
    {
        // God.cameraManager.targetingManager.RemoveTarget(transform);
    }


    public Vector3 GetNewVelocity( Vector3 newDesiredVelocity , Vector3 currentVelocity )
    {

        // Get Angle Between the two vectors
        float angle = Vector3.Angle( currentVelocity , newDesiredVelocity );

        // if the angle is less than the max angle, return the new desired velocity
        // as we arent turning too much!
        if ( angle < parameters.maxAngleTurnBetweenFrames ) {
            return newDesiredVelocity;
        }


        // Get the axis of rotation
        var axis = Vector3.Cross( currentVelocity , newDesiredVelocity ).normalized;

        if ( axis == Vector3.zero ) {
            axis = Vector3.Cross( Random.insideUnitSphere.normalized , currentVelocity ).normalized;
        }

        // Get the rotation
        var rotation = Quaternion.AngleAxis( parameters.maxAngleTurnBetweenFrames , axis );


        // Apply the rotation to the old velocity
        return rotation * currentVelocity;


    }


    public Vector4 RaycastDown()
    {
        RaycastHit hit;

        if ( Physics.Raycast( transform.position , -transform.up , out hit , parameters.maxDownDistance ) ) {

            float distanceToGround = hit.distance;

            if ( hit.point.y < parameters.minimumTotalY ) {
                distanceToGround = transform.position.y - parameters.minimumTotalY;
            }

            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , distanceToGround );
        }

        return new Vector4( 0 , 1 , 0 , transform.position.y - parameters.minimumTotalY );
        ;

    }


    public Vector4 RaycastForward()
    {
        RaycastHit hit;

        if ( Physics.Raycast( transform.position , transform.forward , out hit , parameters.maxForwardDistance ) ) {
            return new Vector4( hit.normal.x , hit.normal.y , hit.normal.z , hit.distance );
        }

        return new Vector4( 0 , 1 , 0 , parameters.maxForwardDistance );
        ;

    }


    private Vector4 rayCastData;

    private int frame = 0;


    private void Update()
    {

        if ( God.wren != null ) {
            vectorToWren = God.wren.transform.position - transform.position;
        } else {
            vectorToWren = manager.debugWren.transform.position - transform.position;
        }


        UpdateData();
        DoPhysics();

        // if we are far enough for long enough ( while the object is old enough )
        // we despawn the prey
        // public float minimumTimeAlive                            = 30f;
        //public float distanceBeforeNotCaught                     = 100f;
        //public float timeOutsideDistanceBeforeNotCaughtTriggered = 5f;


        // if( magnitude > distanceBeforeNotCaught 


        var targetRotation = Quaternion.LookRotation( velocity.normalized , transform.up );
        transform.LookAt( position + flapValue + velocity.normalized * 10 );


        // if life is greatedr than maxScaleStartLife, scale in
        // if life is less than maxScaleEndLife, scale out


    }

    public float notCaught;

    public virtual void CheckForDespawn()
    {

        // if we are far enough for long enough ( while the object is old enough )
        // we despawn the prey
        // public float minimumTimeAlive                            = 30f;
        //public float distanceBeforeNotCaught                     = 100f;
        //public float timeOutsideDistanceBeforeNotCaughtTriggered 

        if ( Time.time - spawnTime > parameters.minimumTimeAlive ) {

            if ( vectorToWren.magnitude > parameters.distanceBeforeNotCaught ) {
                OnNotCaught();
            }


        }

    }

    public virtual Vector3 MoveAlongGroundAndTurnAwayFromObstacles()
    {
        var force = Vector3.zero;
        // if we are above our desired altitude, we should be turning down
        force -= Vector3.down * (parameters.desiredAltitude - distanceToGround) * parameters.strengthTowardsDesiredAltitude;

        // if we are close to the ground, we should be turning away from it
        force += groundNormal * downTurnNormalizedValue * parameters.groundTurnForce;

        if ( downTurnNormalizedValue > .99f ) {
            force = Vector3.up;
        }

        // if we are close to the front we shoudl start turning away from it
        force += forwardNormal * forwardTurnNormalizedValue * parameters.forwardTurnForce;

        if ( forwardTurnNormalizedValue > .99f ) {
            force = forwardNormal - velocity.normalized;
        }

        return force;
    }


    public virtual void DoFlapInfo()
    {
        // if we are even or going down, move towards position in flap cycle to Mathf.PI / 2;
        // if we are going up, increase by flap speed ( more the sharper up)

        climbRate = Mathf.Clamp( velocity.normalized.y , 0 , 1 );


        if ( climbRate > 0 ) {

            positionInFlapCycle += parameters.flapSpeed * climbRate * climbRate;
        } else {


            float currentCycle = Mathf.Floor( positionInFlapCycle / (Mathf.PI * 2) );

            float mid = currentCycle * Mathf.PI * 2 + Mathf.PI;

            positionInFlapCycle = Mathf.Lerp( positionInFlapCycle , mid , .1f );


        }


        // Jiggle it properly ( fast for butterflies slow for vultures etc.)
        flapValue = transform.up * Mathf.Sin( positionInFlapCycle ) * parameters.upBounceSize +
                    transform.forward * Mathf.Sin( positionInFlapCycle + parameters.forwardBounceOffset ) * parameters.forwardBounceSize;

    }


    public virtual void DoPhysics()
    {

        force = Vector3.zero;

        force += MoveAlongGroundAndTurnAwayFromObstacles();


        // run forces
        oldVelocity = velocity;
        velocity += force;

        velocity = velocity.normalized * parameters.speed;

        velocity = GetNewVelocity( velocity , oldVelocity );
        velocity = velocity.normalized * parameters.speed;
        position += velocity;

        DoFlapInfo();

        transform.position = position + flapValue;

    }

    public void UpdateData()
    {
        frame++;

        if ( frame % parameters.physicsResolution == 0 ) {
            rayCastData = RaycastDown();
            rawDistanceToGround = rayCastData.w;
            rawGroundNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );

            rayCastData = RaycastForward();
            rawDistanceToForward = rayCastData.w;
            rawForwardNormal = new Vector3( rayCastData.x , rayCastData.y , rayCastData.z );

        }


        distanceToGround = Mathf.Lerp( distanceToGround , rawDistanceToGround , parameters.physicsInfoLerpSpeed );


        distanceToForward =
            Mathf.Lerp( distanceToForward , rawDistanceToForward - parameters.speed ,
                parameters.physicsInfoLerpSpeed ); // always want to be a little bit ahead of where we are

        groundNormal = Vector3.Lerp( groundNormal , rawGroundNormal , parameters.physicsInfoLerpSpeed );
        forwardNormal = Vector3.Lerp( forwardNormal , rawForwardNormal , parameters.physicsInfoLerpSpeed );

        forwardTurnNormalizedValue =
            Mathf.Clamp(
                (parameters.distanceForStartTurn - distanceToForward) /
                (parameters.distanceForStartTurn - parameters.distanceForHardTurn) , 0 , 1 );
        downTurnNormalizedValue = Mathf.Clamp(
            (parameters.distanceForStartTurn - distanceToGround) / (parameters.distanceForStartTurn - parameters.distanceForHardTurn) ,
            0 , 1 );

    }


    private void OnTriggerEnter( Collider c )
    {
        if ( God.IsOurWren( c ) && !spawning ) {
            manager.PreyGotAte( this );
            Destroy( gameObject );
            StartCoroutine( DestroyCoroutine( ateDieSpeed ) );
        }
    }

    private void OnNotCaught()
    {
        if ( !spawning ) {
            spawning = true;
            StartCoroutine( DestroyCoroutine( dieSpeed ) );
        }
    }


    public IEnumerator DestroyCoroutine( float speed )
    {
        spawning = true;
        float startTime = Time.time;

        while (Time.time < startTime + speed) {
            float t = (Time.time - startTime) / speed;

            life = Mathf.Lerp( 1 , 0 , t );

            WhileSpawning( life );

            yield return null;
        }

        DestroyImmediate( gameObject );
        spawning = false;

    }

    public IEnumerator SpawnCoroutine( float speed )
    {

        spawning = true;
        life = 0;
        float startTime = Time.time;
        float endTime = startTime + speed;

        while (Time.time < endTime) {
            float t = (Time.time - startTime) / speed;

            life = Mathf.Lerp( 0 , 1 , t );
            WhileSpawning( life );
            yield return null;
        }

        life = 1;
        spawning = false;


    }

    public virtual void WhileSpawning( float life )
    {

        float scale = Mathf.Clamp( 1 - (life - parameters.maxScaleStartLife) , 0 , 1 );
        scale = Mathf.Min( Mathf.Clamp( 1 - (parameters.maxScaleEndLife - life) , 0 , 1 ) , scale );
        transform.localScale = Vector3.one * parameters.maxScale * scale;

    }

    public void QuickKill()
    {
        DestroyImmediate( gameObject );
    }

    public void Initialize( PreyConfigSO config , PreyManager manager )
    {
        parameters = config;
        this.manager = manager;


        SetHeight();

        force = Vector3.zero;
        frame = Random.Range( 0 , parameters.physicsResolution );


        enabled = true;
        spawnPoint = transform.position;
        startPosition = transform.position;
        position = startPosition;
        velocity = Random.insideUnitSphere.normalized * parameters.speed;
        oldVelocity = velocity;
        spawnTime = Time.time;

        life = 0;
        StartCoroutine( SpawnCoroutine( config.spawnSpeed ) );

    }


    public void SetHeight()
    {
        RaycastHit hit;

        if ( Physics.Raycast( transform.position , -transform.up , out hit , 100000 ) ) {
            float distance = hit.distance;

            transform.position = hit.point + transform.up * Mathf.Lerp( parameters.minAltitude , parameters.maxAltitude , Random.value );
            position = transform.position;
        }
    }
}