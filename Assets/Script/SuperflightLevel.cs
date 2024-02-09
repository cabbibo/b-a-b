using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SuperflightLevel : MonoBehaviour
{

    public GameObject[] rings;

    public float ringPlaceRange;
    public float ringScaleMin;
    public float ringScaleMax;

    public int ringCount;
    public GameObject ringPrefab;


    public GameObject[] blocks;

    public int blockCount;
    public float blockPlaceRange;
    public float blockScaleMin;
    public float blockScaleMax;



    public GameObject blockPrefab;


    public void OnEnable()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }


        for (int i = 0; i < ringCount; i++)
        {
            GameObject newRing = Instantiate(ringPrefab, transform);
            newRing.transform.position = new Vector3(Random.Range(-ringPlaceRange, ringPlaceRange), Random.Range(-ringPlaceRange, ringPlaceRange), Random.Range(-ringPlaceRange, ringPlaceRange));
            newRing.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            newRing.transform.localScale = Vector3.one * (Random.Range(ringScaleMin, ringScaleMax));
            transform.parent = transform;

        }

        for (int i = 0; i < blockCount; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, transform);
            newBlock.transform.position = new Vector3(Random.Range(-blockPlaceRange, blockPlaceRange), Random.Range(-blockPlaceRange, blockPlaceRange), Random.Range(-blockPlaceRange, blockPlaceRange));
            newBlock.transform.rotation = Quaternion.identity;
            newBlock.transform.localScale = new Vector3(Random.Range(blockScaleMin, blockScaleMax), Random.Range(blockScaleMin, blockScaleMax), Random.Range(blockScaleMin, blockScaleMax));

            transform.parent = transform;
        }


    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
