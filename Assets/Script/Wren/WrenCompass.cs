using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenCompass : WrenInterface
{

    public float baseDist = 1;
    public float tipDist = 2;
    public float baseWidth = .4f;
    public float tipWidth = 0;

    public float maxDist = 300;
    public float minDist = 50;

    public float maxDistLengthMultiplier = 3;
    public float maxDistWidthMultiplier = .1f;

    public Transform holder;




    // At max Dist, long and skinny, at min dist, short and fat



    public Texture2D[] icons;
    public LineRenderer[] lines;
    public Renderer[] renderers;
    public GameObject[] pointers;

    public GameObject wrenPointerPrefab;

    public int oWrenCount;

    public Wren wren;

    public bool active;





    public float target_baseDist = 1;
    public float target_tipDist = 2;
    public float target_baseWidth = .4f;
    public float target_tipWidth = 0;

    public float target_maxDist = 300;
    public float target_minDist = 50;

    public float target_maxDistLengthMultiplier = 3;
    public float target_maxDistWidthMultiplier = .1f;


    public GameObject targetPointerPrefab;

    public GameObject[] target_pointers;


    public LineRenderer[] target_lines;

    public int oTargetableCount;


    public void OnEnable()
    {
        if (wren == null)
        {
            wren = God.wren;
        }
    }

    public void Toggle()
    {
        active = !active;


        holder.gameObject.SetActive(active);
    }


    public void OnDisable()
    {

    }
    // Update is called once per frame
    public void Update()
    {

        if (God.wren)
        {
            wren = God.wren;
            God.wren.compass = this;

            if (God.wrens.Count != oWrenCount)
            {
                WrensChanged();
            }

            transform.position = God.wren.transform.position;

            if (God.targetableObjects.Count != oTargetableCount)
            {
                TargetsChanged();
            }




            int id = 0;
            Vector3 v2 = new Vector3();
            foreach (Wren w in God.wrens)
            {

                if (w != wren)
                {

                    v2 = w.transform.position - transform.position;




                    float v = v2.magnitude;

                    if (v < .0001)
                    {


                        lines[id].SetPosition(0, Vector3.one * 1000);
                        lines[id].SetPosition(1, Vector3.one * 1000);
                        lines[id].startWidth = 0;
                        lines[id].endWidth = 0;

                    }
                    else
                    {

                        // get normalized between min and max
                        v = Mathf.InverseLerp(minDist, maxDist, v);

                        print(v);
                        //
                        float fWidth = Mathf.Lerp(1, maxDistWidthMultiplier, v);
                        float fLength = Mathf.Lerp(1, maxDistLengthMultiplier, v);

                        // pointers[id].transform.position = transform.position + v2.normalized * 8 + v2.normalized * 2 + v2.normalized * 5 * closeness;

                        lines[id].SetPosition(0, transform.position + v2.normalized * baseDist);
                        lines[id].SetPosition(1, transform.position + v2.normalized * (baseDist + fLength * tipDist));
                        lines[id].startWidth = fWidth * baseWidth;
                        lines[id].endWidth = fWidth * tipWidth;

                    }


                    id++;
                }
            }





            id = 0;
            v2 = new Vector3();
            foreach (Transform t in God.targetableObjects)
            {



                if (t == null)
                {

                    target_lines[id].SetPosition(0, Vector3.one * 1000);
                    target_lines[id].SetPosition(1, Vector3.one * 1000);
                    target_lines[id].startWidth = 0;
                    target_lines[id].endWidth = 0;
                }
                else
                {

                    v2 = t.position - transform.position;




                    float v = v2.magnitude;

                    if (v < .0001)
                    {


                        target_lines[id].SetPosition(0, Vector3.one * 1000);
                        target_lines[id].SetPosition(1, Vector3.one * 1000);
                        target_lines[id].startWidth = 0;
                        target_lines[id].endWidth = 0;

                    }
                    else
                    {

                        // get normalized between min and max
                        v = Mathf.InverseLerp(target_minDist, target_maxDist, v);

                        //
                        float fWidth = Mathf.Lerp(1, target_maxDistWidthMultiplier, v);
                        float fLength = Mathf.Lerp(1, target_maxDistLengthMultiplier, v);

                        // pointers[id].transform.position = transform.position + v2.normalized * 8 + v2.normalized * 2 + v2.normalized * 5 * closeness;

                        target_lines[id].SetPosition(0, transform.position + v2.normalized * target_baseDist);
                        target_lines[id].SetPosition(1, transform.position + v2.normalized * (target_baseDist + fLength * target_tipDist));
                        target_lines[id].startWidth = fWidth * target_baseWidth;
                        target_lines[id].endWidth = fWidth * target_tipWidth;

                    }
                }


                id++;

            }
        }


    }

    public void WrensChanged()
    {

        print("changing wren");

        for (int i = 0; i < pointers.Length; i++)
        {
            Destroy(pointers[i]);
        }

        icons = new Texture2D[God.wrens.Count - 1];
        lines = new LineRenderer[God.wrens.Count - 1];
        pointers = new GameObject[God.wrens.Count - 1];
        renderers = new Renderer[God.wrens.Count - 1];

        int id = 0;
        foreach (Wren w in God.wrens)
        {

            if (w != God.wren)
            {
                pointers[id] = GameObject.Instantiate(wrenPointerPrefab);

                pointers[id].transform.parent = holder;
                pointers[id].transform.position = Vector3.zero;
                pointers[id].transform.rotation = Quaternion.identity;

                pointers[id].SetActive(true);


                icons[id] = w.colors.icon;
                lines[id] = pointers[id].GetComponent<LineRenderer>();
                renderers[id] = pointers[id].GetComponent<Renderer>();
                renderers[id].enabled = false;
                lines[id].enabled = true;

                renderers[id].material.SetTexture("_MainTex", icons[id]);
                lines[id].material.SetTexture("_MainTex", icons[id]);
                lines[id].material.SetFloat("_WrenID", id);
                lines[id].material.SetFloat("_Hue1", w.state.hue1);

                id++;
            }
        }


        oWrenCount = God.wrens.Count;


    }


    public void TargetsChanged()
    {

        print("changing targets");

        for (int i = 0; i < target_pointers.Length; i++)
        {
            Destroy(target_pointers[i]);
        }

        target_lines = new LineRenderer[God.targetableObjects.Count];
        target_pointers = new GameObject[God.targetableObjects.Count];

        int id = 0;
        foreach (Transform t in God.targetableObjects)
        {


            target_pointers[id] = GameObject.Instantiate(targetPointerPrefab);

            target_pointers[id].transform.parent = holder;
            target_pointers[id].transform.position = Vector3.zero;
            target_pointers[id].transform.rotation = Quaternion.identity;

            target_pointers[id].SetActive(true);


            target_lines[id] = target_pointers[id].GetComponent<LineRenderer>();
            target_lines[id].enabled = true;

            id++;
        }


        oTargetableCount = God.targetableObjects.Count;


    }


    public void UpdateColors()
    {

    }
}
