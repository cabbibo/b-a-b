using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WrenUtils
{
    public class WindZone : MonoBehaviour
    {

        public float windPower;

        public List<Wren> currentWrens;


        // Start is called before the first frame update
        void OnEnable()
        {

            currentWrens = new List<Wren>();

        }

        // Update is called once per frame
        void Update()
        {


            foreach (Wren w in currentWrens)
            {

                if (!w.state.onGround)
                {
                    w.physics.rb.AddForce(transform.forward * windPower);
                }
            }
        }


        void OnTriggerEnter(Collider c)
        {

            if (God.IsOurWren(c))
            {
                currentWrens.Add(c.attachedRigidbody.GetComponent<Wren>());
            }


        }

        void OnTriggerExit(Collider c)
        {
            if (God.IsOurWren(c))
            {

                currentWrens.Remove(c.attachedRigidbody.GetComponent<Wren>());

            }
        }



    }
}