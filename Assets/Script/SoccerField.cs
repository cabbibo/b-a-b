using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SoccerField : MonoBehaviour
{

    public SoccerBall ball;

    private Rigidbody rb;
    public MeshRenderer[] walls;

    public Transform resetPosition;

    public Goal goal1;
    public Goal goal2;





    // Start is called before the first frame update
    void Start()
    {
        walls[0].sharedMaterial.SetVector( "_BallPosition", ball.transform.position );
        walls[0].sharedMaterial.SetMatrix("_FieldWorldToLocal" , transform.worldToLocalMatrix);
                rb = ball.GetComponent<Rigidbody>();

        ResetBallPos();
    
    }

    // Update is called once per frame
    void Update()
    {
        
        walls[0].sharedMaterial.SetVector( "_BallPosition", ball.transform.position );
        walls[0].sharedMaterial.SetMatrix("_FieldWorldToLocal" , transform.worldToLocalMatrix);
    }


    public void Reset(){
        goal1.OnReset();
        goal2.OnReset();
        ResetBallPos();
    }

    public void OnScore(){
        ResetBallPos();
    }

    void ResetBallPos(){

          rb.position = resetPosition.position;
        rb.velocity = Vector3.zero;
        ball.transform.position = resetPosition.position;
    }
}
