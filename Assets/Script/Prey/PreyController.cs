using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class PreyController : MonoBehaviour
{




    public PreyManager manager;
    public float dieRate;

    public float life;
    public float maxScale;
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




    // DATA POINT
    public float positionInFlapCycle;


    /*
        different behaviors


        vultures circling
        butterflies hanging close to center;

    */


    Vector3 projectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
    {
        //get vector from point on line to point in space
        Vector3 linePointToPoint = point - linePoint;

        float t = Vector3.Dot(linePointToPoint, lineVec);

        return linePoint + lineVec * t;

    }


    Vector3 force;
    Vector3 velocity;
    Vector3 position;

    Vector3 startPosition;

    Vector3 oldVelocity;

    public void OnEnable()
    {
        startPosition = transform.position;
        position = startPosition;
        velocity = Random.insideUnitSphere.normalized * speed;
        oldVelocity = velocity;
        force = Vector3.zero;
        frame = 0;
    }


    // Chatgpt
    public Vector3 AdjustVelocity(Vector3 velocity, Vector3 oldVelocity)
    {
        // Normalize the velocities
        Vector3 normalizedVelocity = velocity.normalized;
        Vector3 normalizedOldVelocity = oldVelocity.normalized;

        // Calculate the current dot product
        float dotProduct = Vector3.Dot(normalizedVelocity, normalizedOldVelocity);

        // Check if the dot product is already >= 0.8
        if (dotProduct >= minimumDotProductMatchForTurn)
        {
            return normalizedVelocity; // No need to adjust
        }

        // Calculate the rotation axis
        Vector3 rotationAxis = Vector3.Cross(normalizedOldVelocity, normalizedVelocity).normalized;

        // Calculate the target angle (in radians) for a dot product of 0.8
        float targetAngle = Mathf.Acos(minimumDotProductMatchForTurn);

        // Rotate the old velocity towards the desired direction
        Quaternion rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * targetAngle, rotationAxis);
        Vector3 newVel = rotation * normalizedOldVelocity;

        return newVel;

    }


    public float maxForwardDistance = 100;
    public float maxDownDistance = 100;

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


    public float groundTurnForce = 1;
    public float forwardTurnForce = 1;


    public float distanceToGround;

    public float rawDistanceToGround;

    public Vector3 groundNormal;
    public Vector3 rawGroundNormal;


    // should be able to reduce this as we move forward between frames

    public float distanceToForward;
    public float rawDistanceToForward;

    public Vector3 forwardNormal;
    public Vector3 rawForwardNormal;

    public float distanceForStartTurn;
    public float distanceForHardTurn;


    public float distanceToStartRun;
    public float distanceToFullRun;

    // Turn Away fRom the normal


    public Vector3 vectorToWren; // DATA POINT

    Vector4 rayCastData;

    int frame = 0;
    void Update()
    {

        frame++;

        force = Vector3.zero;


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


        float forwardTurnNormalizedValue = Mathf.Clamp((distanceForStartTurn - distanceToForward) / (distanceForStartTurn - distanceForHardTurn), 0, 1);
        float downTurnNormalizedValue = Mathf.Clamp((distanceToGround - distanceForStartTurn) / (distanceForStartTurn - distanceForHardTurn), 0, 1);

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

        velocity = AdjustVelocity(velocity, oldVelocity);
        velocity = velocity.normalized * speed;


        position += velocity;

        // Jiggle it properly ( fast for butterflies slow for vultures etc.)
        Vector3 flapValue = transform.up * Mathf.Sin(positionInFlapCycle * Mathf.PI) * upBounceSize + transform.forward * Mathf.Sin(positionInFlapCycle * Mathf.PI + forwardBounceOffset) * forwardBounceSize;
        transform.position = position + flapValue;


        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        transform.LookAt(position + flapValue + velocity.normalized * 10);


        transform.localScale = Vector3.one * maxScale * Mathf.Clamp(Mathf.Min((1 - life) * 4, life), 0, 1);

        life -= dieRate;
        if (life < 0)
        {
            DestroyImmediate(gameObject);
        }


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

        speed = config.speed;
        life = config.life;

        dieRate = config.dieRate;

        maxScale = config.maxScale;


        runForce = config.runForce;
        runRadius = config.runRadius;

    }
}