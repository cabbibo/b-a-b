using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RingCreator : MonoBehaviour
{
    public WrenMaker wrenMaker;

    public GameObject ringSetPrefab;
    public GameObject ringPrefab;
    private GameObject holderObject;

    public Transform playerObj;
    public ControllerTest controller;

    private const string PrefabRootPath = "Assets/Prefabs/RingSets/";

    public void Awake() {
        Setup();
        SetupNewRingHolder();
    }

    private void Setup() {
        wrenMaker = wrenMaker ?? GameObject.FindGameObjectWithTag("Realtime").GetComponent<WrenMaker>();
        playerObj = playerObj ?? wrenMaker.localWren?.transform;
        wrenMaker.localWrenCreated += OnLocalWrenCreated;
    }

    private void OnLocalWrenCreated(Wren w) {
        if (!playerObj) {
            playerObj = w.transform;
        }
    }

    public void Update() {
        if (playerObj) {
            if (controller.r1Pressed) {
                CreateRingAtTarget();
            }
            if (controller.l1Pressed) {
                SavePrefab();
                SetupNewRingHolder();
            }
        }
    }

    public GameObject CreateRingAtTarget() {
        var target = playerObj;
        var rot = Quaternion.LookRotation(target.forward, Vector3.up);
        var newObj = Instantiate(ringPrefab, target.position, rot);
        newObj.transform.parent = holderObject.transform;
        return newObj;
    }

    public void SetupNewRingHolder() {
        holderObject = new GameObject("Ring Holder " + (transform.childCount + 1));
        holderObject.transform.parent = transform;
    }

    public void SavePrefab() {
        #if UNITY_EDITOR

        var numRings = holderObject.transform.childCount;
        if (numRings <= 0) {
            return;
        }

        //var timeStamp = System.DateTime.Now.ToString();
        var name = $"Ringset_" + Random.Range(0, 9999999);

        var newObj = Instantiate(ringSetPrefab, transform);
        newObj.name = name;

        var holderT = holderObject.transform;
        while (holderT.childCount > 0) {
            holderT.GetChild(0).parent = newObj.transform;
        }

        Destroy(holderObject);

        var prefabPath = System.IO.Path.Combine(PrefabRootPath, name + ".prefab");
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
        
        print($"saving rings prefab to {prefabPath}");
        PrefabUtility.SaveAsPrefabAssetAndConnect(newObj, prefabPath, InteractionMode.UserAction);

        #endif
    }
}
