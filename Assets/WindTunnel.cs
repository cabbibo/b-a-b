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
    public AnimationCurve inwardsForceOuter = AnimationCurve.Linear(0,0,1,1);
    public AnimationCurve inwardsForceInner = AnimationCurve.Linear(0,1,1,1);
    public AnimationCurve forwardsForceOuter = AnimationCurve.Linear(0,0,1,1);
    public AnimationCurve forwardsForceInner = AnimationCurve.Linear(0,1,1,1);
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
    void Update()
    {

        Vector3 fPos = Vector3.zero;
        if (debugTransform) fPos = debugTransform.position;
        if (God.wren)
        {
            fPos = God.wren.transform.position;
        }

        Vector3 curveInfo = curve.GetClosestPointData(fPos);

        float t = Mathf.Clamp(curveInfo.z, 0, 1);

        Vector3 closestPos = Vector3.Lerp(curve.bakedPoints[(int)curveInfo.x], curve.bakedPoints[(int)curveInfo.y], t);

        Vector3 inDir = (closestPos - fPos).normalized;
        float dist = Vector3.Distance(closestPos, fPos);

        Vector3 closestForward = Vector3.Lerp(curve.bakedDirections[(int)curveInfo.x], curve.bakedDirections[(int)curveInfo.y], t);

        if (debug)
        {
            lineRenderer.SetPosition(0, fPos);
            lineRenderer.SetPosition(1, closestPos);

            print(curveInfo);
            print(t);
            print(closestForward);
            print(inDir);
            print(dist);
        }
        else
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }


        if (dist > maxDistance)
        {
            return;
        }

        if (God.wren != null)
        {

            //  print(dist);
            //  print((1 / tunnelForceFalloff * dist));
            //  print(closestForward);

            // dont add forces before or at end of tunnel
            if (t != 0)
            {



                float w = curve.GetWidthFromValueAlongCurve(t);
                float wOuter = w * outerRange;
                float hw  = w/2f;
                
                float t_inner = Mathf.Clamp01(dist / w);
                float t_outer = Mathf.Clamp01((dist - w) / wOuter);

                float force_in = 0;
                if (t_inner < 1) force_in += inwardsForceInner.Evaluate(t_inner);
                if (t_outer > 0 && t_inner >= 1) force_in += inwardsForceOuter.Evaluate(t_outer);
                force_in *= tunnelInForce;

                float force_fwd = 0;
                if (t_inner < 1) force_fwd += forwardsForceInner.Evaluate(t_inner);
                if (t_outer > 0 && t_inner >= 1) force_fwd += forwardsForceOuter.Evaluate(t_outer);
                force_fwd *= tunnelForwardForce;
                
                var force_dampen = 0f;
                force_dampen += dampenForce * (1-t_outer);
                force_dampen += dampenForce * (1-t_inner);
                
                var trigger_force = 1 + Mathf.Max(God.input.l2, God.input.r2);
                force_in *= trigger_force * 1.25f;
                force_fwd *= trigger_force * .9f;
                force_dampen *= trigger_force * .25f;

                if (easeFwdForce > 0)
                {
                    // force_fwd = Mathf.SmoothDamp(_last_force_fwd, force_fwd, ref _last_force_fwd_v, easeFwdForce * Time.deltaTime);
                    force_fwd = Mathf.Max(force_fwd, Mathf.Lerp(_last_force_fwd, force_fwd, easeFwdForce * Time.deltaTime));
                    force_in = Mathf.Max(force_in, Mathf.Lerp(_last_force_in, force_in, easeFwdForce * Time.deltaTime));
                    // force_in = Mathf.SmoothDamp(_last_force_in, force_in, ref _last_force_in_v, easeFwdForce * Time.deltaTime);
                }

                #if UNITY_EDITOR
                if (UnityEditor.Selection.Contains(gameObject))
                {
                    print($"T: {t} in: {t_inner} out: {t_outer} dist: {dist} width: {w} force_in:{force_in} force_fwd:{force_fwd}");
                }
                #endif

                var inf = inDir * force_in;
                var fwd = closestForward * force_fwd;
                var dampen = -God.wren.physics.vel * force_dampen;
                God.wren.physics.rb.AddForce(inf);
                God.wren.physics.rb.AddForce(fwd);
                God.wren.physics.rb.AddForce(dampen);
                Debug.DrawLine(fPos, fPos + inf, Color.red);
                Debug.DrawLine(fPos, fPos + fwd, Color.green);
                Debug.DrawLine(fPos, fPos + dampen, Color.yellow);
                // God.wren.physics.rb.AddForce(closestForward * tunnelForwardForce * (1 / tunnelForceFalloff * dist));
                // God.wren.physics.rb.AddForce(inDir * tunnelInForce * (1 / tunnelForceFalloff * dist));
                // God.wren.physics.rb.AddForce(-God.wren.physics.vel * dampenForce * (1 / tunnelForceFalloff * dist));

                _last_force_fwd = force_fwd;
                _last_force_in = force_in;
            }

        }
    }

    void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(debugTransform.position, 1);
        }

        Gizmos.color = Color.blue;
        for(int i = 0; i < 20; i++)
        {
            float t = i / 20f;
            Vector3 p = curve.GetPositionAlongPath(t);
            var r = curve.GetForwardFromLengthAlongCurve(t);
            Gizmos.matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(r), new Vector3(0,1,1));

            float w = curve.GetWidthFromValueAlongCurve(t);
            float wOuter = w * outerRange;
            Gizmos.DrawWireSphere(Vector3.zero, w);

            // Outer 
            for(int j = 1; j < 15; j++)
            {
                var to = j/15f;
                var l = Mathf.Lerp(w, wOuter, to);
                var f = inwardsForceOuter.Evaluate(to);
                Gizmos.color = new Color(f,f-1,f-1,Mathf.Lerp(.2f, 1, f));
                Gizmos.DrawWireSphere(Vector3.zero, l);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(Vector3.zero, Vector3.right * l);
            }

            // Inner 
            for(int j = 1; j < 20; j++)
            {
                var to = j/20f;
                var l = Mathf.Lerp(w, 0, to);
                var f = inwardsForceInner.Evaluate(to);
                Gizmos.color = new Color(f,f-1,f-1,Mathf.Lerp(.2f, 1, f));
                Gizmos.DrawWireSphere(Vector3.zero, l);
            }
        }
        Gizmos.matrix = Matrix4x4.identity;

    }




}
