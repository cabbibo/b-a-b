using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.UI;

public class ChooseControlsSequence : MonoBehaviour
{
    public Canvas canvas;

    public CanvasGroup canvasTopHalf;
    

    void OnEnable()
    {
        canvas.worldCamera = Camera.main;
    }

    IEnumerator Start()
    {
        while (God.wren == null)
        {
            yield return null;
        }
        var wren = God.wren;

        var b = wren.bird;
        b.drawBodyFeather = b.drawLeftWingFeathers = b.drawRightWingFeathers = false;
        b.drawBodyFeatherPoints = b.drawLeftWingFeatherPoints = b.drawRightWingFeatherPoints = true;


        yield return new WaitForSeconds(.5f);

        wren.state.TakeOff();



    }








}
