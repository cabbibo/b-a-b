using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ConnectToCenter))]
public class ConnectToCenterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ConnectToCenter connectToCenter = (ConnectToCenter)target;
        if (GUILayout.Button("Connect All"))
        {
            connectToCenter.ConnectAll();
        }

          if (GUILayout.Button("Disconnect All"))
        {
            connectToCenter.DisconnectAll();
        }

        DrawDefaultInspector();
    }
}


#endif


[System.Serializable]
public class ConnectToCenterEvent : UnityEngine.Events.UnityEvent<GameObject, GameObject> { }

[ExecuteAlways]
public class ConnectToCenter : MonoBehaviour
{



    public float connectionSpeed = 3f;

    public LineRenderer[] lines;
    public GameObject[] objectsToConnect;
    public GameObject[] connectionPoints;

    public bool[] connected = new bool[0];

    public ConnectToCenterEvent OnConnectEvent = new ConnectToCenterEvent();
    public ConnectToCenterEvent OnDisconnectEvent = new ConnectToCenterEvent();

    public UnityEvent<ConnectToCenter> OnAllConnected = new UnityEvent<ConnectToCenter>();


    // public LineRendererPrefab linePrefab;
    // Start is called before the first frame update
    void Start()
    {

        connected = new bool[objectsToConnect.Length];

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetOn()
    {
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            lines[i].enabled = true;
            connectionPoints[i].SetActive(true);
            lines[i].SetPosition(0, objectsToConnect[i].transform.position);
            lines[i].SetPosition(1, transform.position);
            connectionPoints[i].transform.position = transform.position;
            connected[i] = true;
        }
    }

    public void SetOff()
    {
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            lines[i].enabled = false;
            lines[i].SetPosition(0, objectsToConnect[i].transform.position);
            lines[i].SetPosition(1, objectsToConnect[i].transform.position);
            connectionPoints[i].transform.position = objectsToConnect[i].transform.position;
            connected[i] = false;
        }
    }

    public void ConnectAll()
    {
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            Connect(objectsToConnect[i]);
        }

    }

    public void DisconnectAll()
    {
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            UndoConnect(objectsToConnect[i]);
        }

    }


    public void Connect(GameObject obj)
    {
        int id = -1;
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            if (objectsToConnect[i] == obj)
            {
                id = i;
            }
        }

        if (id == -1)
        {
            Debug.LogError("Object not found in objectsToConnect array.");
            return;
        }


        if (connected[id] == true)
        {
            return;
        }


        OnConnectEvent.Invoke(objectsToConnect[id], connectionPoints[id]);
        StartCoroutine(ConnectCoroutine(id));

    }

    public void UndoConnect(GameObject obj)
    {
        int id = -1;
        for (int i = 0; i < objectsToConnect.Length; i++)
        {
            if (objectsToConnect[i] == obj)
            {
                id = i;
            }
        }
        if (id == -1)
        {
            Debug.LogError("Object not found in objectsToConnect array.");
            return;
        }


        if (connected[id] == false)
        {
            return;
        }
        OnDisconnectEvent.Invoke(objectsToConnect[id], connectionPoints[id]);

        StartCoroutine(DisconnectCoroutine(id));
    }


    public void OnConnectEnd(int id)
    {
        // Do something when the connection ends
        Debug.Log("Connection ended.");
        connected[id] = true;

        for (int i = 0; i < connected.Length; i++)
        {
            if (!connected[i])
            {
                return;
            }
        }

        OnAllConnected.Invoke(this);


    }

    public void OnDisconnectEnd(int id)
    {
        // Do something when the connection ends
        Debug.Log("Disconnection ended.");
        connected[id] = false;

        /*for (int i = 0; i < connected.Length; i++)
        {
            if (connected[i])
            {
                return;
            }
        }

        OnAllConnected.Invoke();*/




    }


    public IEnumerator ConnectCoroutine(int id)
    {
        Vector3 start = objectsToConnect[id].transform.position;
        Vector3 end = transform.position;

        LineRenderer line = lines[id];
        line.enabled = true;


        float t = 0;

        while (t < connectionSpeed)
        {
            t += Time.deltaTime;

            float nT = t / connectionSpeed;
            // Calculate the current position of the line renderer
            Vector3 currentPosition = Vector3.Lerp(start, end, nT * nT);

            connectionPoints[id].transform.position = currentPosition;
            // Set the position of the line renderer
            line.SetPosition(0, start);
            line.SetPosition(1, currentPosition);

            yield return null;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.enabled = true;

        OnConnectEnd(id);

        // Instantiate a new line renderer

        yield return null;
    }




    public IEnumerator DisconnectCoroutine(int id)
    {
        Vector3 start = objectsToConnect[id].transform.position;
        Vector3 end = transform.position;

        LineRenderer line = lines[id];
        line.enabled = true;



        float t = 0;

        while (t <= 1)
        {
            // Calculate the current position of the line renderer
            Vector3 currentPosition = Vector3.Lerp(end, start, t);
            t += Time.deltaTime;

            connectionPoints[id].transform.position = currentPosition;
            // Set the position of the line renderer
            line.SetPosition(0, start);
            line.SetPosition(1, currentPosition);

            yield return null;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, start);
        line.enabled = false;

        OnDisconnectEnd(id);
        // Instantiate a new line renderer

        yield return null;
    }



}
