using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;

public class OceanRendererMonitor : MonoBehaviour
{

    public UnderwaterRenderer underwaterRenderer;

    // Update is called once per frame
    void Update()
    {

        if (underwaterRenderer == null)
        {
            underwaterRenderer = Camera.main.GetComponent<UnderwaterRenderer>();
        }

        if (OceanRenderer.Instance != null)
        {
            if (underwaterRenderer != null)
            {
                underwaterRenderer.enabled = true;
                // underwaterRenderer.enabled = OceanRenderer.Instance.ViewerHeightAboveWater < 0;
            }
        }
        else
        {
            if (underwaterRenderer != null)
            {
                underwaterRenderer.enabled = false;
            }
        }

    }
}
