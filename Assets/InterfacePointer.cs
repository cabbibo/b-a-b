using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WrenUtils;

public class InterfacePointer : MonoBehaviour
{

    public float size;


    public Material material;

    public MaterialPropertyBlock mpb;

    public bool fullOn;
    public bool doFade;


    public float fadeOutSpeed;
    public float fadeInSpeed;

    [Header("Debug")]


    public List<Transform> pointerList = new List<Transform>();

    public List<int> pointerTypes = new List<int>();

    public List<float> fades = new List<float>();

    public List<float> targetFades = new List<float>();

    public ComputeBuffer _buffer;
    public ComputeBuffer _typeBuffer;
    public ComputeBuffer _fadeBuffer;

    public Vector3[] pointerPositions;
    public int oPointerCount;

    public void RemakeBuffer()
    {

        ReleaseBuffers();

        print("RemakeBuffer");
        print(pointerList.Count);
        if (pointerList.Count > 0)
        {
            _buffer = new ComputeBuffer(pointerList.Count, 3 * sizeof(float));
            _typeBuffer = new ComputeBuffer(pointerList.Count, 1 * sizeof(float));
            _fadeBuffer = new ComputeBuffer(pointerList.Count, 1 * sizeof(float));
            pointerPositions = new Vector3[pointerList.Count];


        }
        else
        {
            ReleaseBuffers();

        }

        oPointerCount = pointerList.Count;

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

        pointerPositions = new Vector3[0];
        oPointerCount = 0;


    }

    public void LateUpdate()
    {

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

            //            print("drawing");

            for (int i = 0; i < pointerList.Count; i++)
            {
                pointerPositions[i] = pointerList[i].position;

            }


            _buffer.SetData(pointerPositions);
            _typeBuffer.SetData(pointerTypes.ToArray());
            _fadeBuffer.SetData(fades.ToArray());

            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetInt("_Count", pointerList.Count);
            mpb.SetFloat("_Size", size);


            mpb.SetBuffer("_PositionBuffer", _buffer);
            mpb.SetBuffer("_TypeBuffer", _typeBuffer);
            mpb.SetBuffer("_FadeBuffer", _fadeBuffer);


            mpb.SetVector("_WrenPos", God.wren.bird.head.position);


            Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, pointerList.Count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

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

            fades[pointerList.IndexOf(pointer)] = 1;
            targetFades[pointerList.IndexOf(pointer)] = 0;
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
            pointerTypes.Add(type);
            targetFades.Add(0);
            fades.Add(0);
        }

    }


    public void RemovePointer(Transform t)
    {
        if (pointerList.Contains(t))
        {
            pointerTypes.RemoveAt(pointerList.IndexOf(t));
            fades.RemoveAt(pointerList.IndexOf(t));
            targetFades.RemoveAt(pointerList.IndexOf(t));
            pointerList.Remove(t);
        }
        else
        {
            Debug.LogError("Pointer not found in list");
        }
    }


    public void ClearPointers()
    {
        pointerList.Clear();
        pointerTypes.Clear();
        fades.Clear();
        targetFades.Clear();
        ReleaseBuffers();

    }


}
