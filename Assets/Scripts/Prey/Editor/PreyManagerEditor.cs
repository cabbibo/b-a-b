using UnityEngine;
using UnityEditor;

[CustomEditor( typeof(PreyManager) , true )]
public class PreyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var mgr = (PreyManager)target;

        EditorGUILayout.Space( 8 );
        EditorGUILayout.LabelField( "── Simulation ──────────────────" , EditorStyles.boldLabel );

        if ( !Application.isPlaying ) {
            EditorGUILayout.HelpBox( "Enter Play Mode to step the simulation." , MessageType.Info );
            return;
        }

        using ( new EditorGUI.DisabledScope( !mgr.stepThrough ) ) {
            if ( GUILayout.Button( "Step Forward" , GUILayout.Height( 36 ) ) ) {
                StepAllBirds( mgr );
            }
        }

        if ( !mgr.stepThrough ) {
            EditorGUILayout.HelpBox( "Enable 'Step Through' above to pause and step frame by frame." , MessageType.None );
        }
    }

    private static void StepAllBirds( PreyManager mgr )
    {
        if ( mgr.preyHolder == null ) return;

        for ( int i = 0 ; i < mgr.preyHolder.childCount ; i++ ) {
            var bird = mgr.preyHolder.GetChild( i ).GetComponent<PreyController>();

            if ( bird != null ) {
                bird.stepThrough = true;
                bird.stepForward = true;
            }
        }
    }
}
