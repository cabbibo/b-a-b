using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WrenSimulationController : MonoBehaviour
{

    public Wren wren;

    public Transform featherHeart;

    public bool onGround;


    [Range(0,1)]
    public float percentageRendered;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool oldOnGround;

    // Update is called once per frame
    void Update()
    {

        if( wren == null && God.wren != null ){
            wren = God.wren;
            onGround = wren.state.onGround;
        }

        if( wren != null ){
            wren.bird.percentageRendered = percentageRendered;
        }

        if( God.wren != null ){
            wren = God.wren;
            onGround = wren.state.onGround;
        }

         
    }

    public void TrickTakeOff(){
        print("trick ya");
        wren.state.TakeOff();
        wren.state.HitGround();
    }

    public void SetFeatherHeart(){
        wren.bird.specialTarget = featherHeart;
        wren.bird._LockedValue = -1;
        wren.bird.ResetAtLocation(featherHeart.position);
    }

    public void UnsetFeatherHeart(){
        wren.bird.specialTarget = null;
        wren.bird._LockedValue = 0;
        //wren.bird.ResetAtLocation(featherHeart.position);
    }

    public void SetPercentRendered(float v){
        percentageRendered = v;
    }

    public void SetPosition(Transform t){

        wren.startingPosition = t;
        wren.FullReset();
        
    }


    public void TurnOffSoul(){
        wren.soul.SetActive(false);
    }

    public void TurnOnSoul(){
        wren.soul.SetActive(true);
    }


}
