using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TargetingCameraManager : MonoBehaviour
{

    public float FOV;
    public float weight;

    public float distanceBehindBird = 10;

    public Vector2 defaultStrengthNearFar = new Vector2(10, 100);
    public float defaultFOV = 60;
    public float defaultDistanceFromBird = 10;


    public List<Transform> targets = new List<Transform>();
    public List<Vector2> strengthNearFar = new List<Vector2>();

    public List<float> weights = new List<float>();
    public List<float> FOVs = new List<float>();
    public List<float> distancesFromBird = new List<float>();


    public List<Vector3> targetPositions = new List<Vector3>();
    public List<Vector3> targetDirections = new List<Vector3>();


    public Vector3 totalPosition;
    public Vector3 totalDirection;

    public float totalWeight;
    public float totalFOV;

    public void Update()
    {


        if (God.wren != null && targets.Count != 0)
        {
            totalPosition = Vector3.zero;
            totalDirection = Vector3.zero;
            totalFOV = 0;//
            totalFOV = 0;
            totalWeight = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                Vector3 targetPos = targets[i].position;
                Vector3 birdPos = God.wren.transform.position;

                Vector3 direction = targetPos - birdPos;
                float distance = direction.magnitude;


                float distanceValue = (distance - strengthNearFar[i].x) / (strengthNearFar[i].y - strengthNearFar[i].x);
                distanceValue = Mathf.Clamp(distanceValue, 0, 1);
                distanceValue = 1 - distanceValue; // if we are near, full strength, if we are far, no strength

                weights[i] = distanceValue;
                totalWeight += weights[i];

                targetPositions[i] = birdPos - direction.normalized * distancesFromBird[i];
                targetDirections[i] = direction.normalized;


            }

            // normalize weights
            for (int i = 0; i < targets.Count; i++)
            {
                if (weights[i] > 0)
                {
                    //God.wren.transform.position += direction.normalized * strength * Time.deltaTime;
                    totalPosition += targetPositions[i] * weights[i] / totalWeight;
                    totalDirection += targetDirections[i] * weights[i] / totalWeight;
                    totalFOV += FOVs[i] * weights[i] / totalWeight;
                    // totalDistanceFromBird += distancesFromBird[i] * weights[i] / totalWeight;


                    Debug.DrawLine(God.wren.transform.position, targetPositions[i], Color.red);
                }
            }

            if (totalPosition.magnitude > 0 && totalDirection.magnitude > 0)
            {
                FOV = totalFOV;
                transform.position = totalPosition;
                transform.LookAt(transform.position + totalDirection);
            }


            weight = totalWeight * 10f;

        }




    }


    public void AddTarget(Transform target)
    {

        if (!targets.Contains(target))
        {
            targets.Add(target);
            strengthNearFar.Add(defaultStrengthNearFar);
            weights.Add(0);
            targetPositions.Add(target.position);
            targetDirections.Add(target.position - God.wren.transform.position);
            FOVs.Add(defaultFOV);
            distancesFromBird.Add(defaultDistanceFromBird);
        }


    }

    public void AddTarget(Transform target, Vector2 sNearFar)
    {

        if (!targets.Contains(target))
        {
            targets.Add(target);
            strengthNearFar.Add(sNearFar);
            weights.Add(0);
            FOVs.Add(defaultFOV);

            distancesFromBird.Add(defaultDistanceFromBird);
            targetPositions.Add(target.position);
            targetDirections.Add(target.position - God.wren.transform.position);
        }

    }

    public void AddTarget(Transform target, Vector2 sNearFar, float distanceFromBird)
    {
        if (!targets.Contains(target))
        {
            targets.Add(target);
            strengthNearFar.Add(sNearFar);
            weights.Add(0);
            targetPositions.Add(target.position);
            targetDirections.Add(target.position - God.wren.transform.position);
            FOVs.Add(defaultFOV);
            distancesFromBird.Add(distanceFromBird);
        }
    }


    public void AddTarget(Transform target, Vector2 sNearFar, float distanceFromBird, float fov)
    {
        if (!targets.Contains(target))
        {
            targets.Add(target);
            strengthNearFar.Add(sNearFar);
            weights.Add(0);
            targetPositions.Add(target.position);
            targetDirections.Add(target.position - God.wren.transform.position);
            FOVs.Add(fov);
            distancesFromBird.Add(distanceFromBird);
        }
    }

    public void RemoveTarget(Transform target)
    {
        if (targets.Contains(target))
        {
            int index = targets.IndexOf(target);
            targets.RemoveAt(index);
            strengthNearFar.RemoveAt(index);
            weights.RemoveAt(index);
            targetPositions.RemoveAt(index);
            targetDirections.RemoveAt(index);
            FOVs.RemoveAt(index);
            distancesFromBird.RemoveAt(index);
        }
    }









}
