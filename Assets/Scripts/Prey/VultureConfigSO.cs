using UnityEngine;

// Vulture behavior is now configured through PreyConfigSO modules:
//   thermal ON — soar/thermal orbit driven by altitude; set manager.thermalCenter in scene
//   circle ON  — provides circleRadius/circleForce used by the thermal orbit
//   perch ON   — optional; set manager.perchPoints for landing on terrain
//   takeOff ON — burst after leaving perch
//   altitude ON — general altitude seeking at high values
[CreateAssetMenu( fileName = "VultureConfigSO" , menuName = "Prey/VultureConfigSO" , order = 3 )]
public class VultureConfigSO : PreyConfigSO { }
