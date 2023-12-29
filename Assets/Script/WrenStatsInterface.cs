using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenStatsInterface : WrenInterface
{

    public float forwardAmount;
    public float upAmount;
    public float rightAmount;
    public float scale;

    public GameObject healthBar;
    public GameObject staminaBar;
    public GameObject fullnessBar;
    public GameObject quenchednessBar;
    public GameObject awakenessBar;
    public GameObject drynessBar;
    public GameObject sadnessBar;
    public GameObject boredomBar;
    public GameObject ageBar;

    public bool faceCamera;


    public void OnEnable()
    {

    }


    public void Update()
    {
        transform.position = God.camera.transform.position;
        transform.position += God.camera.transform.forward * forwardAmount;
        transform.position += God.camera.transform.up * upAmount;
        transform.position += God.camera.transform.right * rightAmount;

        transform.localScale = Vector3.one * scale;

        if (faceCamera)
        {
            transform.LookAt(God.camera.transform);
        }

        float size = 6;

        healthBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.health / God.wren.stats.maxHealth, .1f);
        healthBar.transform.localPosition = new Vector3((size - healthBar.transform.localScale.y) / 2, healthBar.transform.localPosition.y, healthBar.transform.localPosition.z);
        staminaBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.stamina / God.wren.stats.maxStamina, .1f);
        staminaBar.transform.localPosition = new Vector3((size - staminaBar.transform.localScale.y) / 2, staminaBar.transform.localPosition.y, staminaBar.transform.localPosition.z);
        fullnessBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.fullness / God.wren.stats.maxFullness, .1f);
        fullnessBar.transform.localPosition = new Vector3((size - fullnessBar.transform.localScale.y) / 2, fullnessBar.transform.localPosition.y, fullnessBar.transform.localPosition.z);
        awakenessBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.awakeness / God.wren.stats.maxAwakeness, .1f);
        awakenessBar.transform.localPosition = new Vector3((size - awakenessBar.transform.localScale.y) / 2, awakenessBar.transform.localPosition.y, awakenessBar.transform.localPosition.z);
        quenchednessBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.quenchedness / God.wren.stats.maxQuenchedness, .1f);
        quenchednessBar.transform.localPosition = new Vector3((size - quenchednessBar.transform.localScale.y) / 2, quenchednessBar.transform.localPosition.y, quenchednessBar.transform.localPosition.z);
        drynessBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.dryness / God.wren.stats.maxDryness, .1f);
        drynessBar.transform.localPosition = new Vector3((size - drynessBar.transform.localScale.y) / 2, drynessBar.transform.localPosition.y, drynessBar.transform.localPosition.z);
        // happinessBar.transform.localScale = new Vector3( .1f , size * God.wren.stats.happiness/God.wren.stats.maxHappiness , .1f);
        // happinessBar.transform.localPosition = new Vector3( (size-happinessBar.transform.localScale.y )/2, happinessBar.transform.localPosition.y , happinessBar.transform.localPosition.z );


        ageBar.transform.localScale = new Vector3(.1f, size * God.wren.stats.age / God.wren.stats.maxAge, .1f);
        ageBar.transform.localPosition = new Vector3((size - ageBar.transform.localScale.y) / 2, ageBar.transform.localPosition.y, ageBar.transform.localPosition.z);

    }


}
