using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;


public class WrenCOPY : MonoBehaviour
{



public float timeValue1;
public bool onGround;
public bool inRace;
public Color color;

public WrenNetworked networkInfo;

    /*

        References

    */
    
public TextMesh title;
    public ControllerTest controller;
    public FullBird bird;

    public Transform leftWing;
    public Transform rightWing;

    public Transform leftWingPivot;
    public Transform rightWingPivot;

    public Transform leftStickNetworkData;
    public Transform rightStickNetworkData;

    
    public Transform leftExtraNetworkData;
    public Transform rightExtraNetworkData;

    public bool isLocal;


    public Rigidbody rb;

    public Collider terrainCollider;
    public Terrain terrain;

    public Transform DebugSphere;
    public Transform RestartLocation;

    public Transform camTarget;



    public WrenMaker maker;


    public DebugLines debugger;

    public TextMeshProUGUI debugText;
    public Image debugInvertX;
    public Image debugInvertY;
    public Image debugSwapLR;

    public LineRenderer debugLR;
    public ParticleSystem movementParticleSystem;
    public ParticleSystem liftParticleSystem;

/*

    Sounds

*/

public SampleSynth[] synths;//synth1;
public AudioSource hitSound;
public AudioSource closeLoop;


public float slowFOV;
public float fastFOV;
public float groundFOV;
public float headLookForwardAmount;
public float headLookLerpSpeed;
/*

    Mechanics Params

*/

public bool swapLR;
public bool invert; // TODO

public float gravityForce;
public float tuckAddToGravityVal;


public float slowestTwistAngle;
public float fastestTwistAngle;

public float slowestBendAngle;
public float fastestBendAngle;

public float twistLerpSpeed;
public float bendLerpSpeed;

public float maxSpeed;

public float twistForceVal;

public float slowestAmountToSide;
public float fastestAmountToSide;

public float tuckReduceLiftVal;

public float closeForwardBoostVal;
public float thrustForceMultiplier;

public float rightingForce;
public float strafeVal;


public float tuckedAngularDrag;
public float untuckedAngularDrag;


public float tuckedDrag;
public float untuckedDrag;

public float tuckLerpSpeed;


public float flapToSide;
public float flapPowerUp;
public float flapPowerForward;



/*

How Wind is defined and closness to 
surface matters
*/
public float closestHeight;
public float furthestHeight;
public float closestForce;
public float furthestForce;

public float tuckReduceUpdraftVal;
public float windAmountToTheSide;

public float rightingDependentOnNotTouchingVal;

public float groundForceTweenVal;










/*

    Force Debug and references for other scripts!

*/

public string[] paramFiles;
public int paramID;
public int oParamID;

public bool showDebugForces;
public bool reset;

public float distToGround;
public Vector3 groundNormal;
public Vector3 groundDirection;
public Vector3 vel;
public Vector3 oVel;
public Vector3 inertiaTensor;

public Vector3 leftWingLiftForce;
public Vector3 leftWingLiftForcePosition;
public Vector3 rightWingLiftForce;
public Vector3 rightWingLiftForcePosition;


public Vector3 leftWingGravityForce;
public Vector3 leftWingGravityForcePosition;
public Vector3 rightWingGravityForce;
public Vector3 rightWingGravityForcePosition;


public Vector3 leftWingUpdraftForce;
public Vector3 leftWingUpdraftForcePosition;
public Vector3 rightWingUpdraftForce;
public Vector3 rightWingUpdraftForcePosition;


public Vector3 forwardForce;
public Vector3 forwardForcePosition;

public Vector3 groundBoostForce;
public Vector3 groundBoostForcePosition;

public Vector3 thrustForce;
public Vector3 thrustForcePosition;

public Vector3 upwardsRightingForce;
public Vector3 upwardsRightingForcePosition;

public Vector3 leftFlapForce;
public Vector3 leftFlapForcePosition;

public Vector3 rightFlapForce;
public Vector3 rightFlapForcePosition;

public float speed;


public float tuckAmountL;
public float tuckAmountR;

public float bendValL;
public float bendValR;

public float twistValL;
public float twistValR;

public float angleToUp;

// temp vectors for maths
public Vector3 v1 = new Vector3();
public Vector3 v2 = new Vector3();
protected Vector3 lookTarget = new Vector3();
protected float oTwistR;
protected float oTwistL;
protected float oBendL;
protected float oBendR;

protected bool oDLeft;
protected bool oDRight;
protected bool oDUp;

protected bool oTriangle;
protected bool oSquare;
protected bool oX;
protected bool oCircle;



protected Vector3 upP; 
protected Vector3 upS;
protected Vector3 d; 
protected float flapVel;

protected float distToGroundVal;

/*

    our controller values

*/
public float leftX;
public float leftY;
public float left2;


public float rightX;
public float rightY;
public float right2;

public float left3;
public float square;
public float triangle;

public float right3;
public float ex;
public float circle;

/*

    TODO

private float left3;
private float right3;
private float triangle;
private float square;
private float x;
private float circle;
*/

