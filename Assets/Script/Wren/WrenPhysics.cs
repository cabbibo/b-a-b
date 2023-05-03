using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;
using WrenUtils;


public class WrenPhysics : MonoBehaviour
{


public bool onGround;
public bool walking;

    /*

        References

    */
    
public WrenInput input;

public Wren wren;

    public Transform leftWing;
    public Transform rightWing;

    public Transform leftWingPivot;
    public Transform rightWingPivot;


    public Rigidbody rb;
    public Collider terrainCollider;
    public Terrain terrain;


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





public float velMatchMultiplier;





/*
Ground stuff

*/

public float groundPower = 1;
public float groundOut = 4;
public float groundDampening = .95f;

public float rotateTowardsTargetOnGround = 1;


public float groundUpForce = 40;
public float groundUpVal = 10;


public float takeOffForwardForce = 10;
public float takeOffUpForce = 10;


/*

Painted Wind Force

*/

public float paintedWindForceMultiplier = 10;










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



public Vector3 paintedWindForce;
public Vector3 paintedWindForcePosition;

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

public int raycastDirections = 5;

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

  
       v1 = new Vector3();
       v2 = new Vector3();
    
   
   }


   public void Reset(){
        
    
        rb.velocity = Vector3.zero;
        if( wren.beacon ){ 
            rb.position = wren.beacon.nest.transform.position;
            transform.position = wren.beacon.nest.transform.position;//new Vector3( 0 ,2000, 0 );
        }else{
            rb.position = wren.startingPosition.position;
            transform.position = wren.startingPosition.position;
        }
   }

   public void LocalReset(){
        rb.velocity = Vector3.zero;
        transform.position = new Vector3( transform.position.x , terrain.SampleHeight(transform.position ) + 100 , transform.position.z);// wren.beacon.position;//new Vector3( 0 ,2000, 0 );
        
        rb.position = transform.position;
    }
    

    public void ResetToOther(GameObject other){
            var otherRb = other.GetComponent<Rigidbody>();
        if (otherRb) {
            rb.velocity = otherRb.velocity;
        } else {
            rb.velocity = Vector3.zero;
        }
        transform.position = other.transform.position;
        transform.rotation = other.transform.rotation;
    }



public void HitGround(){
    
    //rb.position = rb.position + Vector3.up * 10;
    onGround = true;
    //rb.angularVelocity *= .5f; //Vector3.zero;
    //rb.velocity *= .5f;//Vector3.zero;//0;
   // rb.velocity = -c.relativeVelocity;
}


public void HitGround(Collision c){

    print("Hit");
    
    //rb.position = rb.position + Vector3.up * 10;
    onGround = true;
    //rb.angularVelocity *= .5f; //Vector3.zero;
    //rb.velocity = -c.relativeVelocity;// .5f;//Vector3.zero;//0;

}

public void TakeOff(){
    
    rb.isKinematic = false;
    onGround = false;
    rb.AddForce( -groundDirection * takeOffUpForce);

    //Vector3.Cross( Vector3.Cross( -groundNormal , transform.forward ).normalized , -groundNormal ).normalized;
    rb.AddForce( Vector3.Cross( Vector3.Cross( -groundNormal , transform.forward ).normalized , -groundNormal ).normalized * takeOffForwardForce );
}

public void TransportToPosition( Vector3 pos , Vector3 vel ){
    rb.velocity = vel;
    rb.position = pos;

    // need to set the transform position too so it updates automatically
    // instead of next rigidbody frame
    transform.position = pos;
    
}

public void UpdatePhysics(){



    SetUp();

    
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
        PaintedWindForce();


        ApplyForces();
   
    }else{
    
        GetGroundDistanceInfo();
        RotationValues();
        WhileOnGround();
    
    }



    
}


public void SetUp(){
  
    oVel = vel;
    vel = rb.velocity;


    oTriangle = input.triangle > .5 ;
    oSquare = input.square> .5 ;
    oCircle = input.circle> .5 ;
    oX = input.ex > .5 ;


    v1 = new Vector3();
    v2 = new Vector3();
    
    // Grabs our Rigidbody
    if(rb == null ){ rb = GetComponent<Rigidbody>(); }


}

