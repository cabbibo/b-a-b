using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PreyWing : MonoBehaviour
{

    public float speed;
    public float dampening;
    public PreyController controller;

    public LineRenderer lineRenderer;
    public float wingSpan;
    public bool leftRight;

    public Vector3 velocity;


    // Start is called before the first frame update
    void Start()
    {

    }

    Vector3 targetPosition;
    // Update is called once per frame
    void Update()
    {

        targetPosition = controller.transform.position + controller.transform.right * wingSpan * (leftRight ? 1 : -1) + controller.transform.up * wingSpan * 0.5f * Mathf.Sin(controller.positionInFlapCycle + Mathf.PI);

        velocity += (targetPosition - transform.position) * speed;

        velocity *= dampening;
        transform.position += velocity;

        lineRenderer.SetPosition(0, controller.transform.position);
        lineRenderer.SetPosition(1, transform.position);

    }
}
