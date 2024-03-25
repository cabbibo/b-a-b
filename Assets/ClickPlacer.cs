using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickPlacer : MonoBehaviour
{
    public float displayScale = 1.0f;
    public GameObject prefab;
    public int maxCount;

    public int count;

    public Vector2 scaleRange;

    public List<GameObject> placedGameObjects;

    public string[] layers;


    public void MouseDown(Ray ray)
    {
        int layer_mask = LayerMask.GetMask(layers);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {

            GameObject go = Instantiate(prefab, hit.point, Quaternion.identity);
            go.transform.parent = transform;
            go.transform.Rotate(Vector3.up, Random.Range(0, 360));
            go.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
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
