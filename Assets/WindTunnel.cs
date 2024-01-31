using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MagicCurve;
using WrenUtils;
using UnityEditor.Media;

public class WindTunnel : MonoBehaviour
{

    public LineRenderer lineRenderer;
    public Curve curve;

    public Transform debugTransform;

    public float tunnelForwardForce = 1;
    public float tunnelInForce = 1;
    public float dampenForce;
    public float maxDistance;

    [HideInInspector] public float tunnelForceFalloff = 1;

    public bool debug;

    [Header("Newer")]
    public AnimationCurve inwardsForceCurve = AnimationCurve.Linear(-1, 0, 1, 1);
    public AnimationCurve forwardsForceCurve = AnimationCurve.Linear(-1, 0, 1, 1);
    public AnimationCurve dampenForceCurve = AnimationCurve.Linear(-1, 0, 1, 1);
    public float outerRange = 5;
    public float easeFwdForce = 0;


    float _last_force_fwd;
    float _last_force_fwd_v;
    float _last_force_in;
    float _last_force_in_v;

    void Start()
    {
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();
        if (!curve)
            curve = GetComponent<Curve>();
    }

    float distToCurve;
    Vector3 curveInfo;

    Vector3 closestPos;
    Vector3 inDir;
    Vector3 closestForward;

    float valueAlongCurve;
    float curveWidth;

    float outsidePercentage;
    float insidePercentage;


    void Update()
    {

        Vector3 fPos = Vector3.zero;
        if (debugTransform) fPos = debugTransform.position;
        if (God.wren)
        {
            fPos = God.wren.transform.position;
        }


        oInsidePercentage = insidePercentage;
        oOutsidePercentage = outsidePercentage;


        CalculateCurveInfo(fPos);

        if (debug)
        {
            print(valueAlongCurve);
        }


        if (outsidePercentage >= 1 || valueAlongCurve <= 0 || valueAlongCurve >= 1)
        {
            return;
        }

        CalculateForces(fPos);

        if (God.wren != null)
        {




            God.wren.physics.rb.AddForce(finalInwardForce);
            God.wren.physics.rb.AddForce(finalForwardForce);
            God.wren.physics.rb.AddForce(finalDampenForce);

            Debug.DrawLine(fPos, fPos + finalInwardForce, Color.red);
            Debug.DrawLine(fPos, fPos + finalForwardForce, Color.green);
            Debug.DrawLine(fPos, fPos + finalDampenForce, Color.yellow);

            DoBoost();

            _last_force_fwd = force_fwd;
            _last_force_in = force_in;

        }
    }


    public void DoBoost()
    {

        if (oInsidePercentage > 0 && insidePercentage <= 0)
        {
            print("ENTER BOOST");
            God.wren.physics.rb.AddForce(closestForward * onEnterForwardForce);
        }

        if (oInsidePercentage <= 0 && insidePercentage > 0)
        {

            print("EXIT BOOST");
            God.wren.physics.rb.AddForce(closestForward * onExitForwardForce);
        }


    }


    public void CalculateCurveInfo(Vector3 fPos)
    {

        curveInfo = curve.GetClosestPointData(fPos);

        valueAlongCurve = Mathf.Clamp(curveInfo.z, 0, 1);

        closestPos = Vector3.Lerp(curve.bakedPoints[(int)curveInfo.x], curve.bakedPoints[(int)curveInfo.y], valueAlongCurve);

        inDir = (closestPos - fPos).normalized;

        // Gets how close we are to the closest point
        distToCurve = Vector3.Distance(closestPos, fPos);

        closestForward = Vector3.Lerp(curve.bakedDirections[(int)curveInfo.x], curve.bakedDirections[(int)curveInfo.y], valueAlongCurve);

        curveWidth = curve.GetWidthFromValueAlongCurve(valueAlongCurve);



        //1 at center , 0 at edge;
        insidePercentage = 1 - Mathf.Clamp01(distToCurve / curveWidth);

        // 0 at tube edge , 1 at outer range edge
        outsidePercentage = Mathf.Clamp01((distToCurve - curveWidth) / (curveWidth * outerRange));

    }


    Vector3 finalForwardForce;
    Vector3 finalInwardForce;
    Vector3 finalDampenForce;

    float force_in;
    float force_fwd;
    float force_dampen;

    public float tuckInForceMultiplier = 1.25f;
    public float tuckForwardForceMultiplier = .9f;
    public float tuckDampenForceMultiplier = .25f;

    public float onEnterForwardForce;
    public float onExitForwardForce;

    public float oInsidePercentage;
    public float oOutsidePercentage;

