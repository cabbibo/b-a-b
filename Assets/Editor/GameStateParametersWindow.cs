using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameStateParametersWindow : EditorWindow
{
    private GameStateParameters gameState;

    private SerializedObject so;

    private SerializedProperty currentSceneID;
    private SerializedProperty currentBiomeID;
    private SerializedProperty currentQuestID;

    private SerializedProperty gameStarted;
    private SerializedProperty tutorialFinished;
    private SerializedProperty tutorialIslandFinished;
    private SerializedProperty gameFinished;
    private SerializedProperty islandDiscovered;

    private SerializedProperty lastPosition;
    private SerializedProperty newIslandDiscovered;
    private SerializedProperty newIslandCompleted;
    private SerializedProperty totalTimeInGame;

    private SerializedProperty crystalsCollectedPerIsland;
    private SerializedProperty tmpCrystalsCollectedPerIsland;
    private SerializedProperty islandsCompleted;

    private SerializedProperty wrenCanDo;

    private bool showGameState = true;
    private bool showCompletion = true;
    private bool showCrystals = true;
    private bool showWrenCanDo = true;

    private string[] buildSceneNames = new string[0];

    [MenuItem("Tools/Wren/Game State Parameters Window")]
    public static void ShowWindow()
    {
        GetWindow<GameStateParametersWindow>("Game State");
    }

    private void OnEnable()
    {
        RefreshBuildSceneNames();
    }

    private void OnFocus()
    {
        RefreshBuildSceneNames();
        if (gameState != null)
        {
            Bind(gameState);
        }
    }

    private void RefreshBuildSceneNames()
    {
        var scenes = EditorBuildSettings.scenes;
        List<string> names = new List<string>();

        for (int i = 0; i < scenes.Length; i++)
        {
            if (!scenes[i].enabled) { continue; }

            string path = scenes[i].path;
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            names.Add(name);
        }

        buildSceneNames = names.ToArray();
    }

    private void Bind(GameStateParameters target)
    {
        if (target == null)
        {
            so = null;
            return;
        }

        so = new SerializedObject(target);

        currentSceneID = so.FindProperty("currentSceneID");
        currentBiomeID = so.FindProperty("currentBiomeID");
        currentQuestID = so.FindProperty("currentQuestID");

        gameStarted = so.FindProperty("gameStarted");
        tutorialFinished = so.FindProperty("tutorialFinished");
        tutorialIslandFinished = so.FindProperty("tutorialIslandFinished");
        gameFinished = so.FindProperty("gameFinished");
        islandDiscovered = so.FindProperty("islandDiscovered");

        lastPosition = so.FindProperty("lastPosition");
        newIslandDiscovered = so.FindProperty("newIslandDiscovered");
        newIslandCompleted = so.FindProperty("newIslandCompleted");
        totalTimeInGame = so.FindProperty("totalTimeInGame");

        crystalsCollectedPerIsland = so.FindProperty("crystalsCollectedPerIsland");
        tmpCrystalsCollectedPerIsland = so.FindProperty("tmpCrystalsCollectedPerIsland");
        islandsCompleted = so.FindProperty("islandsCompleted");

        wrenCanDo = so.FindProperty("wrenCanDo");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        var newGameState = (GameStateParameters)EditorGUILayout.ObjectField(
            "Game State Parameters",
            gameState,
            typeof(GameStateParameters),
            false
        );

        if (newGameState != gameState)
        {
            gameState = newGameState;
            Bind(gameState);
        }

        if (gameState == null)
        {
            EditorGUILayout.HelpBox("Assign a GameStateParameters asset.", MessageType.Info);
            return;
        }

        if (so == null)
        {
            Bind(gameState);
            if (so == null) { return; }
        }

        so.Update();

        DrawSceneSection();
        DrawStateSection();
        DrawCompletionSection();
        DrawCrystalSection();
        DrawWrenCanDoSection();

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Asset"))
            {
                SaveAsset();
            }

            if (GUILayout.Button("Ping Asset"))
            {
                EditorGUIUtility.PingObject(gameState);
                Selection.activeObject = gameState;
            }
        }

        so.ApplyModifiedProperties();
    }

    private void DrawSceneSection()
    {
        EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);

        if (currentSceneID != null)
        {
            int current = Mathf.Max(0, currentSceneID.intValue);

            if (buildSceneNames != null && buildSceneNames.Length > 0)
            {
                if (current >= buildSceneNames.Length)
                {
                    current = 0;
                }

                int next = EditorGUILayout.Popup("Current Scene", current, buildSceneNames);
                currentSceneID.intValue = next;
            }
            else
            {
                EditorGUILayout.PropertyField(currentSceneID, new GUIContent("Current Scene ID"));
            }
        }

        if (currentBiomeID != null) EditorGUILayout.PropertyField(currentBiomeID);
        if (currentQuestID != null) EditorGUILayout.PropertyField(currentQuestID);

        EditorGUILayout.Space();
    }

    private void DrawStateSection()
    {
        showGameState = EditorGUILayout.Foldout(showGameState, "Core State", true);
        if (!showGameState) return;

        EditorGUI.indentLevel++;

        if (gameStarted != null) EditorGUILayout.PropertyField(gameStarted);
        if (tutorialFinished != null) EditorGUILayout.PropertyField(tutorialFinished);
        if (tutorialIslandFinished != null) EditorGUILayout.PropertyField(tutorialIslandFinished);
        if (gameFinished != null) EditorGUILayout.PropertyField(gameFinished);
        if (islandDiscovered != null) EditorGUILayout.PropertyField(islandDiscovered);

        if (lastPosition != null) EditorGUILayout.PropertyField(lastPosition);
        if (newIslandDiscovered != null) EditorGUILayout.PropertyField(newIslandDiscovered);
        if (newIslandCompleted != null) EditorGUILayout.PropertyField(newIslandCompleted);
        if (totalTimeInGame != null) EditorGUILayout.PropertyField(totalTimeInGame);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawCompletionSection()
    {
        showCompletion = EditorGUILayout.Foldout(showCompletion, "Completed Scenes / Islands", true);
        if (!showCompletion) return;

        EditorGUI.indentLevel++;

        if (islandsCompleted != null && islandsCompleted.isArray)
        {
            int size = EditorGUILayout.IntField("Num Entries", islandsCompleted.arraySize);
            if (size != islandsCompleted.arraySize)
            {
                islandsCompleted.arraySize = Mathf.Max(0, size);
            }

            for (int i = 0; i < islandsCompleted.arraySize; i++)
            {
                SerializedProperty elem = islandsCompleted.GetArrayElementAtIndex(i);

                string label;
                if (buildSceneNames != null && i < buildSceneNames.Length)
                {
                    label = $"[{i}] {buildSceneNames[i]}";
                }
                else
                {
                    label = $"[{i}] Completed";
                }

                elem.boolValue = EditorGUILayout.Toggle(label, elem.boolValue);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Mark All Complete"))
                {
                    for (int i = 0; i < islandsCompleted.arraySize; i++)
                    {
                        islandsCompleted.GetArrayElementAtIndex(i).boolValue = true;
                    }
                }

                if (GUILayout.Button("Clear All Complete"))
                {
                    for (int i = 0; i < islandsCompleted.arraySize; i++)
                    {
                        islandsCompleted.GetArrayElementAtIndex(i).boolValue = false;
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No bool[] field named islandsCompleted found.", MessageType.Warning);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawCrystalSection()
    {
        showCrystals = EditorGUILayout.Foldout(showCrystals, "Crystal Info", true);
        if (!showCrystals) return;

        EditorGUI.indentLevel++;

        DrawIntArray("Crystals Collected", crystalsCollectedPerIsland);
        DrawIntArray("TMP Crystals Collected", tmpCrystalsCollectedPerIsland);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawIntArray(string label, SerializedProperty arrayProp)
    {
        if (arrayProp == null || !arrayProp.isArray)
        {
            EditorGUILayout.HelpBox($"No int[] field named {label}.", MessageType.None);
            return;
        }

        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

        int size = EditorGUILayout.IntField("Size", arrayProp.arraySize);
        if (size != arrayProp.arraySize)
        {
            arrayProp.arraySize = Mathf.Max(0, size);
        }

        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            SerializedProperty elem = arrayProp.GetArrayElementAtIndex(i);

            string rowLabel;
            if (buildSceneNames != null && i < buildSceneNames.Length)
            {
                rowLabel = $"[{i}] {buildSceneNames[i]}";
            }
            else
            {
                rowLabel = $"[{i}]";
            }

            elem.intValue = EditorGUILayout.IntField(rowLabel, elem.intValue);
        }

        EditorGUILayout.Space();
    }

    private void DrawWrenCanDoSection()
    {
        showWrenCanDo = EditorGUILayout.Foldout(showWrenCanDo, "Wren Can Do", true);
        if (!showWrenCanDo) return;

        if (wrenCanDo == null || !wrenCanDo.objectReferenceValue)
        {
            EditorGUILayout.HelpBox("Assign the wrenCanDo reference on GameStateParameters.", MessageType.Warning);
            return;
        }

        EditorGUI.indentLevel++;

        SerializedObject wrenSO = new SerializedObject(wrenCanDo.objectReferenceValue);

        DrawWrenBool(wrenSO, "takeOff");
        DrawWrenBool(wrenSO, "hover");
        DrawWrenBool(wrenSO, "boost");
        DrawWrenBool(wrenSO, "ping");
        DrawWrenBool(wrenSO, "disintegrate");

        EditorGUILayout.Space();

        DrawWrenBool(wrenSO, "call");
        DrawWrenBool(wrenSO, "magnetize");
        DrawWrenBool(wrenSO, "placeBeacon");
        DrawWrenBool(wrenSO, "rewind");
        DrawWrenBool(wrenSO, "carry");

        EditorGUILayout.Space();

        DrawWrenBool(wrenSO, "hasLearnedFlight");
        DrawWrenBool(wrenSO, "hasLearnedTakeOff");
        DrawWrenBool(wrenSO, "hasLearnedHover");
        DrawWrenBool(wrenSO, "hasLearnedBoost");
        DrawWrenBool(wrenSO, "hasLearnedPing");
        DrawWrenBool(wrenSO, "hasLearnedDisintegrate");

        EditorGUILayout.Space();

        DrawWrenBool(wrenSO, "hasLearnedCall");
        DrawWrenBool(wrenSO, "hasLearnedMagnetize");
        DrawWrenBool(wrenSO, "hasLearnedPlaceBeacon");
        DrawWrenBool(wrenSO, "hasLearnedRewind");
        DrawWrenBool(wrenSO, "hasLearnedCarry");
        DrawWrenBool(wrenSO, "hasLearnedWalk");

        wrenSO.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(wrenCanDo.objectReferenceValue);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawWrenBool(SerializedObject wrenSO, string propName)
    {
        SerializedProperty prop = wrenSO.FindProperty(propName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop);
        }
    }

    private void SaveAsset()
    {
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameState);

        if (wrenCanDo != null && wrenCanDo.objectReferenceValue != null)
        {
            EditorUtility.SetDirty(wrenCanDo.objectReferenceValue);
        }

        AssetDatabase.SaveAssets();
    }
}