   void OnEnable(){

       FindInfo();
       
       //paramFiles = paramsSaver.allNames();

       v1 = new Vector3();
       v2 = new Vector3();
       
        rb.velocity = Vector3.forward * 10;
        transform.position = Vector3.zero;
        oParamID = -1;

    FullReset();


        CheckParams();
   
   }




void GetInput(){
    
    if( isLocal ){

       
        leftX = controller.left.x;
        leftY = controller.left.y;
        left2 = controller.l2;

        leftStickNetworkData.localPosition = new Vector3( leftX ,leftY , left2 );


        rightX = controller.right.x;
        rightY = controller.right.y;
        right2 = controller.r2;
        
        rightStickNetworkData.localPosition = new Vector3( rightX ,rightY , right2 );


        left3 = controller.l3 ? 1 : 0;
        square = controller.square ? 1 : 0;
        triangle = controller.triangle ? 1 : 0;
        leftExtraNetworkData.localPosition = new Vector3( left3 , square , triangle );
        

        right3 = controller.r3 ? 1 : 0;
        ex = controller.x ? 1 : 0;
        circle = controller.circle ? 1 : 0;
        rightExtraNetworkData.localPosition = new Vector3( right3 , ex , circle );




    }else{

        leftX = leftStickNetworkData.localPosition.x;
        leftY = leftStickNetworkData.localPosition.y;
        left2 = leftStickNetworkData.localPosition.z;

        rightX = rightStickNetworkData.localPosition.x;
        rightY = rightStickNetworkData.localPosition.y;
        right2 = rightStickNetworkData.localPosition.z;

        left3       = leftExtraNetworkData.localPosition.x;
        square      = leftExtraNetworkData.localPosition.y;
        triangle    = leftExtraNetworkData.localPosition.z;
        
        right3  = rightExtraNetworkData.localPosition.x;
        ex      = rightExtraNetworkData.localPosition.y;
        circle  = rightExtraNetworkData.localPosition.z;


    }


}


void FixedUpdate(){


if(!isLocal){
    
    GetInput();
    PassParameters();


}else{

    GetInput();
    SetUp();
    
    PassParameters();




    if( !onGround ){

        UpdateVelocity();

        BendAndTwist();
        Flap();
        RotationValues();

        GravityForces();

        LiftForces();

        GetGroundDistanceInfo();
        GroundUpdraft();

        GroundBoost();
        RightingForces();
   
    }else{
    
        WhileOnGround();
    
    }

    SynthsAndSounds();
    ShowDebugLines();
    UpdateParticleSystem();
    CameraWork();

    UpdateCarriedItems();

    if( ex > .5 && onGround == true ){
        TakeOff();
    }

    // if our controller has dpad down reset
    reset = controller.dDown;

}

    
}



// This will set all the needed values so we can actually see the data we need from all our values
void PassParameters(){

    
    oVel = vel;
    vel = rb.velocity;
    bird._Velocity = rb.velocity;

    

}

public virtual void SetUp(){
    // Resets our position
    // by putting us way in the clouds
    if( reset ){
        FullReset();
    }

    // Reset position/velocity to next player in list
    if (isLocal) {

        // Sees if we can get to the next player in the update loop

        if (controller.x && !oX) {
            DropCarriedItems();
        }


        oTriangle = controller.triangle;
        oSquare = controller.square;
        oCircle = controller.circle;
        oX = controller.x;
    
    }

    CheckParams();
    v1 = new Vector3();
    v2 = new Vector3();
    
    // Grabs our Rigidbody
    if(rb == null ){ rb = GetComponent<Rigidbody>(); }


}

public virtual void UpdateVelocity(){
    // Set our Rotation
    /*

        THIS IS THE LINE THAT SEEMS TO BE PREVENTING YAW,
        but comment it out and u will see the dangers

        if you crank up the twist force 2 you can start to see it working but it feels whack 
        and not physically right to me...


    */


    Vector3 velT = vel;
    vel +=transform.right * leftX * .001f* strafeVal * velT.magnitude;
    vel +=transform.right * rightX * .001f* strafeVal *velT.magnitude;

    vel = vel.normalized * velT.magnitude;


    if( vel.magnitude > maxSpeed ){
        vel = vel.normalized * maxSpeed;
    }

    speed = vel.magnitude;

    rb.velocity = vel;

    if( vel.magnitude == 0 ){
        vel = Vector3.forward * .00001f;
    }
    transform.rotation =  Quaternion.LookRotation(vel, transform.up);//transform.up);//,.1f);
}

public virtual void BendAndTwist(){
    /*


 ______     _______    _          ______           __         _________             _________   _______   _________
(  ___ \   (  ____ \  ( (    /|  (  __  \         /__\        \__   __/  |\     /|  \__   __/  (  ____ \  \__   __/
| (   ) )  | (    \/  |  \  ( |  | (  \  )       ( \/ )          ) (     | )   ( |     ) (     | (    \/     ) (   
| (__/ /   | (__      |   \ | |  | |   ) |        \  /           | |     | | _ | |     | |     | (_____      | |   
|  __ (    |  __)     | (\ \) |  | |   | |        /  \/\         | |     | |( )| |     | |     (_____  )     | |   
| (  \ \   | (        | | \   |  | |   ) |       / /\  /         | |     | || || |     | |           ) |     | |   
| )___) )  | (____/\  | )  \  |  | (__/  )      (  \/  \         | |     | () () |  ___) (___  /\____) |     | |   
|/ \___/   (_______/  |/    )_)  (______/        \___/\/         )_(     (_______)  \_______/  \_______)     )_(   
                                                                                                                   



*/


// inversion val;
float inVal = invert ? -1:1;

// Tilts our wings based on how much we pull back


float big = left3;// ? 1 : 0;

float fAngle = Mathf.Lerp(slowestTwistAngle, fastestTwistAngle,  normalizedSpeed);

float lr = leftY;
if( swapLR ){lr=rightY;}
float twistAngle = Mathf.Lerp( lr *fAngle * inVal, -90, big);
twistAngle = Mathf.Lerp( oTwistL, twistAngle , twistLerpSpeed);
oTwistL = twistAngle;

float bendAngle =Mathf.Lerp(slowestBendAngle, fastestBendAngle,  normalizedSpeed);
bendAngle =  -leftX * bendAngle;
bendAngle = Mathf.Lerp( oBendL , bendAngle , bendLerpSpeed);
oBendL = bendAngle;

twistValL = twistAngle;
bendValL = bendAngle;

leftWingPivot.localRotation = Quaternion.AngleAxis(twistAngle,Vector3.right) * Quaternion.AngleAxis(bendAngle, Vector3.forward);


lr = rightY;
if( swapLR ){lr=leftY;}
big = right3;// ? 1 : 0;
twistAngle = Mathf.Lerp( lr *fAngle * inVal, -90, big);
twistAngle = Mathf.Lerp( oTwistR , twistAngle , twistLerpSpeed);
oTwistR = twistAngle;


bendAngle =Mathf.Lerp(slowestBendAngle, fastestBendAngle,  normalizedSpeed);
bendAngle =  -rightX * bendAngle;
bendAngle = Mathf.Lerp( oBendR , bendAngle , bendLerpSpeed);
oBendR = bendAngle;



twistValR = twistAngle;
bendValR = bendAngle;

rightWingPivot.localRotation = Quaternion.AngleAxis(twistAngle,Vector3.right) * Quaternion.AngleAxis(bendAngle, Vector3.forward);





}

public virtual void Flap(){
    

/*
 _______    _          _______    _______ 
(  ____ \  ( \        (  ___  )  (  ____ )
| (    \/  | (        | (   ) |  | (    )|
| (__      | |        | (___) |  | (____)|
|  __)     | |        |  ___  |  |  _____)
| (        | |        | (   ) |  | (      
| )        | (____/\  | )   ( |  | )      
|/         (_______/  |/     \|  |/       
                                          
*/
flapVel = tuckAmountL - left2;
tuckAmountL =  Mathf.Lerp( tuckAmountL , left2 , tuckLerpSpeed );
leftFlapForce = Mathf.Clamp(-flapVel,0,1) *  (leftWing.up* flapPowerUp + leftWing.forward * flapPowerForward);
leftFlapForcePosition = Vector3.Lerp( transform.position,leftWing.position,flapToSide); 
rb.AddForceAtPosition( leftFlapForce , leftFlapForcePosition  );

flapVel = tuckAmountR - right2;
tuckAmountR =  Mathf.Lerp( tuckAmountR , right2 , tuckLerpSpeed );

rightFlapForce = Mathf.Clamp(-flapVel,0,1) *  (rightWing.up* flapPowerUp + rightWing.forward * flapPowerForward);
rightFlapForcePosition = Vector3.Lerp( transform.position,rightWing.position,flapToSide); 
rb.AddForceAtPosition(  rightFlapForce , rightFlapForcePosition  );




// "Tucks" in our wings
leftWingPivot.localScale = new Vector3( 3 - tuckAmountL * 2.8f , .2f , 1 );
rightWingPivot.localScale = new Vector3( 3 - tuckAmountR * 2.8f , .2f , 1 );


}

public virtual void RotationValues(){
    
    /*

        First Rotation using tuck test
    */
    Vector3 t1 = new Vector3( .41f, .41f , .167f);
    Vector3 t2 = new Vector3( 10 , 10 , .1f);
    float totalTuckVal = (tuckAmountL + tuckAmountR ) /2;
    inertiaTensor = t1;//Vector3.Lerp( t1.normalized , t2.normalized, totalTuckVal);
    rb.inertiaTensor = inertiaTensor;

    rb.angularDrag = Mathf.Lerp( untuckedAngularDrag , tuckedAngularDrag , totalTuckVal);
    rb.drag = Mathf.Lerp( untuckedDrag , tuckedDrag , totalTuckVal);

}

public virtual void GravityForces(){
    
/*

 _______    _______    _______              _________  _________           
(  ____ \  (  ____ )  (  ___  )  |\     /|  \__   __/  \__   __/  |\     /|
| (    \/  | (    )|  | (   ) |  | )   ( |     ) (        ) (     ( \   / )
| |        | (____)|  | (___) |  | |   | |     | |        | |      \ (_) / 
| | ____   |     __)  |  ___  |  ( (   ) )     | |        | |       \   /  
| | \_  )  | (\ (     | (   ) |   \ \_/ /      | |        | |        ) (   
| (___) |  | ) \ \__  | )   ( |    \   /    ___) (___     | |        | |   
(_______)  |/   \__/  |/     \|     \_/     \_______/     )_(        \_/  


    Each wing has a down on it, ( the more tucked it is )
    the more force downwards there is...

*/
float gForce;

gForce = -( gravityForce * ((1-tuckAddToGravityVal) + tuckAmountL * tuckAddToGravityVal));

leftWingGravityForce = gForce * Vector3.up;
leftWingGravityForcePosition = leftWing.position;
rb.AddForceAtPosition(leftWingGravityForce,leftWingGravityForcePosition);

gForce = -( gravityForce * ((1-tuckAddToGravityVal) + tuckAmountR * tuckAddToGravityVal));
rightWingGravityForce = gForce * Vector3.up;
rightWingGravityForcePosition = rightWing.position;
rb.AddForceAtPosition(rightWingGravityForce,rightWingGravityForcePosition);

}

public virtual void LiftForces(){
/*


 _       _________ _______ _________             _______  _______  _______  _______  _______  _______ 
( \      \__   __/(  ____ \\__   __/            (  ____ \(  ___  )(  ____ )(  ____ \(  ____ \(  ____ \
| (         ) (   | (    \/   ) (               | (    \/| (   ) || (    )|| (    \/| (    \/| (    \/
| |         | |   | (__       | |               | (__    | |   | || (____)|| |      | (__    | (_____ 
| |         | |   |  __)      | |               |  __)   | |   | ||     __)| |      |  __)   (_____  )
| |         | |   | (         | |               | (      | |   | || (\ (   | |      | (            ) |
| (____/\___) (___| )         | |               | )      | (___) || ) \ \__| (____/\| (____/\/\____) |
(_______/\_______/|/          )_(               |/       (_______)|/   \__/(_______/(_______/\_______)


                                                                                            

    each wing gets the dot product with itself and velocity, 
    and adds a force in its up direction that is scaled by that
    matching value as well as the velocity.

    The 'location' that this force is added is scaled out
    from the center to the left wing position 
    ( amount to the side bigger means that the force is closer to center
    which means more 'pitching' but less 'rolling' )

    ^
    | ^ 
    \ | ^
      \ |
        \


*/

float match;
float fToTheSide = Mathf.Lerp( slowestAmountToSide , fastestAmountToSide , normalizedSpeed );

v1 = leftWing.up;
match = -Vector3.Dot( leftWing.up , vel.normalized);    

v2 = Vector3.Lerp( transform.position , leftWing.position , fToTheSide );
leftWingLiftForce = match*v1 * twistForceVal * vel.magnitude * (1-tuckAmountL*tuckReduceLiftVal);
leftWingLiftForcePosition =  v2;
rb.AddForceAtPosition( leftWingLiftForce , leftWingLiftForcePosition);


v1 = rightWing.up;
match = -Vector3.Dot( rightWing.up , vel.normalized);
v2=Vector3.Lerp( transform.position , rightWing.position , fToTheSide );  
rightWingLiftForce = match*v1 * twistForceVal * vel.magnitude * (1-tuckAmountR*tuckReduceLiftVal);
rightWingLiftForcePosition = v2;
rb.AddForceAtPosition( rightWingLiftForce , rightWingLiftForcePosition );




// Push our object forward constantly
// this is the 'thrust' and in general should feel
// like pretty much nothing to me 
// ( though maybe it wou make it more interesting )
thrustForce = transform.forward * thrustForceMultiplier;
thrustForcePosition = transform.position;
rb.AddForceAtPosition( thrustForce, thrustForcePosition );



// This is where the 'yaw' should go
// but as you can see it pretty much does nothing
//rb.AddForceAtPosition( transform.right * controller.leftX * twistForce2 , transform.position + transform.forward );
//rb.AddForceAtPosition( transform.right * controller.rightX * twistForce2 , transform.position + transform.forward );


}

public virtual void GetGroundDistanceInfo(){
    /*

 _______    _______    _______               _          ______           ______    _________   _______   _________   _______    _          _______    _______ 
(  ____ \  (  ____ )  (  ___  )  |\     /|  ( (    /|  (  __  \         (  __  \   \__   __/  (  ____ \  \__   __/  (  ___  )  ( (    /|  (  ____ \  (  ____ \
| (    \/  | (    )|  | (   ) |  | )   ( |  |  \  ( |  | (  \  )        | (  \  )     ) (     | (    \/     ) (     | (   ) |  |  \  ( |  | (    \/  | (    \/
| |        | (____)|  | |   | |  | |   | |  |   \ | |  | |   ) |        | |   ) |     | |     | (_____      | |     | (___) |  |   \ | |  | |        | (__    
| | ____   |     __)  | |   | |  | |   | |  | (\ \) |  | |   | |        | |   | |     | |     (_____  )     | |     |  ___  |  | (\ \) |  | |        |  __)   
| | \_  )  | (\ (     | |   | |  | |   | |  | | \   |  | |   ) |        | |   ) |     | |           ) |     | |     | (   ) |  | | \   |  | |        | (      
| (___) |  | ) \ \__  | (___) |  | (___) |  | )  \  |  | (__/  )        | (__/  )  ___) (___  /\____) |     | |     | )   ( |  | )  \  |  | (____/\  | (____/\
(_______)  |/   \__/  (_______)  (_______)  |/    )_)  (______/         (______/   \_______/  \_______)     )_(     |/     \|  |/    )_)  (_______/  (_______/

*/


float tmpDist;
Vector3 tmpDir;
Vector3 tmpNorm;

tmpNorm = Vector3.up;
tmpDir = -Vector3.up;
RaycastHit hit;

tmpDist = 1000000;
if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
    tmpDist = hit.distance;
    tmpNorm = hit.normal;
    tmpDir = -Vector3.up;
}  

bool hitVal = false;
Ray ray = new Ray();
for( int i = 0; i< 10; i++){
for( int j = 0; j< 10; j++){

    float a1 = ((float)i)/10;
    float a2 = ((float)j)/10;

    a2 *= Mathf.PI *2;
    a1 *= Mathf.PI *2;



    Vector3 dir = SphericalToCartesian( a1,a2).normalized;


//    if( i == 5  && j == 5){ print( dir );}

ray.origin = transform.position;
ray.direction = dir;
    
    if (terrainCollider.Raycast (ray, out hit , 10000)) {

        //debugger.SetLine( i + 2 + j * 10 , transform.position , transform.position + dir * hit.distance );
    
            hitVal = true;
        if( hit.distance< tmpDist){
            tmpDist = hit.distance;
            tmpNorm = hit.normal;
            tmpDir = dir;
        }


    }else{
        //debugger.SetLine( i + 2 + j * 10 , transform.position , transform.position  );
    }

}
}

 
 // if we haven't hit anything assign defaults

if( !hitVal ){
    print("hmmm");
     tmpDist = 3000;
            tmpNorm = Vector3.up;
            tmpDir = -Vector3.up;
}

distToGround = Mathf.Lerp( distToGround , tmpDist , groundForceTweenVal);
groundNormal = Vector3.Lerp( groundNormal , tmpNorm , groundForceTweenVal);
groundDirection = Vector3.Lerp( groundDirection , tmpDir , groundForceTweenVal);


// Originally was making it so that you can be
// further away from cliff sides before you hit the updraft
// but it felt a bit to bananas
float fFurthestHeight = furthestHeight * 10 * (1.1f-Vector3.Dot( Vector3.up , groundNormal ));

fFurthestHeight = furthestHeight;

// This is getting our smooth step from the closest and furthest heights
distToGroundVal = (distToGround - closestHeight)/ (fFurthestHeight-closestHeight);
distToGroundVal = Mathf.Clamp( distToGroundVal , 0, 1);
distToGroundVal = distToGroundVal * distToGroundVal * (3 - 2 * distToGroundVal);
distToGroundVal = (furthestForce + closestForce-furthestForce) * (1-distToGroundVal);

// Making it so that there is more upwards force the more vertical
// the slope you are next to is
distToGroundVal *=  (3-2*Vector3.Dot( Vector3.up , groundNormal )) / 3;
}

