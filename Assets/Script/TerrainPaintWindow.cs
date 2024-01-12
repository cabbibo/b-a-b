using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CustomEditorWindow : EditorWindow
{
    Color backgroundColor;

    float displayScale;

    public TerrainPainter painter;

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
        painter = (TerrainPainter)EditorGUILayout.ObjectField(painter, typeof(TerrainPainter), true); //public static Object ObjectField(string label, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options);

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


        /*  // Ensure we have a scene view
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
          }*/








        if (Event.current.type == EventType.KeyDown)
        {


            if (Event.current.keyCode == KeyCode.Z) { painter.fn = -1; }
            if (Event.current.keyCode == KeyCode.X) { painter.shift = -1; }
            if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = -1; }

        }

        if (Event.current.type == EventType.KeyUp)
        {

            if (Event.current.keyCode == KeyCode.Z) { painter.fn = 1; }
            if (Event.current.keyCode == KeyCode.X) { painter.shift = 1; }
            if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = 1; }

        }



        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (Event.current.type == EventType.MouseMove)
        {
            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);

            painter.MouseMove(ray);


            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;


            // painter.paintScreenDirection = Vector2.zero;


            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {


            Debug.Log("mouse");

            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            painter.MouseDown(ray);

            painter.paintScreenPosition = mousePos;
            painter.oSP = mousePos;

            painter.paintScreenDirection = Vector2.zero;

        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {

            Debug.Log(painter.displayScale);
            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            painter.WhileDown(ray);

            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

        }



        if (Event.current.type == EventType.MouseUp)
        {
            //painter.Save();

            painter.Save();
            painter.isPainting = 0;

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



    public void Update()
    {
        /*Vector2 dpi = DPIHelper.GetSystemDPI();
  Debug.Log($"System DPI: X = {dpi.x}, Y = {dpi.y}");

  Debug.Log(dpi.x / 96.0f);*/


        //TerrainPainter painter = (TerrainPainter)target;



        if (Event.current.type == EventType.KeyDown)
        {


            if (Event.current.keyCode == KeyCode.Z) { painter.fn = -1; }
            if (Event.current.keyCode == KeyCode.X) { painter.shift = -1; }
            if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = -1; }

        }

        if (Event.current.type == EventType.KeyUp)
        {

            if (Event.current.keyCode == KeyCode.Z) { painter.fn = 1; }
            if (Event.current.keyCode == KeyCode.X) { painter.shift = 1; }
            if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = 1; }

        }



        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (Event.current.type == EventType.MouseMove)
        {
            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);

            painter.MouseMove(ray);


            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;


            // painter.paintScreenDirection = Vector2.zero;


            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {


            Debug.Log("mouse");

            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            painter.MouseDown(ray);

            painter.paintScreenPosition = mousePos;
            painter.oSP = mousePos;

            painter.paintScreenDirection = Vector2.zero;

        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {

            Debug.Log(painter.displayScale);
            Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            painter.WhileDown(ray);

            painter.oSP = painter.paintScreenPosition;
            painter.paintScreenPosition = mousePos;

            painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

        }



        if (Event.current.type == EventType.MouseUp)
        {
            //painter.Save();

            painter.Save();
            painter.isPainting = 0;

        }


    }
}

#endif