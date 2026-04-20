#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SaveTransformFromPlayMode : MonoBehaviour
{
    [ContextMenu( "Save Transform To Edit Mode" )]
    private void Save()
    {
        if ( !Application.isPlaying ) {
            Debug.LogWarning( "Must be in Play Mode" );
            return;
        }

        var t = transform;

        EditorApplication.delayCall += () =>
        {
            if ( this == null ) {
                return;
            }

            Undo.RecordObject( t , "Save Transform" );

            t.position = t.position;
            t.rotation = t.rotation;
            t.localScale = t.localScale;

            EditorUtility.SetDirty( t );
        };
    }
}
#endif