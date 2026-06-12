using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( PreyInterestPointConfigSO ) )]
public class PreyInterestPointConfigSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var cfg = (PreyInterestPointConfigSO)target;

        // ── Common ───────────────────────────────────────────────────────────
        Prop( "type" );
        Prop( "noticeRadius" , "Notice Radius" );
        Prop( "enterRadius" , "Enter Radius" );
        Prop( "noticeUrgency" , "Notice Urgency" );
        Prop( "alwaysInteresting" );
        Prop( "priority" );
        Prop( "timeToRemainInterested" , "Time To Remain (s)" );
        Prop( "timeToRemainVariance" , "Time To Remain ± Variance" );

        // ── Search / Entrance ────────────────────────────────────────────────
        EditorGUILayout.Space( 4 );
        Prop( "searchTargetType" , "Search Target" );
        if ( cfg.searchTargetType == SearchTargetType.RandomInRange )
            Prop( "searchRandomRadius" , "Random Radius" );
        Prop( "targetRandomness" , "Target Randomness" );
        Prop( "entranceShape" , "Entrance Shape" );
        if ( cfg.entranceShape == EntranceShape.Collider )
            EditorGUILayout.HelpBox( "Collider entrance: the bird arrives when it enters a collider assigned on the " +
                                     "PreyInterestPoint component (Enter Radius is ignored; Notice Radius still applies)." , MessageType.None );

        // ── Type-specific ────────────────────────────────────────────────────
        switch ( cfg.type ) {

            case InterestPointType.Perch:
                EditorGUILayout.Space( 8 );
                EditorGUILayout.LabelField( "── Perch ───────────────────────────" , EditorStyles.boldLabel );
                Prop( "perchSubType" , "Sub-Type" );
                EditorGUILayout.Space( 4 );

                if ( cfg.perchSubType == PerchSubType.OnCollider ) {
                    var s = serializedObject.FindProperty( "perchOnCollider" );
                    Rel( s , "facing" , "Facing" );
                    Rel( s , "radius" , "Radius" );
                    Rel( s , "pointCount" , "Point Count" );
                    Rel( s , "minNormalDot" , "Min Normal Dot" );
                    EditorGUILayout.HelpBox( "Assign the actual colliders (and Generate/Clear points) on the " +
                                             "PreyInterestPoint component — they're scene references." , MessageType.None );
                } else if ( cfg.perchSubType == PerchSubType.InArea ) {
                    var s = serializedObject.FindProperty( "perchInArea" );
                    Rel( s , "facing" , "Facing" );
                    Rel( s , "radius" , "Radius" );
                    Rel( s , "pointCount" , "Point Count" );
                    Rel( s , "castHeight" , "Cast Height" );
                    Rel( s , "groundLayers" , "Ground Layers" );
                    Rel( s , "minNormalDot" , "Min Normal Dot" );
                } else { // Field — runtime-computed, no pre-generation
                    var s = serializedObject.FindProperty( "perchField" );
                    Rel( s , "radius" , "Radius" );
                    Rel( s , "spacing" , "Spacing" );
                    Rel( s , "forwardFromVelocity" , "Forward From Velocity" );
                    Rel( s , "desireToBeClose" , "Desire To Be Close" );
                    Rel( s , "castUp" , "Cast Up" );
                    Rel( s , "castHeightOffset" , "Cast Height Offset" );
                    Rel( s , "groundLayers" , "Ground Layers" );
                    EditorGUILayout.Space( 4 );
                    EditorGUILayout.HelpBox( "Field perches are computed in real time — no points to generate." , MessageType.None );
                }
                break;

            case InterestPointType.Updraft:
                EditorGUILayout.Space( 8 );
                EditorGUILayout.LabelField( "── Updraft ─────────────────────────" , EditorStyles.boldLabel );
                var us = serializedObject.FindProperty( "updraftSettings" );
                Rel( us , "forceUp" , "Force Up" );
                Rel( us , "forceIn" , "Force In" );
                Rel( us , "curlForce" , "Curl Force" );
                Rel( us , "curlDirection" , "Curl Direction" );
                EditorGUILayout.Space( 4 );
                Rel( us , "desiredAltitude" , "Desired Altitude (band top)" );
                Rel( us , "altitudeRange" , "Altitude Range (band depth)" );
                Rel( us , "altitudeHoldStrength" , "Altitude Hold Strength" );
                break;

            case InterestPointType.NewInterest:
                EditorGUILayout.Space( 4 );
                EditorGUILayout.HelpBox( "On arrival: immediately searches for a different interest point (never revisits the one just reached)." , MessageType.None );
                break;

            case InterestPointType.NewCalm:
                EditorGUILayout.Space( 4 );
                EditorGUILayout.HelpBox( "On arrival: bird enters Calm state." , MessageType.None );
                break;

            case InterestPointType.Despawn:
                EditorGUILayout.Space( 4 );
                EditorGUILayout.HelpBox( cfg.entranceShape == EntranceShape.Collider
                    ? "On arrival (entering the assigned Entrance Collider): the bird despawns itself."
                    : "On arrival (within Enter Radius): the bird despawns itself." , MessageType.None );
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void Prop( string name )
        => EditorGUILayout.PropertyField( serializedObject.FindProperty( name ) );

    private void Prop( string name , string label )
        => EditorGUILayout.PropertyField( serializedObject.FindProperty( name ) , new GUIContent( label ) );

    private static void Rel( SerializedProperty parent , string name , string label )
        => EditorGUILayout.PropertyField( parent.FindPropertyRelative( name ) , new GUIContent( label ) );
}
