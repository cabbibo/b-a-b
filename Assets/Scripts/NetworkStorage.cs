using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

public class NetworkStorage : RealtimeComponent<PersistentWrenStorageModel>
{
    private static NetworkStorage _instance;
    public static NetworkStorage Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<NetworkStorage>();
            }
            return _instance;
        }
    }

    // Putting this icon stuff in here for now
    [SerializeField] private Material _iconMaterial;
    private Dictionary<uint, Texture2D> _iconLookup = new Dictionary<uint, Texture2D>();
    private Dictionary<uint, Material> _iconMaterialLookup = new Dictionary<uint, Material>();

    private void Awake() {
        if (_instance == null) {
            _instance = this;
        }
    }

    public PersistentWrenModel GetLocalWrenModel() {
        var id = UserIdService.GetLocalUserId();
        var wModel = GetWrenModel(id);
        if (wModel != null) {
            return wModel;
        }
        
        wModel = new PersistentWrenModel();
        wModel.playerID = id;
        model.wrens.Add(id, wModel);

        return wModel;
    }

    public void SetLocalHue1(float v) {
        GetLocalWrenModel().hue1 = v;
    }

    public void SetLocalHue2(float v) {
        GetLocalWrenModel().hue2 = v;
    }

    public void SetLocalHue3(float v) {
        GetLocalWrenModel().hue3 = v;
    }

    public void SetLocalHue4(float v) {
        GetLocalWrenModel().hue4 = v;
    }

    protected override void OnRealtimeModelReplaced(PersistentWrenStorageModel previousModel, PersistentWrenStorageModel currentModel)
    {  
         if (previousModel != null) {
            // Unregister from events
            previousModel.wrens.modelAdded -= OnEntryAdded;
            previousModel.wrens.modelRemoved -= OnEntryRemoved;
            currentModel.wrens.modelReplaced -= OnEntryReplaced;
        }
        
        if (currentModel != null) {
            // Register for model changes
            currentModel.wrens.modelAdded += OnEntryAdded;
            currentModel.wrens.modelRemoved += OnEntryRemoved;
            currentModel.wrens.modelReplaced += OnEntryReplaced;

            foreach(var kvp in currentModel.wrens) {
                AddWrenModelCallbacks(kvp.Value);
            }
        }
    }

    private void OnEntryAdded(RealtimeDictionary<PersistentWrenModel> dic, uint key, PersistentWrenModel entryModel, bool remote) {
        AddWrenModelCallbacks(entryModel);
    }

    private void OnEntryRemoved(RealtimeDictionary<PersistentWrenModel> dic, uint key, PersistentWrenModel entryModel, bool remote) {
        RemoveWrenModelCallbacks(entryModel);
    }

    private void OnEntryReplaced(RealtimeDictionary<PersistentWrenModel> dic, uint key, PersistentWrenModel oldModel, PersistentWrenModel newModel, bool remote) {
        RemoveWrenModelCallbacks(oldModel);
        AddWrenModelCallbacks(newModel);
    }

    private void AddWrenModelCallbacks(PersistentWrenModel entryModel) {
        if (entryModel != null) {
            entryModel.hue1DidChange += OnHueChanged;
            entryModel.hue2DidChange += OnHueChanged;
            entryModel.hue3DidChange += OnHueChanged;
            entryModel.hue4DidChange += OnHueChanged;
        }
    }

    private void RemoveWrenModelCallbacks(PersistentWrenModel entryModel) {
        if (entryModel != null) {
            entryModel.hue1DidChange -= OnHueChanged;
            entryModel.hue2DidChange -= OnHueChanged;
            entryModel.hue3DidChange -= OnHueChanged;
            entryModel.hue4DidChange -= OnHueChanged;
        }
    }

    private void OnHueChanged(PersistentWrenModel wrenModel, float v) {
        OnWrenModelChanged(wrenModel);
    }

    private void OnWrenModelChanged(PersistentWrenModel wrenModel) {
        UpdateIconMaterial(wrenModel);
    }

    public PersistentWrenModel GetWrenModel(uint playerId) {
        PersistentWrenModel wrenModel;
        if (model.wrens.TryGetEntry(playerId, out wrenModel)) {
            return wrenModel;
        }
        return null;
    }

    private Texture2D UpdateIconTexture(PersistentWrenModel wrenModel) {
        var texture = WrenColors.MakeWrenIcon(wrenModel.hue1, wrenModel.hue2, wrenModel.hue3, wrenModel.hue4);
        // TODO: clean up old texture
        _iconLookup[wrenModel.playerID] = texture;
        return texture;
    }

    public Texture2D GetIconTexture(uint playerId) {
        if (_iconLookup.ContainsKey(playerId)) {
            return _iconLookup[playerId];
        }

        var wrenModel = GetWrenModel(playerId);
        if (wrenModel != null) {
            var texture = UpdateIconTexture(wrenModel);
            return texture;
        }

        return null;
    }

    private void UpdateIconMaterial(PersistentWrenModel wrenModel) {
        var mat = GetIconMaterial(wrenModel.playerID);
        var tex = UpdateIconTexture(wrenModel);
        mat.SetTexture("_MainTex", tex);
    }

    public Material GetIconMaterial(uint playerId) {
        if (_iconMaterialLookup.ContainsKey(playerId)) {
            return _iconMaterialLookup[playerId];
        }

        var mat = new Material(_iconMaterial);
        
        var wrenModel = GetWrenModel(playerId);
        if (wrenModel != null) {    
            var tex = GetIconTexture(playerId);
            if (tex) {
                mat.SetTexture("_MainTex", tex);
            }
        }

        _iconMaterialLookup[playerId] = mat;
        return mat;
    }
}
