using UnityEngine;

// Raven behavior is now configured through PreyConfigSO modules:
//   perch ON   — lands on manager.perchPoints, startleRadius triggers takeoff
//   takeOff ON — upward burst, then circles wren before finding new perch
//   circle ON  — orbit wren after taking off
//   flock ON   — social awareness of nearby ravens
// Set manager.perchPoints in the scene for landing targets.
[CreateAssetMenu( fileName = "RavenConfigSO" , menuName = "Prey/RavenConfigSO" , order = 2 )]
public class RavenConfigSO : PreyConfigSO { }
