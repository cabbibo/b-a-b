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
        { PreyState.Searching , new Color( 0.6f , 0.2f , 1f )  }, // purple
        { PreyState.Landing   , Color.yellow                    },
        { PreyState.Updrafting, Color.green                     },
        { PreyState.Perched   , new Color( 1f , 0.5f , 0f )    }, // orange
        { PreyState.TakingOff , Color.cyan                      },
        { PreyState.Disturbed , Color.red                       },
    };

    private void Awake() => manager = GetComponent<PreyManager>();

    private readonly Dictionary<PreyState, int> counts = new Dictionary<PreyState, int>();

    private void OnDrawGizmos()
    {
        if ( manager == null ) manager = GetComponent<PreyManager>();
        if ( manager == null || manager.preyHolder == null ) return;

        counts.Clear();

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

        // draw interest point markers
        if ( manager.showInterestPointDebug && manager.interestPoints != null ) {
            foreach ( var ip in manager.interestPoints ) {
                if ( ip == null ) continue;

                Color col = ip.type switch {
                    InterestPointType.Perch       => Color.yellow,
                    InterestPointType.Updraft     => Color.green,
                    InterestPointType.NewInterest => new Color( 1f , 0.7f , 0.1f ),
                    InterestPointType.NewCalm     => new Color( 0.6f , 0.9f , 0.6f ),
                    InterestPointType.Despawn     => new Color( 1f , 0.2f , 0.2f ),
                    _                             => Color.white
                };

                float discR = ip.type == InterestPointType.Perch ? 0.5f : 1f;

                Handles.color = col;
                Handles.DrawWireDisc( ip.transform.position , Vector3.up , discR );
                string ipLabel = ip.alwaysInteresting ? $"{ip.type} ★" : ip.type.ToString();
                Handles.Label( ip.transform.position + Vector3.up * 0.8f , ipLabel );
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if ( !Application.isPlaying ) return;
        if ( manager == null || manager.preyHolder == null ) return;

        int calm = 0 , searching = 0 , landing = 0 , updrafting = 0 , perched = 0 , takingOff = 0 , disturbed = 0;

        for ( int i = 0; i < manager.preyHolder.childCount; i++ ) {
            var bird = manager.preyHolder.GetChild( i ).GetComponent<PreyController>();
            if ( bird == null ) continue;
            switch ( bird.state ) {
                case PreyState.Calm:       calm++;       break;
                case PreyState.Searching:  searching++;  break;
                case PreyState.Landing:    landing++;    break;
                case PreyState.Updrafting: updrafting++; break;
                case PreyState.Perched:    perched++;    break;
                case PreyState.TakingOff:  takingOff++;  break;
                case PreyState.Disturbed:  disturbed++;  break;
            }
        }

        int total = manager.preyHolder.childCount;
        GUILayout.BeginArea( new Rect( 10 , 10 , 200 , 200 ) );
        GUI.Box( new Rect( 0 , 0 , 200 , 200 ) , "" );
        GUILayout.Label( $"<b>{manager.name}</b> ({total} birds)" );
        GUILayout.Label( $"<color=green>Calm: {calm}</color>" );
        GUILayout.Label( $"<color=#9933FF>Searching: {searching}</color>" );
        GUILayout.Label( $"<color=yellow>Landing: {landing}</color>" );
        GUILayout.Label( $"<color=green>Updrafting: {updrafting}</color>" );
        GUILayout.Label( $"Orange  Perched: {perched}" );
        GUILayout.Label( $"<color=cyan>TakingOff: {takingOff}</color>" );
        GUILayout.Label( $"<color=red>Disturbed: {disturbed}</color>" );
        GUILayout.EndArea();
    }
#endif
}
