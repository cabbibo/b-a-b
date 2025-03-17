using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;




[ExecuteAlways]
public class Waterfall : MonoBehaviour
{

    public bool individualStrands = false; // if true, each strand will be a separate object
    public float displayScale = 1.25f;
    public AudioClip enterWaterfallClip;
    public AudioClip exitWaterfallClip;

    public float forwardVelocity;
    public float downVelocity;
    public float towardsCenterVelocity;

    public int countWidth;
    public int countHeight;


    public bool recreating;
    public List<Vector3> waterfallTopPoints;


    public Vector3 maxTopPoint;

    public float backDistance = 3;


    public float verticalOffset = 1.0f;

    public bool inWaterfall = false;


    public int crystalsCollectedWhileInside = 1;
    public float crystalType = 1;

    public float dampening = 1.0f;


    public bool evenTop;

    public float waterLevel;

    public AnimationCurve widthCurve;
    public float widthMultiplier;


    public float dampenOnBounce = .5f;
    public float forwardForceOnBounce = 1.0f;

    public EmitFromPoints bouncePointEmitter;
    public EmitFromPoints waterfallTopEmitter;
    public EmitFromPoints waterfallBottomEmitter;

    public bool debug = false;


    //public MeshCollider meshCollider;


    public void OnEnable()
    {
        //RegenerateMesh();

       
    }

    public void ResetPoints(){

            bouncePoints = new List<Vector4>();
            bounceVels = new List<Vector3>();
            finalTopPoints = new List<Vector4>();
            finalTopVels = new List<Vector3>();
            finalBottomPoints = new List<Vector4>();
            finalBottomVels = new List<Vector3>();
    }

