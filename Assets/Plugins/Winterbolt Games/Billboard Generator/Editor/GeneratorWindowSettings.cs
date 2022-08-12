using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal static class GeneratorWindowSettings
	{
		private static string GetSettingsFilePath()
		{
			return Path.Combine(Environment.CurrentDirectory, "billboard_generator_references.txt");
		}

		public static void Save(IEnumerable<GeneratorWindowEntry> entries)
		{
			using FileStream stream = File.Create(GetSettingsFilePath());
			
			using StreamWriter writer = new StreamWriter(stream);

			foreach (GeneratorWindowEntry entry in entries)
			{
				writer.WriteLine($"{entry.gameObject.GetInstanceID()},{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.settingsAsset))}");
			}
		}

		public static List<GeneratorWindowEntry> Load()
		{
			List<GeneratorWindowEntry> entries = new List<GeneratorWindowEntry>();

			if (!File.Exists(GetSettingsFilePath()))
			{
				return entries;
			}

			using (FileStream stream = File.OpenRead(GetSettingsFilePath()))
			{
				using StreamReader reader = new StreamReader(stream);
				
				string line;

				while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
				{
					string[] data = line.Split(',');

					GameObject gameObject = EditorUtility.InstanceIDToObject(int.Parse(data[0])) as GameObject;

					string guid = data[1];

					GeneratorSettingsAsset settingsAsset = null;

					if (!string.IsNullOrWhiteSpace(guid))
					{
						settingsAsset = AssetDatabase.LoadAssetAtPath<GeneratorSettingsAsset>(AssetDatabase.GUIDToAssetPath(guid));
					}

					entries.Add(new GeneratorWindowEntry { gameObject = gameObject, settingsAsset = settingsAsset, });
				}
			}

			return entries;
		}
	}
}
