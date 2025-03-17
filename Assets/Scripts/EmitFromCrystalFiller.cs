using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitFromCrystalFiller : MonoBehaviour
{

    public CrystalFiller crystalFiller;

    public ParticleSystem particleSystem;

    public void OnEnable()
    {
        crystalFiller.OnPartialFillEvent.AddListener(EmitFromCrystalCollect);
        var main = particleSystem.main;
        main.maxParticles = Mathf.RoundToInt(crystalFiller.numCrystalsToFill); // dont need more than the amount!
        EmitFromCrystalCollect(crystalFiller.currentNumCrystals); // start by emitting the current amount

    }
    public void EmitFromCrystalCollect(int numShards)
    {

        //        print(numShards);
        particleSystem.transform.position = crystalFiller.transform.position;
        particleSystem.Emit(numShards);



    }
}
