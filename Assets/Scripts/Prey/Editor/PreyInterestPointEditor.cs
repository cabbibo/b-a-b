using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( PreyInterestPoint ) )]
public class PreyInterestPointEditor : Editor
{
    private Editor _cfgEditor;   // inline embedded inspector for the config asset

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var ip = (PreyInterestPoint)target;

        // ── Scene references ──────────────────────────────────────────────────
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "manager" ) , new GUIContent( "Prey Manager (optional)" ) );

        // ── Config asset (params) ─────────────────────────────────────────────
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "config" ) );
        if ( ip.config == null ) {
            EditorGUILayout.HelpBox( "No config assigned — this point uses inert default params. " +
                                     "Assign or create one to set its behavior." , MessageType.Warning );
            if ( GUILayout.Button( "Create Interest Point Config" ) ) CreateConfig( ip );
        } else {
            EditorGUILayout.Space( 4 );
            EditorGUILayout.LabelField( "Interest Point Config (asset)" , EditorStyles.boldLabel );
            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                CreateCachedEditor( ip.config , null , ref _cfgEditor );
                _cfgEditor.OnInspectorGUI();
            }
        }

        // ── Collider entrance shape: scene-ref collider (component-side) ──────
        if ( ip.entranceShape == EntranceShape.Collider ) {
            EditorGUILayout.Space( 8 );
            EditorGUILayout.LabelField( "── Entrance Collider ───────────────" , EditorStyles.boldLabel );
            EditorGUILayout.PropertyField( serializedObject.FindProperty( "entranceCollider" ) , new GUIContent( "Entrance Collider" ) );
            if ( ip.entranceCollider == null )
                EditorGUILayout.HelpBox( "Assign the collider the bird must enter to arrive at this point " +
                                         "(for a Despawn point, this is where it despawns)." , MessageType.Warning );
        }

        // ── OnCollider scene refs + perch-point generation (component-side) ───
        if ( ip.type == InterestPointType.Perch ) {
            EditorGUILayout.Space( 8 );

            if ( ip.perchSubType == PerchSubType.OnCollider ) {
                EditorGUILayout.LabelField( "── On-Collider Targets ──────────────" , EditorStyles.boldLabel );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "perchColliders" ) , new GUIContent( "Colliders" ) , true );
            }

            if ( ip.perchSubType != PerchSubType.Field ) {
                EditorGUILayout.Space( 4 );

                int childCount = 0;
                for ( int i = 0; i < ip.transform.childCount; i++ )
                    if ( ip.transform.GetChild( i ).name.StartsWith( "_perch_" ) ) childCount++;

                int targetCount = ip.manager != null ? ip.manager.maxPray
                    : (ip.perchSubType == PerchSubType.OnCollider ? ip.perchOnCollider.pointCount : ip.perchInArea.pointCount);
                if ( childCount > 0 )
                    EditorGUILayout.HelpBox( $"{childCount}/{targetCount} perch point(s) generated." , MessageType.Info );

                EditorGUILayout.BeginHorizontal();
                if ( GUILayout.Button( "Generate Points" , GUILayout.Height( 30 ) ) ) {
                    Undo.RegisterFullObjectHierarchyUndo( ip.gameObject , "Generate Perch Points" );
                    ip.GeneratePerchPoints();
                    EditorUtility.SetDirty( ip );
                }
                using ( new EditorGUI.DisabledScope( childCount == 0 ) ) {
                    if ( GUILayout.Button( "Clear Points" , GUILayout.Height( 30 ) , GUILayout.Width( 100 ) ) ) {
                        Undo.RegisterFullObjectHierarchyUndo( ip.gameObject , "Clear Perch Points" );
                        ip.ClearPerchPoints();
                        EditorUtility.SetDirty( ip );
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateConfig( PreyInterestPoint ip )
    {
        var asset = ScriptableObject.CreateInstance<PreyInterestPointConfigSO>();
        string path = EditorUtility.SaveFilePanelInProject(
            "Create PreyInterestPointConfigSO" , ip.name + "Config" , "asset" ,
            "Choose where to save the interest-point config asset" );
        if ( string.IsNullOrEmpty( path ) ) { Object.DestroyImmediate( asset ); return; }

        AssetDatabase.CreateAsset( asset , path );
        AssetDatabase.SaveAssets();
        serializedObject.FindProperty( "config" ).objectReferenceValue = asset;
        serializedObject.ApplyModifiedProperties();
    }
}
