using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalHit : MonoBehaviour
{

    public Goal goal;
    public ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider){
        

        if( collider.tag =="Ball"  && collider.isTrigger == false ){

            goal.OnScore();//goal.score ++;

            Carryable c = collider.gameObject.GetComponent<Carryable>();

            if( c == null ){
                Debug.LogError( "BALL HAS NO COLLIDER: " + collider.gameObject.name );
            }else{

                particleSystem.Play();
                God.audio.Play(God.sounds.scoreSound);

                foreach( Wren w in God.wrens ){
                    w.carrying.DropIfCarrying(c);
                }
            }
        }

    }



}
