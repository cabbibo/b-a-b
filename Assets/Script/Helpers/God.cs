using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class God : MonoBehaviour
{


    public MenuController _menu;

    public bool updateInEdit;

    public Camera _camera;
    public Terrain _terrain;
    public TerrainData _terrainData;
    public Wren _localWren;
    public WrenMaker _wrenMaker;

    public FullState _state;

    public Sounds _sounds;
    public AudioPlayer _audio;

    public List<Wren> _wrens;
    public List<RingSet> _races;
    public ControllerTest _input;

    public FullInterface _groundInterface;    
    public AirInterface _airInterface;


    public CollectableController _collectableController;


    public GlitchHit _glitchHit;


    public ParticleSystem _smallSuccessSystem;
    public ParticleSystem _largeSuccessSystem;


    public List<Transform> _targetableObjects;


    public Tween _tween;

    public FadeToBlack _fade;
    public SceneController _sceneController;

    public SkyboxUpdater _skyboxUpdater;


    public LerpTo _lerpTo;
    public PostController _postController;

    public bool inCutScene;


    public static LerpTo lerpTo{
        get{ return instance._lerpTo; }
    }

    public static PostController postController{
        get{ return instance._postController; }
    }


    public static CollectableController collectableController{
        get{ return instance._collectableController; }
    }

    public static Tween tween{
        get{ return instance._tween; }
    }

    public static MenuController menu{
        get{ return instance._menu; }
    }

    public static FadeToBlack fade{
        get{ return instance._fade; }
    }

    public static SceneController sceneController{
        get{ return instance._sceneController;}
    }
    private static God _instance;
    public static God instance{

        get{
            if( _instance == null ){
                _instance = Object.FindObjectOfType<God>();//WithTag GetComponent
            }
            if( Application.isPlaying ){ DontDestroyOnLoad(_instance); }
            return _instance;
        }
    
    }

    public static ControllerTest input{
        get{
            return instance._input;
        }
    }

    public static GlitchHit glitchHit{
       get{ return instance._glitchHit; }
    }


    public static Sounds sounds{
          get{
            return instance._sounds;
        }
    }

    public static AudioPlayer audio{
        get{
            return instance._audio;
        }
    }
    
    public static Camera camera{
        get{
            return instance._camera;
        }
    }

    
    public static Wren wren{
        get{
            return instance._localWren;
        }
    }

    public static List<Wren> wrens{
        get{
            return instance._wrenMaker.wrens;
        }
    }

    public static List<RingSet> races{
        get{
            return instance._races;
        }
    }

    public static Terrain terrain{
        get{

            if( instance._terrain == null ){
                print("FINDING TERRAIN");
                instance._terrain = (Terrain)FindObjectOfType(typeof(Terrain));
                print(instance._terrain);
            }
            return instance._terrain;
        }   
    }

    public static TerrainData terrainData{
        get{
            return instance._terrainData;
        }
    }
    
    public static FullInterface groundInterface{
        get{
            return instance._groundInterface;
        }
    }

    public static AirInterface airInterface{
        get{
            return instance._airInterface;
        }
    }

    public static WrenMaker wrenMaker{
        get{
            return instance._wrenMaker;
        }
    }

    public static ParticleSystem  smallSuccessSystem{
        get{
            return instance._smallSuccessSystem;
        }
    }

     public static ParticleSystem  largeSuccessSystem{
        get{
            return instance._largeSuccessSystem;
        }
    }



    public static FullState state{
        get{
            return instance._state;
        }
    }

     public static List<Transform> targetableObjects{
        get{
            return instance._targetableObjects;
        }
    }


         public static SkyboxUpdater skyboxUpdater{
        get{
            return instance._skyboxUpdater;
        }
    }


    public void SetTerrainCompute( int kernel , ComputeShader shader){
   
        if( terrainData != null ){
        shader.SetTexture( kernel, "_HeightMap" ,  terrainData.heightmapTexture );
        shader.SetVector("_MapSize", terrainData.size);
        }
      }

    public void SetTerrainMPB( MaterialPropertyBlock mpb){
        if( terrainData != null ){
                mpb.SetTexture( "_HeightMap" ,  terrainData.heightmapTexture );
                mpb.SetVector("_MapSize", terrainData.size);
        }
      }


      public void SetWrenCompute( int kernel , ComputeShader shader){
        if( wrenMaker.wrenBuffer != null ){
            shader.SetBuffer( 0, "_WrenBuffer" ,  wrenMaker.wrenBuffer );
            shader.SetInt("_NumWrens", wrenMaker.numWrens);
        }
    }




Vector3 p1;

public static Wren ClosestWren( Vector3 p ){
    return instance._ClosestWren(p);
}
public Wren _ClosestWren( Vector3 p ){
    float closest = 1000000;
    Wren wren= _localWren;
    foreach ( Wren w  in _wrens ){
        p1 = w.transform.position - p;
        if( p1.magnitude < closest){
            wren = w;
            closest = p1.magnitude;
        }
    }

    return wren;
} 


public static bool IsOurWren( Collider c ){

    var result = false;

    if( c.attachedRigidbody != null && God.wren != null){
    if( c.attachedRigidbody.gameObject == God.wren.gameObject ){   
        result = true;
    }}


    return result;

}

public static bool IsOurWren( Collision c ){
    return God.IsOurWren( c.collider );
}



public static WrenBeacon ClosestBeacon( Vector3 p ){
    return instance._ClosestBeacon(p);
}
public WrenBeacon _ClosestBeacon( Vector3 p ){
    float closest = 1000000;
    WrenBeacon beacon = _localWren.beacon;
    foreach ( Wren w  in _wrens ){
        p1 = w.beacon.transform.position - p;
        if( p1.magnitude < closest){
            beacon = w.beacon;
            closest = p1.magnitude;
        }
    }

    return beacon;
} 


public static void GetWrenSavedPosition(){


        float x = PlayerPrefs.GetFloat("_CurrentWrenX",0);
        float y = PlayerPrefs.GetFloat("_CurrentWrenY",100);
        float z = PlayerPrefs.GetFloat("_CurrentWrenZ",0);

        God.wren.startingPosition.position = new Vector3(x,y,z);
        God.wren.FullReset();

}


public static void SetWrenSavedPosition( Vector3 v ){

    PlayerPrefs.SetFloat("_CurrentWrenX",v.x);
    PlayerPrefs.SetFloat("_CurrentWrenY",v.y);
    PlayerPrefs.SetFloat("_CurrentWrenZ",v.z);

}
   // Updates in Edit Mode!
   void OnDrawGizmos()
   {
 
      #if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying && updateInEdit )
            {
        
               UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
               UnityEditor.SceneView.RepaintAll();
            }
      #endif

   }


 void Update()
    {


        if( terrainData != null ){

             Shader.SetGlobalTexture( "_HeightMap" ,  terrainData.heightmapTexture );
            Shader.SetGlobalVector("_MapSize", terrainData.size);
        }
        if( wren ){
            Shader.SetGlobalVector("_WrenPos", wren.transform.position );
        }

        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
        
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}
