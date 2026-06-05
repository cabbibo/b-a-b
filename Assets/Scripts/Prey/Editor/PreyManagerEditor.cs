using UnityEngine;
using UnityEditor;

[CustomEditor( typeof(PreyManager) , true )]
public class PreyManagerEditor : Editor
{
    private bool _foldDebug   = false;
    private bool _foldScene   = true;
    private bool _foldConfig  = true;
    private bool _foldWiring  = true;
    private bool _foldRuntime = false;

    private Editor _cfgEditor;   // inline embedded inspector for the manager config asset

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var mgr = (PreyManager)target;

        Section( "Debug" , ref _foldDebug , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "debugWren" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "stepThrough" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "simulationSpeed" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "showRegionEntrance" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "showInterestPointDebug" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "showDespawnDebug" ) );
        });

        Section( "Scene References" , ref _foldScene , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "interestPoints" ) , true );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyHolder" ) );
        });

        Section( "Config" , ref _foldConfig , () => {
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyConfig" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "preyPrefab" ) );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "managerConfig" ) );

            if ( mgr.managerConfig == null ) {
                EditorGUILayout.HelpBox( "No manager config assigned — this manager is inert (won't spawn). " +
                                         "Assign or create one to set spawn timing, capacity, placement, etc." , MessageType.Warning );
                if ( GUILayout.Button( "Create Manager Config Asset" ) ) CreateManagerConfig( mgr );
            } else {
                EditorGUILayout.Space( 4 );
                EditorGUILayout.LabelField( "Manager Config (asset)" , EditorStyles.boldLabel );
                using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                    CreateCachedEditor( mgr.managerConfig , null , ref _cfgEditor );
                    _cfgEditor.OnInspectorGUI();
                }
            }
        });

        // Scene wiring whose visibility depends on the config's type enums (read via proxies).
        Section( "Scene Wiring" , ref _foldWiring , () => {
            // Region scene refs
            EditorGUILayout.LabelField( $"Region: {mgr.regionType}" , EditorStyles.miniBoldLabel );
            if ( mgr.regionType == RegionType.Box ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "boxRegion" ) , new GUIContent( "Box Transform" ) );
            } else if ( mgr.regionType == RegionType.Spline ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "regionSpline" ) , new GUIContent( "Spline" ) );
            } else if ( mgr.regionType == RegionType.Collider ) {
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "regionCollider" ) , new GUIContent( "Collider" ) );
            } else {
                EditorGUILayout.HelpBox( "Painted region detection coming soon." , MessageType.None );
            }

            // Despawn collider only matters for the Collider despawn type
            if ( mgr.despawnType == DespawnType.Collider ) {
                EditorGUILayout.Space( 2 );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "despawnCollider" ) );
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
                    EditorGUILayout.HelpBox( "Enable 'Step Through' in the manager config to step frame by frame." , MessageType.None );
            } else {
                EditorGUILayout.HelpBox( "Enter Play Mode to step the simulation." , MessageType.Info );
            }
        });

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateManagerConfig( PreyManager mgr )
    {
        var asset = ScriptableObject.CreateInstance<PreyManagerConfigSO>();
        string path = EditorUtility.SaveFilePanelInProject(
            "Create PreyManagerConfigSO" , mgr.name + "ManagerConfig" , "asset" ,
            "Choose where to save the manager config asset" );
        if ( string.IsNullOrEmpty( path ) ) { Object.DestroyImmediate( asset ); return; }

        AssetDatabase.CreateAsset( asset , path );
        AssetDatabase.SaveAssets();
        serializedObject.FindProperty( "managerConfig" ).objectReferenceValue = asset;
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
