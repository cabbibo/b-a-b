using UnityEngine;
using UnityEditor;

[CustomEditor( typeof(PreyManager) , true )]
public class PreyManagerEditor : Editor
{
    private bool _foldDebug       = false;
    private bool _foldScene       = true;
    private bool _foldSpawnTiming = true;
    private bool _foldConfig      = true;
    private bool _foldOnEat       = false;
    private bool _foldRegion      = true;
    private bool _foldRuntime     = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var mgr = (PreyManager)target;

        Section( "Debug" , ref _foldDebug , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "stepThrough" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "simulationSpeed" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "debugWren" ) );
        });

        Section( "Scene References" , ref _foldScene , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "interestPoints" ) , true );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "spawnPoints" ) , true );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyHolder" ) );
        });

        Section( "Spawn Timing" , ref _foldSpawnTiming , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "spawnInterval" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "bugsPerCluster" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "clusterRadius" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "spawnMaxOnWrenEnter" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "wrenEnterOnEnabled" ) );
        });

        Section( "Config" , ref _foldConfig , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyConfig" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyPrefab" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "maxPray" ) );
        });

        Section( "On Eat Effects" , ref _foldOnEat , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyFullnessIncrease" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyStaminaIncrease" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "gotAteParticles" ) );
        });

        Section( "Region Detection" , ref _foldRegion , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "regionType" ) , new GUIContent( "Type" ) );
            if ( mgr.regionType == RegionType.Box ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "boxRegion" ) , new GUIContent( "Box Transform" ) );
            } else if ( mgr.regionType == RegionType.Spline ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "regionSpline" ) , new GUIContent( "Spline" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "splineEnterDistance" ) , new GUIContent( "Enter Distance" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "splineExitDistance" ) , new GUIContent( "Exit Distance" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "splineCheckInterval" ) , new GUIContent( "Check Interval (s)" ) );
            } else if ( mgr.regionType == RegionType.Collider ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "regionCollider" ) , new GUIContent( "Collider" ) );
            } else {
                EditorGUILayout.HelpBox( "Painted region detection coming soon." , MessageType.None );
            }
        });

        Section( "Runtime Info" , ref _foldRuntime , () => {
            using ( new EditorGUI.DisabledScope( true ) ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "birdInsideRegion" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "currentNumberOfPrey" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "lastSpawnTime" ) );
            }
            if ( Application.isPlaying ) {
                EditorGUILayout.Space( 4 );
                using ( new EditorGUI.DisabledScope( !mgr.stepThrough ) )
                    if ( GUILayout.Button( "Step Forward" , GUILayout.Height( 36 ) ) )
                        StepAllBirds( mgr );
                if ( !mgr.stepThrough )
                    EditorGUILayout.HelpBox( "Enable 'Step Through' in Debug to step frame by frame." , MessageType.None );
            } else {
                EditorGUILayout.HelpBox( "Enter Play Mode to step the simulation." , MessageType.Info );
            }
        });

        serializedObject.ApplyModifiedProperties();
    }

    private static void Section( string title , ref bool open , System.Action body )
    {
        EditorGUILayout.Space( 2 );
        open = EditorGUILayout.Foldout( open , title , true , EditorStyles.foldoutHeader );
        if ( open ) {
            EditorGUI.indentLevel++;
            body();
            EditorGUI.indentLevel--;
        }
    }

    private static void StepAllBirds( PreyManager mgr )
    {
        if ( mgr.preyHolder == null ) return;
        for ( int i = 0; i < mgr.preyHolder.childCount; i++ ) {
            var bird = mgr.preyHolder.GetChild( i ).GetComponent<PreyController>();
            if ( bird != null ) bird.stepForward = true;
        }
    }
}
