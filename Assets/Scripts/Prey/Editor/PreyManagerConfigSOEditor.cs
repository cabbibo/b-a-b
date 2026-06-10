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
                    Prop( "spawnClosenessToBird" , "Closeness To Bird" );
                    break;
                case SpawnType.Painted:
                    Prop( "spawnDistanceMin" , "Distance Ahead Min" );
                    Prop( "spawnDistanceMax" , "Distance Ahead Max" );
                    Prop( "spawnRadius" , "Lateral Scatter" );
                    EditorGUILayout.HelpBox( "Spawns ahead of the bird, inside the painted area. Choose the food channels under Region Detection → Painted.", MessageType.None );
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
                case SpawnType.OnPointOfInterest:
                    Prop( "spawnRadius" , "Randomness Radius" );
                    Prop( "poiIdealDistance" , "Ideal Distance (ahead)" );
                    Prop( "poiIdealSpread" , "Ideal Spread" );
                    Prop( "poiIdealDistanceWeight" , "Ideal Weight (0..1)" );
                    EditorGUILayout.HelpBox( "Spawns the whole cluster at one in-region POI — for a Painted region, the same painted CHUNK " +
                        "as the wren. Ideal Weight 0 = random; 1 = strongly prefer the POI nearest a point Ideal Distance in front of the wren " +
                        "(within Ideal Spread).", MessageType.None );
                    break;
            }
        } );

        Section( "Despawn" , ref _foldDespawn , () => {
            Prop( "despawnType" , "Type" );
            if ( cfg.despawnType == DespawnType.Collider || cfg.despawnType == DespawnType.Region )
                Prop( "despawnSubject" , "Tested On" );
            Prop( "despawnOnWrenExit" );
            Prop( "minimumTimeAlive" );
            Prop( "timeOutsideBeforeDespawn" );

            string subject = cfg.despawnSubject == DespawnSubject.Wren ? "the WREN" : "each BIRD";
            switch ( cfg.despawnType ) {
                case DespawnType.Distance:
                    Prop( "distanceBeforeNotCaught" );
                    EditorGUILayout.HelpBox( "Despawns a bird when it's farther than this from the wren (subject doesn't matter — it's the gap between them).", MessageType.None );
                    break;
                case DespawnType.Collider:
                    EditorGUILayout.HelpBox( $"Despawns when {subject} leaves the despawn collider (assign it on the PreyManager component), after the grace period.", MessageType.None );
                    break;
                case DespawnType.Region:
                    EditorGUILayout.HelpBox( $"Despawns when {subject} leaves this manager's region (the Region Detection type: box / collider / painted), after the grace period.", MessageType.None );
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
            }

            // Painted channels live here, but show them whenever paint is in use
            // (painted region OR painted spawn) — not only when regionType is Painted.
            bool usesPaint = cfg.regionType == RegionType.Painted
                          || cfg.spawnType  == SpawnType.Painted;
            if ( usesPaint ) {
                EditorGUILayout.Space( 2 );
                EditorGUILayout.LabelField( "Painted Channels" , EditorStyles.miniBoldLabel );
                DrawChannelMask( "paintedChannels" , "Food Channels" );
                Prop( "paintedThreshold" , "Threshold" );
                EditorGUILayout.HelpBox( "A spot is 'in' the painted area where ANY listed channel ≥ threshold." , MessageType.None );
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

    private static readonly string[] _channelNames = { "R" , "G" , "B" , "A" };

    // Culling-mask-style dropdown (R/G/B/A rows) backed by the int[] channel array.
    private void DrawChannelMask( string propName , string label )
    {
        var arr = serializedObject.FindProperty( propName );
        if ( arr == null ) return;

        int mask = 0;
        for ( int i = 0; i < arr.arraySize; i++ ) {
            int v = arr.GetArrayElementAtIndex( i ).intValue;
            if ( v >= 0 && v < 4 ) mask |= ( 1 << v );
        }

        int newMask = EditorGUILayout.MaskField( label , mask , _channelNames );
        if ( newMask == mask ) return;

        arr.ClearArray();
        for ( int b = 0; b < 4; b++ ) {
            if ( ( newMask & ( 1 << b ) ) == 0 ) continue;
            int idx = arr.arraySize;
            arr.InsertArrayElementAtIndex( idx );
            arr.GetArrayElementAtIndex( idx ).intValue = b;
        }
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
}