public virtual void GroundUpdraft(){
    
/*
    For each wing:

    See if the up force would cause lift or not
    ( when our wings are tucked will be none )

    lying about the direction, calling it up instead of in wing normal

    ^|
   ^!|
  ^!!|
 ^!!!|
^!!!!|

*/


leftWingUpdraftForce = Vector3.up * Mathf.Abs(Vector3.Dot( leftWing.up,Vector3.up))*distToGroundVal * (1-tuckAmountL*tuckReduceUpdraftVal);
leftWingUpdraftForcePosition = Vector3.Lerp( transform.position , leftWing.position , windAmountToTheSide);
rb.AddForceAtPosition( leftWingUpdraftForce , leftWingUpdraftForcePosition );

rightWingUpdraftForce = Vector3.up * Mathf.Abs(Vector3.Dot( rightWing.up,Vector3.up))*distToGroundVal * (1-tuckAmountR*tuckReduceUpdraftVal);
rightWingUpdraftForcePosition = Vector3.Lerp( transform.position , rightWing.position , windAmountToTheSide);
rb.AddForceAtPosition( rightWingUpdraftForce , rightWingUpdraftForcePosition );
}

public virtual void GroundBoost(){
    /*

    closer ground faster forwards force
    (its a lie)

    ->
    -->
    ----->
    --------->
    ___________________

*/

// Forward push as well!
// if you are close enough to the surface!
groundBoostForce =  transform.forward * distToGroundVal  * .01f * closeForwardBoostVal;
groundBoostForcePosition = transform.position;
rb.AddForceAtPosition( groundBoostForce, groundBoostForcePosition );

} 

