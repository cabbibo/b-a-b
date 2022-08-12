using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal sealed class GeneratorWindowEntry
	{
		public bool isExpanded;

		public GameObject gameObject;

		public GeneratorSettingsAsset settingsAsset;

		public bool areSettingsExpanded;
	}
}
