using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlacePrefabsOnTerrain))]
public class PlacePrefabsOnTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlacePrefabsOnTerrain sc = (PlacePrefabsOnTerrain)target;
        if (!sc.enabled)
            return;
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            sc.Regenerate();
        }
    }
}
#endif


[ExecuteAlways]
public class PlacePrefabsOnTerrain : MonoBehaviour
{

    public GameObject prefab;
    public int count;

    public float verticalOffset;

    public int biome;

    public float minScale = 10;
    public float maxScale = 30;

    public float minSteepness;
    public float maxSteepness;

    public float matchTerrainNormal;

    public Vector3 rotationRandomness;



    public Transform[] transforms;
    public List<GameObject> allGameObjects;
    public void OnEnable()
    {
        Regenerate();
    }


    public int chances = 100;
    public void Regenerate()
    {

        if (allGameObjects == null)
            allGameObjects = new List<GameObject>();

        allGameObjects.Clear();
        // CLEAR
        transforms = new Transform[count];

        int childrenTransformCount = transform.childCount;
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }




        // GENERATE
        for (int i = 0; i < count; i++)
        {

            for (int j = 0; j < chances; j++)
            {

                Vector3 nPos = new Vector3(Random.value, 0, Random.value);
                // get random position on terrain
                Vector3 pos = Vector3.Scale(new Vector3(nPos.x - .5f, 0, nPos.z - .5f), God.islandData.size);

                // see if it is in the right biome
                float[] biomeVal = God.islandData.GetBiomeValues(pos);


                //if it is, place it
                if (biomeVal[biome] > .01f)
                {


                    Vector3 n = God.terrain.terrainData.GetInterpolatedNormal(nPos.x, nPos.y);

                    //print(n);


                    if (n.y > minSteepness && n.y < maxSteepness)
                    {
                        /* RaycastHit hit;

                         LayerMask mask = LayerMask.GetMask("Terrain");



                        if (Physics.Raycast(pos + Vector3.up * 1000, Vector3.down, out hit, 10000, mask))
                         {
                            pos = hit.point;
                            pos += Vector3.up * verticalOffset;

                         }*/

                        // print(God.terrain.SampleHeight(transform.position));

                        pos.y = God.terrain.SampleHeight(pos) + verticalOffset;


                        GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);
                        go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
                        // go.transform.LookAt(go.transform.position + n);
                        //go.transform.RotateAround(go.transform.position, go.transform.right, 90);

                        //Quaternion targetRotation = Quaternion.FromToRotation(go.transform.up, n) * go.transform.rotation;
                        /// transform.rotation = targetRotation;
                        //    go.transform.rotation = targetRotation;

                        // go.transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(rotationRandomness * Random.value), Random.value);
                        transforms[i] = go.transform;

                        allGameObjects.Add(go);
                        break;
                    }


                }


            }
        }



    }

    // Update is called once per frame
    void Update()
    {

    }
}