public virtual void RightingForces(){
/*


 _______   _________   _______              _________  _________   _          _______          _______    _______    _______    _______    _______    _______ 
(  ____ )  \__   __/  (  ____ \  |\     /|  \__   __/  \__   __/  ( (    /|  (  ____ \        (  ____ \  (  ___  )  (  ____ )  (  ____ \  (  ____ \  (  ____ \
| (    )|     ) (     | (    \/  | )   ( |     ) (        ) (     |  \  ( |  | (    \/        | (    \/  | (   ) |  | (    )|  | (    \/  | (    \/  | (    \/
| (____)|     | |     | |        | (___) |     | |        | |     |   \ | |  | |              | (__      | |   | |  | (____)|  | |        | (__      | (_____ 
|     __)     | |     | | ____   |  ___  |     | |        | |     | (\ \) |  | | ____         |  __)     | |   | |  |     __)  | |        |  __)     (_____  )
| (\ (        | |     | | \_  )  | (   ) |     | |        | |     | | \   |  | | \_  )        | (        | |   | |  | (\ (     | |        | (              ) |
| ) \ \__  ___) (___  | (___) |  | )   ( |     | |     ___) (___  | )  \  |  | (___) |        | )        | (___) |  | ) \ \__  | (____/\  | (____/\  /\____) |
|/   \__/  \_______/  (_______)  |/     \|     )_(     \_______/  |/    )_)  (_______)        |/         (_______)  |/   \__/  (_______/  (_______/  \_______)
                                                                                                                                                              


-->|<--
   |
   |

   

*/

// Righting Force by
// pushing towrds the desired normal!
upP = transform.up;

// Desired up value
upS = Vector3.Cross(Vector3.Cross(vel.normalized,Vector3.up).normalized,vel.normalized);
d = upP - upS;

angleToUp = SignedAngleBetween(upP , upS , Vector3.up );

// Only rights if we are NOT touching stuff
float maxVal = Mathf.Max( new float[]{ 
        //Mathf.Abs(controller.leftY) , 
        //Mathf.Abs(controller.leftX) , 
        //Mathf.Abs(controller.rightY) , 
        //Mathf.Abs(controller.rightX) , 
        Mathf.Abs(left2) , 
        Mathf.Abs(right2) 
    });

upwardsRightingForce = d * rightingForce * (1-maxVal*rightingDependentOnNotTouchingVal);

/*
float multi = 0;
if( Mathf.Abs(angleToUp) > rightingForceAngleApplication ){
    multi = Mathf.Abs(angleToUp)-rightingForceAngleApplication;
    multi /= 90;
}

// upwardsRightingForce = -transform.right*multi * .1f * angleToUp * rightingForce;// * (1-maxVal*rightingDependentOnNotTouchingVal);
*/
upwardsRightingForcePosition = transform.position + transform.up;


rb.AddForceAtPosition( upwardsRightingForce , upwardsRightingForcePosition );

}


