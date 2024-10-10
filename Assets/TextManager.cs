using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
public class TextManager : MonoBehaviour
{


    public TMPro.TextMeshPro textLarge;
    public TMPro.TextMeshPro textSmall;
    public TMPro.TextMeshPro textInfo;


    public Transform tmpTextTransform;

    public void OnEnable()
    {
        SetLargeText("");
        SetSmallText("");
        SetInfoText("");

    }

    public void SetLargeText(string text)
    {
        CalculateTextPosition(God.camera.transform, 1.5f);
        textLarge.transform.position = tmpTextTransform.position;
        textLarge.transform.rotation = tmpTextTransform.rotation;
        textLarge.text = text;
    }

    public void SetSmallText(string text)
    {
        CalculateTextPosition(God.camera.transform, 1.5f);
        textSmall.transform.position = tmpTextTransform.position;
        textSmall.transform.rotation = tmpTextTransform.rotation;
        textSmall.text = text;
    }

    public void SetInfoText(string text)
    {
        CalculateTextPosition(God.camera.transform, 1.5f);
        textInfo.transform.position = tmpTextTransform.position;
        textInfo.transform.rotation = tmpTextTransform.rotation;
        textInfo.text = text;
    }



    public void SetLargeText(string text, Transform t)
    {
        CalculateTextPosition(t, 1.5f);
        textLarge.transform.position = tmpTextTransform.position;
        textLarge.transform.rotation = tmpTextTransform.rotation;
        textLarge.text = text;
    }

    public void SetSmallText(string text, Transform t)
    {
        CalculateTextPosition(t, 1.5f);
        textSmall.transform.position = tmpTextTransform.position;
        textSmall.transform.rotation = tmpTextTransform.rotation;
        textSmall.text = text;
    }

    public void SetInfoText(string text, Transform t)
    {
        CalculateTextPosition(t, 1.5f);
        textInfo.transform.position = tmpTextTransform.position;
        textInfo.transform.rotation = tmpTextTransform.rotation;

        textInfo.text = text;
    }

    public void CalculateTextPosition(Transform lookFrom, float distance)
    {
        tmpTextTransform.position = lookFrom.position + lookFrom.forward * distance;
        tmpTextTransform.rotation = lookFrom.rotation;

    }




}
