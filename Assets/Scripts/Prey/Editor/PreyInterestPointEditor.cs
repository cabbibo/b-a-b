using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( PreyInterestPoint ) )]
public class PreyInterestPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var ip = (PreyInterestPoint)target;

        // ── Common fields ────────────────────────────────────────────────────
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "manager" ) , new GUIContent( "Prey Manager (optional)" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "type" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "noticeRadius" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "alwaysInteresting" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "priority" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "timeToRemainInterested" ) , new GUIContent( "Time To Remain (s)" ) );

        // ── Type-specific fields ──────────────────────────────────────────────
        switch ( ip.type ) {

            case InterestPointType.Perch:
                EditorGUILayout.Space( 8 );
                EditorGUILayout.LabelField( "── Perch ───────────────────────────" , EditorStyles.boldLabel );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "perchSubType" ) ,
                    new GUIContent( "Sub-Type" ) );

                EditorGUILayout.Space( 4 );

                if ( ip.perchSubType == PerchSubType.OnCollider ) {
                    var s = serializedObject.FindProperty( "perchOnCollider" );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "collider" )     , new GUIContent( "Collider" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "facing" )       , new GUIContent( "Facing" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "radius" )       , new GUIContent( "Radius" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "pointCount" )   , new GUIContent( "Point Count" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "minNormalDot" ) , new GUIContent( "Min Normal Dot" ) );
                } else {
                    var s = serializedObject.FindProperty( "perchInArea" );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "facing" )       , new GUIContent( "Facing" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "radius" )       , new GUIContent( "Radius" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "pointCount" )   , new GUIContent( "Point Count" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "castHeight" )   , new GUIContent( "Cast Height" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "groundLayers" ) , new GUIContent( "Ground Layers" ) );
                    EditorGUILayout.PropertyField( s.FindPropertyRelative( "minNormalDot" ) , new GUIContent( "Min Normal Dot" ) );
                }

                EditorGUILayout.Space( 8 );

                int childCount = 0;
                for ( int i = 0; i < ip.transform.childCount; i++ )
                    if ( ip.transform.GetChild( i ).name.StartsWith( "_perch_" ) ) childCount++;

                int targetCount = ip.manager != null ? ip.manager.maxPray
                    : (ip.perchSubType == PerchSubType.OnCollider ? ip.perchOnCollider.pointCount : ip.perchInArea.pointCount);
                string countLabel = childCount > 0 ? $"{childCount}/{targetCount} perch point(s) generated." : "No points generated yet.";
                if ( childCount > 0 )
                    EditorGUILayout.HelpBox( countLabel , MessageType.Info );

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
                break;

            case InterestPointType.Updraft:
                EditorGUILayout.Space( 8 );
                EditorGUILayout.LabelField( "── Updraft ─────────────────────────" , EditorStyles.boldLabel );
                var us = serializedObject.FindProperty( "updraftSettings" );
                EditorGUILayout.PropertyField( us.FindPropertyRelative( "forceUp" )       , new GUIContent( "Force Up" ) );
                EditorGUILayout.PropertyField( us.FindPropertyRelative( "forceIn" )       , new GUIContent( "Force In" ) );
                EditorGUILayout.PropertyField( us.FindPropertyRelative( "curlForce" )     , new GUIContent( "Curl Force" ) );
                EditorGUILayout.PropertyField( us.FindPropertyRelative( "curlDirection" ) , new GUIContent( "Curl Direction" ) );
                break;

            case InterestPointType.NewInterest:
                EditorGUILayout.Space( 4 );
                EditorGUILayout.HelpBox( "On arrival: immediately searches for a different interest point (never revisits the one just reached)." , MessageType.None );
                break;

            case InterestPointType.NewCalm:
                EditorGUILayout.Space( 4 );
                EditorGUILayout.HelpBox( "On arrival: bird enters Calm state." , MessageType.None );
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