public  void UpdateVelocity(){
    // Set our Rotation
    /*

        THIS IS THE LINE THAT SEEMS TO BE PREVENTING YAW,
        but comment it out and u will see the dangers

        if you crank up the twist force 2 you can start to see it working but it feels whack 
        and not physically right to me...


    */


    Vector3 velT = vel;
    vel +=transform.right * input.leftX * .001f* strafeVal * velT.magnitude;
    vel +=transform.right * input.rightX * .001f* strafeVal *velT.magnitude;

    vel = vel.normalized * velT.magnitude;


    if( vel.magnitude > maxSpeed ){
        vel = vel.normalized * maxSpeed;
    }

    speed = vel.magnitude;

    rb.velocity = vel;

    if( vel.magnitude == 0 ){
        vel = Vector3.forward * .00001f;
    }

   // transform.rotation =  Quaternion.LookRotation(vel, transform.up);//transform.up);//,.1f);

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


float big = input.left3;// ? 1 : 0;

float fAngle = Mathf.Lerp(slowestTwistAngle, fastestTwistAngle,  normalizedSpeed);

float lr = input.leftY;
if( swapLR ){lr=input.rightY;}
float twistAngle = Mathf.Lerp( lr *fAngle * inVal, -90, big);
twistAngle = Mathf.Lerp( oTwistL, twistAngle , twistLerpSpeed);
oTwistL = twistAngle;

float bendAngle =Mathf.Lerp(slowestBendAngle, fastestBendAngle,  normalizedSpeed);
bendAngle =  -input.leftX * bendAngle;
bendAngle = Mathf.Lerp( oBendL , bendAngle , bendLerpSpeed);
oBendL = bendAngle;

twistValL = twistAngle;
bendValL = bendAngle;

leftWingPivot.localRotation = Quaternion.AngleAxis(twistAngle,Vector3.right) * Quaternion.AngleAxis(bendAngle, Vector3.forward);


lr = input.rightY;
if( swapLR ){lr=input.leftY;}
big = input.right3;// ? 1 : 0;
twistAngle = Mathf.Lerp( lr *fAngle * inVal, -90, big);
twistAngle = Mathf.Lerp( oTwistR , twistAngle , twistLerpSpeed);
oTwistR = twistAngle;


bendAngle =Mathf.Lerp(slowestBendAngle, fastestBendAngle,  normalizedSpeed);
bendAngle =  -input.rightX * bendAngle;
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
flapVel = tuckAmountL - input.left2;
tuckAmountL =  Mathf.Lerp( tuckAmountL , input.left2 , tuckLerpSpeed );
leftFlapForce = Mathf.Clamp(-flapVel,0,1) *  (leftWing.up* flapPowerUp + leftWing.forward * flapPowerForward);
leftFlapForcePosition = Vector3.Lerp( transform.position,leftWing.position,flapToSide); 

flapVel = tuckAmountR - input.right2;
tuckAmountR =  Mathf.Lerp( tuckAmountR , input.right2 , tuckLerpSpeed );

rightFlapForce = Mathf.Clamp(-flapVel,0,1) *  (rightWing.up* flapPowerUp + rightWing.forward * flapPowerForward);
rightFlapForcePosition = Vector3.Lerp( transform.position,rightWing.position,flapToSide); 





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

gForce = -( gravityForce * ((1-tuckAddToGravityVal) + tuckAmountR * tuckAddToGravityVal));
rightWingGravityForce = gForce * Vector3.up;
rightWingGravityForcePosition = rightWing.position;



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


v1 = rightWing.up;
match = -Vector3.Dot( rightWing.up , vel.normalized);
v2=Vector3.Lerp( transform.position , rightWing.position , fToTheSide );  
rightWingLiftForce = match*v1 * twistForceVal * vel.magnitude * (1-tuckAmountR*tuckReduceLiftVal);
rightWingLiftForcePosition = v2;



// Push our object forward constantly
// this is the 'thrust' and in general should feel
// like pretty much nothing to me 
// ( though maybe it wou make it more interesting )
thrustForce = transform.forward * thrustForceMultiplier;
thrustForcePosition = transform.position;



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

Vector3 closestDirection;

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


for( int i = 0; i< raycastDirections; i++){
for( int j = 0; j< raycastDirections; j++){

    float a1 = ((float)i)/raycastDirections;
    float a2 = ((float)j)/raycastDirections;

    a2 *= Mathf.PI *2;
    a1 *= Mathf.PI *2;



    Vector3 dir = SphericalToCartesian( a1,a2).normalized;



//    if( i == 5  && j == 5){ print( dir );}

ray.origin = transform.position;
ray.direction = dir;
    //terrainCollider.Raycast....
    if (Physics.Raycast (ray, out hit , 500)) {

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

rightWingUpdraftForce = Vector3.up * Mathf.Abs(Vector3.Dot( rightWing.up,Vector3.up))*distToGroundVal * (1-tuckAmountR*tuckReduceUpdraftVal);
rightWingUpdraftForcePosition = Vector3.Lerp( transform.position , rightWing.position , windAmountToTheSide);
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
rb.AddForceAtPosition( groundBoostForce, groundBoostForcePosition);

} 





// Do we make it so that it dies the further away we are?
public virtual void PaintedWindForce(){


if( God.islandData != null ){

    Vector3 wind =  God.islandData.GetWindPower(transform.position);
    paintedWindForce = wind * paintedWindForceMultiplier;
    paintedWindForcePosition = transform.position;

}



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


// Only rights if we are NOT touching stuff
float maxVal = Mathf.Max( new float[]{ 
        //Mathf.Abs(controller.leftY) , 
        //Mathf.Abs(controller.leftX) , 
        //Mathf.Abs(controller.rightY) , 
        //Mathf.Abs(controller.rightX) , 
        Mathf.Abs(input.left2) , 
        Mathf.Abs(input.right2) 
    });


float forceFactor = Mathf.Pow(1-Mathf.Pow(Vector3.Dot( vel.normalized, Vector3.up), 2),2);

//print( forceFactor );


upwardsRightingForce = forceFactor * d * rightingForce * (1-maxVal*rightingDependentOnNotTouchingVal);

/*
float multi = 0;
if( Mathf.Abs(angleToUp) > rightingForceAngleApplication ){
    multi = Mathf.Abs(angleToUp)-rightingForceAngleApplication;
    multi /= 90;
}

// upwardsRightingForce = -transform.right*multi * .1f * angleToUp * rightingForce;// * (1-maxVal*rightingDependentOnNotTouchingVal);
*/
upwardsRightingForcePosition = transform.position + transform.up;



}


void ApplyForces(){

        rb.AddForceAtPosition(  leftFlapForce             , leftFlapForcePosition           );
        rb.AddForceAtPosition(  rightFlapForce            , rightFlapForcePosition          );
        rb.AddForceAtPosition(  leftWingGravityForce      , leftWingGravityForcePosition    );
        rb.AddForceAtPosition(  rightWingGravityForce     , rightWingGravityForcePosition   );
        rb.AddForceAtPosition(  leftWingLiftForce         , leftWingLiftForcePosition       );
        rb.AddForceAtPosition(  rightWingLiftForce        , rightWingLiftForcePosition      );
        rb.AddForceAtPosition(  thrustForce               , thrustForcePosition             );
        rb.AddForceAtPosition(  leftWingUpdraftForce      , leftWingUpdraftForcePosition    );
        rb.AddForceAtPosition(  rightWingUpdraftForce     , rightWingUpdraftForcePosition   );
        rb.AddForceAtPosition(  groundBoostForce          , groundBoostForcePosition        );
        rb.AddForceAtPosition(  upwardsRightingForce      , upwardsRightingForcePosition    );
        rb.AddForceAtPosition(  paintedWindForce          , paintedWindForcePosition    );

        // Straightens out!
        v1 = Vector3.Cross( rb.velocity , transform.forward );
        rb.AddTorque(  v1 * velMatchMultiplier );
   // transform.rotation =  Quaternion.LookRotation(vel, transform.up);//transform.up);//,.1f);
}























/*


   _____   _____     ____    _    _   _   _   _____  
  / ____| |  __ \   / __ \  | |  | | | \ | | |  __ \ 
 | |  __  | |__) | | |  | | | |  | | |  \| | | |  | |
 | | |_ | |  _  /  | |  | | | |  | | | . ` | | |  | |
 | |__| | | | \ \  | |__| | | |__| | | |\  | | |__| |
  \_____| |_|  \_\  \____/   \____/  |_| \_| |_____/ 
                                                     
                                                     


*/

public void WhileOnGround(){

    

 Vector3 forward = new Vector3(transform.forward.x, 0 , transform.forward.z ).normalized;

        transform.LookAt( transform.position + forward,new Vector3(0,1,0));

    if( wren.state.inInterface == false ){
        rb.AddForceAtPosition( transform.forward  * input.rightY *  groundPower,  transform.position + transform.right * groundOut);
        rb.AddForceAtPosition( transform.right    * input.rightX *  groundPower,  transform.position + transform.right * groundOut);
        rb.AddForceAtPosition( transform.forward  * input.leftY  *  groundPower,  transform.position - transform.right * groundOut);
        rb.AddForceAtPosition( transform.right    * input.leftX  *  groundPower,  transform.position - transform.right * groundOut);
    }





        Vector3 targetPos =  wren.GroundIntersection(rb.position) + Vector3.up * groundUpVal;
        rb.AddForce( (targetPos-rb.position) * groundUpForce  );

        

        if( wren.cameraWork.objectTargeted != null ){
            v1 = Vector3.Cross( (transform.position - wren.cameraWork.objectTargeted.position).normalized , transform.forward );
            rb.AddTorque(  v1 * rotateTowardsTargetOnGround );
        }

        rb.velocity *= groundDampening;


    //print( Vector3.Dot(groundDirection.normalized,Vector3.up));


    //print( distToGround );
       // print( groundDirection );


       
 
/*
        Vector3 targetPos = rb.position;
    if( wren.state.inInterface == false ){

        targetPos += transform.forward * input.leftY * .3f;
        targetPos += transform.right * input.leftX * .3f;
    }

        rb.velocity *= .95f;

        targetPos = wren.GroundIntersection(targetPos) + Vector3.up * 10;
    
        rb.AddForce( (targetPos-rb.position) * 100 );
    
        rb.AddForce(Vector3.up * -90);
        
        if( wren.state.inInterface == false ){
            transform.LookAt( transform.position + transform.forward + transform.right * input.rightX * .02f + transform.up * input.rightY * .02f - transform.up * .01f * Vector3.Dot( transform.forward , Vector3.up ) ,new Vector3(0,1,0));
        }

        */
    //}



}



public void Boost(  float boostVal ){
    rb.AddForce( rb.velocity * boostVal );
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


}
