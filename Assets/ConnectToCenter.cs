using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof(ConnectToCenter) )]
public class ConnectToCenterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var connectToCenter = (ConnectToCenter)target;

        if ( GUILayout.Button( "Connect All" ) ) {
            connectToCenter.ConnectAll();
        }

        if ( GUILayout.Button( "Disconnect All" ) ) {
            connectToCenter.DisconnectAll();
        }

        DrawDefaultInspector();
    }
}


#endif


[System.Serializable]
public class ConnectToCenterEvent : UnityEvent<GameObject , GameObject>
{
}

[ExecuteAlways]
public class ConnectToCenter : MonoBehaviour
{
    public float connectionSpeed = 3f;

    public LineRenderer[] lines;
    public GameObject[]   objectsToConnect;
    public GameObject[]   connectionPoints;

    public bool[] connected = new bool[0];

    public ConnectToCenterEvent OnConnectEvent    = new();
    public ConnectToCenterEvent OnDisconnectEvent = new();

    public UnityEvent<ConnectToCenter> OnAllConnected = new();

    public AudioClip[] connectionClips;


    // public LineRendererPrefab linePrefab;
    // Start is called before the first frame update
    private void Start()
    {

        print( "hello i am starting" );

        if ( connected == null || connected.Length != objectsToConnect.Length ) {
            connected = new bool[objectsToConnect.Length];
        }

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void SetOn()
    {

        print( "hello i am setting on" );

        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            lines[i].enabled = true;
            connectionPoints[i].SetActive( true );
            lines[i].SetPosition( 0 , objectsToConnect[i].transform.position );
            lines[i].SetPosition( 1 , transform.position );
            connectionPoints[i].transform.position = transform.position;
            connected[i] = true;
        }
    }

    public void SetOff()
    {
        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            lines[i].enabled = false;
            lines[i].SetPosition( 0 , objectsToConnect[i].transform.position );
            lines[i].SetPosition( 1 , objectsToConnect[i].transform.position );
            connectionPoints[i].transform.position = objectsToConnect[i].transform.position;
            connected[i] = false;
        }
    }

    public void ConnectAll()
    {
        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            Connect( objectsToConnect[i] );
        }

    }

    public void DisconnectAll()
    {
        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            UndoConnect( objectsToConnect[i] );
        }

    }


    public void ToggleConnect( GameObject obj )
    {
        int id = -1;

        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            if ( objectsToConnect[i] == obj ) {
                id = i;
            }
        }

        if ( id == -1 ) {
            Debug.LogError( "Object not found in objectsToConnect array." );
            return;
        }

        if ( connected[id] == true ) {
            UndoConnect( obj );
        } else {
            Connect( obj );
        }

    }

    public void Connect( GameObject obj )
    {
        int id = -1;

        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            if ( objectsToConnect[i] == obj ) {
                id = i;
            }
        }

        if ( id == -1 ) {
            Debug.LogError( "Object not found in objectsToConnect array." );
            return;
        }


        if ( connected[id] == true ) {
            return;
        }


        God.audio.Play( connectionClips , Random.Range( .8f , 1.2f ) );
        

        OnConnectEvent.Invoke( objectsToConnect[id] , connectionPoints[id] );
        StartCoroutine( ConnectCoroutine( id ) );

    }

    public void UndoConnect( GameObject obj )
    {
        int id = -1;

        for ( int i = 0; i < objectsToConnect.Length; i++ ) {
            if ( objectsToConnect[i] == obj ) {
                id = i;
            }
        }

        if ( id == -1 ) {
            Debug.LogError( "Object not found in objectsToConnect array." );
            return;
        }


        if ( connected[id] == false ) {
            return;
        }

        God.audio.Play( connectionClips , Random.Range( -.8f , -1.2f ) );
        OnDisconnectEvent.Invoke( objectsToConnect[id] , connectionPoints[id] );

        StartCoroutine( DisconnectCoroutine( id ) );
    }


    public void OnConnectEnd( int id )
    {

        // Do something when the connection ends
        Debug.Log( "Connection ended." );
        connected[id] = true;

        for ( int i = 0; i < connected.Length; i++ ) {
            if ( !connected[i] ) {
                return;
            }
        }

        OnAllConnected.Invoke( this );


    }

    public void OnDisconnectEnd( int id )
    {
        // Do something when the connection ends
        Debug.Log( "Disconnection ended." );
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


    public IEnumerator ConnectCoroutine( int id )
    {
        var start = objectsToConnect[id].transform.position;
        var end = transform.position;

        var line = lines[id];
        line.enabled = true;


        float t = 0;

        while (t < connectionSpeed) {
            t += Time.deltaTime;

            float nT = t / connectionSpeed;
            // Calculate the current position of the line renderer
            var currentPosition = Vector3.Lerp( start , end , nT * nT );

            connectionPoints[id].transform.position = currentPosition;
            // Set the position of the line renderer
            line.SetPosition( 0 , start );
            line.SetPosition( 1 , currentPosition );

            yield return null;
        }

        line.SetPosition( 0 , start );
        line.SetPosition( 1 , end );
        line.enabled = true;

        OnConnectEnd( id );

        // Instantiate a new line renderer

        yield return null;
    }


    public IEnumerator DisconnectCoroutine( int id )
    {
        var start = objectsToConnect[id].transform.position;
        var end = transform.position;

        var line = lines[id];
        line.enabled = true;


        float t = 0;

        while (t <= 1) {
            // Calculate the current position of the line renderer
            var currentPosition = Vector3.Lerp( end , start , t );
            t += Time.deltaTime;

            connectionPoints[id].transform.position = currentPosition;
            // Set the position of the line renderer
            line.SetPosition( 0 , start );
            line.SetPosition( 1 , currentPosition );

            yield return null;
        }

        line.SetPosition( 0 , start );
        line.SetPosition( 1 , start );
        line.enabled = false;

        OnDisconnectEnd( id );
        // Instantiate a new line renderer

        yield return null;
    }
}