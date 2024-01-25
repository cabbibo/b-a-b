using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class PreyController : MonoBehaviour
{


    public Rigidbody rb;

    public float dieRate;
    public float drag;
    public float life;
    public PreyManager manager;
    public float maxScale;
    public float maxSpeed;
    public float oscilationSize;
    public float oscilationSpeed;
    public float speed;
    public float cageCutOff = .9f;
    public float cagePushBack = 100;
    public float cageCurl = 1;


    public float runForce;
    public float runRadius;

    public Transform cage;

    Vector3 projectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
    {
        //get vector from point on line to point in space
        Vector3 linePointToPoint = point - linePoint;

        float t = Vector3.Dot(linePointToPoint, lineVec);

        return linePoint + lineVec * t;

    }
    void Update()
    {
        rb.drag = drag;

        // Gently move around the cage
        Vector3 d = cage.position - transform.position;

        Vector3 curlVec = Vector3.Cross(d, Vector3.up).normalized;

        rb.AddForce(curlVec * cageCurl);


        // push in when close to sides of cage
        Vector3 localPosition = cage.InverseTransformPoint(transform.position);

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(localPosition[i]) > cageCutOff)
            {

                Vector3 fDir = Vector3.right;
                if (i == 1) { fDir = Vector3.up; }
                if (i == 2) { fDir = Vector3.forward; }


              //  print("pushing back " + fDir + " " + Mathf.Sign(localPosition[i]) * cagePushBack);

                // push back in
                rb.AddForce(cage.TransformDirection(fDir * Mathf.Sign(localPosition[i]) * cagePushBack));

            }
        }


        //Oscilate up and down
        rb.AddForce(Vector3.up * Mathf.Sin(Time.time * oscilationSpeed) * oscilationSize);



        if (God.wren)
        {
            Wren wren = God.ClosestWren(transform.position);



            // Repel From Wren
            d = wren.transform.position - transform.position;

            // rb.AddForce(-d.normalized * Mathf.Clamp(speed * 1 / (.4f + d.magnitude * .02f), 0, maxSpeed));
            //rb.AddForce(d.normalized * speed * .1f);


            if (wren.physics.rb.velocity.magnitude > 0)
            {
                Vector3 p = projectPointOnLine(wren.transform.position, wren.physics.rb.velocity, transform.position);
                Vector3 diff = transform.position - p;

                Vector3 diff2 = transform.position - wren.transform.position;
                if (diff2.magnitude < runRadius)
                {
                    rb.AddForce(diff.normalized * ((runRadius - diff2.magnitude) / runRadius) * runForce);
                }

            }


        }

        if (rb.velocity.magnitude > 0)
        {
            rb.rotation = Quaternion.RotateTowards(
                            rb.rotation,
                             Quaternion.LookRotation(rb.velocity, Vector3.up),
                            rb.velocity.magnitude
                     );
        }

        transform.localScale = Vector3.one * maxScale * Mathf.Clamp(Mathf.Min((1 - life) * 4, life), 0, 1);

        life -= dieRate;
        if (life < 0)
        {
            DestroyImmediate(gameObject);
        }




    }



    void OnCollisionEnter(Collision c)
    {
        if (God.IsOurWren(c))
        {
            manager.PreyGotAte(this);
            Destroy(gameObject);
        }
    }


    public void Initialize(PreyConfigSO config, PreyManager manager)
    {

        enabled = true;
        this.manager = manager;

        cage = manager.cage;


        speed = config.speed;
        drag = config.drag;
        life = config.life;

        dieRate = config.dieRate;

        maxSpeed = config.maxSpeed;
        maxScale = config.maxScale;


        oscilationSize = config.oscilationSize;
        oscilationSpeed = config.oscilationSpeed;
        cageCutOff = config.cageCutOff;
        cagePushBack = config.cagePushBack;
        cageCurl = config.cageCurl;

        runForce = config.runForce;
        runRadius = config.runRadius;

    }
}