public void WhileOnGround(){

rb.position += transform.forward * leftY * .3f;
rb.position += transform.right * leftX * .3f;


        rb.velocity = Vector3.zero;
Ray ray = new Ray();
RaycastHit hit;
ray.origin = rb.position;
ray.direction = Vector3.up * -1;
    
    if (terrainCollider.Raycast (ray, out hit , 10000)) {
        rb.position = hit.point + Vector3.up * 10;
    }else{
        ray.direction = Vector3.up * 1; 
        if (terrainCollider.Raycast (ray, out hit , 10000)) {
            rb.position = hit.point + Vector3.up * 10;
        }
    }

transform.LookAt( transform.position + transform.forward + transform.right * rightX * .02f + transform.up * rightY * .02f - transform.up * .01f * Vector3.Dot( transform.forward , Vector3.up ) ,new Vector3(0,1,0));

}


public virtual void ShowDebugLines(){
if( showDebugForces ){

debugger.SetForceLine(0,leftWingUpdraftForcePosition,leftWingUpdraftForce);
debugger.SetForceLine(1,rightWingUpdraftForcePosition,rightWingUpdraftForce);

debugger.SetForceLine(  2 , leftWingLiftForcePosition ,leftWingLiftForce );
debugger.SetForceLine(  3 , rightWingLiftForcePosition ,rightWingLiftForce );
debugger.SetForceLine(  4 , leftWingGravityForcePosition ,leftWingGravityForce );
debugger.SetForceLine(  5 , rightWingGravityForcePosition ,rightWingGravityForce );
debugger.SetForceLine(  6 , leftWingUpdraftForcePosition ,leftWingUpdraftForce );
debugger.SetForceLine(  7 , rightWingUpdraftForcePosition ,rightWingUpdraftForce );
debugger.SetForceLine(  8 , forwardForcePosition ,forwardForce );
debugger.SetForceLine(  9 , groundBoostForcePosition ,groundBoostForce );
debugger.SetForceLine( 10 , thrustForcePosition ,thrustForce );
debugger.SetForceLine( 11 , upwardsRightingForcePosition ,upwardsRightingForce );
debugger.SetForceLine( 12 , leftFlapForcePosition ,leftFlapForce );
debugger.SetForceLine( 13 , rightFlapForcePosition ,rightFlapForce );
}
}

