using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WrenUtils
{
    public class WindZone : MonoBehaviour
    {

        public float windPower;



        // Start is called before the first frame update
        void OnEnable()
        {



        }
        public bool wrenInside;

        // Update is called once per frame
        void Update()
        {

            if (wrenInside)
            {
                if (!God.wren.state.onGround)
                {
                    God.wren.physics.AddForce(transform.forward * windPower, God.wren.transform.position);
                }
            }
        }

        void OnTriggerEnter(Collider c)
        {

            if (God.IsOurWren(c))
            {
                wrenInside = true;
            }


        }

        void OnTriggerExit(Collider c)
        {
            if (God.IsOurWren(c))
            {
                wrenInside = false;
            }
        }



    }
}