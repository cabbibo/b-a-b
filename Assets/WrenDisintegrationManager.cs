using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
public class WrenDisintegrationManager : MonoBehaviour
{


    public string layerWhole;
    public string layerDisintegrated;

    public Wren wren;

    public float disintegrationTime;
    public float reintegrationDistance = 5;


    public void OnEnable()
    {
        SetLayer(layerWhole);
    }


    public bool isRunning = false;
    Coroutine disintegrateRoutine;
    public void Disintegrate()
    {


        print("disintegrate");
        SetLayer(layerDisintegrated);
        if (wren.state.onGround == true)
        {

        }
        else
        {
            wren.bird.Reintegrate(); // only reintegrate if we are not on the ground
        }

        wren.bird.Disintegrate();
        StopAllCoroutines();

        if (isRunning == true)
        {
            StopCoroutine(disintegrateRoutine);
        }

        disintegrateRoutine = StartCoroutine(DisintegrateRoutine());

    }

    public void Reintegrate()
    {
        print("reintegrate");
        SetLayer(layerWhole);

        if (wren.state.onGround == true)
        {

        }
        else
        {
            wren.bird.Reintegrate(); // only reintegrate if we are not on the ground
        }
    }

    public IEnumerator DisintegrateRoutine()
    {

        isRunning = true;

        yield return new WaitForSeconds(disintegrationTime);

        // TODO see if there is a way to check if we are currently "inside" a mesh 
        // maybe ray case from way above, see what layer we hit, if its not terrain, wait to reintegrate?
        // ray cast from behind, and from in front, if they both hit same object we are in it? wait to reintegrate?
        // see how close wrens closest object is if its too close wait to reintegrate

        print("startReintegrate");
        print(God.wren.physics.rawDistToGround);
        print(God.wren.physics.closestTag);

        /*if( God.wren.physics.closestTag == "Terrain" && God.wren.physics.rawDistToGround < reintegrationDistance)
        {
            Reintegrate();
            isRunning = false;
            yield break;
        }*/

        bool canReintegrate = false;

        string layer = LayerMask.LayerToName(God.wren.physics.closestObject.layer);
        while (canReintegrate == false)
        {

            if (layer == "Terrain" || God.wren.physics.rawDistToGround > reintegrationDistance)
            {
                canReintegrate = true;
                yield return new WaitForSeconds(.1f);
            }
            else
            {
                yield return new WaitForSeconds(.1f);
            }

        }

        /*

                string layer = LayerMask.LayerToName(God.wren.physics.closestObject.layer);
                while (God.wren.physics.rawDistToGround < reintegrationDistance)
                {



                    if (layer == "Terrain")
                    {
                        print("terrain");
                        continue;
                    }
                    else
                    {
                        print("not terrain");
                        yield return new WaitForSeconds(.1f);
                        continue;
                    }


                }*/

        print("next section before reintergate");


        Reintegrate();

        isRunning = false;

    }

    public void DoGroundHit()
    {

    }


    public void SetLayer(string layer)
    {
        print("set layer " + layer);
        wren.gameObject.layer = LayerMask.NameToLayer(layer);

        SetGameLayerRecursive(wren.gameObject, LayerMask.NameToLayer(layer));
    }


    // TODO: dont need to set all these
    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);

        }
    }


}
