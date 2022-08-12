using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenStats : WrenInterface
{

    public GameObject healthBar;
    public LineRenderer healthLR;
    public GameObject staminaBar;
    public LineRenderer staminaLR;




    public bool active = false;
    public override void Activate()
    {
        active = true;
    }

    public override void Deactivate()
    {
        active = false;
    }

    public void OnEnable(){
        healthLR = healthBar.GetComponent<LineRenderer>();
        staminaLR = staminaBar.GetComponent<LineRenderer>();
    }


    public void  Update(){

        if( active ){

            healthBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.health/God.wren.state.maxHealth , .1f);
            healthLR.SetPosition(0,Vector3.up * -3 * God.wren.state.health/God.wren.state.maxHealth );
            healthLR.SetPosition(1,Vector3.up * 3 * God.wren.state.health/God.wren.state.maxHealth );
            staminaBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.stamina/God.wren.state.maxStamina , .1f);
        }

    }


}
