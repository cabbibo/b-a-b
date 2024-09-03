using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;


[ExecuteAlways]
public class LakeMaker : MonoBehaviour
{

    List<Vector4> points = new List<Vector4>();
    List<Vector3> directions = new List<Vector3>();

    public Terrain terrain;

    public Vector3 center;

    public int firstRadialPoints = 10;

    public int directionCount;
    public int pointsCount;
    public void OnEnable()
    {
        points = new List<Vector4>();

        center = new Vector3(transform.position.x, terrain.SampleHeight(transform.position), transform.position.z);

        //points.Add(new Vector4(transform.position.x, terrain.SampleHeight(transform.position), transform.position.z, 0));
        for (int i = 0; i < firstRadialPoints; i++)
        {

            float r = 1;
            float a = ((float)i / firstRadialPoints) * Mathf.PI * 2;

            Vector3 newPos = new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r) + center;
            points.Add(new Vector4(newPos.x, terrain.SampleHeight(newPos), newPos.z, 0));
            directions.Add(newPos - center);

        }


    }

    public void OnStep()
    {

        bool allLocked = true;

        for (int i = 0; i < points.Count; i++)
        {
            Vector4 p = points[i];

            float isLocked = p.w;

            Vector3 pos = new Vector3(p.x, p.y, p.z);

            if (isLocked < 1)
            {

                allLocked = false;
                pos += (pos - center).normalized * 1.0f;

                if (Mathf.Abs(terrain.SampleHeight(pos) - center.y) > 1)
                {
                    isLocked = 1;
                }
            }

            points[i] = new Vector4(pos.x, pos.y, pos.z, isLocked);

        }


        if (allLocked == true)
        {

            AddNewPoints();

        }


    }

    public float distanceCutoff = 10;
    public void AddNewPoints()
    {

        int index = 0;
        while (index < points.Count && index < 1000)
        {


            Vector3 p1 = new Vector3(points[index].x, points[index].y, points[index].z);
            Vector3 p2 = new Vector3(points[(index + 1) % points.Count].x, points[(index + 1) % points.Count].y, points[(index + 1) % points.Count].z);

            if ((p1 - p2).magnitude > distanceCutoff)
            {

                for (int j = 0; j < 10; j++)
                {
                    index++;
                    Vector3 newPos = Vector3.Lerp(p1, p2, (float)j / 10);
                    points.Insert(index, new Vector4(newPos.x, terrain.SampleHeight(newPos), newPos.z, 0));
                    directions.Insert(index, Vector3.Cross(p1 - p2, Vector3.up).normalized);
                }



            }

            index++;

        }

    }
    // Update is called once per frame
    void Update()
    {

        OnStep();
        pointsCount = points.Count;
        directionCount = directions.Count;
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < points.Count; i++)
        {
            //Gizmos.DrawCube(points[i], Vector3.one * 4);
            if (i > 0)
            {
                Gizmos.DrawLine(points[i], points[i - 1]);
            }
        }

    }
}