public virtual void UpdateParticleSystem(){
 ParticleSystem ps = liftParticleSystem;
        var sz = ps.sizeOverLifetime;
        sz.enabled = true;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0f);
        curve.AddKey(0.2f, distToGroundVal / closestForce );
        curve.AddKey(1.0f, 0.0f);

        sz.size = new ParticleSystem.MinMaxCurve(1.5f, curve);


        var emission = ps.emission;

        emission.rateOverTime = (distToGroundVal / closestForce) * (distToGroundVal / closestForce) * 300; 

/*
  var velPS = movementParticleSystem.velocityOverLifetime;
        velPS.enabled = true;
        velPS.space = ParticleSystemSimulationSpace.World;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(vel.x, 0.0f);
        curve.AddKey(vel.x, 1.0f);
        velPS.x = new ParticleSystem.MinMaxCurve(1.0f, curve);

        
        curve.AddKey(vel.y, 0.0f);
        curve.AddKey(vel.y, 1.0f);
        velPS.y = new ParticleSystem.MinMaxCurve(1.0f, curve);

        
        curve.AddKey(vel.z, 0.0f);
        curve.AddKey(vel.z, 1.0f);
        velPS.z = new ParticleSystem.MinMaxCurve(1.0f, curve);*/
}

public virtual void SynthsAndSounds(){
/*


 _______               _         _________              _______    _______            __              _______    _______               _          ______     _______ 
(  ____ \  |\     /|  ( (    /|  \__   __/  |\     /|  (  ____ \  (  ____ \          /__\            (  ____ \  (  ___  )  |\     /|  ( (    /|  (  __  \   (  ____ \
| (    \/  ( \   / )  |  \  ( |     ) (     | )   ( |  | (    \/  | (    \/         ( \/ )           | (    \/  | (   ) |  | )   ( |  |  \  ( |  | (  \  )  | (    \/
| (_____    \ (_) /   |   \ | |     | |     | (___) |  | (__      | (_____           \  /            | (_____   | |   | |  | |   | |  |   \ | |  | |   ) |  | (_____ 
(_____  )    \   /    | (\ \) |     | |     |  ___  |  |  __)     (_____  )          /  \/\          (_____  )  | |   | |  | |   | |  | (\ \) |  | |   | |  (_____  )
      ) |     ) (     | | \   |     | |     | (   ) |  | (              ) |         / /\  /                ) |  | |   | |  | |   | |  | | \   |  | |   ) |        ) |
/\____) |     | |     | )  \  |     | |     | )   ( |  | (____/\  /\____) |        (  \/  \          /\____) |  | (___) |  | (___) |  | )  \  |  | (__/  )  /\____) |
\_______)     \_/     |/    )_)     )_(     |/     \|  (_______/  \_______)         \___/\/          \_______)  (_______)  (_______)  |/    )_)  (______/   \_______)
                                                                                                                                                                     


*/


// Sound for when you get reallllly close to something
float loopVal =  .005f * vel.magnitude *vel.magnitude/(100+distToGround*distToGround);
closeLoop.pitch = loopVal;
closeLoop.volume = loopVal;


// synth 3 just correspodns to our velocity!
synths[2].pitch = .02f * vel.magnitude;

// Synths for left wing
synths[0].speed =  .01f/Mathf.Abs(Vector3.Dot(leftWingLiftForce * .01f , transform.forward));//m* twistForceVal * vel.magnitude;
synths[0].pitch = vel.magnitude * .01f;


// Synths for right wing
synths[1].speed =  .01f/Mathf.Abs(Vector3.Dot( rightWingLiftForce * .01f , transform.forward));//m* twistForceVal * vel.magnitude;
synths[1].pitch = vel.magnitude * .01f;


synths[4].pitch = .1f * leftWingUpdraftForce.magnitude;
synths[4].speed = .1f/synths[4].pitch;
synths[4].volume = synths[4].pitch;

synths[3].pitch = .1f * rightWingUpdraftForce.magnitude;
synths[3].speed = .1f/synths[3].pitch;
synths[3].volume = synths[3].pitch;

}

