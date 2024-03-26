using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClickPlacer : MonoBehaviour
{
    public float displayScale = 1.0f;
    public GameObject prefab;
    public int maxCount;

    public int count;

    public Vector2 scaleRange;

    public List<GameObject> placedGameObjects;

    public string[] layers;

    public Vector3 offset;
    public Vector3 rotationalOffset;

    public Vector3 rotationRandomness;
    public Vector3 offsetRandomness;

    public Vector2 normalMatchRange;




    public void MouseDown(Ray ray)
    {
        int layer_mask = LayerMask.GetMask(layers);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            go.transform.parent = transform;
            go.transform.position = hit.point;


            // Randomly rotate around the up axis to give us some range
            go.transform.Rotate(Vector3.up, Random.Range(0, 360));


            Vector3 upVector = Vector3.Lerp(Vector3.up, hit.normal, Random.Range(normalMatchRange.x, normalMatchRange.y));
            Vector3 lookVector = Vector3.Scale(Random.onUnitSphere, new Vector3(1, 0, 1));

            go.transform.rotation = Quaternion.LookRotation(lookVector, upVector);
            // hit.normal

            go.transform.Rotate(rotationalOffset + new Vector3(
                Random.Range(-rotationRandomness.x, rotationRandomness.x),
                Random.Range(-rotationRandomness.y, rotationRandomness.y),
                Random.Range(-rotationRandomness.z, rotationRandomness.z))
            );


            go.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            go.transform.position += go.transform.up * go.transform.localScale.y * (offset.y + Random.Range(-offsetRandomness.y, offsetRandomness.y));
            go.transform.position += go.transform.right * go.transform.localScale.x * (offset.x + Random.Range(-offsetRandomness.x, offsetRandomness.x));
            go.transform.position += go.transform.forward * go.transform.localScale.z * (offset.z + Random.Range(-offsetRandomness.z, offsetRandomness.z));

            placedGameObjects.Add(go);
            count++;
        }
    }

    public void Reset()
    {

        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        placedGameObjects.Clear();
        count = 0;
    }
}
