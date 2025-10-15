using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Playables;


namespace WrenUtils
{
    [ExecuteAlways]
    public class God : MonoBehaviour
    {
        public MenuController _menu;

        public bool updateInEdit;

        public Camera      _camera;
        public Terrain     _terrain;
        public TerrainData _terrainData;
        public Vector3     _terrainOffset;

        public IslandData       _islandData;
        public IslandController _islandController;

        public PlayableDirector _playableDirector;

        public bool _hasIslandData;

        public Wren      _localWren;
        public WrenMaker _wrenMaker;

        public FullState _state;

        public Sounds      _sounds;
        public AudioPlayer _audio;

        public List<Wren>     _wrens;
        public List<RingSet>  _races;
        public ControllerTest _input;

        public FullInterface _groundInterface;
        public AirInterface  _airInterface;

        public OceanInfoManager _oceanInfo;


        public CollectableController _collectableController;


        public GlitchHit _glitchHit;

        public Light _sun;

        public WrenCanDo _wrenCanDo;


        public ParticleSystems _particleSystems;
        public FeedbackSystems _feedbackSystems;


        public List<Transform> _targetableObjects;


        public Tween _tween;

        public SceneController _sceneController;

        public SkyboxUpdater _skyboxUpdater;


        public LerpTo         _lerpTo;
        public PostController _postController;

        public bool inCutScene;

        public Texture fullColorMap;

        public BiomeController _biomeController;

        public OverallCameraManager _cameraManager;

        public TextManager _text;


        public WeatherManager _weatherManager;


        public InterfaceTutorial _interfaceTutorial;


        private static God _instance;

