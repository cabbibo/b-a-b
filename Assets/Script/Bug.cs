using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Bug : MonoBehaviour
{

    public Transform cage;
    public float speed;
    public float drag;
    public float osscilationSize;
    public float life;

    public float dieRate;

    public float maxSpeed;
    public float maxScale;

    public BugSpawner  bugSpawner;
    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void OnEnable()
    {
        life = 1;
        rigidbody = GetComponent<Rigidbody>();
        transform.localScale = Vector3.one * maxScale;
    }

    // Update is called once per frame
    void Update()
    {



if( God.wren ){
      Wren wren = God.ClosestWren( transform.position );

      life -= dieRate;

    Vector3 d = (wren.transform.position - transform.position);

    rigidbody.AddForce( -d.normalized  * Mathf.Clamp( speed  * 1/(.4f+d.magnitude * .02f) , 0, maxSpeed) );
    rigidbody.AddForce( d.normalized * speed * .1f );
    rigidbody.drag = drag;


  
    transform.localScale = Vector3.one * maxScale * Mathf.Clamp( Mathf.Min( (1-life)*4, life),0,1);

  

      if( life < 0 ){
        Destroy(gameObject);
      }

}
    }



       void OnCollisionEnter(Collision c){

        if( God.IsOurWren( c )){
           GotAte();
        }
    }


    public void GotAte(){ 


      print("GOT ATE BUG)");
     
     bugSpawner.BugGotAte( this );
     Destroy(gameObject);
    }
}
