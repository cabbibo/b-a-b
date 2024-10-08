using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class PreyController : MonoBehaviour
{




    public PreyManager manager;

    [Header("Prey Settings")]
    public float dieRate;

    public float maxScale;

    public float maxScaleEndLife = 0;
    public float maxScaleStartLife = 1;


    public float maxAngleTurnBetweenFrames = 4f;

    public float speed;
    public float runForce;
    public float runRadius;


    // how many frames between physics casts;
    // only need to crank to bigger numbers if weve got lots of little things
    public int physicsResolution = 1;

    // lerping the physics esp if we are doing it between frames
    public float physicsInfoLerpSpeed = .1f;



    public float desiredAltitude = 10;
    public float minAltitude = 5;

    public float maxAltitude = 20;

    public float strengthTowardsDesiredAltitude = 1;


    public float circleForce = 1;
    public float circleRadius;

    public float updraft;



    // when you get far enough away, start pulling back in ( just XY? )
    public float maxDistanceStart;
    public float maxDistanceEnd;
    public float forceInwardsAtMaxDistanceEnd;


    // tween 'current altitude' slowly so it can stay in valleys etc
    // cast down ray and cast forward ray


    public float minimumDotProductMatchForTurn;
    public float forwardSpeed;


    // applied after all other movement so we can get a 'bounce' effect for things like butterflies
    public float flapSpeed;
    public float upBounceSize;
    public float forwardBounceSize;
    public float forwardBounceOffset;


    public float alwaysCenterForce;

    public float noiseSize;
    public float noiseSpeed;
    public float noiseForce;

    public float alwaysTowardsSpawnPointForce;

    public float maxForwardDistance = 100;
    public float maxDownDistance = 100;



    public float groundTurnForce = 1;
    public float forwardTurnForce = 1;



    // should be able to reduce this as we move forward between frames


    public float distanceForStartTurn;
    public float distanceForHardTurn;


    public float distanceToStartRun;
    public float distanceToFullRun;

    // Turn Away fRom the normal


    public int numCrystals;
    public float crystalType;

    // DATA 
    [Header("Data")]
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



    // DATA POINT
    public float positionInFlapCycle;

    public Vector3 flapValue;
    public float climbRate;


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

    public Vector3 oldVelocity;
    Vector3 startPosition;


    public void OnEnable()
    {
        startPosition = transform.position;
        position = startPosition;
        velocity = Random.insideUnitSphere.normalized * speed;
        oldVelocity = velocity;

        SetHeight();

        force = Vector3.zero;
        frame = 0;
    }



    public Vector3 GetNewVelocity(Vector3 newDesiredVelocity, Vector3 currentVelocity)
    {

        // Get Angle Between the two vectors
        float angle = Vector3.Angle(currentVelocity, newDesiredVelocity);

        // if the angle is less than the max angle, return the new desired velocity
        // as we arent turning too much!
        if (angle < maxAngleTurnBetweenFrames)
        {
            return newDesiredVelocity;
        }



        // Get the axis of rotation
        Vector3 axis = Vector3.Cross(currentVelocity, newDesiredVelocity).normalized;

        if (axis == Vector3.zero)
        {
            axis = Vector3.Cross(Random.insideUnitSphere.normalized, currentVelocity).normalized;
        }

        // Get the rotation
        Quaternion rotation = Quaternion.AngleAxis(maxAngleTurnBetweenFrames, axis);


        // Apply the rotation to the old velocity
        return rotation * currentVelocity;


    }




    public Vector4 RaycastDown()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, maxDownDistance))
        {
            return new Vector4(hit.normal.x, hit.normal.y, hit.normal.z, hit.distance);
        }

        return new Vector4(0, 1, 0, maxDownDistance); ;

    }



    public Vector4 RaycastForward()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxForwardDistance))
        {
            return new Vector4(hit.normal.x, hit.normal.y, hit.normal.z, hit.distance);
        }

        return new Vector4(0, 1, 0, maxForwardDistance); ;

    }



    Vector4 rayCastData;

    int frame = 0;




    void Update()
    {



        UpdateData();
        DoPhysics();

        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        transform.LookAt(position + flapValue + velocity.normalized * 10);


        // if life is greatedr than maxScaleStartLife, scale in
        // if life is less than maxScaleEndLife, scale out

        float scale = Mathf.Clamp(1 - (life - maxScaleStartLife), 0, 1);
        scale = Mathf.Min(Mathf.Clamp(1 - (maxScaleEndLife - life), 0, 1), scale);

        transform.localScale = Vector3.one * maxScale * scale;

        life -= dieRate;
        if (life < 0)
        {
            DestroyImmediate(gameObject);
        }


    }



    public virtual void DoPhysics()
    {

        force = Vector3.zero;

        // if we are above our desired altitude, we should be turning down
        force -= Vector3.down * (desiredAltitude - distanceToGround) * strengthTowardsDesiredAltitude;

        // if we are close to the ground, we should be turning away from it
        force += groundNormal * downTurnNormalizedValue * groundTurnForce;
        if (downTurnNormalizedValue > .99f)
        {
            force = Vector3.up;
        }

        // if we are close to the front we shoudl start turning away from it
        force += forwardNormal * forwardTurnNormalizedValue * forwardTurnForce;
        if (forwardTurnNormalizedValue > .99f)
        {
            force = forwardNormal - velocity.normalized;
        }




        if (God.wren != null)
        {
            vectorToWren = God.wren.transform.position - transform.position;
        }
        else
        {
            vectorToWren = manager.debugWren.transform.position - transform.position;
        }



        // run forces
        oldVelocity = velocity;
        velocity += force;


        velocity = velocity.normalized * speed;

        velocity = GetNewVelocity(velocity, oldVelocity);
        velocity = velocity.normalized * speed;


        position += velocity;



        // if we are even or going down, move towards position in flap cycle to Mathf.PI / 2;
        // if we are going up, increase by flap speed ( more the sharper up)

        climbRate = Mathf.Clamp(velocity.normalized.y, 0, 1);


        if (climbRate > 0)
        {

            positionInFlapCycle += flapSpeed * climbRate * climbRate;
        }
        else
        {


            float currentCycle = Mathf.Floor(positionInFlapCycle / (Mathf.PI * 2));

            float mid = currentCycle * Mathf.PI * 2 + Mathf.PI;

            positionInFlapCycle = Mathf.Lerp(positionInFlapCycle, mid, .1f);



        }


        // Jiggle it properly ( fast for butterflies slow for vultures etc.)
        flapValue = transform.up * Mathf.Sin(positionInFlapCycle) * upBounceSize + transform.forward * Mathf.Sin(positionInFlapCycle + forwardBounceOffset) * forwardBounceSize;

        transform.position = position + flapValue;

    }

    public void UpdateData()
    {
        frame++;

        if (frame % physicsResolution == 0)
        {
            rayCastData = RaycastDown();
            rawDistanceToGround = rayCastData.w;
            rawGroundNormal = new Vector3(rayCastData.x, rayCastData.y, rayCastData.z);

            rayCastData = RaycastForward();
            rawDistanceToForward = rayCastData.w;
            rawForwardNormal = new Vector3(rayCastData.x, rayCastData.y, rayCastData.z);

        }

        distanceToGround = Mathf.Lerp(distanceToGround, rawDistanceToGround, physicsInfoLerpSpeed);
        distanceToForward = Mathf.Lerp(distanceToForward, rawDistanceToForward - speed, physicsInfoLerpSpeed); // always want to be a little bit ahead of where we are

        groundNormal = Vector3.Lerp(groundNormal, rawGroundNormal, physicsInfoLerpSpeed);
        forwardNormal = Vector3.Lerp(forwardNormal, rawForwardNormal, physicsInfoLerpSpeed);




        /*

            Turning away from the ground and from in front of us!
            ( do we only need to choose the minimum for this? )

        */


        forwardTurnNormalizedValue = Mathf.Clamp((distanceForStartTurn - distanceToForward) / (distanceForStartTurn - distanceForHardTurn), 0, 1);
        downTurnNormalizedValue = Mathf.Clamp((distanceForStartTurn - distanceToGround) / (distanceForStartTurn - distanceForHardTurn), 0, 1);

    }



    void OnTriggerEnter(Collider c)
    {
        if (God.IsOurWren(c))
        {
            manager.PreyGotAte(this);
            Destroy(gameObject);
        }
    }


    public void QuickKill()
    {
        DestroyImmediate(gameObject);
    }

    public void Initialize(PreyConfigSO config, PreyManager manager)
    {

        enabled = true;
        this.manager = manager;

        life = 1;
        dieRate = config.dieRate;

        maxScale = config.maxScale;
        speed = config.speed;

        maxAngleTurnBetweenFrames = config.maxAngleTurnBetweenFrames;
        maxScaleStartLife = config.maxScaleStartLife;
        maxScaleEndLife = config.maxScaleEndLife;

        runForce = config.runForce;
        runRadius = config.runRadius;

        physicsResolution = config.physicsResolution;

        physicsInfoLerpSpeed = config.physicsInfoLerpSpeed;
        desiredAltitude = config.desiredAltitude;
        minAltitude = config.minAltitude;
        maxAltitude = config.maxAltitude;
        strengthTowardsDesiredAltitude = config.strengthTowardsDesiredAltitude;
        circleForce = config.circleForce;
        circleRadius = config.circleRadius;
        updraft = config.updraft;
        maxDistanceStart = config.maxDistanceStart;
        maxDistanceEnd = config.maxDistanceEnd;
        forceInwardsAtMaxDistanceEnd = config.forceInwardsAtMaxDistanceEnd;
        minimumDotProductMatchForTurn = config.minimumDotProductMatchForTurn;
        forwardSpeed = config.forwardSpeed;
        flapSpeed = config.flapSpeed;
        upBounceSize = config.upBounceSize;
        forwardBounceSize = config.forwardBounceSize;
        forwardBounceOffset = config.forwardBounceOffset;
        alwaysCenterForce = config.alwaysCenterForce;
        noiseSize = config.noiseSize;
        noiseSpeed = config.noiseSpeed;
        noiseForce = config.noiseForce;
        maxForwardDistance = config.maxForwardDistance;
        maxDownDistance = config.maxDownDistance;
        groundTurnForce = config.groundTurnForce;
        forwardTurnForce = config.forwardTurnForce;
        distanceForStartTurn = config.distanceForStartTurn;
        distanceForHardTurn = config.distanceForHardTurn;
        distanceToStartRun = config.distanceToStartRun;
        distanceToFullRun = config.distanceToFullRun;
        numCrystals = config.numCrystals;
        crystalType = config.crystalType;

    }


    public void SetHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 100000))
        {
            float distance = hit.distance;

            transform.position = hit.point + transform.up * Mathf.Lerp(minAltitude, maxAltitude, Random.value);
            position = transform.position;
        }
    }
}