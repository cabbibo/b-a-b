using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class GroundSignifier : MonoBehaviour
{


    public ParticleSystem particles;
    public ParticleSystem superCloseParticles;
    public ParticleSystem kickUpDustParticles;
    public ParticleSystem leftWingParticles;
    public ParticleSystem rightWingParticles;

    public float kickUpDustHeight = 10;



    ParticleSystem.EmitParams emitParams;
    // Start is called before the first frame update
    void Start()
    {

        emitParams = new ParticleSystem.EmitParams();
    }

    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {

            if (God.wren.physics.rawDistToGround < God.wren.physics.furthestHeight)
            {

                float v = (God.wren.physics.rawDistToGround - God.wren.physics.closestHeight) / (God.wren.physics.furthestHeight - God.wren.physics.closestHeight);
                float emitVal = Mathf.Lerp(0, 10, 1 - Mathf.Clamp(v, 0, 1));
                emitVal *= emitVal;
                emitVal *= .2f;



                particles.transform.position = God.wren.physics.rb.position;
                particles.Emit((int)emitVal);



                superCloseParticles.transform.position = God.wren.physics.rb.position;
                superCloseParticles.Emit((int)(emitVal * emitVal * .006f));





            }


            if (God.wren.physics.straightDownDistance < kickUpDustHeight)
            {

                emitParams.position = God.wren.physics.straightDownIntersectionPosition;
                emitParams.startSize = Mathf.Clamp(10 / God.wren.physics.straightDownDistance, 0, 5);
                for (int i = 0; i < 10; i++)
                {
                    emitParams.position = God.wren.physics.straightDownIntersectionPosition + Random.insideUnitSphere * .4f;
                    kickUpDustParticles.Emit(emitParams, 1);
                }

            }

          //  float leftWingDif = Mathf.Abs(God.wren.physics.leftWing.position.y - -God.wren.physics.leftWingAngleAtRest);
        }
        else
        {

        }

    }
}