    public void RegeneratePoints(){
        if (bouncePoints != null)
        {
            bouncePointEmitter.SetPoints(bouncePoints.ToArray(), bounceVels.ToArray());
        }

        if( waterfallTopEmitter){
            
            waterfallTopEmitter.SetPoints(finalTopPoints.ToArray(), finalTopVels.ToArray());
        }

        if( waterfallBottomEmitter){
                
            waterfallBottomEmitter.SetPoints(finalBottomPoints.ToArray(), finalBottomVels.ToArray());
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (debug)
        {
            for (int i = 0; i < waterfallTopPoints.Count; i++)
            {
                Debug.DrawLine(waterfallTopPoints[i], waterfallTopPoints[i] + transform.forward, Color.red);
            }


            for (int i = 0; i < 10; i++)
            {

                Vector3 v1 = new Vector3(transform.position.x - 100 * ((float)i / 10), waterLevel, transform.position.z - 100);
                Vector3 v2 = new Vector3(transform.position.x - 100 * ((float)i / 10), waterLevel, transform.position.z + 100);

                Debug.DrawLine(v1, v2, Color.yellow);
            }



            if (Paths != null)
            {
                for (int i = 0; i < Paths.Count; i++)
                {
                    for (int j = 0; j < Paths[i].Count - 1; j++)
                    {
                        Debug.DrawLine(Paths[i][j], Paths[i][j] + Vector3.up, Color.green);
                    }
                }


                for (int i = 0; i < points.Length; i++)
                {
                    Debug.DrawLine(transform.TransformPoint(points[i]), transform.TransformPoint(points[i]) + Vector3.up, Color.blue);
                }
            }

        }




        if (inWaterfall)
        {
            God.wren.shards.CollectShards(crystalsCollectedWhileInside, crystalType);
        }


    }



    public float outAmount;
    public float upAmount;
    public float powMultiplier = 1.0f;



    public void GenerateIndiviualStrands()
    {





    }



    public List<List<Vector3>> Paths;
    public void RegenerateMesh()
    {


        countWidth = waterfallTopPoints.Count;


        maxTopPoint = new Vector3(0, -10000, 0);

        for (int i = 0; i < countWidth; i++)
        {
            if (waterfallTopPoints[i].y > maxTopPoint.y)
            {
                maxTopPoint = waterfallTopPoints[i];
            }

        }

        if (evenTop)
        {

            for (int i = 0; i < waterfallTopPoints.Count; i++)
            {

                waterfallTopPoints[i] = new Vector3(waterfallTopPoints[i].x, maxTopPoint.y + verticalOffset, waterfallTopPoints[i].z);
            }
        }

        Mesh mesh;

        //if (individualStrands)
        //{
            ResetPoints();
            GetPaths(waterfallTopPoints);
            mesh = GenerateIndividualStrandsMesh();
            RegeneratePoints();

        //}
        //else
        //{

        //    Debug.Log("Generating Extruded Mesh");
           // mesh = GenerateExtrudedMesh();

        //}

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        UnityEngine.MeshCollider meshCollider = GetComponent<UnityEngine.MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    Vector3[] points;
    Color[] colors;
    Vector3[] normals;
    Vector4[] tangents;

    Vector2[] uvs;

    // EXTRUDED PLANE
    int[] triangles;

/*
    public Mesh GenerateExtrudedMesh()
    {




        points = new Vector3[countWidth * countHeight * 2];
        colors = new Color[countWidth * countHeight * 2];

        uvs = new Vector2[countWidth * countHeight * 2];

        // EXTRUDED PLANE
        triangles = new int[(countWidth - 1) * (countHeight - 1) * 6 * 2 + (countWidth - 1) * 6 * 2 + (countHeight - 1) * 6 * 2];

        Mesh mesh = new Mesh();


        for (int fb = 0; fb < 2; fb++)
        {
            for (int i = 0; i < countWidth; i++)
            {


                // making the top points even vertically

                Vector3 topPoint = waterfallTopPoints[i];






                for (int j = 0; j < countHeight; j++)
                {
                    float z = forwardVelocity * Mathf.Pow((float)j, .5f);// (float)j);
                    float y = downVelocity * (float)j;
                    float x = ((float)i / (float)countWidth) - .5f;

                    float fOut = (float)j / (float)countHeight;

                    Vector3 p = topPoint + transform.forward * fOut * outAmount;// + transform.up * y + transform.right * x;


                    Vector3 downPoint = p;

                    Vector3 downPointCast = topPoint + transform.forward * Mathf.Pow(fOut, powMultiplier) * outAmount;

                    RaycastHit hit;
                    if (Physics.Raycast(downPointCast, Vector3.down, out hit))
                    {
                        downPoint = new Vector3(downPoint.x, Mathf.Lerp(topPoint.y, hit.point.y, Mathf.Pow(fOut, .6f)), downPoint.z) + Vector3.up * upAmount * (1 - fOut * fOut * fOut * 1.05f);
                    }





                    float backOffset = 0;
                    if (fb == 1)
                    {
                        backOffset = backDistance;
                    }

                    // Vector3 fPoint = topPoint + transform.forward * z + transform.up * y + transform.forward * backOffset;
                    Vector3 fPoint = downPoint + transform.forward * backOffset;
                    points[i * countHeight + j + fb * countWidth * countHeight] = transform.InverseTransformPoint(fPoint);////new Vector3(i, j, 0);
                    uvs[i * countHeight + j + fb * countWidth * countHeight] = new Vector2((float)i / ((float)countWidth - 1), (float)j / ((float)countHeight - 1));
                    colors[i * countHeight + j + fb * countWidth * countHeight] = new Color((float)i / ((float)countWidth - 1), (float)j / ((float)countHeight - 1), (float)fb, 1);
                }


            }

        }


        // LAST ONE GOES TO THE BACK PART
        for (int i = 0; i < countWidth - 1; i++)
        {
            for (int j = 0; j < countHeight - 1; j++)
            {
                triangles[(i * (countHeight - 1) + j) * 6] = i * countHeight + j;
                triangles[(i * (countHeight - 1) + j) * 6 + 1] = (i + 1) * countHeight + j;
                triangles[(i * (countHeight - 1) + j) * 6 + 2] = i * countHeight + j + 1;


                triangles[(i * (countHeight - 1) + j) * 6 + 3] = i * countHeight + j + 1;
                triangles[(i * (countHeight - 1) + j) * 6 + 4] = (i + 1) * countHeight + j;
                triangles[(i * (countHeight - 1) + j) * 6 + 5] = (i + 1) * countHeight + j + 1;

            }

        }


        // Back side
        int backPartOffset = countWidth * countHeight;
        int backPartTriangleOffset = (countWidth - 1) * (countHeight - 1) * 6;
        for (int i = 0; i < countWidth - 1; i++)
        {
            for (int j = 0; j < countHeight - 1; j++)
            {


                triangles[(i * (countHeight - 1) + j) * 6 + backPartTriangleOffset] = i * countHeight + j + backPartOffset;
                triangles[(i * (countHeight - 1) + j) * 6 + 1 + backPartTriangleOffset] = i * countHeight + j + 1 + backPartOffset;
                triangles[(i * (countHeight - 1) + j) * 6 + 2 + backPartTriangleOffset] = (i + 1) * countHeight + j + backPartOffset;

                triangles[(i * (countHeight - 1) + j) * 6 + 3 + backPartTriangleOffset] = i * countHeight + j + 1 + backPartOffset;
                triangles[(i * (countHeight - 1) + j) * 6 + 4 + backPartTriangleOffset] = (i + 1) * countHeight + j + 1 + backPartOffset;
                triangles[(i * (countHeight - 1) + j) * 6 + 5 + backPartTriangleOffset] = (i + 1) * countHeight + j + backPartOffset;


            }

        }


        // Sides
        // connecting the front to the back on the top section
        int extraOffset = (countWidth - 1) * (countHeight - 1) * 6 * 2;

        // Side one

        for (int i = 0; i < countHeight - 1; i++)
        {

            triangles[(i) * 6 + 0 + extraOffset] = i;
            triangles[(i) * 6 + 1 + extraOffset] = i + 1;
            triangles[(i) * 6 + 2 + extraOffset] = i + 1 + backPartOffset;

            triangles[(i) * 6 + 3 + extraOffset] = i;
            triangles[(i) * 6 + 4 + extraOffset] = i + 1 + backPartOffset;
            triangles[(i) * 6 + 5 + extraOffset] = i + backPartOffset;

        }


        extraOffset += (countHeight - 1) * 6;

        // Side two!
        for (int i = 0; i < countHeight - 1; i++)
        {

            triangles[(i) * 6 + 0 + extraOffset] = i + countWidth * countHeight - countHeight;
            triangles[(i) * 6 + 1 + extraOffset] = i + countWidth * countHeight - countHeight + 1 + backPartOffset;
            triangles[(i) * 6 + 2 + extraOffset] = i + countWidth * countHeight - countHeight + 1;

            triangles[(i) * 6 + 3 + extraOffset] = i + countWidth * countHeight - countHeight;
            triangles[(i) * 6 + 4 + extraOffset] = i + countWidth * countHeight - countHeight + backPartOffset;
            triangles[(i) * 6 + 5 + extraOffset] = i + countWidth * countHeight - countHeight + 1 + backPartOffset;

        }

        extraOffset += (countHeight - 1) * 6;
        // top side

        for (int i = 0; i < countWidth - 1; i++)
        {

            triangles[(i) * 6 + 0 + extraOffset] = i * countHeight;
            triangles[(i) * 6 + 1 + extraOffset] = i * countHeight + backPartOffset;
            triangles[(i) * 6 + 2 + extraOffset] = (i + 1) * countHeight;

            triangles[(i) * 6 + 3 + extraOffset] = i * countHeight + backPartOffset;
            triangles[(i) * 6 + 4 + extraOffset] = (i + 1) * countHeight + backPartOffset;
            triangles[(i) * 6 + 5 + extraOffset] = (i + 1) * countHeight;
        }

        extraOffset += (countWidth - 1) * 6;

        // bottom side
        for (int i = 0; i < countWidth - 1; i++)
        {
            triangles[(i) * 6 + 0 + extraOffset] = (i + 1) * countHeight - 1;
            triangles[(i) * 6 + 1 + extraOffset] = (i + 2) * countHeight - 1 + backPartOffset;
            triangles[(i) * 6 + 2 + extraOffset] = (i + 1) * countHeight - 1 + backPartOffset;

            triangles[(i) * 6 + 3 + extraOffset] = (i + 1) * countHeight - 1;
            triangles[(i) * 6 + 4 + extraOffset] = (i + 2) * countHeight - 1;
            triangles[(i) * 6 + 5 + extraOffset] = (i + 2) * countHeight - 1 + backPartOffset;

        }











        mesh.vertices = points;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;




        //  MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        //meshCollider.sharedMesh = mesh;


        // print(meshCollider.convex);
        //  meshCollider.sharedMesh = mesh;


    }
*/




    public Mesh GenerateIndividualStrandsMesh()
    {



        int totalPointCount = 0;
        int totalTriCount = 0;

        for (int i = 0; i < Paths.Count; i++)
        {

            totalPointCount += Paths[i].Count;
            totalTriCount += (Paths[i].Count - 1) * 6;

        }

        totalPointCount *= 2;


        // EXTRUDED PLANE
        triangles = new int[totalTriCount];
        points = new Vector3[totalPointCount];
        colors = new Color[totalPointCount];
        uvs = new Vector2[totalPointCount];
        normals = new Vector3[totalPointCount];
        tangents = new Vector4[totalPointCount];


        int totalPoints = 0;

        float fWidth = 1;
        for (int i = 0; i < Paths.Count; i++)
        {

            for (int j = 0; j < Paths[i].Count; j++)
            {

                fWidth = widthCurve.Evaluate((float)j / (float)Paths[i].Count) * widthMultiplier;

                Vector3 left;
                
                // = Vector3.cross((Paths[i][j] - Paths[i][j + 1]).normalized, Vector3.up).normalized;
                if( j ==  0){
                    left = Vector3.Cross((Paths[i][j] - Paths[i][j + 1]).normalized, Vector3.up).normalized;
                }else{
                    
                    left  = Vector3.Cross((Paths[i][j-1] - Paths[i][j]).normalized, Vector3.up).normalized;
                }

                Vector3 localLeft = transform.InverseTransformDirection(left);

                Vector3 up = Vector3.up;
                Vector3 normal = Vector3.Cross(left, up).normalized;

                points[totalPoints + j * 2 + 0] = transform.InverseTransformPoint(Paths[i][j] - (left * .5f) * fWidth);
                points[totalPoints + j * 2 + 1] = transform.InverseTransformPoint(Paths[i][j] + (left * .5f) * fWidth);

                uvs[totalPoints + j * 2 + 0] = new Vector2(0, (float)j / (float)Paths[i].Count);
                uvs[totalPoints + j * 2 + 1] = new Vector2(1, (float)j / (float)Paths[i].Count);

                colors[totalPoints + j * 2 + 0] = new Color((float)i / (float)Paths.Count, (float)j / (float)Paths[i].Count, 0, 1);
                colors[totalPoints + j * 2 + 1] = new Color((float)i / (float)Paths.Count, (float)j / (float)Paths[i].Count, 0, 1);

                normals[totalPoints + j * 2 + 0] = transform.InverseTransformDirection(normal);
                normals[totalPoints + j * 2 + 1] = transform.InverseTransformDirection(normal);

                tangents[totalPoints + j * 2 + 0] = new Vector4(localLeft.x,localLeft.y,localLeft.z, 1);
                tangents[totalPoints + j * 2 + 1] = new Vector4(localLeft.x,localLeft.y,localLeft.z, 1) ;



            }

            totalPoints += Paths[i].Count * 2;

        }

        totalPoints = 0;
        int totalPointsPlus = 0;
        for (int i = 0; i < Paths.Count; i++)
        {


            for (int j = 0; j < Paths[i].Count - 1; j++)
            {

                print(2 * totalPointsPlus + j * 2 + 0);
                print(6 * totalPoints + j * 6 + 0);
                print(totalTriCount);
                print(totalPointCount);
                triangles[6 * totalPoints + j * 6 + 0] = 2 * totalPointsPlus + j * 2 + 0;
                triangles[6 * totalPoints + j * 6 + 1] = 2 * totalPointsPlus + j * 2 + 1;
                triangles[6 * totalPoints + j * 6 + 2] = 2 * totalPointsPlus + j * 2 + 3;

                triangles[6 * totalPoints + j * 6 + 3] = 2 * totalPointsPlus + j * 2 + 0;
                triangles[6 * totalPoints + j * 6 + 4] = 2 * totalPointsPlus + j * 2 + 3;
                triangles[6 * totalPoints + j * 6 + 5] = 2 * totalPointsPlus + j * 2 + 2;

            }

            totalPoints += Paths[i].Count - 1;
            totalPointsPlus += Paths[i].Count;


        }



        Mesh mesh = new Mesh();


        mesh.vertices = points;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.normals = normals;

        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;


    }

    public void Rengenerate()
    {

        recreating = false;
        RegenerateMesh();
    }


    public float maxDistance = 100;

    public Vector3 velocity = new Vector3(0, 0, 0);
    public int numberBounces = 0;
    public int maxBounces = 10;
    public int maxPathCount = 100;
    public float stepSize = 4;

    public List<Vector4> bouncePoints;
    public List<Vector3> bounceVels;


    public List<Vector4> finalTopPoints;
    public List<Vector3> finalTopVels;

    public List<Vector4> finalBottomPoints;
    public List<Vector3> finalBottomVels;

    void GetPaths(List<Vector3> topPoints)
    {

        waterLevel = transform.position.y;
        Paths = new List<List<Vector3>>();
        bouncePoints = new List<Vector4>();
        bounceVels = new List<Vector3>();

        for (int i = 0; i < topPoints.Count; i++)
        {
            List<Vector3> path = new List<Vector3>();
            path.Add(topPoints[i]);
            numberBounces = 0;

            bool canDo = true;
            float downVel = 0;

            Vector3 towardsCenter = transform.position - topPoints[i];
            towardsCenter = new Vector3(towardsCenter.x, 0, towardsCenter.z);

            velocity = towardsCenter.normalized * towardsCenterVelocity;
            velocity += transform.forward * forwardVelocity;
            bouncePoints.Add(new Vector4(topPoints[i].x, topPoints[i].y, topPoints[i].z, velocity.magnitude));
            bounceVels.Add(velocity);

            finalTopPoints.Add(new Vector4(topPoints[i].x, topPoints[i].y, topPoints[i].z, velocity.magnitude));
            finalTopVels.Add(velocity);

            while (canDo == true && path.Count < maxPathCount)
            {
                Vector3 lastPoint = path[path.Count - 1];


                Vector3 nextPoint = lastPoint + velocity.normalized * stepSize + Vector3.up * upAmount;
                velocity += Vector3.up * downVelocity;
                velocity *= dampening;

                print(velocity);

                RaycastHit hit;

                int layer_mask = LayerMask.GetMask("Terrain");
                if (Physics.Raycast(nextPoint + Vector3.up * maxDistance * 2, Vector3.down, out hit, maxDistance * 10, layer_mask))
                {

                    if (hit.point.y > nextPoint.y)
                    {

                        velocity = Vector3.Reflect(velocity, hit.normal);

                        towardsCenter = transform.position - hit.point;
                        towardsCenter = new Vector3(towardsCenter.x, 0, towardsCenter.z);
                        velocity += towardsCenter.normalized * forwardVelocity * forwardForceOnBounce;
                        velocity *= dampenOnBounce;
                        velocity += Vector3.up * downVelocity;

                        nextPoint = hit.point + Vector3.up * upAmount;// new Vector3(nextPoint.x, hit.point.y, nextPoint.z);
                        bouncePoints.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, velocity.magnitude));
                        bounceVels.Add(velocity);


                        // downVel = -downVel * .01f;
                        numberBounces++;

                        if (numberBounces > maxBounces)
                        {
                            canDo = false;
                        }

                    }



                    /*if (velocity.y < -30)
                    {
                        canDo = false;
                    }*/
                }
                else
                {
                    canDo = false;
                }


                if (nextPoint.y < waterLevel)
                {
                    nextPoint = new Vector3(nextPoint.x, waterLevel, nextPoint.z);
                    canDo = false;
                }

                if (canDo == false)
                {
                    bouncePoints.Add(new Vector4(nextPoint.x, nextPoint.y, nextPoint.z, velocity.magnitude));
                    bounceVels.Add(velocity);

                    finalBottomPoints.Add(new Vector4(nextPoint.x, nextPoint.y, nextPoint.z, velocity.magnitude));
                    finalBottomVels.Add(velocity);
                }


                path.Add(nextPoint);

            }

            print(path.Count);

            Paths.Add(path);
        }

        print(Paths.Count);
        print(bouncePoints.Count);
        print(bounceVels.Count);
        bouncePointEmitter.SetPoints(bouncePoints.ToArray(), bounceVels.ToArray());


    }



    public void Reset()
    {

        recreating = true;
        waterfallTopPoints = new List<Vector3>();
        maxTopPoint = new Vector3(0, -10000, 0);

    }


    public void OnTriggerEnter(Collider other)
    {
        if (God.IsOurWren(other))
        {
            print("Wren is in the waterfall");
            EnterWaterfall();

        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (God.IsOurWren(other))
        {
            if (inWaterfall)
            {
                print("Wren is out of the waterfall");
                ExitWaterfall();
            }
        }
    }
    public void EnterWaterfall()
    {
        inWaterfall = true;
        God.audio.Play(enterWaterfallClip);
    }

    public void ExitWaterfall()
    {
        inWaterfall = false;
        God.audio.Play(exitWaterfallClip);
    }

    public void MouseDown(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.point);

            if (recreating)
            {
                waterfallTopPoints.Add(hit.point);
            }
        }
    }






}