public virtual void CameraWork(){
    
  /*
  Tweens out FOV
  if we are going fast enough
  */
  float fov =  Mathf.Clamp( vel.magnitude * .7f,slowFOV, fastFOV);

  if( onGround ){ fov = groundFOV;  }
  Camera.main.fieldOfView = Mathf.Lerp( Camera.main.fieldOfView , fov , .1f);


  //camTarget.transform.position = Vector3.Lerp( camTarget.transform.position , bird.head.position - bird.head.TransformDirection( new Vector3( 0 ,-4,20)) , headLookLerpSpeed);
  
  Vector3 newTarget = Vector3.Lerp( camTarget.forward ,  bird.head.forward * headLookForwardAmount, headLookLerpSpeed);
  


 lookTarget =  Vector3.zero;//Vector3.Lerp( lookTarget , bird.head.forward * headLookForwardAmount , headLookLerpSpeed );
 //lookTarget = Vector3.Lerp( lookTarget , bird.head.forward * headLookForwardAmount , headLookLerpSpeed );

  camTarget.LookAt(  transform.position + lookTarget, Vector3.up);

}


public List<GameObject> CarriedItems;


void OnCollisionEnter( Collision c ){
   // if( c.collider.tag == "terrain" && onGround == false ){
   // print("hellsosds");
        HitGround();
    //}

    //print( c.collider.tag );

    if( c.collider.tag == "Food"){
        PickUpItem( c.collider.gameObject);
    }
 }

 void PickUpItem(GameObject g){
     if( CarriedItems == null ){
         CarriedItems = new List<GameObject>();
     }
 
     CarriedItems.Add( g );
    g.transform.position = transform.position - transform.up * 1- transform.forward * 3;
    //g.transform.parent = transform;
    g.transform.localScale = new Vector3(4,4,4);
    g.GetComponent<Rigidbody>().drag = 3;;
 
 }

 public virtual void DropCarriedItems(){

     foreach( GameObject g in CarriedItems){
        g.GetComponent<Rigidbody>().drag = .4f;
        g.transform.localScale = new Vector3(4,4,4);
     } 
     CarriedItems = new List<GameObject>();
 }

 public virtual void UpdateCarriedItems(){
     foreach( GameObject g in CarriedItems){

            Vector3 targetPosition = transform.position - transform.up * 1- transform.forward * 3;

            g.GetComponent<Rigidbody>().AddForce( -100 * (g.transform.position - targetPosition) );
     }

 }


public virtual void CheckParams(){


    if( controller.dUp && !oDUp ){

        print(controller.invertX);
        print(controller.invertY);
        print(swapLR);

        if( controller.invertX == false && controller.invertY == false && swapLR == false){
            controller.invertX = false;
            controller.invertY = false;
            swapLR = true;
        }else if( controller.invertX == false  && controller.invertY == false && swapLR == true){
            controller.invertX = true;
            controller.invertY = false;
            swapLR = true;
        }else if( controller.invertX == true  && controller.invertY == false && swapLR == true){
            controller.invertX = true;
            controller.invertY = true;
            swapLR = true;
        }else if( controller.invertX == true  && controller.invertY == true && swapLR == true){
            controller.invertX = false;
            controller.invertY = true;
            swapLR = true;
        }else if( controller.invertX == false  && controller.invertY == true && swapLR == true){
            controller.invertX = true;
            controller.invertY = false;
            swapLR = false;
        }else if( controller.invertX == true  && controller.invertY == false && swapLR == false){
            controller.invertX = true;
            controller.invertY = true;
            swapLR = false;
        }else if( controller.invertX == true  && controller.invertY == true && swapLR == false){
            controller.invertX = false;
            controller.invertY = true;
            swapLR = false;
        }else if( controller.invertX == false  && controller.invertY == true && swapLR == false){
            controller.invertX = false;
            controller.invertY = false;
            swapLR = false;
        }






        if( isLocal ){
            debugInvertX.enabled = controller.invertX;
            debugInvertY.enabled = controller.invertY;
            debugSwapLR.enabled  = swapLR;
        }
        //

    }

    if( controller.dLeft && ! oDLeft ){
        paramID -= 1;
        if( paramID < 0 ){ paramID = paramFiles.Length-1;}
    }

    if( controller.dRight && ! oDRight ){
        paramID += 1;
        if( paramID >= paramFiles.Length ){ paramID = 0; }
    }

    oDRight = controller.dRight;
    oDLeft = controller.dLeft;
    oDUp = controller.dUp;

    if( paramID  != oParamID ){
        loadParams(paramID);
    }

}

public virtual void loadParams( int id ){
           //paramsSaver.Load(paramFiles[id]);
           if( isLocal ) debugText.text = paramFiles[id];
            oParamID = paramID;
}

public virtual void FullReset(){
bird.ResetFeatherValues();
        rb.velocity = Vector3.zero;
        transform.position = RestartLocation.position;//new Vector3( 0 ,2000, 0 );
       
       
       if( isLocal ) Camera.main.transform.position = transform.position - Vector3.forward * 4;
    

}

