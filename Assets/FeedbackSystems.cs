using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class FeedbackSystems : MonoBehaviour
{


    public LineRenderer currentTargetLineRenderer;
    public ParticleSystem skimParticles;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        currentTargetLineRenderer.enabled = false;

    }

    public void UpdateTargetLineRenderer(Transform t)
    {
        if (God.wren)
        {
            currentTargetLineRenderer.enabled = true;
            currentTargetLineRenderer.SetPosition(0, t.position);
            currentTargetLineRenderer.SetPosition(1, (God.wren.transform.position - t.position) * .8f + t.position);
            currentTargetLineRenderer.SetPosition(2, God.wren.transform.position);
        }
    }

}
