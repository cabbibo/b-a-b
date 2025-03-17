
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FollowSceneView : MonoBehaviour
{
#if UNITY_EDITOR
    private SceneView sceneView;
    private Camera gameViewCamera;
    private static bool followCamera = true; // Change to static

    private void OnEnable()
    {
        // Get the Scene view camera
        sceneView = SceneView.lastActiveSceneView;

        // Get the Game view camera
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        this.sceneView = sceneView;
    }

    private void Update()
    {
        if (followCamera && sceneView != null)
        {
            // Get the Game view camera
            if (gameViewCamera == null)
            {
                gameViewCamera = SceneView.GetAllSceneCameras()[0];
            }

            // Update the position and rotation of the Game view camera to match the Scene view camera
            gameViewCamera.transform.position = sceneView.camera.transform.position;
            gameViewCamera.transform.rotation = sceneView.camera.transform.rotation;
        }
    }

    [MenuItem("Tools/Follow Scene Camera")]
    private static void ToggleFollowSceneCamera()
    {
        followCamera = !followCamera;
        Debug.Log("Follow Scene Camera: " + (followCamera ? "Enabled" : "Disabled"));
    }
#endif
}