using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Play-mode tool for authoring full race courses with a controller.
///
///   dUp   -> START a course (records the starting platform at your current position)
///   dLeft -> place a ring at your current position / heading
///   dUp   -> END the course (records the ending platform, assembles a RingSet,
///            saves it as a prefab, and leaves it live in the scene)
///
/// Rings are recorded into a plain holder while building. The real RingSet is only
/// instantiated at the END, so its OnEnable/SetupRings snapshots the complete ring
/// list in one go (rather than an empty list it captured at START).
/// </summary>
public class RingSetMaker : MonoBehaviour
{
    [Header("References")]
    public WrenMaker wrenMaker;
    public Transform playerObj;
    public ControllerTest controller;

    [Header("Course pieces")]
    [Tooltip("A finished RingSet prefab (e.g. your Race 1) to clone. All its wiring " +
             "(leaderboard, platforms, particles, line renderer) is reused.")]
    public GameObject ringSetPrefab;

    [Tooltip("The ring prefab placed when you press dLeft.")]
    public GameObject ringPrefab;

    [Header("Options")]
    [Tooltip("Give each saved course a fresh random raceID instead of inheriting the " +
             "template's. Avoids two courses fighting over the same id at runtime.")]
    public bool randomizeRaceID = true;

    private const string PrefabRootPath = "Assets/Prefabs/RingSets/";

    // --- build state ---
    public bool isBuilding;
    private GameObject ringHolder;          // temp parent for rings while recording
    private Vector3 startPos;
    private Quaternion startRot;

    public void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        wrenMaker = wrenMaker ?? GameObject.FindGameObjectWithTag("Realtime")?.GetComponent<WrenMaker>();
        playerObj = playerObj ?? wrenMaker?.localWren?.transform;
        if (wrenMaker != null)
        {
            wrenMaker.localWrenCreated += OnLocalWrenCreated;
        }
    }

    private void OnLocalWrenCreated(Wren w)
    {
        if (!playerObj)
        {
            playerObj = w.transform;
        }
    }

    public void Update()
    {
        if (!playerObj || controller == null)
        {
            return;
        }

        if (controller.dUpPressed)
        {
            if (!isBuilding)
            {
                StartCourse();
            }
            else
            {
                EndCourse();
            }
        }

        if (isBuilding && controller.dLeftPressed)
        {
            PlaceRing();
        }
    }

    public void StartCourse()
    {
        isBuilding = true;

        startPos = playerObj.position;
        startRot = Quaternion.LookRotation(playerObj.forward, Vector3.up);

        ringHolder = new GameObject("RingSetMaker_Holder");
        ringHolder.transform.SetParent(transform, false);

        Debug.Log("[RingSetMaker] Course STARTED. dLeft places rings, dUp again to finish.");
    }

    public GameObject PlaceRing()
    {
        var rot = Quaternion.LookRotation(playerObj.forward, Vector3.up);
        var ring = Instantiate(ringPrefab, playerObj.position, rot);
        ring.transform.SetParent(ringHolder.transform, true);
        Debug.Log($"[RingSetMaker] Placed ring #{ringHolder.transform.childCount}.");
        return ring;
    }

    public void EndCourse()
    {
        isBuilding = false;

        var endPos = playerObj.position;
        var endRot = Quaternion.LookRotation(playerObj.forward, Vector3.up);

        int ringCount = ringHolder != null ? ringHolder.transform.childCount : 0;
        if (ringCount == 0)
        {
            Debug.LogWarning("[RingSetMaker] No rings placed — discarding course.");
            CleanupHolder();
            return;
        }

        // Instantiate the template INACTIVE so its RingSet.OnEnable does not run until
        // we have swapped in the new rings and repositioned the platforms.
        var course = Instantiate(ringSetPrefab);
        course.SetActive(false);
        course.name = $"Ringset_{Random.Range(0, 9999999)}";

        var ringSet = course.GetComponentInChildren<RingSet>(true);
        if (ringSet == null)
        {
            Debug.LogError("[RingSetMaker] ringSetPrefab has no RingSet component — aborting.");
            Destroy(course);
            CleanupHolder();
            return;
        }

        // Clear the template's existing rings (reparent out so ringBase.childCount drops
        // synchronously this frame, then destroy).
        var ringBase = ringSet.ringBase;
        var oldRings = new List<Transform>();
        foreach (Transform child in ringBase)
        {
            oldRings.Add(child);
        }
        foreach (var old in oldRings)
        {
            old.SetParent(null, false);
            Destroy(old.gameObject);
        }

        // Move our recorded rings into the RingSet's ringBase, preserving world placement.
        var newRings = new List<Transform>();
        foreach (Transform child in ringHolder.transform)
        {
            newRings.Add(child);
        }
        foreach (var ring in newRings)
        {
            ring.SetParent(ringBase, true);
        }

        // Position the platforms from the recorded start/end transforms.
        if (ringSet.startingPlatform != null)
        {
            ringSet.startingPlatform.transform.SetPositionAndRotation(startPos, startRot);
        }
        if (ringSet.endingPlatform != null)
        {
            ringSet.endingPlatform.transform.SetPositionAndRotation(endPos, endRot);
        }

        if (randomizeRaceID)
        {
            ringSet.raceID = Random.Range(1, 999999);
        }

        // Activate -> RingSet.OnEnable now snapshots the full ring list.
        course.SetActive(true);

        SaveCoursePrefab(course);
        CleanupHolder();

        Debug.Log($"[RingSetMaker] Course FINISHED: {ringCount} rings, raceID {ringSet.raceID}. Live in scene.");
    }

    private void SaveCoursePrefab(GameObject course)
    {
#if UNITY_EDITOR
        var prefabPath = System.IO.Path.Combine(PrefabRootPath, course.name + ".prefab");
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
        Debug.Log($"[RingSetMaker] saving course prefab to {prefabPath}");
        // SaveAsPrefabAssetAndConnect leaves the scene instance linked to the new asset,
        // so the finished course stays live and flyable.
        PrefabUtility.SaveAsPrefabAssetAndConnect(course, prefabPath, InteractionMode.UserAction);
#endif
    }

    private void CleanupHolder()
    {
        if (ringHolder != null)
        {
            Destroy(ringHolder);
            ringHolder = null;
        }
    }
}