public virtual void LocalReset(){
     

    rb.velocity = Vector3.zero;
    transform.position = new Vector3( transform.position.x , terrain.SampleHeight(transform.position ) + 100 , transform.position.z);// RestartLocation.position;//new Vector3( 0 ,2000, 0 );
    if( isLocal ) Camera.main.transform.position = transform.position;//- Vector3.forward * 4;

    bird.ResetFeatherValues();

 }

 public virtual void ResetToOtherPlayer(GameObject other){
     bird.ResetFeatherValues();
        var otherRb = other.GetComponent<Rigidbody>();
        if (otherRb) {
            rb.velocity = otherRb.velocity;
        } else {
            rb.velocity = Vector3.zero;
        }
        transform.position = other.transform.position;
        transform.rotation = other.transform.rotation;
}
  
public virtual void SaveNewParamSet(){
    string name = "controlSet" + Mathf.Floor(Random.Range(0.001f,.999f) * 100000);
    //paramsSaver.Save(name);
   // paramFiles = paramsSaver.allNames();
}

public virtual void Save(){
   // paramsSaver.Save(paramFiles[paramID]);
}


/*


            _______    _          _______    _______    _______    _______ 
|\     /|  (  ____ \  ( \        (  ____ )  (  ____ \  (  ____ )  (  ____ \
| )   ( |  | (    \/  | (        | (    )|  | (    \/  | (    )|  | (    \/
| (___) |  | (__      | |        | (____)|  | (__      | (____)|  | (_____ 
|  ___  |  |  __)     | |        |  _____)  |  __)     |     __)  (_____  )
| (   ) |  | (        | |        | (        | (        | (\ (           ) |
| )   ( |  | (____/\  | (____/\  | )        | (____/\  | ) \ \__  /\____) |
|/     \|  (_______/  (_______/  |/         (_______/  |/   \__/  \_______)
                                                                           


*/
public Vector3 PolarToCartesian(float x,float y)
 {
 
     //an origin vector, representing lat,lon of 0,0. 
 
     var origin=Vector3.forward;//(0,0,1);
     //build a quaternion using euler angles for lat,lon
     var rotation = Quaternion.Euler(x,y,0);
     //transform our reference vector by the rotation. Easy-peasy!
     Vector3  point=rotation*origin;
 
     return point;
 }

public float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n){
    // angle in [0,180]
    float angle = Vector3.Angle(a,b);
    float sign = Mathf.Sign(Vector3.Dot(n,Vector3.Cross(a,b)));

    // angle in [-179,180]
    float signed_angle = angle * sign;

    // angle in [0,360] (not used but included here for completeness)
    //float angle360 =  (signed_angle + 180) % 360;

    return signed_angle;
}

public Vector3 SphericalToCartesian(float polar, float elevation ){
        float a = 1 * Mathf.Cos(elevation);


        float x = a * Mathf.Cos(polar);
        float y = 1 * Mathf.Sin(elevation);
        float z = a * Mathf.Sin(polar);

        return new Vector3( x,y,z);
    }

public float normalizedSpeed{
    get { return vel.magnitude/maxSpeed; }
}

public void FindInfo(){


    terrain = GameObject.Find( "Terrain").GetComponent<Terrain>();
    terrainCollider = GameObject.Find( "Terrain").GetComponent<Collider>();
    RestartLocation = GameObject.Find( "StartPosition").transform;   
    controller = GameObject.Find("Rewired Input Manager").GetComponent<ControllerTest>();
    maker = GameObject.FindGameObjectWithTag("Realtime")?.GetComponent<WrenMaker>();

 
}
 


public void SetLocal( bool connected  ){
    isLocal = true;

    if( connected ){
        leftStickNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
        rightStickNetworkData.GetComponent<RealtimeTransform>().RequestOwnership();
    }
    
    debugText = GameObject.Find("ControllerType").GetComponent<TextMeshProUGUI>();
    debugInvertX = GameObject.Find("CheckmarkX").GetComponent<Image>();
    debugInvertY = GameObject.Find("CheckmarkY").GetComponent<Image>();
    debugSwapLR = GameObject.Find("CheckmarkSwap").GetComponent<Image>();
    debugger = GameObject.Find("DebugLines").GetComponent<DebugLines>();

}



/*



    Network








*/
public void HitGround(){

  //  rb.isKinematic = true;
    rb.position =rb.position + Vector3.up * 10;
    hitSound.Play();
    onGround = true;
    rb.angularVelocity = Vector3.zero;
    rb.velocity = Vector3.zero;//0;

    bird.HitGround();

    if( isLocal ){
       networkInfo.SetOnGround( true );
   }

}

public void TransportToPosition( Vector3 pos , Vector3 vel ){
    bird.Explode();
    rb.velocity = vel;
    rb.position = pos;

}

public void TakeOff(){

  if( isLocal ){
      networkInfo.SetOnGround( false );
  }

    rb.isKinematic = false;
    onGround = false;
    bird.TakeOff();
    rb.AddForce( Vector3.up * 800 );
    rb.AddForce( transform.forward * 1000 );
}

public void GroundChange( bool val ){

    if( !isLocal ){
        if( val ){
            bird.HitGround();
        }else{
            bird.TakeOff();
        }
    }
}

public void RaceChange( bool val ){
    print("heyyy");
    if( val ){
        title.GetComponent<MeshRenderer>().enabled = true;
    }else{
        title.GetComponent<MeshRenderer>().enabled = false;
    }


}


public void TimeValue1Change( float val ){
    title.text = "t :" + Mathf.Floor( val * 100000 )/100000;

}

public void SetInRace(bool v){
    
    //if( isLocal ){ RaceChange(v); } 

    // THIS IS A BUG FROM NEW WOIRK
       
     networkInfo.SetInRace( 0 ); 
}
public void SetRaceTime(float v){ 

    networkInfo.SetTimeValue1(v); 

}





}
