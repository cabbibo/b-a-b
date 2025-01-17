using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TargetManager : MonoBehaviour
{

    public GameObject targetPrefab;
    public int targetPoolCount = 5;

    public List<Target> targetPool;
    public int currentTargetIndex = 0;

    public Target currentTarget;



    public Target closestTarget;
    public Target oClosestTarget;
    public float closestDistance = 1000;
    public float oClosestDistance = 1000;




    public void OnEnable()
    {

        targetPool = new List<Target>();
        for (int i = 0; i < targetPoolCount; i++)
        {
            GameObject target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            target.SetActive(false);
            targetPool.Add(target.GetComponent<Target>());
            target.transform.parent = transform;
        }


    }

    public void OnDisable()
    {
        for (int i = 0; i < targetPoolCount; i++)
        {
            if (targetPool[i] != null)
            {
                Destroy(targetPool[i].gameObject);
            }
        }
    }


    public void SetTarget(Vector3 position)
    {


        print("SETTT");

        currentTargetIndex++;
        if (currentTargetIndex >= targetPoolCount)
        {
            currentTargetIndex = 0;
        }


        targetPool[currentTargetIndex].transform.position = position;
        targetPool[currentTargetIndex].gameObject.SetActive(true);
        targetPool[currentTargetIndex].facingType = 0;
        targetPool[currentTargetIndex].transform.rotation = Quaternion.identity;
        targetPool[currentTargetIndex].OnSet();

        currentTarget = targetPool[currentTargetIndex];


        OnTargetPlace();
    }

    public void SetTarget(Vector3 position, int faceDirection)
    {

        print("SEETTT 2");

        currentTargetIndex++;
        if (currentTargetIndex >= targetPoolCount)
        {
            currentTargetIndex = 0;
        }


        targetPool[currentTargetIndex].transform.position = position;
        targetPool[currentTargetIndex].gameObject.SetActive(true);
        targetPool[currentTargetIndex].facingType = faceDirection;
        targetPool[currentTargetIndex].transform.rotation = Quaternion.identity;
        targetPool[currentTargetIndex].OnSet();

        currentTarget = targetPool[currentTargetIndex];


        OnTargetPlace();

    }

    public void Update()
    {
        if (God.wren != null)
        {

            oClosestDistance = closestDistance;
            oClosestTarget = closestTarget;

            for (int i = 0; i < targetPoolCount; i++)
            {
                if (targetPool[i].gameObject.activeSelf)
                {
                    float distance = Vector3.Distance(God.wren.transform.position, targetPool[i].transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetPool[i];
                    }
                }
            }

        }

        if (oClosestTarget != closestTarget)
        {
            OnTargetChange();
        }

    }


    public void OnTargetChange()
    {

        if (oClosestTarget != null)
        {
            God.wren.interfaceUtils.RemovePointer(oClosestTarget.gameObject.transform);
        }

        if (closestTarget != null)
        {
            God.wren.interfaceUtils.AddPointer(closestTarget.gameObject.transform);
        }
    }



    public void DestroyAllPointers()
    {
        for (int i = 0; i < targetPoolCount; i++)
        {
            God.wren.interfaceUtils.RemovePointer(targetPool[i].gameObject.transform);
        }

    }


    public void OnTargetPlace()
    {

    }

    public void HitTarget(float speed)
    {
        if (currentTarget != null)
        {
            currentTarget.OnHit(speed);
        }
    }

    public void EraseCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.Erase();
        }
    }

    public void EraseAllTargets()
    {
        for (int i = 0; i < targetPoolCount; i++)
        {
            targetPool[i].Erase();
        }
    }




}
