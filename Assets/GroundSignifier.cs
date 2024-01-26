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

    public float wingKickUpHeight = 10;


    public LineRenderer lineRenderer;

    public float lineRendererWidthMultiplier = .3f;

    public Transform groundRepresent;
    public float groundRepresentOffset;
    public float groundRepresentSizeMultiplier = 3;


    ParticleSystem.EmitParams emitParams;
    // Start is called before the first frame update
    void Start()
    {

        emitParams = new ParticleSystem.EmitParams();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 emitPos;
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


                float nDistToGround = -(God.wren.physics.rawDistToGround - God.wren.physics.furthestHeight) / God.wren.physics.furthestHeight;


                if (lineRenderer)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.positionCount = 3;


                    lineRenderer.widthMultiplier = nDistToGround * lineRendererWidthMultiplier;



                    Vector3 startPos = God.wren.physics.rb.position + God.wren.transform.forward * .5f;
                    Vector3 endPos = God.wren.physics.straightDownIntersectionPosition;

                    startPos = startPos - (startPos - endPos) * .2f;
                    lineRenderer.SetPosition(0, startPos);
                    lineRenderer.SetPosition(1, startPos - (startPos - endPos) * .3f);
                    lineRenderer.SetPosition(2, endPos);
                }

                if (groundRepresent)
                {

                    // Vector3 targetPosition = God.wren.transform.position;
                    //targetPosition -= God.wren.physics.straightDownIntersectionNormal * groundRepresentOffset;
                    groundRepresent.position = God.wren.physics.straightDownIntersectionPosition;
                    //groundRepresent.position = God.wren.transform.position;// * groundRepresentOffset;
                    groundRepresent.position += God.wren.physics.straightDownIntersectionNormal * groundRepresentOffset;

                    // groundRepresent.position = Vector3.Lerp(groundRepresent.position, targetPosition, .1f);
                    groundRepresent.rotation = Quaternion.Slerp(groundRepresent.rotation, Quaternion.FromToRotation(Vector3.up, God.wren.physics.straightDownIntersectionNormal), .1f);



                    groundRepresent.localScale = Vector3.one * Mathf.Min(nDistToGround, 1 - nDistToGround) * groundRepresentSizeMultiplier;
                    //transform.rotation = Quaternion.LookRotation(God.wren.physics.straightDownIntersectionNormal, Vector3.forward);
                }



            }
            else
            {
                if (lineRenderer)
                {
                    lineRenderer.enabled = false;
                }
                if (groundRepresent)
                {
                    groundRepresent.localScale = Vector3.zero;
                }


                if (God.wren.physics.straightDownDistance < kickUpDustHeight)
                {

                    emitPos = God.wren.physics.straightDownIntersectionPosition;
                    emitParams.startSize = Mathf.Clamp(kickUpDustHeight / God.wren.physics.straightDownDistance, 0, 2);
                    for (int i = 0; i < (int)emitParams.startSize; i++)
                    {



                        // TODO how to make it be on the ground?
                        emitPos = God.wren.physics.straightDownIntersectionPosition + Random.insideUnitSphere * .4f;
                        emitPos += God.wren.transform.forward * God.wren.physics.vel.magnitude * .3f;
                        emitPos.y = God.wren.physics.straightDownIntersectionPosition.y;
                        emitParams.position = emitPos;

                        kickUpDustParticles.Emit(emitParams, 1);
                    }

                }

                // get delta of the wing
                float leftWingDif = (God.wren.physics.leftWing.position.y - God.wren.physics.rb.position.y);



                if (leftWingDif < 0 && God.wren.physics.straightDownDistance < wingKickUpHeight)
                {

                    emitPos = God.wren.physics.leftWing.position;
                    //emitPos += God.wren.transform.forward * God.wren.physics.vel.magnitude * .3f;
                    emitPos.y = God.wren.physics.straightDownIntersectionPosition.y;
                    leftWingParticles.transform.position = emitPos;
                    leftWingParticles.Emit((int)(-leftWingDif * 1 * (wingKickUpHeight / God.wren.physics.straightDownDistance)));
                }



                // get delta of the wing
                float rightWingDif = (God.wren.physics.rightWing.position.y - God.wren.physics.rb.position.y);

                if (rightWingDif < 0 && God.wren.physics.straightDownDistance < wingKickUpHeight)
                {
                    emitPos = God.wren.physics.rightWing.position;
                    //emitPos += God.wren.transform.forward * God.wren.physics.vel.magnitude * .3f;
                    emitPos.y = God.wren.physics.straightDownIntersectionPosition.y;
                    rightWingParticles.transform.position = emitPos;
                    rightWingParticles.Emit((int)(-rightWingDif * 1 * (wingKickUpHeight / God.wren.physics.straightDownDistance)));
                }







            }

        }
    }
}
