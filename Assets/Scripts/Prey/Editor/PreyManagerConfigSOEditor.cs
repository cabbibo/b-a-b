using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( PreyManagerConfigSO ) )]
public class PreyManagerConfigSOEditor : Editor
{
    private bool _foldTiming     = true;
    private bool _foldCapacity   = true;
    private bool _foldPlacement  = true;
    private bool _foldDespawn    = true;
    private bool _foldRegion     = true;
    private bool _foldOnEat      = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var cfg = (PreyManagerConfigSO)target;

        Section( "Spawn Timing" , ref _foldTiming , () => {
            Prop( "spawnInterval" );
            Prop( "preyPerCluster" , "Prey Per Cluster" );
            Prop( "clusterRadius" );
            Prop( "spawnMaxOnWrenEnter" );
            Prop( "wrenEnterOnEnabled" );
        } );

        Section( "Capacity" , ref _foldCapacity , () => {
            Prop( "maxPray" );
            Prop( "whenFull" , "When Full" );
        } );

        Section( "Spawn Placement" , ref _foldPlacement , () => {
            Prop( "spawnType" , "Type" );
            switch ( cfg.spawnType ) {
                case SpawnType.InsideBox:
                case SpawnType.BiomePaint:
                    Prop( "spawnClosenessToBird" , "Closeness To Bird" );
                    break;
                case SpawnType.NextToCurve:
                    Prop( "spawnRadius" , "Spawn Radius" );
                    Prop( "spawnClosenessToBird" , "Closeness To Bird" );
                    break;
                case SpawnType.DesiredAltitude:
                    Prop( "spawnRadius" , "Spawn Radius" );
                    Prop( "spawnClosenessToBird" , "Closeness To Bird" );
                    EditorGUILayout.HelpBox( "XZ from the region, Y from the altitude module's desired range." , MessageType.None );
                    break;
                case SpawnType.InDistance:
                    Prop( "spawnDistanceMin" , "Distance Min" );
                    Prop( "spawnDistanceMax" , "Distance Max" );
                    break;
            }
        } );

        Section( "Despawn" , ref _foldDespawn , () => {
            Prop( "despawnType" , "Type" );
            Prop( "despawnOnWrenExit" );
            Prop( "minimumTimeAlive" );
            Prop( "timeOutsideBeforeDespawn" );
            switch ( cfg.despawnType ) {
                case DespawnType.Distance:
                    Prop( "distanceBeforeNotCaught" );
                    break;
                case DespawnType.Collider:
                    EditorGUILayout.HelpBox( "Assign the despawn collider on the PreyManager component (scene reference)." , MessageType.None );
                    break;
                case DespawnType.Cage:
                    EditorGUILayout.HelpBox( "Despawns when outside the Region Detection cage (box region or region collider)." , MessageType.None );
                    break;
            }
        } );

        Section( "Region Detection" , ref _foldRegion , () => {
            Prop( "regionType" , "Type" );
            switch ( cfg.regionType ) {
                case RegionType.Spline:
                    Prop( "splineEnterDistance" , "Enter Distance" );
                    Prop( "splineExitDistance" , "Exit Distance" );
                    Prop( "splineCheckInterval" , "Check Interval (s)" );
                    break;
                case RegionType.Box:
                case RegionType.Collider:
                    EditorGUILayout.HelpBox( "Assign the region transform/collider on the PreyManager component (scene reference)." , MessageType.None );
                    break;
                default:
                    EditorGUILayout.HelpBox( "Painted region detection coming soon." , MessageType.None );
                    break;
            }
        } );

        Section( "On Eat Effects" , ref _foldOnEat , () => {
            Prop( "preyFullnessIncrease" );
            Prop( "preyStaminaIncrease" );
            Prop( "gotAteParticle" , "Got Ate Particle" );
        } );

        serializedObject.ApplyModifiedProperties();
    }

    private void Prop( string name )
        => EditorGUILayout.PropertyField( serializedObject.FindProperty( name ) );

    private void Prop( string name , string label )
        => EditorGUILayout.PropertyField( serializedObject.FindProperty( name ) , new GUIContent( label ) );

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
}
