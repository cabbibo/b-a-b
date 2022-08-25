using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenStats : WrenInterface
{

    public float forwardAmount;
    public float upAmount;
    public float rightAmount;
    public float scale;

    public GameObject healthBar;
    public GameObject staminaBar;
    public GameObject fullnessBar;
    public GameObject awakenessBar;
    public GameObject drynessBar;
    public GameObject ageBar;



    public void OnEnable(){
        
    }


    public void  Update(){
            transform.position = God.camera.transform.position;
            transform.position += God.camera.transform.forward * forwardAmount;
            transform.position += God.camera.transform.up * upAmount;
            transform.position += God.camera.transform.right* rightAmount;

            transform.localScale = Vector3.one * scale;

            healthBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.health/God.wren.state.maxHealth , .1f);
            staminaBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.stamina/God.wren.state.maxStamina , .1f);
            fullnessBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.fullness/God.wren.state.maxFullness , .1f);
            awakenessBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.awakeness/God.wren.state.maxAwakeness , .1f);
            drynessBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.dryness/God.wren.state.maxDryness , .1f);
            ageBar.transform.localScale = new Vector3( .1f , 6 * God.wren.state.age/God.wren.state.maxAge , .1f);

    }


}
