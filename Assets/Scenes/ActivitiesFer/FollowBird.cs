using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(FollowBird))]
public class FollowBirdEditor : Editor
{

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FollowBird myScript = (FollowBird)target;
        if (GUILayout.Button("Reset"))
        {
            // myScript.ResetParticles();
        }
        var s = SceneView.lastActiveSceneView.sceneViewState;
        s.alwaysRefresh = true;
        SceneView.lastActiveSceneView.sceneViewState = s;
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        FollowBird myScript = (FollowBird)target;
        Handles.color = Color.red;
        // Handles.DrawWireDisc(myScript.PlayerPosition, Vector3.up, myScript.playerPushRadius);
    }
}
#endif

[ExecuteAlways()]
public class FollowBird : MonoBehaviour
{
    public Transform player;

    public float startMovingRadius = 15;
    public float stopMovingRadius = 30;

    public Curve curve;
    public Vector3[] positions;
    float _curveTime = 0;
    Vector3 _lastCurvePosition;
    float _lastTouchTime;

    bool _moving = true;

    void Start()
    {
        _lastCurvePosition = curve.GetPositionAlongPath(_curveTime);
        transform.position = _lastCurvePosition;

        _lastTouchTime = Time.time;
        _moving = true;
    }

    void Update()
    {
        var dtp = Vector3.Distance(player.position, transform.position);
        bool playerTouching = dtp < startMovingRadius;

        float moveSpeed = 0.06f;
        if (playerTouching)
        {
            _lastTouchTime = Time.time;
            moveSpeed *= 3;
        }
        
        if (_moving)
        {
            _curveTime += Time.deltaTime * moveSpeed;
            _lastCurvePosition = curve.GetPositionAlongPath(_curveTime);

            if (Time.time - _lastTouchTime > 11)
                _moving = false;
        }
        else
        {
            if (dtp < startMovingRadius)
                _moving = true;
        }
        var wiggle = new Vector3(
            Mathf.Sin(Time.time * 20) * 0.2f,
            Mathf.Sin(Time.time * 30) * 0.2f,
            Mathf.Sin(Time.time * 24) * 0.2f
        );
        transform.position = _lastCurvePosition + wiggle;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,1,1);
        Gizmos.DrawWireSphere(transform.position, startMovingRadius);
        Gizmos.DrawWireSphere(transform.position, stopMovingRadius);
    }
}
