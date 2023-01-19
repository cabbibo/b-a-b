using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;


public class WrenSynths : MonoBehaviour
{


/*

    Sounds

*/

public SampleSynth[] synths;//synth1;
public AudioSource hitSound;
public AudioSource closeLoop;

public WrenPhysics physics;

public void UpdateSound(){
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
float loopVal =  .05f * physics.vel.magnitude *physics.vel.magnitude/(100+physics.distToGround*physics.distToGround);
closeLoop.pitch = loopVal;
closeLoop.volume = loopVal;


// synth 3 just correspodns to our velocity!
synths[2].pitch = .02f * physics.vel.magnitude;

// Synths for left wing
synths[0].speed =  .01f/Mathf.Abs(Vector3.Dot(physics.leftWingLiftForce * .01f , transform.forward));//m* twistForceVal * physics.vel.magnitude;
synths[0].pitch = physics.vel.magnitude * .01f;


// Synths for right wing
synths[1].speed =  .01f/Mathf.Abs(Vector3.Dot( physics.rightWingLiftForce * .01f , transform.forward));//m* twistForceVal * physics.vel.magnitude;
synths[1].pitch = physics.vel.magnitude * .01f;


synths[4].pitch = .1f * physics.leftWingUpdraftForce.magnitude;
synths[4].speed = .1f/synths[4].pitch;
synths[4].volume = synths[4].pitch;

synths[3].pitch = .1f * physics.rightWingUpdraftForce.magnitude;
synths[3].speed = .1f/synths[3].pitch;
synths[3].volume = synths[3].pitch;

}

}
