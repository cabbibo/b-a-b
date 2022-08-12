using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public delegate void HitRing();

    public event HitRing localPlayerHitRing;

    protected void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Wren>()?.state.isLocal == true) {
            localPlayerHitRing?.Invoke();
        }
    }
}
