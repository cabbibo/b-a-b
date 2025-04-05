using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;

public class InterfacePointer : MonoBehaviour
{

    //public float size;


    public Material pointerMaterial;
    public Material skyColumnMaterial;

    public MaterialPropertyBlock mpb;

    public bool fullOn;
    public bool doFade;


    public float fadeOutSpeed;
    public float fadeInSpeed;

    [Header("Debug")]


    public List<Transform> pointerList = new List<Transform>();

    public List<float> pointerTypes = new List<float>();

    public List<float> fades = new List<float>();

    public List<float> targetFades = new List<float>();
    public List<Vector4> extraData = new List<Vector4>();

    // EXTRADATA

    // times complete
    // fully complete
    // ???
    // only show directional pointer


    public ComputeBuffer _buffer;
    public ComputeBuffer _typeBuffer;
    public ComputeBuffer _fadeBuffer;
    public ComputeBuffer _extraDataBuffer;

    public Vector3[] pointerPositions;
    public int oPointerCount;


    public int updateRate;
    public int updateCounter;


    /*


    TYPES
    0 = Quest
    1 = Activity
    2 = other wren
    3 = birds ( 3.0 = butterflies , 2.25 = small birds, 2.5 = medium birds, 2.75 = large birds)
    4 = mana pools
    5 = windRings
    6 = windTunnels
    7 = updrafts
    9 = tunnels


    */




    public void RemakeBuffer()
    {

        ReleaseBuffers();

        //      print("RemakeBuffer");
        //        print(pointerList.Count);
        if (pointerList.Count > 0)
        {
            _buffer = new ComputeBuffer(pointerList.Count, 3 * sizeof(float));
            _typeBuffer = new ComputeBuffer(pointerList.Count, 1 * sizeof(float));
            _fadeBuffer = new ComputeBuffer(pointerList.Count, 1 * sizeof(float));
            _extraDataBuffer = new ComputeBuffer(pointerList.Count, 4 * sizeof(float));
            pointerPositions = new Vector3[pointerList.Count];


        }
        else
        {
            ReleaseBuffers();

        }

        oPointerCount = pointerList.Count;

    }


    public void OnEnable()
    {
        updateCounter = 0;

    }


    public void OnDisable()
    {
        ReleaseBuffers();
    }

    public void ReleaseBuffers()
    {
        if (_buffer != null)
        {
            _buffer.Dispose();
        }

        if (_typeBuffer != null)
        {
            _typeBuffer.Dispose();
        }

        if (_fadeBuffer != null)
        {
            _fadeBuffer.Dispose();
        }

        if (_extraDataBuffer != null)
        {
            _extraDataBuffer.Dispose();
        }

        pointerPositions = new Vector3[0];
        oPointerCount = 0;


    }

