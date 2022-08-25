using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;


public class WrenState : MonoBehaviour
{

public Wren wren;
public WrenNetworked networkInfo;

public bool isLocal;

public bool onGround;
public bool canTakeOff = true;


public float hue1;
public float hue2;
public float hue3;
public float hue4;

public float timeVal;

public Vector3 beaconPos;
public bool beaconOn;

public bool inInterface;

public int inRace;

public string name;
public string uniqueID;

public uint playerID;


public bool dead;


public float health; // this goes to 0 and you die
public float stamina; // this goes to 0 you can't flap? also your health starts going down?
public float fullness; // this goes to 0 and you fly very slowly ( health also goes down? )
public float dryness; // this goes to 0 and gravity is harder ( health also goes down? )
public float awakeness; //this goes to 0 and you need to land ( health also goes down? ) 
public float age; // this is a general 'progression' where you get faster and can turn better the older you are?

public float maxHealth;
public float maxStamina;
public float maxFullness;
public float maxDryness;
public float maxAwakeness;
public float maxAge;

public void TransportToPosition( Vector3 pos , Vector3 vel ){
    wren.bird.Explode();
    wren.physics.TransportToPosition( pos , vel );
    wren.bird.ResetFeatherValues();
    wren.bird.ResetAtLocation(pos);
    wren.bird.Explode();
    
}


public void LookAt( Vector3 position ){
    wren.transform.LookAt( position , Vector3.up);
}





public void HitGround(Collision c){

    wren.bird.HitGround();


    if( isLocal ){
        
        wren.physics.HitGround(c);
        onGround = true;
        networkInfo.SetOnGround( true );
      
    }

}


public void HitGround(){

    wren.bird.HitGround();


    if( isLocal ){
        
        wren.physics.HitGround();
        onGround = true;
        networkInfo.SetOnGround( true );
      
    }

}


//TODO: do we need to pass state on this one?
/*public void HurtCollision(Collision c){


    if( isLocal){
      health -= c.relativeVelocity.magnitude;

    God.audio.Play( God.sounds.hurtClip );
     if( God.glitchHit != null ){ God.glitchHit.StartGlitch(); }
      
        if( health < 0 ){
            OnDie();
        }
    }
}*/



public void HealthAdd( float healthAddAmount ){

    bool alreadyMax = health == maxHealth;
    health += healthAddAmount;
    if( health > maxHealth && !alreadyMax ){
        health = maxHealth;
        God.audio.Play( God.sounds.maxHealthReachedClip );
    }

    
    if( health < 0 ){
        OnDie();
    }

    
    health = Mathf.Clamp( health , 0 , maxHealth );
    
    
}


public void StaminaAdd( float staminaAddAmount ){


    bool alreadyMax = stamina == maxStamina;
    stamina += staminaAddAmount;
    if( stamina > maxStamina && !alreadyMax ){
        stamina = maxStamina;
        God.audio.Play( God.sounds.maxStaminaReachedClip );
    }

    
    stamina = Mathf.Clamp( stamina , 0 , maxStamina );
    
}


public void DrynessAdd( float drynessAddAmount ){


    bool alreadyMax = dryness == maxDryness;
    dryness += drynessAddAmount;
    if( dryness > maxDryness && !alreadyMax ){
        dryness = maxDryness;
        God.audio.Play( God.sounds.maxStaminaReachedClip );
    }

    
    dryness = Mathf.Clamp( dryness , 0 , maxDryness );
    
}


public void FullnessAdd( float fullnessAddAmount ){


    bool alreadyMax = fullness == maxFullness;
    fullness += fullnessAddAmount;
    if( fullness > maxFullness && !alreadyMax ){
        fullness = maxFullness;
        God.audio.Play( God.sounds.maxStaminaReachedClip );
    }

    
    fullness = Mathf.Clamp( fullness , 0 , maxFullness );
    
}


public void awakenessAdd( float awakenessAddAmount ){


    bool alreadyMax = awakeness == maxAwakeness;
    awakeness += awakenessAddAmount;
    if( awakeness > maxAwakeness && !alreadyMax ){
        awakeness = maxAwakeness;
        God.audio.Play( God.sounds.maxStaminaReachedClip );
    }

    
    awakeness = Mathf.Clamp( awakeness , 0 , maxAwakeness );
    
}


public void AgeAdd( float AgeAddAmount ){



     bool alreadyMax = age == maxAge;
    age += AgeAddAmount;
    if( age > maxAge && !alreadyMax ){
        age = maxAge;

        //TODO SOMETHING V V SPECIAL!

    }



    God.audio.Play( God.sounds.newAgeClip );


    
}







public void OnDie(){

    print("DEATH");

    health = maxHealth;
    stamina = maxStamina;
    awakeness = maxAwakeness;
    fullness = maxFullness;
    age = 0;


    wren.FullReset();

    // WHAT IS DEAD MAY NEVER DIE
    if( !dead ){
        wren.deathAnimation.OnDeath();
    }
}


public void TakeOff(){

    wren.bird.TakeOff();
    wren.physics.TakeOff();
    
    if( isLocal ){
        onGround = false;
        networkInfo.SetOnGround( false );
    }

}




public void GroundChange( bool val ){
    if( !isLocal ){        
        if( val ){
            wren.bird.HitGround();
        }else{
            wren.bird.TakeOff();
        }
    }
}

public void RaceChange( int val ){
    
    inRace = val;
    if( val != -1 ){
        wren.title.GetComponent<MeshRenderer>().enabled = true;
    }else{
        wren.title.GetComponent<MeshRenderer>().enabled = false;
    }

}


public void TimeValue1Change( float val ){


    int tens = (int)Mathf.Floor( val /10);
    int ones = (int)Mathf.Floor( val )%10;
    int deci1 = (int)Mathf.Floor( (val%1) * 10);
    int deci2 = (int)Mathf.Floor( (val%.1f) * 100);
    wren.title.text = "" + tens + ones +":"  + deci1 + deci2;
}


public void Hue1Change( float v){
    hue1 = v;
    wren.colors.OnColorUpdate();
    wren.bird.SetMaterialProperties();
}

public void Hue2Change( float v){
    hue2 = v;
    wren.colors.OnColorUpdate();
    wren.bird.SetMaterialProperties();
}

public void Hue3Change( float v){
    hue3 = v;
    wren.colors.OnColorUpdate();
    wren.bird.SetMaterialProperties();
}

public void Hue4Change( float v){
    hue4 = v;
    wren.colors.OnColorUpdate();
    wren.bird.SetMaterialProperties();
}


public void BeaconOnDidChange( bool b ){
    beaconOn = b;
    if( wren.beacon ){ wren.beacon.gameObject.SetActive(b); }
}

public void BeaconLocationDidChange( Vector3  v ){
    beaconPos = v;
    if( wren.beacon ){ wren.beacon.transform.position = v; }
}

public void PlayerIDDidChange(uint v) {
    playerID = v;
}


public void Explode(bool val ){

    if( val){
        wren.bird.Explode();
    }
}

public void SetInRace(int v){   
    networkInfo.SetInRace( v ); 
}
public void SetRaceTime(float v){ 
    networkInfo.SetTimeValue1(v); 
}


public void SetBeaconLocation( Vector3 p ){

    PlayerPrefs.SetFloat("_BeaconX", p.x);
    PlayerPrefs.SetFloat("_BeaconY", p.y);
    PlayerPrefs.SetFloat("_BeaconZ", p.z);
    
    networkInfo.SetBeaconLocation(p);

}


public void SetBeaconForward( Vector3 p ){

   //PlayerPrefs.SetFloat("_BeaconForwardX", p.x);
   //PlayerPrefs.SetFloat("_BeaconForwardY", p.y);
   //PlayerPrefs.SetFloat("_BeaconForwardZ", p.z);


    //wren.beacon.transform.rotation = Quaternion.LookRotation( p.normalized , Vector3.up );
    

}
public void SetBeaconOn( bool b ){
    PlayerPrefs.SetInt( "_BeaconOn" , b?1:0);
    networkInfo.SetBeaconOn(b);
}

public void SetHue1( float v ){
    networkInfo.SetHue1( v );
    PlayerPrefs.SetFloat("_Hue1" , v );
    if( NetworkStorage.Instance ){ NetworkStorage.Instance.SetLocalHue1(v); }
}


public void SetHue2( float v ){
    networkInfo.SetHue2( v );
    PlayerPrefs.SetFloat("_Hue2" , v );
    if( NetworkStorage.Instance ){ NetworkStorage.Instance.SetLocalHue2(v); }
}

public void SetHue3( float v ){
    networkInfo.SetHue3( v );
    PlayerPrefs.SetFloat("_Hue3" , v );
    if( NetworkStorage.Instance ){ NetworkStorage.Instance.SetLocalHue3(v); }
}

public void SetHue4( float v ){
    networkInfo.SetHue4( v );
    PlayerPrefs.SetFloat("_Hue4" , v );
    if( NetworkStorage.Instance ){ NetworkStorage.Instance.SetLocalHue4(v); }
}



// should only happen locally
public void CheckForOriginals(){

    hue1 = PlayerPrefs.GetFloat("_Hue1" , Random.Range(0.00001f, .9999f) );
    hue2 = PlayerPrefs.GetFloat("_Hue2" , Random.Range(0.00001f, .9999f) );
    hue3 = PlayerPrefs.GetFloat("_Hue3" , Random.Range(0.00001f, .9999f) );
    hue4 = PlayerPrefs.GetFloat("_Hue4" , Random.Range(0.00001f, .9999f) );

   
    maxHealth = PlayerPrefs.GetFloat("_MaxHealth" , 1 );
    health = PlayerPrefs.GetFloat("_Health" , maxHealth );

    maxStamina = PlayerPrefs.GetFloat("_MaxStamina" , 1 );
    stamina = PlayerPrefs.GetFloat("_Stamina" , maxStamina );

    
    maxFullness = PlayerPrefs.GetFloat("_MaxFullness" , 1 );
    fullness = PlayerPrefs.GetFloat("_Fullness" , maxFullness );

    
    maxDryness = PlayerPrefs.GetFloat("_MaxDryness" , 1 );
    dryness = PlayerPrefs.GetFloat("_Dryness" , maxDryness );

    
    maxAwakeness = PlayerPrefs.GetFloat("_MaxAwakeness" , 1 );
    awakeness = PlayerPrefs.GetFloat("_Awakeness" , maxAwakeness );

    maxAge = PlayerPrefs.GetFloat("_MaxAge" , 1 );
    age = PlayerPrefs.GetFloat("_Age" , 0 );




    int whichEdge = Random.Range(0,4);

    float x = PlayerPrefs.GetFloat("_BeaconX" , 0 );
    float y = PlayerPrefs.GetFloat("_BeaconY" , 0 );
    float z = PlayerPrefs.GetFloat("_BeaconZ" , 0 );

    Vector3 pos = new Vector3(x,y,z);

    // If its a new position
    if( pos.magnitude < .01f ){


        if( whichEdge == 0 ){
            x = -500; z = Random.Range(-500,5500);
        }else if( whichEdge == 1 ){
            x = 5500; z = Random.Range(-500,5500);
        }else if( whichEdge == 2 ){
            z = -500; x = Random.Range(-500,5500);
        }else if( whichEdge == 3 ){
            z = 5500; x = Random.Range(-500,5500);
        }

        y = 6000;

        pos = new Vector3(x,y,z);

        //pos = GameObject.Find("StartPosition").transform.position;
    
    }



    pos = wren.GroundIntersection(pos) + Vector3.up * 10;

    SetBeaconLocation(pos);
    

    //x = PlayerPrefs.GetFloat("_BeaconForwardX" , 0 );
    //y = PlayerPrefs.GetFloat("_BeaconForwardY" , 0 );
    //z = PlayerPrefs.GetFloat("_BeaconForwardZ" , 0 );
    //pos = new Vector3(x,y,z);
//
    // if( pos.magnitude < .0001f ){
    //    print("ASSIGNING NEW POS");
    //
    //    pos = GameObject.Find("StartPosition").transform.forward;
    //    print( pos);
    //
    //}
    //SetBeaconForward( pos );

    int isOn = PlayerPrefs.GetInt("_BeaconOn", 1);
    SetBeaconOn(isOn==1?true:false);
    

    SetHue1(hue1);
    SetHue2(hue2);
    SetHue3(hue3);
    SetHue4(hue4);


    //if( wren.fullInterface ){ wren.fullInterface.fourRingInterface.angleValues[0].setValue(hue1); }
    //if( wren.fullInterface ){ wren.fullInterface.fourRingInterface.angleValues[1].setValue(hue2); }
    //if( wren.fullInterface ){ wren.fullInterface.fourRingInterface.angleValues[2].setValue(hue3); }
    //if( wren.fullInterface ){ wren.fullInterface.fourRingInterface.angleValues[3].setValue(hue4); }

    SetPlayerID(UserIdService.GetLocalUserId());

}

public void SetStateMPB( MaterialPropertyBlock mpb ){
    mpb.SetFloat("_Hue1", hue1);
    mpb.SetFloat("_Hue2", hue2);
    mpb.SetFloat("_Hue3", hue3);
    mpb.SetFloat("_Hue4", hue4);
    mpb.SetVector("_Hues", new Vector4( hue1,hue2,hue3,hue4) );
}

public void SetPlayerID(uint pID) {
    networkInfo.SetPlayerID(pID);
}



}
