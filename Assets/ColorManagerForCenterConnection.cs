using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ColorManagerForCenterConnection : MonoBehaviour
{
    public ShardShaderValues[] rings;

    public LineRenderer[] connectors;
    public Shard[]        shards;

    public float[] hues;
    public float   lineColorChangeAmount = .1f;

    public Transform center;

    public void OnEnable()
    {

        for ( int i = 0; i < rings.Length; i++ ) {
            rings[i].hueStart = hues[i];
        }

        for ( int i = 0; i < shards.Length; i++ ) {
            shards[i].type = i;
        }

        for ( int i = 0; i < connectors.Length; i++ ) {
            connectors[i].startColor = Color.HSVToRGB( hues[i] , 1 , 1 );
            connectors[i].endColor = Color.HSVToRGB( hues[i] + lineColorChangeAmount , 1 , 1 );
        }

    }

    public void Update()
    {
        for ( int i = 0; i < rings.Length; i++ ) {
            rings[i].transform.LookAt( center.position );
            rings[i].transform.Rotate( new Vector3( 90 , 0 , 0 ) );
        }
    }
}