    public void LateUpdate()
    {

        updateCounter++;
        if (updateCounter % updateRate == 0)
        {
            updateCounter = 0;
            UpdateAllPointers();
        }

        if (pointerList.Count != oPointerCount)
        {
            RemakeBuffer();
        }




        bool noneOn = true;
        for (int i = 0; i < fades.Count; i++)
        {
            if (fades[i] > 0.01f)
            {
                noneOn = false;
            }

            fades[i] = Mathf.Lerp(fades[i], targetFades[i], fades[i] < targetFades[i] ? fadeInSpeed : fadeOutSpeed);
        }



        if (pointerList.Count > 0 && !noneOn)
        {

            /*

            Should we only update buffer when we remake it? can pointers change location?

            */

            for (int i = 0; i < pointerList.Count; i++)
            {
                pointerPositions[i] = pointerList[i].position;

            }


            _buffer.SetData(pointerPositions);

            float[] pointerTypeArray = pointerTypes.ToArray();



            // Set object of interest just in the render buffer not anywhere else!
            if (objectOfInterest != null)
            {
                // if we have an object of interest, set its type to 0.5f
                pointerTypeArray[pointerList.IndexOf(objectOfInterest)] = 10f;

            }

            _typeBuffer.SetData(pointerTypeArray);
            _fadeBuffer.SetData(fades.ToArray());
            _extraDataBuffer.SetData(extraData.ToArray());



            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetInt("_Count", pointerList.Count);
            //mpb.SetFloat("_Size", size);


            mpb.SetBuffer("_PositionBuffer", _buffer);
            mpb.SetBuffer("_TypeBuffer", _typeBuffer);
            mpb.SetBuffer("_FadeBuffer", _fadeBuffer);
            mpb.SetBuffer("_ExtraDataBuffer", _extraDataBuffer);


            mpb.SetVector("_WrenPos", God.wren.bird.head.position);


            Graphics.DrawProcedural(pointerMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, pointerList.Count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
            Graphics.DrawProcedural(skyColumnMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, pointerList.Count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

        }

    }

    public void SetFade(Transform pointer, float v)
    {

        if (pointerList.Contains(pointer))
        {
            // fading
            targetFades[pointerList.IndexOf(pointer)] = v;
        }
        else
        {
            Debug.LogError("Pointer not found in list");
        }

    }



    public void SetFullOn(Transform pointer, bool b)
    {
        if (pointerList.Contains(pointer))
        {
            // fading
            targetFades[pointerList.IndexOf(pointer)] = b ? 1 : 0;
        }

    }

    // immediately set to full brightness then fade out
    public void Ping(Transform pointer)
    {

        if (pointerList.Contains(pointer))
        {



            print("HAS POINTER");

            if (objectOfInterest != null)
            {

                // Only ping object of interest if thats what weve got!
                if (pointer == objectOfInterest)
                {

                    //print("OBJECT OF INTEREST");

                    fades[pointerList.IndexOf(pointer)] = 1;
                    targetFades[pointerList.IndexOf(pointer)] = 0;
                }
                else
                {
                    fades[pointerList.IndexOf(pointer)] = 0;
                    targetFades[pointerList.IndexOf(pointer)] = 0;
                    // print("NOT OBJECT OF INTEREST");
                }
            }
            else
            {

                //                print("NO OBJECT OF INTEREST");
                fades[pointerList.IndexOf(pointer)] = 1;
                targetFades[pointerList.IndexOf(pointer)] = 0;
            }
        }
        else
        {
            print("NO POINTER");
        }

    }

    public void TurnOnPointer(Transform pointer)
    {
        if (pointerList.Contains(pointer))
        {
            targetFades[pointerList.IndexOf(pointer)] = 1;
        }
    }

    public void TurnOffPointer(Transform pointer)
    {
        if (pointerList.Contains(pointer))
        {
            targetFades[pointerList.IndexOf(pointer)] = 0;
        }
    }


    public void PingAll()
    {

        for (int i = 0; i < pointerList.Count; i++)
        {
            Ping(pointerList[i]);
        }

    }

    public void AddPointer(Transform t)
    {
        AddPointer(t, 0);
    }


    public void AddPointer(Transform t, int type)
    {

        if (!pointerList.Contains(t))
        {
            pointerList.Add(t);
            if (type < 0)
            {
                type = 0;
            }

            pointerTypes.Add((float)type);

            targetFades.Add(0);
            fades.Add(0);
            extraData.Add(new Vector4(0, 0, 0, 0));
        }
        else
        {
            if (type >= null)
            {
                pointerTypes[pointerList.IndexOf(t)] = (float)type;
            }
        }

    }

    public void AddPointer(Transform t, int type, float tc)
    {
        if (!pointerList.Contains(t))
        {
            pointerList.Add(t);

            if (type < 0)
            {
                type = 0;
            }

            pointerTypes.Add((float)type);


            targetFades.Add(0);
            fades.Add(0);
            extraData.Add(new Vector4(tc, 0, 0, 0)); // adding to our extra data!
        }
        else
        {

            if (type >= 0)
            {
                pointerTypes[pointerList.IndexOf(t)] = (float)type;
            }


            extraData[pointerList.IndexOf(t)] = new Vector4(tc, 0, 0, 0);
        }
    }



    public void AddPointer(Transform t, int type, Vector4 tc)
    {
        if (!pointerList.Contains(t))
        {
            pointerList.Add(t);
            if (type < 0)
            {
                type = 0;
            }

            pointerTypes.Add((float)type);

            targetFades.Add(0);
            fades.Add(0);
            extraData.Add(tc); // adding to our extra data!
        }
        else
        {
            if (type >= 0)
            {
                pointerTypes[pointerList.IndexOf(t)] = (float)type;
            }

            extraData[pointerList.IndexOf(t)] = tc;
        }
    }



    public void ShowSinglePointer(Transform t, int type, Vector4 tc)
    {

        ClearPointers();
        AddPointer(t, type, tc);
        TurnOnPointer(t);

    }

    public void SetSinglePointer(Transform t, int type, Vector4 tc)
    {

        ClearPointers();
        AddPointer(t, type, tc);


    }



    public Transform objectOfInterest;

    public void SetObjectOfInterest(Transform t)
    {
        objectOfInterest = t;
        AddPointer(t, -1, new Vector4(0, 0, 0, 0)); // add it to the list so we can ping it!


    }

    public void ReleaseObjectOfInterest()
    {
        if (objectOfInterest != null)
        {
            objectOfInterest = null;
        }

        RemovePointer(objectOfInterest); // remove it from the list so we can ping it!
    }





    public void RemovePointer(Transform t)
    {
        if (pointerList.Contains(t))
        {
            pointerTypes.RemoveAt(pointerList.IndexOf(t));
            fades.RemoveAt(pointerList.IndexOf(t));
            targetFades.RemoveAt(pointerList.IndexOf(t));
            extraData.RemoveAt(pointerList.IndexOf(t));
            pointerList.Remove(t);
        }
        else
        {
            //Debug.LogError("Pointer not found in list");
        }
    }


    public void ClearPointers()
    {
        pointerList.Clear();
        pointerTypes.Clear();
        fades.Clear();
        targetFades.Clear();
        extraData.Clear();
        ReleaseBuffers();

    }




























    public GameObject[] getAllOfTag(string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag);
    }



    public void UpdateAllPointers()
    {
        AddAllQuests();
        AddAllActivities();
        AddAllPortals();

    }


    public void AddAllQuests()
    {
        GameObject[] allQuests = getAllOfTag("Quest");
        foreach (GameObject quest in allQuests)
        {

            AddPointer(quest.GetComponent<Quest>().portal.transform, 0, quest.GetComponent<Quest>().completed ? 1 : 0);
        }
    }


    public void AddAllPortals()
    {
        GameObject[] gameObjects = getAllOfTag("Portal");
        foreach (GameObject portal in gameObjects)
        {
            AddPointer(portal.transform, 2, new Vector4(0, 0, 0, 0));
        }
    }

    public void AddAllActivities()
    {
        GameObject[] allActivities = getAllOfTag("Activity");
        foreach (GameObject activity in allActivities)
        {
            Vector4 tc = new Vector4(
                activity.GetComponent<Activity>().numTimesCompleted,
                activity.GetComponent<Activity>().fullCompleted ? 1 : 0,
                0, 0);
            AddPointer(activity.GetComponent<Activity>().mainPointOfInterest, 1, activity.GetComponent<Activity>().numTimesCompleted);
        }
    }

    // This one will update Dynamically?

    public void AddAllBirds()
    {

    }


    public void UpdateState()
    {
        UpdateAllPointers();
    }




    // Addd all pointers to all of them?


}
