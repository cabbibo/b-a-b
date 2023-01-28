// Project: WorldCreatorBridge
// Filename: UnityTerrainUtility.cs
// Copyright (c) 2022 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR

namespace BtB.WC.Bridge
{
    public static class UnityTerrainUtility
    {
        #region Methods (Static / Public)

        public static void CreateTerrainFromFile(BridgeSettings settings)
        {
            string dir = Path.GetDirectoryName(settings.bridgeFilePath);
            string terrainDirectory = "Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + "/";
            string assetsDirectory = terrainDirectory + "Assets/";
            
            GameObject newTerrainGameObject;

            Vector3 terrainPos = Vector3.zero;
            float realLength, realHeight;
            int xParts, yParts;
            int realResX, realResY;
            int splitRes = settings.SplitResolution;
            int width, length;
            Terrain[,] parts;

            // Load sync file
            XmlDocument doc = new XmlDocument();
            doc.Load(settings.bridgeFilePath);

            // Load surface
            XmlNodeList surfaceElements = doc.GetElementsByTagName("Surface");
            if (surfaceElements.Count > 0)
            {
                XmlNode surface = surfaceElements[0];

                int xBase, yBase, heightMapRes, alphaMapRes;
                float height, minHeight, maxHeight, heightCenter;

                int.TryParse(surface.Attributes["ResolutionX"].Value, out xBase);
                int.TryParse(surface.Attributes["ResolutionY"].Value, out yBase);
                int.TryParse(surface.Attributes["Width"].Value, out width);
                int.TryParse(surface.Attributes["Length"].Value, out length);
                float.TryParse(surface.Attributes["Height"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out height);
                float.TryParse(surface.Attributes["MinHeight"].Value, NumberStyles.Any, CultureInfo.InvariantCulture,  out minHeight);
                float.TryParse(surface.Attributes["MaxHeight"].Value, NumberStyles.Any, CultureInfo.InvariantCulture,  out maxHeight);
                float.TryParse(surface.Attributes["HeightCenter"].Value, NumberStyles.Any, CultureInfo.InvariantCulture,  out heightCenter);

                realResX = xBase;
                realResY = yBase;
                int maxRes = Mathf.Max(xBase, yBase);
                int nextP2 = Mathf.NextPowerOfTwo(maxRes);
                if (nextP2 < splitRes)
                    splitRes = nextP2;

                xParts = Mathf.CeilToInt((float) xBase / splitRes);
                yParts = Mathf.CeilToInt((float) yBase / splitRes);

                parts = new Terrain[xParts, yParts];

                int resPow2 = Mathf.CeilToInt((float) maxRes / splitRes) * splitRes;
                splitRes = Mathf.Min(resPow2, splitRes);

                int baseRes = Mathf.Max(yBase, xBase);
                length = Mathf.FloorToInt(length * ((float) resPow2 / baseRes));
                width = Mathf.FloorToInt(width * ((float) resPow2 / baseRes));
                heightMapRes = alphaMapRes = splitRes;

                heightMapRes += 1;

                float[,] splitHeightMap = new float[heightMapRes, heightMapRes];

                float splitLength = (float) length / yParts;
                float splitWidth = (float) width / xParts;
                realLength = Mathf.Max(splitLength, splitWidth);
                float heightSize = maxHeight - minHeight;

                // Load complete height data
                float[,] heightMap = Importer.RawUint16FromFile(dir + "/heightmap.raw", xBase, yBase, false);

                newTerrainGameObject = GameObject.Find(settings.TerrainAssetName);
                if (newTerrainGameObject != null)
                    GameObject.DestroyImmediate(newTerrainGameObject);
                newTerrainGameObject = new GameObject(settings.TerrainAssetName);

                for (int yP = 0; yP < yParts; yP++)
                {
                    for (int xP = 0; xP < xParts; xP++)
                    {
                        int xOff = xP * splitRes;
                        int yOff = yP * splitRes;

                        string assetPath = @"Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + @"/" + settings.TerrainAssetName + "_" + xP + "_" + yP + ".asset";
                        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);
                        GameObject terrainPartObject = null;

                        if (terrainData == null)
                        {
                            terrainData = new TerrainData();
                            terrainData.SetDetailResolution(splitRes, 16);
                            terrainData.name = "WCTerrainData" + "_" + xP + "_" + yP;
                            AssetDatabase.CreateAsset(terrainData, assetPath);
                        }

                        // Load height values
                        terrainData.heightmapResolution = heightMapRes;
                        terrainData.alphamapResolution = alphaMapRes;
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        // Fill missing values if terrain is not quadratic
                        if (xBase != yBase || xBase != heightMapRes)
                        {
                            {
                                for (int y = 0; y < heightMapRes; y++)
                                for (int x = 0; x < heightMapRes; x++)
                                {
                                    int realY = y + yOff;
                                    int realX = x + xOff;
                                    if (realY > yBase - 1 || realX > xBase - 1)
                                        splitHeightMap[y, x] = 0;
                                    else
                                        splitHeightMap[y, x] = heightMap[realY, realX];
                                }
                            }
                        }

                        terrainData.SetHeights(0, 0, splitHeightMap);
                        realHeight = height * heightSize;
                        terrainData.size = new Vector3(realLength, realHeight, realLength) * settings.WorldScale;
                        terrainPos = new Vector3(realLength * xP, height * minHeight, realLength * yP) * settings.WorldScale;

                        Transform terrainPartObjectTransform = newTerrainGameObject.transform.Find(settings.TerrainAssetName + "_" + xP + "_" + yP);
                        if (terrainPartObjectTransform == null)
                        {
                            terrainPartObject = Terrain.CreateTerrainGameObject(terrainData);
                            terrainPartObject.name = settings.TerrainAssetName + "_" + xP + "_" + yP;
                            terrainPartObject.transform.parent = newTerrainGameObject.transform;
                        }
                        else
                        {
                            terrainPartObject = terrainPartObjectTransform.gameObject;
                            terrainPartObject.GetComponent<Terrain>().terrainData = terrainData;
                        }

                        // set pixel error to 1 for crisp textures
                        terrainPartObject.GetComponent<Terrain>().heightmapPixelError = 1f;

                        Terrain terrain = terrainPartObject.GetComponent<Terrain>();
                        parts[xP, yP] = terrain;

                        // Set/Create Material
                        Material createdMat = null;
                        if (settings.MaterialType != MaterialType.Custom)
                        {
                            Material mat = AssetDatabase.LoadAssetAtPath("Assets/WorldCreatorBridge/Content/Materials/WC_Default_Terrain_" + settings.MaterialType + ".mat", typeof(Material)) as Material;
                            Texture2D tex = AssetDatabase.LoadAssetAtPath("Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + "/colormap.png", typeof(Texture2D)) as Texture2D;
                            string newAssetPath = "Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + $"/terrain_material_x{xP}_y{yP}.mat";

                            Material matInstance = new Material(mat);
                            switch (settings.MaterialType)
                            {
                                case MaterialType.HDRP:
                                    matInstance.EnableKeyword("HDRP_ENABLED");
                                    break;
                                case MaterialType.URP:
                                    matInstance.EnableKeyword("URP_ENABLED");
                                    break;
                            }

                            matInstance.SetTexture("_ColorMap", tex);
                            matInstance.SetVector("_OffsetSize", new Vector4((float) xP / xParts, (float) yP / yParts, 1f / xParts, 1f / yParts));
                            AssetDatabase.CreateAsset(matInstance, newAssetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();

                            createdMat = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Material)) as Material;
                            terrain.materialTemplate = createdMat;
                        }
                        else
                            terrain.materialTemplate = settings.CustomMaterial;
                        
                        terrainPartObject.transform.position = terrainPos;
                    }
                }

                for (int yP = 0; yP < yParts; yP++)
                for (int xP = 0; xP < xParts; xP++)
                {
                    Terrain left = xP > 0 ? parts[xP - 1, yP] : null;
                    Terrain right = xP < xParts - 1 ? parts[xP + 1, yP] : null;
                    Terrain top = yP > 0 ? parts[xP, yP - 1] : null;
                    Terrain bottom = yP < yParts - 1 ? parts[xP, yP + 1] : null;

                    parts[xP, yP].SetNeighbors(left, bottom, right, top);
                }
            }
            else return;

            // Load Splatmaps
            if (settings.IsImportTextures)
            {
                XmlNodeList texturingElements = doc.GetElementsByTagName("Texturing");
                if (texturingElements.Count > 0)
                {
                    XmlNode texturing = texturingElements[0];
                    int textureCount = 0;
                    List<TerrainLayer> splatPrototypes = new List<TerrainLayer>();
                    foreach (XmlElement splatmap in texturing)
                    {
                        foreach (XmlElement textureInfo in splatmap.ChildNodes)
                        {
                            Vector2 tileSize = Vector2FromString(textureInfo.Attributes["TileSize"].Value);
                            Vector2 tileOffset = Vector2FromString(textureInfo.Attributes["TileOffset"].Value);
                            
                            string smoothnessString = textureInfo.Attributes["Smoothness"].Value;
                            string metallicString = textureInfo.Attributes["Metallic"].Value;
                            float.TryParse(smoothnessString, NumberStyles.Any, CultureInfo.InvariantCulture, out var smoothness);
                            float.TryParse(metallicString, NumberStyles.Any, CultureInfo.InvariantCulture, out var metallic);

                            TerrainLayer tmp = new TerrainLayer
                            {
                                metallic = metallic,
                                smoothness = smoothness,
                                specular = Color.white,
                                tileOffset = tileOffset,
                                tileSize = new Vector2(width, length) / tileSize
                            };
                            
                            string assetProjectPath = "Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + "/Assets/";

                            // Albedo - if built-in rp try to pack the roughness map into the diffuse maps alpha channel
                            if (textureInfo.HasAttribute("AlbedoFile"))
                            {
                                string albedoPath = textureInfo.Attributes["AlbedoFile"].Value;
                                Texture2D diffuseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + albedoPath);
                                
                                // pack the roughness map into the diffuse maps alpha channel
                                if (settings.MaterialType == MaterialType.Standard && textureInfo.HasAttribute("RoughnessFile"))
                                {
                                    string roughnessPath = textureInfo.Attributes["RoughnessFile"].Value;
                                    Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + roughnessPath);

                                    if (diffuseTex.width == roughnessTex.width && diffuseTex.height == roughnessTex.height)
                                    {
                                        ComputeShader maskMapConversionCs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/WorldCreatorBridge/Content/Shaders/Compute/MaskMap.compute");

                                        int res = diffuseTex.width;
                                        maskMapConversionCs.SetInt("_Res", res);
                                        maskMapConversionCs.SetTexture(3, "_AlbedoTex", diffuseTex);
                                        maskMapConversionCs.SetTexture(3, "_RoughnessTex", roughnessTex);
                                        
                                        ComputeBuffer maskBuffer = new ComputeBuffer(res * res, 16);
                                        maskMapConversionCs.SetBuffer(3, "_OutBuffer", maskBuffer);
                                        maskMapConversionCs.Dispatch(3, res / 8, res / 8, 1);
                                    
                                        Color[] values = new Color[res * res]; 
                                        maskBuffer.GetData(values);
                                        diffuseTex = new Texture2D(res, res, TextureFormat.ARGB32, true);
                                        diffuseTex.SetPixels(values);
                                        diffuseTex.Apply(true);
                                    }
                                    else
                                    {
                                        Debug.Log("Roughness map was not applied, please make sure that the exported diffuse map and roughness map have the same resolution.");
                                    }
                                }

                                tmp.diffuseTexture = diffuseTex;
                            }
                            else
                            {
                                tmp.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WorldCreatorBridge/Content/Textures/albedo_default.jpg");
                                
                                if (settings.MaterialType == MaterialType.URP || settings.MaterialType == MaterialType.HDRP)
                                {
                                    string colorString = textureInfo.Attributes["Color"].Value;
                                    ColorUtility.TryParseHtmlString(colorString, out Color color);
                                }
                            }
                            
                            // Normal 
                            if (textureInfo.HasAttribute("NormalFile"))
                            {
                                string normalPath = textureInfo.Attributes["NormalFile"].Value;
                                tmp.normalMapTexture = ImportNormal(assetProjectPath + normalPath);
                            }
                            else
                                tmp.normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WorldCreatorBridge/Content/Textures/normal_default.jpg");
                            
                            // Mask map - only used in HDRP and URP terrain shaders, excluded for built-in rp
                            if (settings.MaterialType != MaterialType.Standard && (textureInfo.HasAttribute("AoFile") || textureInfo.HasAttribute("RoughnessFile")))
                            {
                                Texture2D maskMap = Texture2D.whiteTexture;
                                ComputeShader maskMapConversionCs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/WorldCreatorBridge/Content/Shaders/Compute/MaskMap.compute");

                                string aoPath = textureInfo.Attributes["AoFile"].Value;
                                string roughnessPath = textureInfo.Attributes["RoughnessFile"].Value;

                                if (!string.IsNullOrEmpty(aoPath) && !string.IsNullOrEmpty(roughnessPath))
                                {
                                    Texture2D aoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + aoPath);
                                    Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + roughnessPath);

                                    bool roughnessOnly = false;
                                    int res = roughnessTex.width;

                                    if (aoTex.width != roughnessTex.width)
                                    {
                                        roughnessOnly = true;
                                        Debug.Log("AO map has not been loaded into mask map. To do so, please make sure that both maps are of the same resolution.");
                                    }

                                    int kernel = roughnessOnly ? 2 : 0;
                                    maskMapConversionCs.SetInt("_Res", res);
                                    if (!roughnessOnly) maskMapConversionCs.SetTexture(0, "_AOTex", aoTex);
                                    maskMapConversionCs.SetTexture(kernel, "_RoughnessTex", roughnessTex);

                                    ComputeBuffer maskBuffer = new ComputeBuffer(res * res, 16);
                                    maskMapConversionCs.SetBuffer(kernel, "_OutBuffer", maskBuffer);
                                    maskMapConversionCs.Dispatch(kernel, res / 8, res / 8, 1);

                                    Color[] colors = new Color[res * res];
                                    maskBuffer.GetData(colors);
                                    maskMap = new Texture2D(res, res, TextureFormat.ARGB32, true);
                                    maskMap.SetPixels(colors);
                                    maskMap.Apply(true);
                                }
                                else if (!string.IsNullOrEmpty(aoPath))
                                {
                                    Texture2D aoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + aoPath);

                                    int res = aoTex.width;

                                    maskMapConversionCs.SetInt("_Res", res);
                                    maskMapConversionCs.SetTexture(1, "_AOTex", aoTex);

                                    ComputeBuffer maskBuffer = new ComputeBuffer(res * res, 16);
                                    maskMapConversionCs.SetBuffer(1, "_OutBuffer", maskBuffer);
                                    maskMapConversionCs.Dispatch(1, res / 8, res / 8, 1);

                                    Color[] colors = new Color[res * res];
                                    maskBuffer.GetData(colors);
                                    maskMap = new Texture2D(res, res, TextureFormat.ARGB32, true);
                                    maskMap.SetPixels(colors);
                                    maskMap.Apply(true);
                                }
                                else if (!string.IsNullOrEmpty(roughnessPath))
                                {
                                    Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetProjectPath + roughnessPath);

                                    int res = roughnessTex.width;

                                    maskMapConversionCs.SetInt("_Res", res);
                                    maskMapConversionCs.SetTexture(2, "_AOTex", roughnessTex);

                                    ComputeBuffer maskBuffer = new ComputeBuffer(res * res, 16);
                                    maskMapConversionCs.SetBuffer(2, "_OutBuffer", maskBuffer);
                                    maskMapConversionCs.Dispatch(2, res / 8, res / 8, 1);

                                    Color[] colors = new Color[res * res];
                                    maskBuffer.GetData(colors);
                                    maskMap = new Texture2D(res, res, TextureFormat.ARGB32, true);
                                    maskMap.SetPixels(colors);
                                    maskMap.Apply(true);
                                    tmp.maskMapTexture = maskMap;
                                }

