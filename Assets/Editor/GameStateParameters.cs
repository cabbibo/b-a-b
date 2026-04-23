using UnityEngine;

[CreateAssetMenu( menuName = "Wren/Game State Parameters" )]
public class GameStateParameters : ScriptableObject
{
    public int currentSceneID;
    public int currentBiomeID;
    public int currentQuestID;

    public bool gameStarted;
    public bool tutorialFinished;
    public bool tutorialIslandFinished;
    public bool gameFinished;
    public bool islandDiscovered;

    public Vector3 lastPosition;
    public int     newIslandDiscovered = -1;
    public int     newIslandCompleted  = -1;
    public float   totalTimeInGame;

    public int[]  crystalsCollectedPerIsland;
    public int[]  tmpCrystalsCollectedPerIsland;
    public bool[] islandsCompleted;

    public WrenCanDo wrenCanDo;
}