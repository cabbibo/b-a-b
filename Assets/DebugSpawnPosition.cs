using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class DebugSpawnPosition : MonoBehaviour
{
    public KeyCode keyCode = KeyCode.Alpha0;
    public bool useRespawnArea = false;
    public Vector3 size;

    void OnEnable()
    {
        if (!Application.isEditor)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(keyCode))
        {
            God.wren.PhaseShift(transform.position);
            God.wren.physics.transform.forward = transform.forward;
            God.wren.physics.vel = transform.forward * 5;

        }
        if (useRespawnArea)
        {
            var p = God.wren.physics.transform.position;
            var r = new Rect(transform.position, size / 2);
            if (!r.Contains(p) && !respawning)
            {
                StartCoroutine(Respawn());
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 5);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 20 + transform.right * -10);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 20 + transform.right * 10);
        Gizmos.color = Color.white;

        if (useRespawnArea)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }

    bool respawning = false;
    IEnumerator Respawn()
    {
        respawning = true;
        God.wren.PhaseShift(transform.position);
        God.wren.physics.transform.forward = transform.forward;
        God.wren.physics.vel = transform.forward * 5;
        yield return new WaitForSeconds(3f);
        respawning = false;
    }
}
