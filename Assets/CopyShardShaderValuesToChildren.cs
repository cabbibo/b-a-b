using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CopyShardShaderValuesToChildren : MonoBehaviour
{

    public ClickPlacer clickPlacer;
    public ShardShaderValues shardShaderValues;
    public void OnEnable()
    {

        for (int i = 0; i < clickPlacer.placedGameObjects.Count; i++)
        {
            print(clickPlacer.placedGameObjects.Count);
            var shard = clickPlacer.placedGameObjects[i].GetComponent<ShardShaderValues>();
            if (shard != null)
            {
                shard.hueStart = shardShaderValues.hueStart;
                shard.hueSize = shardShaderValues.hueSize;
                shard.noiseSpeed = shardShaderValues.noiseSpeed;
                shard.noiseSize = shardShaderValues.noiseSize;
                shard.saturation = shardShaderValues.saturation;
                shard.lightness = shardShaderValues.lightness;
                shard.contrast = shardShaderValues.contrast;
                shard._ColorMultiplier = shardShaderValues._ColorMultiplier;
                shard._CenterOrbFalloff = shardShaderValues._CenterOrbFalloff;
                shard._CenterOrbFalloffSharpness = shardShaderValues._CenterOrbFalloffSharpness;
                shard._CenterOrbImportance = shardShaderValues._CenterOrbImportance;

            }
        }

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
