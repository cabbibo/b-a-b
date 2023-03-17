using UnityEngine;

[CreateAssetMenu( fileName = "PreyConfigSO", menuName = "Prey/PreyConfigSO", order = 1 )]
public class PreyConfigSO : ScriptableObject
{

    public float speed;
    public float drag;
    public float life;

    public float dieRate;

    public float maxSpeed;
    public float maxScale;
}