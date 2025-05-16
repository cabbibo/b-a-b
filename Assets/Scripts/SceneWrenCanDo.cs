using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class SceneWrenCanDo : MonoBehaviour
{
    public bool hover;
    public bool boost;
    public bool ping;
    public bool disintegrate;

    public bool call;
    public bool magnetize;
    public bool placeBeacon;
    public bool rewind;

    public bool carry;

    public bool hasDoneTutorial;

    public bool hardSet;


    // Start is called before the first frame update
    public void Set()
    {


        if ( hardSet ) {

            God.wrenCanDo.hover = hover;
            God.wrenCanDo.boost = boost;
            God.wrenCanDo.ping = ping;
            God.wrenCanDo.disintegrate = disintegrate;

            God.wrenCanDo.call = call;
            God.wrenCanDo.magnetize = magnetize;
            God.wrenCanDo.placeBeacon = placeBeacon;
            God.wrenCanDo.rewind = rewind;

            God.wrenCanDo.carry = carry;

        } else {
            if ( hover ) {
                God.wrenCanDo.hover = true;
            }

            if ( boost ) {
                God.wrenCanDo.boost = true;
            }

            if ( ping ) {
                God.wrenCanDo.ping = true;
            }

            if ( disintegrate ) {
                God.wrenCanDo.disintegrate = true;
            }

            if ( call ) {
                God.wrenCanDo.call = true;
            }

            if ( magnetize ) {
                God.wrenCanDo.magnetize = true;
            }

            if ( placeBeacon ) {
                God.wrenCanDo.placeBeacon = true;
            }

            if ( rewind ) {
                God.wrenCanDo.rewind = true;
            }

            if ( carry ) {
                God.wrenCanDo.carry = true;
            }
        }

    }

    // Update is called once per frame
    private void Update()
    {

    }
}