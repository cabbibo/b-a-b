using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Attach alongside a PreyManager to get visual debugging of the bird system.
// In the Scene view: colored spheres per bird, radius gizmos, state counts.
[RequireComponent( typeof( PreyManager ) )]
public class PreyManagerDebug : MonoBehaviour
{
    public bool showStateColors   = true;
    public bool showModuleRadii   = false;
    public bool showVelocities    = false;

    private PreyManager manager;

    // state → color
    private static readonly Dictionary<PreyState, Color> StateColors = new Dictionary<PreyState, Color>
    {
        { PreyState.Calm      , Color.green                     },
        { PreyState.Landing   , Color.yellow                    },
        { PreyState.Perched   , new Color( 1f , 0.5f , 0f )    }, // orange
        { PreyState.TakingOff , Color.cyan                      },
        { PreyState.Disturbed , Color.red                       },
    };

    private void Awake() => manager = GetComponent<PreyManager>();

    private void OnDrawGizmos()
    {
        if ( manager == null ) manager = GetComponent<PreyManager>();
        if ( manager == null || manager.preyHolder == null ) return;

        var counts = new Dictionary<PreyState, int>();

        for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
            var bird = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
            if ( bird == null ) continue;

            if ( showStateColors ) {
                Gizmos.color = StateColors.TryGetValue( bird.state , out Color c ) ? c : Color.white;
                Gizmos.DrawSphere( bird.transform.position , 0.3f );
            }

            if ( showVelocities && bird.velocity.sqrMagnitude > 0.001f ) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine( bird.transform.position ,
                                 bird.transform.position + bird.velocity.normalized * 2f );
            }

            if ( showModuleRadii && bird.parameters != null ) {
                var p = bird.parameters;
                if ( p.modules.run ) {
                    Gizmos.color = new Color( 1f , 0f , 0f , 0.1f );
                    Gizmos.DrawWireSphere( bird.transform.position , p.run.startleRadius );
                }
                if ( p.modules.flock ) {
                    Gizmos.color = new Color( 0f , 1f , 0f , 0.05f );
                    Gizmos.DrawWireSphere( bird.transform.position , p.flock.detectionRadius );
                }
            }

            if ( !counts.ContainsKey( bird.state ) ) counts[bird.state] = 0;
            counts[bird.state]++;
        }

#if UNITY_EDITOR
        // draw state summary label above the manager
        string label = $"{manager.name}\n";
        foreach ( var kv in counts )
            label += $"  {kv.Key}: {kv.Value}\n";
        label += $"  Total: {manager.preyHolder.childCount}";

        Handles.Label( transform.position + Vector3.up * 5f , label );

        // draw perch point markers
        if ( manager.perchPoints != null ) {
            Handles.color = Color.yellow;
            foreach ( var p in manager.perchPoints ) {
                if ( p == null ) continue;
                Handles.DrawWireDisc( p.position , Vector3.up , 0.5f );
                Handles.Label( p.position + Vector3.up * 0.6f , "perch" );
            }
        }

        if ( manager.thermalCenters != null ) {
            Handles.color = new Color( 0.2f , 0.8f , 1f );
            foreach ( var tc in manager.thermalCenters ) {
                if ( tc == null ) continue;
                Handles.DrawWireDisc( tc.position , Vector3.up , 2f );
                Handles.Label( tc.position + Vector3.up , "thermal" );
            }
        }

        if ( manager.anchorPoints != null ) {
            Handles.color = Color.magenta;
            foreach ( var ap in manager.anchorPoints ) {
                if ( ap == null ) continue;
                Handles.DrawWireDisc( ap.position , Vector3.up , 1f );
                Handles.Label( ap.position + Vector3.up , "anchor" );
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if ( !Application.isPlaying ) return;
        if ( manager == null || manager.preyHolder == null ) return;

        int calm = 0 , landing = 0 , perched = 0 , takingOff = 0 , disturbed = 0;

        for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
            var bird = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
            if ( bird == null ) continue;
            switch ( bird.state ) {
                case PreyState.Calm:      calm++;      break;
                case PreyState.Landing:   landing++;   break;
                case PreyState.Perched:   perched++;   break;
                case PreyState.TakingOff: takingOff++; break;
                case PreyState.Disturbed: disturbed++; break;
            }
        }

        int total = manager.preyHolder.childCount;
        GUILayout.BeginArea( new Rect( 10 , 10 , 200 , 160 ) );
        GUI.Box( new Rect( 0 , 0 , 200 , 160 ) , "" );
        GUILayout.Label( $"<b>{manager.name}</b> ({total} birds)" );
        GUILayout.Label( $"<color=green>Calm: {calm}</color>" );
        GUILayout.Label( $"<color=yellow>Landing: {landing}</color>" );
        GUILayout.Label( $"Orange  Perched: {perched}" );
        GUILayout.Label( $"<color=cyan>TakingOff: {takingOff}</color>" );
        GUILayout.Label( $"<color=red>Disturbed: {disturbed}</color>" );
        GUILayout.EndArea();
    }
#endif
}
