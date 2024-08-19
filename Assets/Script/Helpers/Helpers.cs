using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;

public static class Helpers
{
  [System.Serializable]
  public class PositionEvent : UnityEvent<Vector3> { }


  [System.Serializable]
  public class FloatEvent : UnityEvent<float> { }

  [System.Serializable]
  public class IntEvent : UnityEvent<int> { }


  [System.Serializable]
  public class DoubleIntEvent : UnityEvent<int, int> { }


  [System.Serializable]
  public class PositionGameObjectEvent : UnityEvent<Vector3, GameObject> { }


  [System.Serializable]
  public class BoostEvent : UnityEvent<Booster> { }

  public static T GetOrAddComponent<T>(this GameObject go) where T : Component
  {
    return go.GetComponent<T>() ?? go.AddComponent<T>();
  }

  public static T GetOrAddComponent<T>(this Component component) where T : Component
  {
    return component.gameObject.GetOrAddComponent<T>();
  }


  public static Vector3 QuadraticBez(float v, Vector3 p1, Vector3 p2, Vector3 p3)
  {
    return (1 - v) * (1 - v) * p1 + 2 * (1 - v) * v * p2 + v * v * p3;//P2
  }

  public static bool IsIndexValid<T>(this List<T> list, int index)
  {
    return index >= 0 && index < list.Count;
  }

  public static bool IsIndexValid<T>(this T[] array, int index)
  {
    return index >= 0 && index < array.Length;
  }



  public static Vector3 cubicCurve(float t, Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
  {

    float s = 1 - t;

    Vector3 v1 = c0 * (s * s * s);
    Vector3 v2 = 3 * c1 * (s * s) * t;
    Vector3 v3 = 3 * c2 * s * (t * t);
    Vector3 v4 = c3 * (t * t * t);

    Vector3 value = v1 + v2 + v3 + v4;

    return value;

  }




  public static float cubicCurve(float t, float c0, float c1, float c2, float c3)
  {

    float s = 1 - t;

    float v1 = c0 * (s * s * s);
    float v2 = 3 * c1 * (s * s) * t;
    float v3 = 3 * c2 * s * (t * t);
    float v4 = c3 * (t * t * t);

    float value = v1 + v2 + v3 + v4;

    return value;

  }






  public static Vector3 cubicFromValue(float val, Vector3[] points)
  {

    Vector3 p0 = new Vector3();
    Vector3 v0 = new Vector3();
    Vector3 p1 = new Vector3();
    Vector3 v1 = new Vector3();

    Vector3 p2 = new Vector3();

    float vPP = points.Length;// float(_NumVertsPerHair);

    float baseVal = val * (vPP - 1);

    int baseUp = (int)Mathf.Floor(baseVal);
    int baseDown = (int)Mathf.Ceil(baseVal);
    float amount = baseVal - (float)baseUp;

    if (baseUp == baseDown)
    {
      baseVal += .01f;
      baseUp = (int)Mathf.Floor(baseVal);
      baseDown = (int)Mathf.Ceil(baseVal);
      amount = baseVal - (float)baseUp;
    }


    if (baseUp == 0)
    {

      p0 = points[baseUp];
      p1 = points[baseDown];
      p2 = points[baseDown + 1];

      v1 = .5f * (p2 - p0);

    }
    else if (baseDown == vPP - 1)
    {

      p0 = points[baseUp];
      p1 = points[baseDown];
      p2 = points[baseUp - 1];

      v0 = .5f * (p1 - p2);

    }
    else
    {

      p0 = points[baseUp];
      p1 = points[baseDown];


      Vector3 pMinus = new Vector3(0, 0, 0);

      pMinus = points[baseUp - 1];
      p2 = points[baseDown + 1];

      v1 = .5f * (p2 - p0);
      v0 = .5f * (p1 - pMinus);

    }

    Vector3 c0 = p0;
    Vector3 c1 = p0 + v0 / 3;
    Vector3 c2 = p1 - v1 / 3;
    Vector3 c3 = p1;

    Vector3 pos = cubicCurve(amount, c0, c1, c2, c3);
    return pos;

  }

  public static float cubicFromValue(float val, float[] points)
  {

    float p0 = 0;
    float v0 = 0;
    float p1 = 0;
    float v1 = 0;

    float p2 = 0;

    float vPP = points.Length;// float(_NumVertsPerHair);

    float baseVal = val * (vPP - 1);

    int baseUp = (int)Mathf.Floor(baseVal);
    int baseDown = (int)Mathf.Ceil(baseVal);
    float amount = baseVal - (float)baseUp;



    if (baseUp == 0)
    {

      p0 = points[baseUp];
      p1 = points[baseDown];
      p2 = points[baseDown + 1];

      v1 = .5f * (p2 - p0);

    }
    else if (baseDown == vPP - 1)
    {

      p0 = points[baseUp];
      p1 = points[baseDown];
      p2 = points[baseUp - 1];

      v0 = .5f * (p1 - p2);

    }
    else
    {

      p0 = points[baseUp];
      p1 = points[baseDown];


      float pMinus = 0;

      pMinus = points[baseUp - 1];
      p2 = points[baseDown + 1];

      v1 = .5f * (p2 - p0);
      v0 = .5f * (p1 - pMinus);

    }

    float c0 = p0;
    float c1 = p0 + v0 / 3;
    float c2 = p1 - v1 / 3;
    float c3 = p1;

    float pos = cubicCurve(amount, c0, c1, c2, c3);
    return pos;

  }



  public static bool isWrenCollision(Collider c)
  {
    if (God.wren == null) return false;
    return (c.attachedRigidbody == God.wren.physics.rb);
  }


}


