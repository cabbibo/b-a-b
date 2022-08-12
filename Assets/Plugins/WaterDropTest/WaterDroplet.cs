using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using static Unity.Mathematics.math;
using Unity.Mathematics;


[ExecuteAlways]
public class WaterDroplet : MonoBehaviour
{


    public Renderer[] drops;

    private MaterialPropertyBlock mpb;

    Vector4[] dropInfo;


    public Vector3[] previousPositions;


    private Renderer renderer;
    // Start is called before the first frame update
    void OnEnable()
    {

        previousPositions = new Vector3[drops.Length+1];
        mpb = new MaterialPropertyBlock();
        dropInfo = new Vector4[ drops.Length];
        renderer = GetComponent<Renderer>();

    }

    Vector4 t;
    // Update is called once per frame
    void Update()
    {

        previousPositions[0] = transform.position;
        for( int i = previousPositions.Length-1; i > 0; i--){
            previousPositions[i] = previousPositions[i-1];
            drops[i-1].transform.position = previousPositions[i];
        }
        for( int i = 0; i < drops.Length; i++ ){
            dropInfo[i] = float4( drops[i].transform.position ,  drops[i].transform.lossyScale.x );
        }

        mpb.SetVector("_MainBall", float4( transform.position , transform.lossyScale.x));
        mpb.SetVectorArray("_Drops", dropInfo);
        mpb.SetInt("_NumDrops", dropInfo.Length);
        for( int i = 0; i < drops.Length; i++ ){
            drops[i].SetPropertyBlock(mpb);
        }

        renderer.SetPropertyBlock(mpb);

    }


}
