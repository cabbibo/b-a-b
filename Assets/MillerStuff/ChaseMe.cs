#if UNITY_EDITOR
using UnityEditor; // This namespace is required for Selection
#endif
using UnityEngine;
using WrenUtils;
public class ChaseMe : MonoBehaviour
{
    public Transform predator; // Reference to the predator GameObject
    public Transform focusPointHolder;
    private Transform[] focusPoints; // Array of focus points
    public bool debugMode = false; // Debug mode to enable selection in the editor
    public float PredatorAvoidanceDistance = 200; // Distance at which the prey starts avoiding the predator
    public float PredatorAvoidanceTail = 100f; // When the predator is this distance or closer, move at max speed
    public float maxSpeed = 30f; // Maximum speed of the prey
    public float minSpeed = 10f; // Minimum speed of the prey
    public float baseRotationSpeed = 50f; // Base rotation speed (degrees per second)
    public float maxRotationSpeed = 100f; // Maximum rotation speed (degrees per second) when directly in front
    public float maxEvasionRotationSpeed = 100f;
    public float maxRaycastDistance = 500f; // Maximum distance for raycasting
    public int coneRaycastCountX = 5; // Number of raycasts in the horizontal direction for obstacle detection
    public int coneRaycastCountY = 3; // Number of raycasts in the vertical direction for obstacle detection
    public float coneAngleX = 30f; // Initial horizontal cone angle for the special array of raycasts
    public float coneAngleY = 20f; // Initial vertical cone angle for the special array of raycasts
    public Transform raycastPoint; // Transform used as the origin for raycasts
    public float focusDistance = 2000; // Desired distance from focus points
    public float switchFocusPointDistance = 100f; // Distance to switch to the next focus point
    public float currentVelocity;
    public LayerMask obstacleLayerMask; // LayerMask to specify which layers to detect with raycasts

    public float minHeight = -720; // Minimum height the prey can reach
    public float maxHeight = 326; // Maximum height the prey can reach
    public float obstacleAvoidanceThreshold = 300; // Distance threshold to start avoiding obstacles
    public float maxUpwardAngle = 45f; // Maximum upward angle in degrees when not near obstacles
    public float maxUpwardAngleNearObstacle = 60f; // Maximum upward angle in degrees when near obstacles
    public float isNearObstacleThreshold = 100f; // Distance threshold to consider the prey "near" an obstacle

    public float verticalOscillationFrequency = 0.5f; // Frequency of the sine wave for vertical movement
    private float verticalOscillationOffset = 0f; // Offset for the sine wave

    private Rigidbody rb;
    private int currentFocusPointIndex = 0; // Index to track the current focus point
    private bool isNearObstacle = false; // Flag to track if the prey is near an obstacle