        public static God instance
        {
            get
            {
                if ( _instance == null ) {
                    _instance = FindObjectOfType<God>(); //WithTag GetComponent

                    if ( _instance != null && Application.isPlaying ) {
                        DontDestroyOnLoad( _instance.gameObject );
                    }

                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        private static void Bootstrap()
        {
            if ( _instance == null ) {
                _instance = FindObjectOfType<God>();

                if ( _instance == null ) {
                    Debug.LogError( "No God instance in scene at startup!" );
                } else {
                    if ( Application.isPlaying ) {
                        DontDestroyOnLoad( _instance.gameObject );
                    }
                }
            }
        }

        private void Awake()
        {
            if ( _instance == null ) {
                _instance = this;

                if ( Application.isPlaying ) {
                    DontDestroyOnLoad( gameObject );
                }
            } else if ( _instance != this ) {
                DestroyImmediate( gameObject );
            }
        }


        public Scene _currentScene;

        public static Scene currentScene
        {
            set => instance._currentScene = value;
            get => instance._currentScene;
        }

        public static LerpTo lerpTo => instance._lerpTo;

        public static PostController postController => instance._postController;


        public static CollectableController collectableController => instance._collectableController;

        public static OceanInfoManager oceanInfo => instance._oceanInfo;
        public static Tween tween => instance._tween;

        public static MenuController menu => instance._menu;

        public static IslandController islandController => instance._islandController;

        public static PlayableDirector playableDirector
        {
            get => instance._playableDirector;
            set => instance._playableDirector = value;
        }

        //cempa says : oooooooooooooooooo

        // poooooooooo

        public static void SetPlayableDirector( PlayableDirector pd )
        {
            instance._playableDirector = pd;
        }

        public static void SetIslandController( IslandController ic )
        {
            instance._islandController = ic;
        }


        public static SceneController sceneController => instance._sceneController;

        public static ControllerTest input => instance._input;

        public static GlitchHit glitchHit => instance._glitchHit;


        public static Sounds sounds => instance._sounds;

        public static ParticleSystems particleSystems => instance._particleSystems;

        public static FeedbackSystems feedbackSystems => instance._feedbackSystems;


        public static AudioPlayer audio => instance._audio;

        public static Camera camera => instance._camera;

        public static OverallCameraManager cameraManager => instance._cameraManager;


        public static Wren wren => instance._localWren;

        public static WeatherManager weatherManager => instance._weatherManager;

        public static List<Wren> wrens => instance._wrenMaker.wrens;

        public static List<RingSet> races => instance._races;

        public static Terrain terrain
        {
            get
            {

                if ( instance._terrain == null ) {
                    instance._terrain = (Terrain)FindObjectOfType( typeof(Terrain) );
                }

                return instance._terrain;
            }
        }

        public static bool hasIslandData => instance._hasIslandData;

        public static Vector3 terrainOffset => instance._terrainOffset;

        public static void SetIslandData( IslandData islandData )
        {
            instance._islandData = islandData;
            instance._terrain = islandData.terrain;
            instance._terrainData = islandData.terrain.terrainData;
            instance._terrainOffset = islandData.transform.position;
            instance._hasIslandData = true;
        }


        public static void UnsetIslandData()
        {
            instance._hasIslandData = false;

        }

        public static IslandData islandData
        {
            get
            {

                if ( instance._islandData == null ) {
                    instance._islandData = (IslandData)FindObjectOfType( typeof(IslandData) );
                }

                return instance._islandData;
            }
        }

        public static TerrainData terrainData => instance._terrainData;

        public static FullInterface groundInterface => instance._groundInterface;

        public static AirInterface airInterface => instance._airInterface;

        public static WrenMaker wrenMaker => instance._wrenMaker;


        public static FullState state => instance._state;

        public static List<Transform> targetableObjects => instance._targetableObjects;


        public static SkyboxUpdater skyboxUpdater => instance._skyboxUpdater;

        public static BiomeController biomeController => instance._biomeController;


        public static TextManager text => instance._text;


        public static Light sun => instance._sun;

        public static WrenCanDo wrenCanDo => instance._wrenCanDo;

        public static InterfaceTutorial interfaceTutorial => instance._interfaceTutorial;


        public void SetTerrainCompute( int kernel , ComputeShader shader )
        {

            if ( terrainData != null ) {
                shader.SetTexture( kernel , "_HeightMap" , terrainData.heightmapTexture );
                shader.SetVector( "_MapSize" , terrainData.size );
                shader.SetVector( "_MapOffset" , terrainOffset );
                shader.SetInt( "_HasIslandData" , hasIslandData ? 1 : 0 );
            }
        }

        public void SetTerrainMPB( MaterialPropertyBlock mpb )
        {
            if ( terrainData != null ) {
                mpb.SetTexture( "_HeightMap" , terrainData.heightmapTexture );
                mpb.SetVector( "_MapSize" , terrainData.size );
                mpb.SetVector( "_MapOffset" , terrainOffset );
                mpb.SetInt( "_HasIslandData" , hasIslandData ? 1 : 0 );
            }
        }


        public void SetWrenCompute( int kernel , ComputeShader shader )
        {
            if ( wrenMaker.wrenBuffer != null ) {
                shader.SetBuffer( 0 , "_WrenBuffer" , wrenMaker.wrenBuffer );
                shader.SetInt( "_NumWrens" , wrenMaker.numWrens );
            }
        }


        private Vector3 p1;

        public static Wren ClosestWren( Vector3 p )
        {
            return instance._ClosestWren( p );
        }

        public Wren _ClosestWren( Vector3 p )
        {
            float closest = 1000000;
            var wren = _localWren;

            foreach (var w in _wrens) {
                p1 = w.transform.position - p;

                if ( p1.magnitude < closest ) {
                    wren = w;
                    closest = p1.magnitude;
                }
            }

            return wren;
        }


        public static bool IsOurWren( Collider c )
        {

            bool result = false;

            if ( c.attachedRigidbody != null && wren != null ) {
                if ( c.attachedRigidbody.gameObject == wren.gameObject ) {
                    result = true;
                }
            }


            return result;

        }

        public static bool IsOurWren( Collision c )
        {
            return IsOurWren( c.collider );
        }


        public static WrenBeacon ClosestBeacon( Vector3 p )
        {
            return instance._ClosestBeacon( p );
        }

        public WrenBeacon _ClosestBeacon( Vector3 p )
        {
            float closest = 1000000;
            var beacon = _localWren.beacon;

            foreach (var w in _wrens) {
                p1 = w.beacon.transform.position - p;

                if ( p1.magnitude < closest ) {
                    beacon = w.beacon;
                    closest = p1.magnitude;
                }
            }

            return beacon;
        }


        public static void GetWrenSavedPosition()
        {


            // float x = PlayerPrefs.GetFloat("_CurrentWrenX", 0);
            // float y = PlayerPrefs.GetFloat("_CurrentWrenY", 100);
            // float z = PlayerPrefs.GetFloat("_CurrentWrenZ", 0);

            state.LoadState();
            wren.startingPosition.position = state.lastPosition;
            wren.FullReset();

        }


        public static void SetWrenSavedPosition( Vector3 v )
        {

            PlayerPrefs.SetFloat( "_CurrentWrenX" , v.x );
            PlayerPrefs.SetFloat( "_CurrentWrenY" , v.y );
            PlayerPrefs.SetFloat( "_CurrentWrenZ" , v.z );

        }

        public static Vector3 NormalizedPosition( Vector3 p , Vector3 mapSize , Vector3 offset )
        {
            var difference = p - offset - Vector3.right * mapSize.x / 2 - Vector3.forward * mapSize.z / 2;

            return new Vector3(
                (difference.x + mapSize.x / 2) / mapSize.x ,
                difference.y / mapSize.y ,
                (difference.z + mapSize.z / 2) / mapSize.z
            );

        }


        public static Vector3 NormalizedPositionInMap( Vector3 p )
        {
            return NormalizedPosition( p , terrainData.size , terrainOffset );
        }

        public static Vector2 UVInMap( Vector3 p )
        {
            var nPos = NormalizedPosition( p , terrainData.size , terrainOffset );

            return new Vector2( nPos.x , nPos.z );

        }

        /*
                public static Vector2 UVInMap(Vector3 p, IslandData islandData)
                {
                    // Transform to local terrain space
                    p -= islandData.transform.position;
                    p += new Vector3(islandData.terrainData.size.x / 2, 0, islandData.terrainData.size.z / 2);

                    return new Vector2(p.x / islandData.terrainData.size.x, p.z / islandData.terrainData.size.z);

                }*/


        // Updates in Edit Mode!
        private void OnDrawGizmos()
        {

#if UNITY_EDITOR
            // Ensure continuous Update calls.
            if ( !Application.isPlaying && updateInEdit ) {

                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif

        }


        private void Update()
        {


            if ( terrainData != null ) {
                Shader.SetGlobalTexture( "_HeightMap" , terrainData.heightmapTexture );
                Shader.SetGlobalVector( "_MapSize" , terrainData.size );
                Shader.SetGlobalVector( "_MapOffset" , terrainOffset );
                Shader.SetGlobalInt( "_HasIslandData" , hasIslandData ? 1 : 0 );
                Shader.SetGlobalTexture( "_FullColorMap" , fullColorMap );
            }

            if ( wren ) {
                Shader.SetGlobalVector( "_WrenPos" , wren.transform.position );
            }

            Shader.SetGlobalFloat( "_UnscaledTime" , Time.unscaledTime );

            if ( Input.GetKey( "escape" ) ) {
                Application.Quit();
            }
        }
    }
}