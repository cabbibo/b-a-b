using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteInEditMode]
public class BeeController : MonoBehaviour
{
    public Biome biome;

    public List<Transform> bees;
    public List<bool> droppedOff;
    public List<bool> followingWren;
    public List<bool> lockedOnTrunk;

    public List<int> whichTemple;

    public List<Vector3> vels;
    public List<Vector3> positionsOnTrunk;

    public Transform[] temples;
    public Transform dropOffLocation;


    public int totalBees;

    public bool reset;

    public GameObject beePrefab;

    public Transform beeHolder;


    public Transform debugWren;
    public bool debug;

    public Transform toFollow;

    // TODO: load based on biome
    // TODO: 
    public void OnEnable()
    {



        if (reset)
        {

            while (beeHolder.childCount > 0)
            {
                DestroyImmediate(beeHolder.GetChild(0).gameObject);
            }


            bees = new List<Transform>();
            droppedOff = new List<bool>();
            followingWren = new List<bool>();
            lockedOnTrunk = new List<bool>();
            vels = new List<Vector3>();
            positionsOnTrunk = new List<Vector3>();
            whichTemple = new List<int>();

            for (int i = 0; i < totalBees; i++)
            {

                whichTemple.Add(Random.Range(0, temples.Length));
                //                print(whichTemple[i]);
                bees.Add(Instantiate(beePrefab, temples[whichTemple[i]].position + Random.insideUnitSphere, Quaternion.identity).transform);
                bees[i].parent = beeHolder;

                if (biome.completed)
                {
                    droppedOff.Add(true);
                    followingWren.Add(false);
                    lockedOnTrunk.Add(true);
                }
                else
                {
                    droppedOff.Add(false);
                    followingWren.Add(false);
                    lockedOnTrunk.Add(false);
                }
                vels.Add(Vector3.zero);
                positionsOnTrunk.Add(dropOffLocation.position + Random.insideUnitSphere * 4f);
            }


        }


    }

    public float followForce;
    public float pickUpRadius;
    public float dropOffRadius;
    public float beeDampening = .9f;

    public float followDampening = .9f;
    public float toTrunkDampening = .9f;


    //  public float percentageCollected;

    //

    public int totalLocked;
    public float completionAmount;
    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {
            toFollow = God.wren.transform;
        }
        else
        {
            toFollow = debugWren;
        }


        totalLocked = 0;
        for (int i = 0; i < bees.Count; i++)
        {


            if (lockedOnTrunk[i])
            {

                totalLocked++;
                bees[i].transform.position = positionsOnTrunk[i];
                continue;
            }

            if (followingWren[i])
            {
                WhileFollowingWren(i);
            }

            if (droppedOff[i])
            {
                MoveTowardsTrunkPosition(i);
            }

            // if we arent following wren and we havent' been dropped off ( and not locked by continue implication)
            // check to see if we should follow wren
            if (!followingWren[i] && !droppedOff[i])
            {

                WhileAtTemple(i);


            }



            bees[i].transform.position += vels[i];

            if (!followingWren[i] && !droppedOff[i])
            {
                vels[i] *= whileAtTempleDampening;
            }
            else
            {
                if (followingWren[i])
                {
                    vels[i] *= followDampening;
                }

                if (droppedOff[i])
                {
                    vels[i] *= toTrunkDampening;
                }
            }

            // check to see if we should lock
            if (Vector3.Distance(bees[i].transform.position, positionsOnTrunk[i]) < 0.1f)
            {
                lockedOnTrunk[i] = true;
            }



        }

        completionAmount = (float)totalLocked / (float)bees.Count;

        if (biome.completed == false)
        {
            biome.SetCompletion(completionAmount);
        }

    }

    public float moveTowardsTrunkForce;
    public void MoveTowardsTrunkPosition(int i)
    {
        vels[i] += (positionsOnTrunk[i] - bees[i].transform.position) * moveTowardsTrunkForce;
    }

    public void WhileFollowingWren(int i)
    {
        if (Vector3.Distance(bees[i].transform.position, dropOffLocation.position) < dropOffRadius)
        {
            droppedOff[i] = true;
            followingWren[i] = false;
        }
        else
        {
            vels[i] += (toFollow.position - bees[i].transform.position) * followForce;

        }

    }


    public float whileAtTempleForce;

    public float whileAtTempleDampening;
    public float templePushAway;
    public float templeCurl;


    public float desiredRadius;

    Vector3 tmp1; Vector3 tmp2;
    public void WhileAtTemple(int i)
    {
        if (Vector3.Distance(bees[i].transform.position, toFollow.position) < pickUpRadius)
        {
            followingWren[i] = true;
        }
        else
        {

            tmp1 = temples[whichTemple[i]].position - bees[i].transform.position;


            float dist = tmp1.magnitude;

            if (dist < desiredRadius)
            {
                tmp2 = Random.insideUnitSphere;
                tmp2.Normalize();

                tmp2 *= .4f;
                tmp2 -= tmp1.normalized;

                vels[i] += tmp2 * templePushAway;
            }

            vels[i] += Vector3.Cross(tmp1.normalized, Vector3.up) * templeCurl;

            vels[i] += (temples[whichTemple[i]].position - bees[i].transform.position) * whileAtTempleForce;
        }

    }


}