    bool isEscapingChokePoint=false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        focusPoints = focusPointHolder.GetComponentsInChildren<Transform>();

#if UNITY_EDITOR
        if (debugMode)
        {
            // Select the current GameObject in the Unity Editor
            Selection.activeGameObject = this.gameObject;
        }
#endif
    }

    void Update()
    {
        // Determine the predator's reference based on some external logic
        if (God.wren)
        {
            Wren wren = God.ClosestWren(transform.position);
            predator = wren.transform;
        }
        else
        {
            predator = null;
        }

        // Update the vertical oscillation based on a sine wave
        verticalOscillationOffset = Mathf.Sin(Time.time * verticalOscillationFrequency);

        Vector3 direction = CalculateDirection();
        MoveAndRotate(direction);
    }

    Vector3 CalculateDirection()
    {
        Vector3 direction = Vector3.zero;
        Vector3 predatorDirection = (predator != null) ? (predator.position - transform.position).normalized : Vector3.zero;

        // Calculate the distance to the predator
        float distanceToPredator = (predator != null) ? Vector3.Distance(transform.position, predator.position) : float.MaxValue;

        // Check if prey is within range of the current focus point
        if (focusPoints.Length > 0 && Vector3.Distance(transform.position, focusPoints[currentFocusPointIndex].position) <= switchFocusPointDistance)
        {
            // Switch to the next focus point
            currentFocusPointIndex = (currentFocusPointIndex + 1) % focusPoints.Length;
        }

        // Perform obstacle detection and adjust raycast angles if needed
        bool obstacleDetected = DetectObstacles();

        // Only prioritize avoiding obstacles if an obstacle is detected within the threshold. forget all other limits
        if (obstacleDetected)
        {
            Debug.Log("Prioritizing Obstacle Avoidance");
            direction = AvoidObstacles(direction); // Focus solely on obstacle avoidance
        }
        else
        {
            
            // Default behavior: move towards next focus point or avoid predator
            if (predator == null || distanceToPredator > PredatorAvoidanceDistance)
                {
                    direction = (focusPoints[currentFocusPointIndex].position - transform.position).normalized;
                    Debug.Log("Moving Towards Focus Point");
                }
                else
                {
                    // Predator avoidance
                    float predatorWeight = Mathf.Clamp01((PredatorAvoidanceDistance - distanceToPredator) / PredatorAvoidanceDistance);
                    direction = -predatorDirection * predatorWeight;
                    Debug.Log("Avoiding Predator");
                }

            Vector3 redirectToNearestFocus;
            if(CheckIfOutOfFocusPointRange(out redirectToNearestFocus))
            {
                Debug.Log("Redirecting");
                direction = redirectToNearestFocus;
            }
           

            // Add sine wave oscillation to vertical direction
            direction.y += verticalOscillationOffset;

            // Adjust vertical direction if outside height bounds
            if (transform.position.y > maxHeight)
            {
                direction.y = -Mathf.Abs(direction.y); // Force downward
                Debug.Log("Adjusting Direction Downwards");
                isEscapingChokePoint = true;
            }
            else if (transform.position.y < minHeight)
            {
                direction.y = Mathf.Abs(direction.y); // Force upward
                Debug.Log("Adjusting Direction Upwards");
                isEscapingChokePoint = true;
            }


        }

        // Constrain the upward angle of direction dynamically
        float currentMaxUpwardAngle = maxUpwardAngle;
        if (isNearObstacle)
            currentMaxUpwardAngle = maxUpwardAngleNearObstacle;

        float upwardLimitRadians = currentMaxUpwardAngle * Mathf.Deg2Rad;
        float verticalComponent = Mathf.Clamp(direction.y, -Mathf.Sin(upwardLimitRadians), Mathf.Sin(upwardLimitRadians));
        direction = new Vector3(direction.x, verticalComponent, direction.z).normalized;


        return direction.normalized;
    }

    bool RaycastToFocusPoints(out Vector3 clearFocusDirection)
    {
        clearFocusDirection = Vector3.zero;
        bool foundClearPath = false;
        float closestFocusDistance = float.MaxValue;

        foreach (Transform focusPoint in focusPoints)
        {
            Vector3 toFocusPoint = focusPoint.position - transform.position;
            float distanceToFocusPoint = toFocusPoint.magnitude;

            if (Physics.Raycast(transform.position, toFocusPoint.normalized, out RaycastHit hit, distanceToFocusPoint, obstacleLayerMask))
            {
                Debug.DrawRay(transform.position, toFocusPoint.normalized * hit.distance, Color.blue); // Draw ray in blue if blocked
            }
            else
            {
                Debug.DrawRay(transform.position, toFocusPoint, Color.green); // Draw ray in green if clear
                if (distanceToFocusPoint < closestFocusDistance)
                {
                    closestFocusDistance = distanceToFocusPoint;
                    clearFocusDirection = toFocusPoint.normalized;
                    foundClearPath = true;
                }
            }
        }

        return foundClearPath;
    }

    bool CheckIfOutOfFocusPointRange(out Vector3 ToClosestOutofRange)
    {
        
        Vector3 FocusDirection = Vector3.zero;
        float closestFocusDistance = float.MaxValue;
        ToClosestOutofRange = FocusDirection;

        foreach (Transform focusPoint in focusPoints)
        {
            Vector3 toFocusPoint = focusPoint.position - transform.position;
            float distanceToFocusPoint = toFocusPoint.magnitude;

                Debug.DrawRay(transform.position, toFocusPoint, Color.green); // Draw ray in green if clear
                if (distanceToFocusPoint < closestFocusDistance)
                {
                    closestFocusDistance = distanceToFocusPoint;
                    ToClosestOutofRange = toFocusPoint;
                }
        }


        return closestFocusDistance > focusDistance;
    }

    bool DetectObstacles()
    {
        isNearObstacle = false; // Reset near obstacle flag

        if (raycastPoint != null)
        {
            Vector3 forwardDirection = raycastPoint.forward; // This should be the direction the prey is facing

            for (int i = 0; i < coneRaycastCountX; i++)
            {
                float angleX = (i - (coneRaycastCountX / 2)) * (coneAngleX / (coneRaycastCountX - 1)); // Horizontal spread
                for (int j = 0; j < coneRaycastCountY; j++)
                {
                    float angleY = (j - (coneRaycastCountY / 2)) * (coneAngleY / (coneRaycastCountY - 1)); // Vertical spread
                    Vector3 rayDirection = Quaternion.Euler(angleY, angleX, 0) * forwardDirection;
                    RaycastHit hit;

                    if (Physics.Raycast(raycastPoint.position, rayDirection, out hit, maxRaycastDistance, obstacleLayerMask))
                    {
                        Debug.DrawRay(raycastPoint.position, rayDirection * hit.distance, Color.red); // Draw ray in red when it hits an obstacle

                        if (hit.distance < isNearObstacleThreshold)
                        {
                            isNearObstacle = true; // Set flag if within evasion threshold

                        }
                    }
                    else
                    {
                        Debug.DrawRay(raycastPoint.position, rayDirection * maxRaycastDistance, Color.gray); // Draw ray in gray when no hit
                    }
                }
            }


        }
        else
        {
            Debug.LogWarning("Raycast point is not set. Raycasting will not work.");
        }

        return isNearObstacle;
    }

    Vector3 AvoidObstacles(Vector3 currentDirection)
    {
        Vector3 avoidanceDirection = Vector3.zero;
        bool isNoWayOut = true;
        bool isAllClear = true;

        if (raycastPoint != null)
        {
            Vector3 forwardDirection = raycastPoint.forward; // This should be the direction the prey is facing

            for (int i = 0; i < coneRaycastCountX; i++)
            {
                float angleX = (i - (coneRaycastCountX / 2)) * (coneAngleX / (coneRaycastCountX - 1)); // Horizontal spread
                for (int j = 0; j < coneRaycastCountY; j++)
                {
                    float angleY = (j - (coneRaycastCountY / 2)) * (coneAngleY / (coneRaycastCountY - 1)); // Vertical spread
                    Vector3 rayDirection = Quaternion.Euler(angleY, angleX, 0) * forwardDirection;
                    RaycastHit hit;

                    if (Physics.Raycast(raycastPoint.position, rayDirection, out hit, maxRaycastDistance, obstacleLayerMask))
                    {
                        Debug.DrawRay(raycastPoint.position, rayDirection * hit.distance, Color.red); // Draw ray in red when it hits an obstacle

                        if (hit.distance < obstacleAvoidanceThreshold)
                        {
                            float centralityFactor = Mathf.Abs((float)i / (coneRaycastCountX - 1) - 0.5f) * 2 + Mathf.Abs((float)j / (coneRaycastCountY - 1) - 0.5f) * 2;

                            if (centralityFactor < 1f)
                            {
                                // If raycast is more central
                                Debug.Log("Inner Obstacle Detected");
                                avoidanceDirection = (raycastPoint.position - hit.point).normalized; // Move away from the obstacle
                            }
                            else
                            {
                                // If raycast is more on the outer edges
                                Debug.Log("Outer Obstacle Detected. hug the edge");
                                avoidanceDirection += (raycastPoint.position - hit.point).normalized;// Move away from the obstacle just as much for now
                            }

                            isAllClear = false;

                        }
                    }
                    else
                    {
                        Debug.DrawRay(raycastPoint.position, rayDirection * maxRaycastDistance, Color.gray); // Draw ray in gray when no hit
                        isNoWayOut = false;
                        
                    }
                }
            }

            if (isAllClear)
            {
                isEscapingChokePoint = false;
            }

            if (isNoWayOut)
            {
                isEscapingChokePoint = true;
            }

            if (isEscapingChokePoint)
            {
                // If no open space is directly in front, raycast towards focus points. use focus direction until all clear
                //bool foundClearPathToFocusPoint = RaycastToFocusPoints(out Vector3 focusDirection);

              /*  if (foundClearPathToFocusPoint)
                {
                    currentDirection = focusDirection;
                }*/

                currentDirection = Vector3.up;
            }


            if (avoidanceDirection != Vector3.zero)
            {
                Debug.Log("Avoiding Obstacles");
                currentDirection += avoidanceDirection.normalized; // Apply avoidance direction
            }
        }
        else
        {
            Debug.LogWarning("Raycast point is not set. Raycasting will not work.");
        }

        return currentDirection.normalized;
    }

    void MoveAndRotate(Vector3 direction)
    {
        // Calculate the desired speed based on predator proximity
        float speed = (predator != null && Vector3.Distance(transform.position, predator.position) < PredatorAvoidanceDistance)
            ? Mathf.Lerp(minSpeed, maxSpeed, 1 - (Vector3.Distance(transform.position, predator.position) - PredatorAvoidanceTail) / (PredatorAvoidanceDistance - PredatorAvoidanceTail))
            : maxSpeed; // Move at max speed if no predator is detected or it's too far
        currentVelocity = speed;


        
        currentVelocity = speed;

        // Apply movement force
        Vector3 moveForce = transform.forward * speed;
        rb.velocity = moveForce;

        // Calculate the desired rotation based on predator proximity

        float rotationSpeed = (predator != null && Vector3.Distance(transform.position, predator.position) < PredatorAvoidanceDistance)
            ? Mathf.Lerp(baseRotationSpeed, maxEvasionRotationSpeed, 1 - (Vector3.Distance(transform.position, predator.position) - PredatorAvoidanceTail) / (PredatorAvoidanceDistance - PredatorAvoidanceTail))
            : maxEvasionRotationSpeed; // Move at max speed if no predator is detected or it's too far

        // Smoothly rotate towards the target direction



        Quaternion targetRotation = Quaternion.LookRotation(direction);

            if (isEscapingChokePoint) // Adjust rotation speed based on evasive needs
            {
                rotationSpeed = maxEvasionRotationSpeed; // Rotate faster if moving at high speed
            }
    

            Quaternion smoothedRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Apply the rotation to the Transform directly
            transform.rotation = smoothedRotation;
        
    }
}
