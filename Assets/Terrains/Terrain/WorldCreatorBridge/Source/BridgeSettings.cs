// Project: WorldCreatorBridge
// Filename: BridgeSettings.cs
// Copyright (c) 2022 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using UnityEngine;

#if UNITY_EDITOR

namespace BtB.WC.Bridge
{
    [Serializable]
    public class BridgeSettings
    {
        public int SplitThreshold = 2;

        public string bridgeFilePath = "";
        public string TerrainsFolderName = "WorldCreatorTerrains";
        public string TerrainAssetName = "WC_Terrain";
        public bool DeleteUnusedAssets = true;
        
        public bool IsImportTextures = true;
        public bool LayerWarning = false;
        
        public float WorldScale = 1;
        public string WorldScaleString = "1.00";
        
        public MaterialType MaterialType = MaterialType.Standard;
        public Material CustomMaterial;

        public int SplitResolution => 128 << SplitThreshold;

        public BridgeSettings()
        {
            bridgeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/World Creator/Sync/bridge.xml";
        }

        public bool IsBridgeFileValid()
        {
            return !string.IsNullOrEmpty(bridgeFilePath);
        }
    }

    public enum MaterialType
    {
        Standard,
        HDRP,
        URP,
        Custom
    }
}

#endif