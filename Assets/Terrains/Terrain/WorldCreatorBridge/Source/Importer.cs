// Project: WorldCreatorBridge
// Filename: Importer.cs
// Copyright (c) 2022 BiteTheBytes GmbH. All rights reserved
// *********************************************************

#region Using

using System.IO;

#endregion

#if UNITY_EDITOR

namespace BtB.WC.Bridge
{
    public static class Importer
    {
        #region Methods (Static / Public)

        public static float[,] RawUint16FromFile(string filePath, int width, int height, bool bigEndian, int stride = 0,
            int headerSize = 0, bool flipY = false, bool flipX = false)
        {
            stride = stride == 0 ? width * 2 : stride;
            float[,] terrainData = new float[height, width];
            using (FileStream stream = File.OpenRead(filePath))
            {
                if (!bigEndian)
                {
                    for (int y = 0; y < height; y++)
                    {
                        stream.Position = y * stride + headerSize;
                        int readY = flipY ? (height - y) - 1 : y;
                        for (int x = 0; x < width; x++)
                        {
                            byte lower = (byte) stream.ReadByte();
                            byte upper = (byte) stream.ReadByte();
                            float val = (float) (lower | (upper << 8)) / ushort.MaxValue;
                            int readX = flipX ? (width - x) - 1 : x;

                            terrainData[readY, readX] = val;
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        stream.Position = y * stride + headerSize;
                        int readY = flipY ? (height - y) - 1 : y;
                        for (int x = 0; x < width; x++)
                        {
                            byte upper = (byte) stream.ReadByte();
                            byte lower = (byte) stream.ReadByte();
                            float val = (float) (lower | (upper << 8)) / ushort.MaxValue;
                            int readX = flipX ? (width - x) - 1 : x;

                            terrainData[readY, readX] = val;
                        }
                    }
                }
            }

            return terrainData;
        }

        #endregion
    }
}

#endif