                                tmp.maskMapTexture = maskMap;
                            }

                            AssetDatabase.CreateAsset(tmp, terrainDirectory + $"terrain_layer_{textureCount}.terrainlayer");
                            splatPrototypes.Add(tmp);
                            textureCount++;
                        }
                    }

                    settings.LayerWarning = false;

                    if ((settings.MaterialType == MaterialType.URP || settings.MaterialType == MaterialType.Standard) && textureCount > 4)
                        settings.LayerWarning = true;
                    else if (settings.MaterialType == MaterialType.HDRP && textureCount > 8)
                        settings.LayerWarning = true;

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    TerrainLayer[] splatProtoArray = splatPrototypes.ToArray();

                    float[,,] alphaMaps = new float[realResX, realResY, textureCount];

                    int textureIndex = 0;
                    foreach (XmlElement splatmap in texturing)
                    {
                        string fileName = splatmap.Attributes["Name"].Value;
                        int textureWidth, textureHeight;
                        Vector4[] pixels = ReadRGBA(dir + "/" + fileName, out textureWidth, out textureHeight);

                        int channelIndex = 0;
                        foreach (XmlElement textureInfo in splatmap.ChildNodes)
                        {
                            for (int y = 0; y < realResY; y++)
                            {
                                for (int x = 0; x < realResX; x++)
                                {
                                    int realY = Mathf.FloorToInt(y);
                                    int realX = (Mathf.FloorToInt(x)) - 1;

                                    realX = Mathf.Clamp(realX, 0, textureWidth - 1);
                                    realY = Mathf.Clamp(realY, 0, textureHeight - 1);

                                    int pixelIndex = realY * textureWidth + realX;
                                    switch (channelIndex)
                                    {
                                        case 2:
                                            alphaMaps[x, y, textureIndex] = pixels[pixelIndex].x;
                                            break;
                                        case 1:
                                            alphaMaps[x, y, textureIndex] = pixels[pixelIndex].y;
                                            break;
                                        case 0:
                                            alphaMaps[x, y, textureIndex] = pixels[pixelIndex].z;
                                            break;
                                        case 3:
                                            alphaMaps[x, y, textureIndex] = pixels[pixelIndex].w;
                                            break;
                                    }
                                }
                            }

                            channelIndex++;
                            textureIndex++;
                        }
                    }

                    int alphaMapRes = Mathf.Min(4096, splitRes);
                    float[,,] splitAlphaMaps = new float[alphaMapRes, alphaMapRes, textureCount];
                    for (int yP = 0; yP < yParts; yP++)
                    {
                        for (int xP = 0; xP < xParts; xP++)
                        {
                            TerrainData terrainData = parts[xP, yP].terrainData;
                            terrainData.terrainLayers = splatProtoArray;


                            int xOff = xP * splitRes;
                            int yOff = yP * splitRes;

                            float scale = splitRes <= 4096 ? 1 : (float) splitRes / alphaMapRes;

                            for (int y = 0; y < alphaMapRes; y++)
                            {
                                for (int x = 0; x < alphaMapRes; x++)
                                {
                                    int realY = yOff + Mathf.CeilToInt(y * ((float) (splitRes + 1) / splitRes) * scale);
                                    int realX = xOff + Mathf.CeilToInt(x * ((float) (splitRes + 1) / splitRes) * scale);
                                    
                                    if (realX >= realResX) realX = realResX - 1;
                                    if (realY >= realResY) realY = realResY - 1;

                                    for (int i = 0; i < textureCount; i++)
                                        splitAlphaMaps[y, x, i] = alphaMaps[realX, realY, i];
                                }
                            }

                            terrainData.SetAlphamaps(0, 0, splitAlphaMaps);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }
            
            // Load Details
            
            // Load Objects

            // Finish Terrain
            foreach (Terrain t in newTerrainGameObject.transform.GetComponentsInChildren<Terrain>())
                t.Flush();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        
        private static Texture2D ImportNormal(string path)
        {
            TextureImporter normalImporter = TextureImporter.GetAtPath(path) as TextureImporter;
            if (normalImporter != null)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        

        /// <summary>
        ///   Creates an instance of Vector3 from the specified string.
        /// </summary>
        /// <param name="value">A string that encodes a Vector3.</param>
        /// <returns>A new instance of Vector3.</returns>
        public static Vector2 Vector2FromString(string value)
        {
            var parts = value.Replace("(", "").Replace(")", "").Split(',');
            var v = new Vector2();
            try
            {
                v.x = float.Parse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                v.y = float.Parse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch
            {
                Debug.Log("Vector parse failed");
            }

            return v;
        }

        /// <summary>
        /// Loads a tga from the given filepath
        /// </summary>
        /// <param name="path"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Vector4[] ReadRGBA(string path, out int width, out int height)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    reader.BaseStream.Seek(12, SeekOrigin.Begin);
                    width = reader.ReadInt16();
                    height = reader.ReadInt16();
                    int bitDepth = reader.ReadByte();
                    reader.BaseStream.Seek(1, SeekOrigin.Current);

                    int size = width * height;
                    Vector4[] textureData = new Vector4[size];
                    const float invByte = 1.0f / 255.0f;
                    if (bitDepth == 32)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            textureData[i] = new Vector4(
                                reader.ReadByte() * invByte,
                                reader.ReadByte() * invByte,
                                reader.ReadByte() * invByte,
                                reader.ReadByte() * invByte);
                        }
                    }
                    else if (bitDepth == 24)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            textureData[i] = new Vector4(
                                reader.ReadByte() * invByte,
                                reader.ReadByte() * invByte,
                                reader.ReadByte() * invByte, 1);
                        }
                    }
                    else if (bitDepth == 8)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            float v = reader.ReadByte() * invByte;
                            textureData[i] = new Vector4(
                                v, 
                                v, 
                                v, 
                                1);
                        }
                    }
                    else
                        return null;

                    return textureData;
                }
            }
        }
        
        #endregion Methods (Static / Public)
    }
}

#endif