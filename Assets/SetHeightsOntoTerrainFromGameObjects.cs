using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(SetHeightsOntoTerrainFromGameObjects))]
public class SetHeightsOntoTerrainFromGameObjectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SetHeightsOntoTerrainFromGameObjects script = (SetHeightsOntoTerrainFromGameObjects)target;
        if (GUILayout.Button("Modify Terrain"))
        {
            script.ModifyTerrain();
        }

        if (GUILayout.Button("Blur Terrain"))
        {
            script.BlurTerrain();
        }
        if (GUILayout.Button("SET TO ZERO"))
        {
            script.SetToZero();
        }


    }
}
#endif


[ExecuteAlways]
public class SetHeightsOntoTerrainFromGameObjects : MonoBehaviour
{

    
#if UNITY_EDITOR

    public string intersectionLayer = "Ground";
    public GameObject targetObject; // Assign a game object to modify the terrain around
    public float heightIncrease = 100f; // Amount to increase the height
    public Terrain terrain;


    public float brushRadius = 10f;
    public float brushStrength = 0.01f;
    public Vector3 center;




    void Start()
    {
        /*if (targetObject != null)
        {
            ModifyTerrain();
        }*/
    }

    /*public void ModifyTerrain()
    {
        if (targetObject != null)
        {
            ModifyTerrain(targetObject, heightIncrease);
        }
        else
        {
            Debug.LogWarning("Target object is not assigned.");
        }
    }*/


    public void ModifyTerrain()
    {

        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Modify Terrain");
        /*
                if (terrain != null)
                {
                    print("modifying);");
                    TerrainData terrainData = terrain.terrainData;
                    int terrainWidth = terrainData.heightmapResolution;
                    int terrainHeight = terrainData.heightmapResolution;

                    // Calculate the normalized position on the terrain
                    float normalizedX = target.transform.position.x / terrainData.size.x;
                    float normalizedZ = target.transform.position.z / terrainData.size.z;

                    // Convert normalized position to terrain coordinates
                    int x = Mathf.RoundToInt(normalizedX * terrainWidth);
                    int y = Mathf.RoundToInt(normalizedZ * terrainHeight);

                    // Get the current height values
                    float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

                    // Modify the height at the target's position
                    heights[x, y] += height;

                    // Set the new height values back to the terrain
                    terrainData.SetHeights(0, 0, heights);
                }*/


        var data = terrain.terrainData;
       // var pos = targetObject.transform.position - terrain.transform.position;
        int heightmapWidth = data.heightmapResolution;
        int heightmapHeight = data.heightmapResolution;
        Vector3 size = data.size;

        Vector3 worldPosition;
        RaycastHit hit;
        int layerMask = LayerMask.GetMask(intersectionLayer);

        float[,] heights = data.GetHeights(0, 0, heightmapWidth, heightmapHeight);
        for (int i = 0; i < heightmapWidth; i++)
        {
            for (int j = 0; j < heightmapHeight; j++)
            {
                float height = data.GetHeight(i, j);

                worldPosition = new Vector3(i * size.x / heightmapWidth, size.y, j * size.z / heightmapHeight);

                if (Physics.Raycast(worldPosition + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, layerMask))
                {

                    height = hit.point.y / size.y;

                    height = Mathf.Clamp( height , 0 , 1 );
                }

                // height += heightIncrease;
                //  height = 0;
                heights[j, i] = height;
                //}
            }

        }

        /*int xBase = Mathf.RoundToInt((pos.x / data.size.x) * heightmapWidth);
        int yBase = Mathf.RoundToInt((pos.z / data.size.z) * heightmapHeight);
        int radius = Mathf.RoundToInt((brushRadius / data.size.x) * heightmapWidth);

        int xStart = Mathf.Clamp(xBase - radius, 0, heightmapWidth);
        int yStart = Mathf.Clamp(yBase - radius, 0, heightmapHeight);
        int xEnd = Mathf.Clamp(xBase + radius, 0, heightmapWidth);
        int yEnd = Mathf.Clamp(yBase + radius, 0, heightmapHeight);

        int width = xEnd - xStart;
        int height = yEnd - yStart;

        float[,] heights = data.GetHeights(xStart, yStart, width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float dx = x - (xBase - xStart);
                float dy = y - (yBase - yStart);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float strength = Mathf.Clamp01(1f - (dist / radius));
                heights[y, x] += brushStrength * strength;
            }*/

        data.SetHeights(0, 0, heights);
        EditorUtility.SetDirty(terrain);

    }

    public void SetToZero()
    {
        if (terrain != null)
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Zero Terrain");
            TerrainData terrainData = terrain.terrainData;
            int terrainWidth = terrainData.heightmapResolution;
            int terrainHeight = terrainData.heightmapResolution;

            // Get the current height values
            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

            // Set all height values to zero
            for (int i = 0; i < terrainWidth; i++)
            {
                for (int j = 0; j < terrainHeight; j++)
                {
                    heights[i, j] = 0f;
                }
            }

            // Set the new height values back to the terrain
            terrainData.SetHeights(0, 0, heights);
        }
    }
    
    public int blurSamples    = 4;
    public int blurMultiplier = 1;
    
    
    public void BlurTerrain(){
        
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Blur Terrain");
        TerrainData terrainData = terrain.terrainData;
        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;

        // Get the current height values
        float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);
        float[,] blurredHeights = new float[terrainWidth, terrainHeight];

        // Apply a simple blur effect
        for (int i = 1; i < terrainWidth - 1; i++)
        {
            for (int j = 1; j < terrainHeight - 1; j++)
            {
                
                float sum = 0f;
                int count = 0;
                for (int oy = -blurSamples; oy <= blurSamples; oy++)
                for (int ox = -blurSamples; ox <= blurSamples; ox++)
                {
                    int sx = Mathf.Clamp(i + ox * blurMultiplier, 0, terrainWidth - 1);
                    int sy = Mathf.Clamp(j + oy* blurMultiplier, 0, terrainHeight - 1);
                    sum += heights[sx, sy];
                    
                    count++;
                }
                
                
                
                
                 sum /= count;
                
                blurredHeights[i,j] =sum;
            }
        }

        // Set the new height values back to the terrain
        terrainData.SetHeights(0, 0, blurredHeights);
    }

        


#endif

}