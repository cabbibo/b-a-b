using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{

    public GameObject targetPrefab;
    public int targetPoolCount = 5;

    public List<Target> targetPool;
    public int currentTargetIndex = 0;


    public void OnEnable()
    {
        targetPool = new List<Target>();
        for (int i = 0; i < targetPoolCount; i++)
        {
            GameObject target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            target.SetActive(false);
            targetPool.Add(target.GetComponent<Target>());
        }
    }


    public void SetTarget(Vector3 position)
    {

        targetPool[currentTargetIndex].transform.position = position;
        targetPool[currentTargetIndex].gameObject.SetActive(true);
        targetPool[currentTargetIndex].facingType = 0;
        targetPool[currentTargetIndex].transform.rotation = Quaternion.identity;
        targetPool[currentTargetIndex].OnSet();


        currentTargetIndex++;
        if (currentTargetIndex >= targetPoolCount)
        {
            currentTargetIndex = 0;
        }
    }

    public void SetTarget(Vector3 position, int faceDirection)
    {

    }



    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetTarget(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }







}
