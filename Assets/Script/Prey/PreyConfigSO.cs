using UnityEngine;

[CreateAssetMenu(fileName = "PreyConfigSO", menuName = "Prey/PreyConfigSO", order = 1)]
public class PreyConfigSO : ScriptableObject
{

    public float maxScaleStartLife = 1;
    public float maxScaleEndLife = 0;

    public float maxAngleTurnBetweenFrames = 4f;

    public float dieRate = .001f;

    public float maxScale = .1f;
    public float speed = .1f;


    public float runForce = 0;
    public float runRadius = 10;


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
    public float circleRadius = 10;

    public float updraft = 0;



    // when you get far enough away, start pulling back in ( just XY? )
    public float maxDistanceStart = 100;
    public float maxDistanceEnd = 200;
    public float forceInwardsAtMaxDistanceEnd = 1;


    // tween 'current altitude' slowly so it can stay in valleys etc
    // cast down ray and cast forward ray


    public float minimumDotProductMatchForTurn;
    public float forwardSpeed;


    // applied after all other movement so we can get a 'bounce' effect for things like butterflies
    public float flapSpeed = 1;
    public float upBounceSize = 1f;
    public float forwardBounceSize = .5f;
    public float forwardBounceOffset = .5f;


    public float alwaysCenterForce = 0;

    public float noiseSize = 1;
    public float noiseSpeed = 1;
    public float noiseForce = 1;


    public float maxForwardDistance = 100;
    public float maxDownDistance = 100;



    public float groundTurnForce = 1;
    public float forwardTurnForce = 1;


    public float distanceForStartTurn = 30;
    public float distanceForHardTurn = 10;


    public float distanceToStartRun = 10;
    public float distanceToFullRun = 5;


}