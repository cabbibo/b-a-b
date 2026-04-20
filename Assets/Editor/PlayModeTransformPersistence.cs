#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeTransformPersistence
{
    private struct SavedTransform
    {
        public string     scenePath;
        public string     hierarchyPath;
        public Vector3    localPosition;
        public Quaternion localRotation;
        public Vector3    localScale;
    }

    private static readonly Dictionary<string , SavedTransform> saved             = new();
    private static          bool                                shouldApplyOnExit = false;

    static PlayModeTransformPersistence()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem( "CONTEXT/Transform/Mark Transform(s) For Save On Exit" )]
    private static void MarkTransformOrSelection( MenuCommand command )
    {
        if ( !Application.isPlaying ) {
            Debug.LogWarning( "This only works during Play Mode." );
            return;
        }

        var clicked = (Transform)command.context;
        int count = 0;

        bool clickedIsInSelection = false;

        foreach (var go in Selection.gameObjects)
            if ( go != null && go.transform == clicked ) {
                clickedIsInSelection = true;
                break;
            }

        if ( Selection.gameObjects.Length > 1 && clickedIsInSelection ) {
            foreach (var go in Selection.gameObjects) {
                if ( go == null ) {
                    continue;
                }

                SaveOrUpdateTransform( go.transform );
                count++;
            }

            Debug.Log( "Marked " + count + " selected transforms for save on Play Mode exit." );
        } else {
            SaveOrUpdateTransform( clicked );
            count = 1;
            Debug.Log( "Marked transform for save on Play Mode exit: " + GetHierarchyPath( clicked ) );
        }

        if ( count > 0 ) {
            shouldApplyOnExit = true;
        }
    }

    [MenuItem( "CONTEXT/Transform/Unmark Transform(s) For Save On Exit" )]
    private static void UnmarkTransformOrSelection( MenuCommand command )
    {
        if ( !Application.isPlaying ) {
            Debug.LogWarning( "This only works during Play Mode." );
            return;
        }

        var clicked = (Transform)command.context;
        int count = 0;

        bool clickedIsInSelection = false;

        foreach (var go in Selection.gameObjects)
            if ( go != null && go.transform == clicked ) {
                clickedIsInSelection = true;
                break;
            }

        if ( Selection.gameObjects.Length > 1 && clickedIsInSelection ) {
            foreach (var go in Selection.gameObjects) {
                if ( go == null ) {
                    continue;
                }

                if ( RemoveTransform( go.transform ) ) {
                    count++;
                }
            }
        } else {
            if ( RemoveTransform( clicked ) ) {
                count = 1;
            }
        }

        shouldApplyOnExit = saved.Count > 0;

        Debug.Log( "Unmarked " + count + " transform(s). Remaining marked: " + saved.Count );
    }

    [MenuItem( "CONTEXT/Transform/Clear All Marked Play Mode Transforms" )]
    private static void ClearSaved( MenuCommand command )
    {
        if ( !Application.isPlaying ) {
            Debug.LogWarning( "This only works during Play Mode." );
            return;
        }

        saved.Clear();
        shouldApplyOnExit = false;
        Debug.Log( "Cleared all marked Play Mode transforms." );
    }

    private static void SaveOrUpdateTransform( Transform t )
    {
        string key = GetKey( t );

        saved[key] = new SavedTransform
        {
            scenePath = t.gameObject.scene.path ,
            hierarchyPath = GetHierarchyPath( t ) ,
            localPosition = t.localPosition ,
            localRotation = t.localRotation ,
            localScale = t.localScale
        };
    }

    private static bool RemoveTransform( Transform t )
    {
        return saved.Remove( GetKey( t ) );
    }

    private static string GetKey( Transform t )
    {
        return t.gameObject.scene.path + "|" + GetHierarchyPath( t );
    }

    private static void OnPlayModeStateChanged( PlayModeStateChange state )
    {
        if ( state != PlayModeStateChange.EnteredEditMode ) {
            return;
        }

        if ( !shouldApplyOnExit || saved.Count == 0 ) {
            return;
        }

        int applied = 0;

        foreach (var kvp in saved) {
            var data = kvp.Value;

            if ( string.IsNullOrEmpty( data.scenePath ) ) {
                continue;
            }

            var scene = EditorSceneManager.GetSceneByPath( data.scenePath );

            if ( !scene.IsValid() || !scene.isLoaded ) {
                continue;
            }

            var target = FindTransformInScene( scene , data.hierarchyPath );

            if ( target == null ) {
                continue;
            }

            Undo.RecordObject( target , "Restore Play Mode Transform" );

            target.localPosition = data.localPosition;
            target.localRotation = data.localRotation;
            target.localScale = data.localScale;

            EditorUtility.SetDirty( target );
            applied++;
        }

        if ( applied > 0 ) {
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log( "Restored " + applied + " marked transform(s) from Play Mode." );
        }

        saved.Clear();
        shouldApplyOnExit = false;
    }

    private static string GetHierarchyPath( Transform t )
    {
        string path = t.name;

        while (t.parent != null) {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }

    private static Transform FindTransformInScene( UnityEngine.SceneManagement.Scene scene , string hierarchyPath )
    {
        string[] parts = hierarchyPath.Split( '/' );

        foreach (var root in scene.GetRootGameObjects()) {
            if ( root.name != parts[0] ) {
                continue;
            }

            var current = root.transform;

            for ( int i = 1; i < parts.Length; i++ ) {
                current = current.Find( parts[i] );

                if ( current == null ) {
                    break;
                }
            }

            if ( current != null ) {
                return current;
            }
        }

        return null;
    }
}
#endif