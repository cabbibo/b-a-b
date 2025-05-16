using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenMagnetize : MonoBehaviour
{
    public Wren wren;
    public bool isMagnetized = false;

    public GameObject magFeedback;

    public float fadeSpeed = 1f;

    public void DoMagnetize()
    {
        God.audio.Play( God.sounds.magnetizeClip );
        wren.shards.DoMagnetize();
        StartCoroutine( MagnetizeCoroutine() );
    }

    public IEnumerator MagnetizeCoroutine()
    {
        float time = 0;
        magFeedback.SetActive( true );

        while (time < 1) {
            time += Time.deltaTime * fadeSpeed;
            isMagnetized = true;
            magFeedback.transform.localScale = Vector3.one * (1.01f - time);
            yield return null;
        }

        magFeedback.SetActive( false );
        isMagnetized = false;

    }
}