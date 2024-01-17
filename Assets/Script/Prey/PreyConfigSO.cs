using UnityEngine;

[CreateAssetMenu(fileName = "PreyConfigSO", menuName = "Prey/PreyConfigSO", order = 1)]
public class PreyConfigSO : ScriptableObject
{

    public float speed;
    public float drag;
    public float life;

    public float dieRate;

    public float maxSpeed;
    public float maxScale;

    public float oscilationSize;
    public float oscilationSpeed;
    public float cageCutOff;
    public float cagePushBack;
    public float cageCurl;
    public float runForce;
    public float runRadius;

}