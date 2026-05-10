using UnityEngine;

// Butterfly behavior is now configured entirely through PreyConfigSO modules:
//   noise ON  — perlin flutter
//   perch ON  — set manager.anchorPoint for a wander center
//   altitude ON, turning ON — standard flight
[CreateAssetMenu( fileName = "ButterflyConfigSO" , menuName = "Prey/ButterflyConfigSO" , order = 4 )]
public class ButterflyConfigSO : PreyConfigSO { }
