using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CrystalFiller : MonoBehaviour
{
    public UnityEvent OnFilledEvent;
    public UnityEvent OnUnfilledEvent;

    public Helpers.IntEvent OnPartialFillEvent;
    public Helpers.FloatEvent OnPercentageFillEvent;

    public int numCrystalsToFill;
    public int currentNumCrystals;
    public int maxCrystals;
    public bool filled;
    public float drainSpeed;
    public float drainedAmount;

    [Header("Persistence")]
    public bool persistent = false;

    [SerializeField]
    private string uniqueFillerID;

    private string SaveKeyCurrent => $"crystal_filler_current_{uniqueFillerID}";
    private string SaveKeyFilled => $"crystal_filler_filled_{uniqueFillerID}";
    private string SaveKeyDrainedAmount => $"crystal_filler_drained_{uniqueFillerID}";

    private void Awake()
    {
        LoadState();
        ApplyLoadedStateAndInvokeEvents();
    }

    private void OnEnable()
    {
        LoadState();
        ApplyLoadedStateAndInvokeEvents();
    }

    public void FillCrystal(int numCrystals)
    {
        currentNumCrystals += numCrystals;
        currentNumCrystals = Mathf.Clamp(currentNumCrystals, 0, maxCrystals);

        if (currentNumCrystals >= numCrystalsToFill)
        {
            if (!filled)
            {
                OnFilled();
            }
            else
            {
                OnExtraFill(numCrystals);
            }
        }
        else
        {
            OnPartialFill(numCrystals);
        }

        SaveState();
    }

    public void OnFilled()
    {
        filled = true;

        God.particleSystems.largeSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.largeSuccessParticleSystem.Emit(3000);
        God.audio.Play(God.sounds.largeSuccessSound);

        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);
        OnFilledEvent.Invoke();

        SaveState();
    }

    public void OnUnfilled()
    {
        filled = false;
        OnUnfilledEvent.Invoke();
        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);

        SaveState();
    }

    public void OnExtraFill(int numCrystals)
    {
        God.particleSystems.smallSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.smallSuccessParticleSystem.Emit(100);
        God.audio.Play(God.sounds.smallSuccessSound);

        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);
        OnPartialFillEvent.Invoke(numCrystals);

        SaveState();
    }

    public void OnPartialFill(int amount)
    {
        if (amount > 0)
        {
            God.particleSystems.smallSuccessParticleSystem.transform.position = transform.position;
            God.particleSystems.smallSuccessParticleSystem.Emit(100);
            God.audio.Play(God.sounds.smallSuccessSound);
        }

        OnPartialFillEvent.Invoke(amount);
        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);

        SaveState();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (God.IsOurWren(other))
        {
            int numCrystals = (int)God.wren.shards.GetShardTrailAmount();
            print("HELLO this is number shaders");
            print(numCrystals);

            FillCrystal(numCrystals);

            God.wren.shards.SpendExtraShards();
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (drainSpeed > 0)
        {
            drainedAmount += drainSpeed;

            if (drainedAmount >= 1)
            {
                drainedAmount = 0;
                currentNumCrystals--;
                currentNumCrystals = Mathf.Clamp(currentNumCrystals, 0, maxCrystals);

                OnPartialFill(-1);

                if (currentNumCrystals < numCrystalsToFill)
                {
                    if (filled)
                    {
                        OnUnfilled();
                    }
                }

                SaveState();
            }
        }
    }

    private void ApplyLoadedStateAndInvokeEvents()
    {
        currentNumCrystals = Mathf.Clamp(currentNumCrystals, 0, maxCrystals);

        bool shouldBeFilled = currentNumCrystals >= numCrystalsToFill;
        filled = shouldBeFilled;

        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);

        if (filled)
        {
            OnFilledEvent.Invoke();
        }
        else
        {
            OnUnfilledEvent.Invoke();
            OnPartialFillEvent.Invoke(currentNumCrystals);
        }
    }

    private void SaveState()
    {
        if (!persistent)
        {
            return;
        }

        if (string.IsNullOrEmpty(uniqueFillerID))
        {
            return;
        }

        PlayerPrefs.SetInt(SaveKeyCurrent, currentNumCrystals);
        PlayerPrefs.SetInt(SaveKeyFilled, filled ? 1 : 0);
        PlayerPrefs.SetFloat(SaveKeyDrainedAmount, drainedAmount);
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        if (!persistent)
        {
            return;
        }

        if (string.IsNullOrEmpty(uniqueFillerID))
        {
            return;
        }

        if (PlayerPrefs.HasKey(SaveKeyCurrent))
        {
            currentNumCrystals = PlayerPrefs.GetInt(SaveKeyCurrent, currentNumCrystals);
        }

        if (PlayerPrefs.HasKey(SaveKeyFilled))
        {
            filled = PlayerPrefs.GetInt(SaveKeyFilled, filled ? 1 : 0) == 1;
        }

        if (PlayerPrefs.HasKey(SaveKeyDrainedAmount))
        {
            drainedAmount = PlayerPrefs.GetFloat(SaveKeyDrainedAmount, drainedAmount);
        }
    }

    public void ClearSavedState()
    {
        if (string.IsNullOrEmpty(uniqueFillerID))
        {
            return;
        }

        PlayerPrefs.DeleteKey(SaveKeyCurrent);
        PlayerPrefs.DeleteKey(SaveKeyFilled);
        PlayerPrefs.DeleteKey(SaveKeyDrainedAmount);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureUniqueID();
        EditorUtility.SetDirty(this);
    }

    private void EnsureUniqueID()
    {
        if (Application.isPlaying)
        {
            return;
        }

        bool needsNewId = string.IsNullOrEmpty(uniqueFillerID) || HasDuplicateID(uniqueFillerID);

        if (needsNewId)
        {
            uniqueFillerID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }

    private bool HasDuplicateID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        var allFillers = Resources.FindObjectsOfTypeAll<CrystalFiller>();
        int count = 0;

        foreach (var filler in allFillers)
        {
            if (EditorUtility.IsPersistent(filler))
            {
                continue;
            }

            if (filler.uniqueFillerID == id)
            {
                count++;

                if (count > 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
#endif
}