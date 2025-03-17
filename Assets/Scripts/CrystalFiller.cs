using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

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
    }

    public void OnFilled()
    {
        filled = true;

        God.particleSystems.largeSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.largeSuccessParticleSystem.Emit(3000);
        God.audio.Play(God.sounds.largeSuccessSound);


        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);
        OnFilledEvent.Invoke();

    }


    public void OnUnfilled()
    {
        filled = false;
        OnUnfilledEvent.Invoke();
    }
    public void OnExtraFill(int numCrystals)
    {
        God.particleSystems.smallSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.smallSuccessParticleSystem.Emit(100);
        God.audio.Play(God.sounds.smallSuccessSound);

        OnPercentageFillEvent.Invoke((float)currentNumCrystals / (float)numCrystalsToFill);
        OnPartialFillEvent.Invoke(numCrystals);

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

    }

    public void OnTriggerEnter(Collider other)
    {
        if (God.IsOurWren(other))
        {

            // Check to see how many crystals wren has

            int numCrystals = (int)God.wren.shards.GetShardTrailAmount();
            print("HELLO this is number shaders");
            print(numCrystals);

            FillCrystal(numCrystals);

            God.wren.shards.SpendExtraShards();
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    public float drainedAmount;

    // Update is called once per frame
    void Update()
    {

        // slowly drain the thing we are filling;
        if (drainSpeed > 0)
        {

            drainedAmount += drainSpeed;

            if (drainedAmount >= 1)
            {
                drainedAmount = 0;
                currentNumCrystals--;
                OnPartialFill(-1);
                currentNumCrystals = Mathf.Clamp(currentNumCrystals, 0, maxCrystals);


                if (currentNumCrystals <= numCrystalsToFill)
                {
                    if (filled == true)
                    {
                        filled = false;
                        OnUnfilled();
                    }
                }
            }





        }

    }


}
