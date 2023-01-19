using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenStats : MonoBehaviour
{
public Wren wren;
public WrenState state;

public float health; // this goes to 0 and you die
public float stamina; // this goes to 0 you can't flap? also your health starts going down?
public float fullness; // this goes to 0 and you fly very slowly ( health also goes down? ) // refill by eating bugs!
public float dryness; // this goes to 0 and gravity is harder ( health also goes down? )
public float awakeness; //this goes to 0 and you need to land ( health also goes down? ) 
public float quenchedness; // get too thirsty? refill by landing in water
public float happiness; // this is how we reward people for playing with other people 
public float excitment; // this is how we reward people for  doing cooler stuff when you are close to the surface etc.

public float age; // this is a general 'progression' where you get faster and can turn better the older you are?

public float maxHealth;
public float maxStamina;
public float maxFullness;
public float maxDryness;
public float maxAwakeness;
public float maxHappiness;
public float maxExcitment;
public float maxQuenchedness;


public float maxAge;



public void HealthAdd( float healthAddAmount ){

    bool alreadyMax = health == maxHealth;
    health += healthAddAmount;
    if( health > maxHealth && !alreadyMax ){
        health = maxHealth;
    }

    
    if( health < 0 ){
        state.OnDie();
    }

    
    health = Mathf.Clamp( health , 0 , maxHealth );
    
    
}


public void StaminaAdd( float staminaAddAmount ){


    bool alreadyMax = stamina == maxStamina;
    stamina += staminaAddAmount;
    if( stamina > maxStamina && !alreadyMax ){
        stamina = maxStamina;
    }

    
    stamina = Mathf.Clamp( stamina , 0 , maxStamina );
    
}


public void DrynessAdd( float drynessAddAmount ){


    bool alreadyMax = dryness == maxDryness;
    dryness += drynessAddAmount;
    if( dryness > maxDryness && !alreadyMax ){
        dryness = maxDryness;
    }

    
    dryness = Mathf.Clamp( dryness , 0 , maxDryness );
    
}


public void FullnessAdd( float fullnessAddAmount ){


    bool alreadyMax = fullness == maxFullness;
    fullness += fullnessAddAmount;
    if( fullness > maxFullness && !alreadyMax ){
        fullness = maxFullness;
    }

    
    fullness = Mathf.Clamp( fullness , 0 , maxFullness );
    
}


public void AwakenessAdd( float awakenessAddAmount ){


    bool alreadyMax = awakeness == maxAwakeness;
    awakeness += awakenessAddAmount;
    if( awakeness > maxAwakeness && !alreadyMax ){
        awakeness = maxAwakeness;
    }

    
    awakeness = Mathf.Clamp( awakeness , 0 , maxAwakeness );
    
}

public void QuenchednessAdd( float QuenchednessAddAmount ){


    bool alreadyMax = quenchedness == maxQuenchedness;
    quenchedness += QuenchednessAddAmount;
    if( quenchedness > maxQuenchedness && !alreadyMax ){
        quenchedness = maxQuenchedness;
    }

    
    quenchedness = Mathf.Clamp( quenchedness , 0 , maxQuenchedness );
    
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



public void ResetStats(){
    health = maxHealth;
    stamina = maxStamina;
    awakeness = maxAwakeness;
    fullness = maxFullness;
    quenchedness = maxQuenchedness;
    fullness = maxFullness;
    age = 0;
}

}
