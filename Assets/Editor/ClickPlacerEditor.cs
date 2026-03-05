using UnityEditor;
using UnityEngine;

[CustomEditor( typeof(ClickPlacer) )]
public class ClickPlacerEditor : Editor
{
    public GameObject prefab;
    public int        maxCount;

    public int count;

    private bool hasStartedDragging;

    private RaycastHit originalHit;

    public void OnSceneGUI()
    {

        var placer = (ClickPlacer)target;


        // What is this?
        HandleUtility.AddDefaultControl( GUIUtility.GetControlID( FocusType.Passive ) );
        var ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

        if ( Event.current.type == EventType.MouseDown && Event.current.button == 0 ) {
            if ( placer.CastMouseRay( ray , out originalHit ) ) {
                hasStartedDragging = true;
                return;
            }
        }

        bool hasHit = placer.CastMouseRay( ray , out var newHit );

        if ( Event.current.type == EventType.MouseUp && Event.current.button == 0 ) {
            if ( hasStartedDragging ) {
                var forward = newHit.point - originalHit.point;

                if ( forward.magnitude < 0.1f ) {
                    forward = Vector3.Cross( originalHit.normal , Vector3.up );
                }

                placer.PlaceObject( originalHit.point , originalHit.normal , forward );
                hasStartedDragging = false;
            }
        }

        if ( Event.current.type == EventType.Repaint && hasStartedDragging && hasHit ) {
            Handles.color = Color.blue;
            Handles.DrawLine( originalHit.point , newHit.point );
            Handles.color = Color.green;
            var offset = newHit.point - originalHit.point;
            Handles.DrawLine( originalHit.point , originalHit.point + originalHit.normal * offset.magnitude );
            Handles.color = Color.red;
            Handles.DrawLine( originalHit.point , originalHit.point + Vector3.Cross( offset , originalHit.normal ) );


            Handles.color = Color.yellow;
            Handles.DrawWireDisc( originalHit.point , originalHit.normal , placer.scaleDragSizeRange.x );
            Handles.DrawWireDisc( originalHit.point , originalHit.normal , placer.scaleDragSizeRange.y );


            SceneView.RepaintAll();
        }
    }

    public override void OnInspectorGUI()
    {
        var placer = (ClickPlacer)target;

        if ( GUILayout.Button( "Reset" ) ) {
            placer.Reset();
        }

        if ( GUILayout.Button( "Replace Objects Down" ) ) {
            placer.ReplaceObjectsDown();
        }

        DrawDefaultInspector();
    }
}