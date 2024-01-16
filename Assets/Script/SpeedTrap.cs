using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using WrenUtils;

public class SpeedTrap : MonoBehaviour
{
    public TextMeshPro text;


    [SerializeField] private RaceLeaderboard raceLeaderboard;


    float currentSpeed;

    public ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Wren w = God.ClosestWren(transform.position);
        if (w != null)
        {
            currentSpeed = God.ClosestWren(transform.position).physics.vel.magnitude;
            text.text = "" + Mathf.Floor(currentSpeed);
        }
    }




    public void OnHit()
    {

        if (ps)
        {
            print("hit");
            ps.Emit(100);
        }
        if (raceLeaderboard)
        {
            var id = UserIdService.GetLocalUserId();
            raceLeaderboard.AddEntry(id, currentSpeed);
            raceLeaderboard.RefreshUI();
        }

    }


    void OnTriggerEnter(Collider collider)
    {


        if (God.IsOurWren(collider))
        {
            OnHit();
        }
        print("trigggered");

    }

}