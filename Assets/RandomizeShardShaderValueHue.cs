using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class RandomizeShardShaderValueHue : MonoBehaviour
{

    public ShardShaderValues shardShaderValues; // Reference to the ShardShaderValues scriptable object
    // Start is called before the first frame update

    public float hueChangeSpeed;


public void OnEnable(){
    shardShaderValues.hueStart = Random.Range(0.0f, 1.0f); // Randomize the hue value between 0 and 1
}

    // Update is called once per frame
    void Update()
    {
        
        shardShaderValues.hueStart += Time.deltaTime * hueChangeSpeed; // Increment the hue value over time
    }
}
