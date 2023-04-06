using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using UnityEngine.Rendering;

public class DataForm : Form
{


    public float[] values;
    public int ss;

    public bool reset;

    public override void SetStructSize()
    {
        structSize = ss;
    }
    public override void OnBirthed()
    {
        values = new float[count * structSize];
        _readbackPending = false;
    }
    public override void WhileLiving(float v)
    {
        // _buffer.GetData(values);


        RequestAsyncReadback();
        if (reset)
        {
            _buffer.SetData(new float[count * structSize]);
        }



    }



    private AsyncGPUReadbackRequest _readbackRequest;
    public bool _readbackPending;
    private void RequestAsyncReadback()
    {
        if (!_readbackPending)
        {
            _readbackRequest = AsyncGPUReadback.Request(_buffer, OnReadbackComplete);
            _readbackPending = true;
        }
    }

    private void OnReadbackComplete(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("Error during readback.");
            return;
        }

        // Get the data from the request
        values = request.GetData<float>().ToArray();

        // Reset the readback flag
        _readbackPending = false;
    }
}
