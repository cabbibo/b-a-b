using UnityEditor;
using UnityEngine;

public class CustomEditorWindow : EditorWindow
{
    Color backgroundColor;

    float displayScale;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/TerrainPaint")]
    public static void ShowWindow()
    {
        //Show existing window instance or create one if none exists
        GetWindow<CustomEditorWindow>("TerrainPaint");
    }

    // OnGUI is called for rendering and handling GUI events
    void OnGUI()
    {
        displayScale = EditorGUILayout.FloatField("DisplayScale", displayScale);

    }


    void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    private Vector3? hitPoint; // Stores the hit point. Nullable to know if ray hit something.

    void OnSceneGUI(SceneView sceneView)
    {


        RaycastFromMouse();

        if (hitPoint.HasValue)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, hitPoint.Value, Quaternion.identity, 10, EventType.Repaint);
        }

    }


    void RaycastFromMouse()
    {

        // Ensure we have a scene view
        if (SceneView.lastActiveSceneView == null) return;


        // Get the active scene view camera
        Camera sceneCam = SceneView.lastActiveSceneView.camera;

        // Convert mouse position to screen space position for the scene camera
        Vector2 mousePos = Event.current.mousePosition * displayScale;
        mousePos.y = sceneCam.pixelHeight - mousePos.y;
        Ray ray = sceneCam.ScreenPointToRay(mousePos);

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit))
        {


            hitPoint = hit.point;
            SceneView.RepaintAll(); // Request repaint to see gizmos immediately
        }
        else
        {
            hitPoint = null; // If no hit, reset the hitPoint
        }
    }

    // Draw gizmos when the scene view is updated
    void OnDrawGizmos()
    {

        Debug.Log("draw1");
        if (hitPoint.HasValue)
        {


            Debug.Log("draw2");
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPoint.Value, 0.5f);
        }
    }

}