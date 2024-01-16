using UnityEngine;
using WrenUtils;



[ExecuteAlways]
public class PreyController : MonoBehaviour
{


    [SerializeField]
    Rigidbody thisRigidbody;

    float dieRate;
    float drag;
    float life;
    PreyManager manager;
    float maxScale;
    float maxSpeed;
    float oscilationSize;
    float speed;

    void Update()
    {

        if (God.wren)
        {
            Wren wren = God.ClosestWren(transform.position);



            Vector3 d = wren.transform.position - transform.position;

            thisRigidbody.AddForce(-d.normalized * Mathf.Clamp(speed * 1 / (.4f + d.magnitude * .02f), 0, maxSpeed));
            thisRigidbody.AddForce(d.normalized * speed * .1f);
            thisRigidbody.drag = drag;

            if (thisRigidbody.velocity.magnitude > 0)
            {
                thisRigidbody.rotation = Quaternion.RotateTowards(
                                thisRigidbody.rotation,
                                Quaternion.LookRotation(thisRigidbody.velocity, Vector3.up),
                                thisRigidbody.velocity.magnitude
                        );
            }



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

        speed = config.speed;
        drag = config.drag;
        life = config.life;

        dieRate = config.dieRate;

        maxSpeed = config.maxSpeed;
        maxScale = config.maxScale;
    }
}