    public void CalculateForces(Vector3 pos)
    {

        float fVal = insidePercentage - outsidePercentage;

        //        print(fVal);

        force_in = 0;

        force_in += inwardsForceCurve.Evaluate(fVal);
        force_in *= tunnelInForce;

        force_fwd = 0;
        force_fwd += forwardsForceCurve.Evaluate(fVal);
        force_fwd *= tunnelForwardForce;


        force_dampen = 0f;
        force_dampen += dampenForceCurve.Evaluate(fVal);
        force_dampen *= dampenForce;

        //  force_dampen += (fVal + 1) / 2; //dampenForce * (1 - t_outer);
        //force_dampen += dampenForce * (1 - t_inner);



        var trigger_force = 1 + Mathf.Max(God.input.l2, God.input.r2);
        force_in *= trigger_force * tuckInForceMultiplier;
        force_fwd *= trigger_force * tuckForwardForceMultiplier;
        force_dampen *= trigger_force * tuckDampenForceMultiplier;


        if (easeFwdForce > 0)
        {
            // force_fwd = Mathf.SmoothDamp(_last_force_fwd, force_fwd, ref _last_force_fwd_v, easeFwdForce * Time.deltaTime);
            force_fwd = Mathf.Max(force_fwd, Mathf.Lerp(_last_force_fwd, force_fwd, easeFwdForce * Time.deltaTime));
            force_in = Mathf.Max(force_in, Mathf.Lerp(_last_force_in, force_in, easeFwdForce * Time.deltaTime));
            // force_in = Mathf.SmoothDamp(_last_force_in, force_in, ref _last_force_in_v, easeFwdForce * Time.deltaTime);
        }


        finalInwardForce = inDir * force_in;
        finalForwardForce = closestForward * force_fwd;

        if (God.wren != null)
        {
            finalDampenForce = -God.wren.physics.vel * force_dampen;
        }
        else
        {
            finalDampenForce = Vector3.zero;
        }






    }


    public bool drawForceLines = false;
    void OnDrawGizmos()
    {
        if (!curve)
            curve = GetComponent<Curve>();

        Gizmos.color = new Color(1, .5f, .2f);
        if (God.wren)
        {
            Gizmos.DrawLine(God.wren.transform.position, closestPos);
        }

        if (drawForceLines)
        {

            Gizmos.color = Color.red;
            //   Gizmos.DrawSphere(debugTransform.position, 1);


            Gizmos.color = Color.blue;
            for (int i = 0; i < 50; i++)
            {
                float t = i / 50f;
                Vector3 p = curve.GetPositionFromValueAlongCurve(t);
                var fwd = curve.GetForwardFromValueAlongCurve(t);
                var up = curve.GetUpFromValueAlongCurve(t);
                var right = curve.GetRightFromValueAlongCurve(t);

                //if (i < 5) { print(r); }
                Gizmos.color = new Color(1, 1, 1, 1);
                Gizmos.DrawLine(p, p + fwd * 100);
                Gizmos.DrawLine(p, p + up * 100);
                Gizmos.DrawLine(p, p + right * 100);
                Gizmos.matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(fwd), new Vector3(1, 1, 0));

                float w = curve.GetWidthFromValueAlongCurve(t);
                float wOuter = w * outerRange;
                //Gizmos.DrawWireSphere(Vector3.zero, w);



                Gizmos.matrix = Matrix4x4.identity;

                Vector3 fPos = Vector3.zero;

                int angles = 10;
                int radiuses = 10;
                float debugDirectionMultiplier = .4f;

                // Outer 
                for (int j = 0; j < angles; j++)
                {
                    for (int k = 0; k < radiuses; k++)
                    {
                        float angle = (float)j / (float)angles;
                        float radius = ((float)k + .01f) / (float)radiuses;

                        radius = Mathf.Pow(radius, .5f);

                        angle *= 6.28f;
                        radius *= w;
                        radius *= outerRange;

                        fPos = p - right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;

                        CalculateCurveInfo(fPos);
                        CalculateForces(fPos);

                        //Gizmos.color = Color.green;
                        //   Gizmos.DrawLine(fPos, fPos + finalInwardForce * debugDirectionMultiplier);
                        //Gizmos.color = Color.red;
                        // Gizmos.DrawLine(fPos, fPos + finalForwardForce * debugDirectionMultiplier);

                        Vector3 fdir = finalForwardForce + finalInwardForce;

                        Vector3 fDirNorm = fdir.normalized;

                        // Convert normalized velocity to color
                        Color colorFromVelocity = new Color(
                            (fDirNorm.x + 1) * 0.5f, // Scale and shift X component
                            (fDirNorm.y + 1) * 0.5f, // Scale and shift Y component
                            (fDirNorm.z + 1) * 0.5f  // Scale and shift Z component
                        );
                        Gizmos.color = colorFromVelocity;
                        Gizmos.DrawLine(fPos, fPos + (fdir) * debugDirectionMultiplier);

                        //Gizmos.DrawLine(fPos, fPos + Vector3.one * 20);


                    }

                }



                Gizmos.matrix = Matrix4x4.identity;
            }

        }


    }




}
