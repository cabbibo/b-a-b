using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[ExecuteAlways]
public class AudioPlayer:MonoBehaviour{

[Header("User Set")]
public AudioMixer defaultMixer;
public int numSources;
public GameObject sourceHolder;



[Header("Debug")]
  public int playID;
  public int oPlayID;



  

  public int loopPlayID;
  public int loopOPlayID;



    public static AudioPlayer Instance { get; private set; }

    private static AudioPlayer _instance;

    [HideInInspector]public GameObject[] objects;
    [HideInInspector]public AudioSource[] sources;



    private AudioMixerGroup defaultMixerGroup;


    void OnEnable(){


        
        // Destroy All Game Objects that are holding sound
        if( objects != null ){
            for( int i = 0; i < objects.Length; i++ ){
                Object.DestroyImmediate(objects[i]);//.Destroy();
            }
        }else{
            print("nullll");
        }

        defaultMixerGroup = defaultMixer.FindMatchingGroups("default")[0];

        sources = new AudioSource[numSources];
        objects = new GameObject[numSources];


        // Generate a bunch of sources!
        for( int i = 0; i < numSources; i++){
            objects[i] = new GameObject();
            objects[i].transform.parent = sourceHolder.transform;
            sources[i] = objects[i].AddComponent<AudioSource>() as AudioSource;
            sources[i].dopplerLevel = 0;
            sources[i].playOnAwake = false;
        }



        loops = new List<Loop>();

        



    }

    // Destroy all the game objects again!
    void OnDisable(){
        if( objects != null ){
        for( int i = 0; i < objects.Length; i++ ){
                Object.DestroyImmediate(objects[i]);//.Destroy();
            }
        }
    }


     public void Update(){
        foreach( Loop loop in loops ){

            if( loop.enabled ){
                loop.Update();
            }
        }
    }

    public void BasePlay( AudioClip clip ){

    
        if( sources.Length>playID){
            if( sources[playID] != null ){
                sources[playID].clip = clip;
                sources[playID].Play();

                oPlayID = playID;
                playID ++;
                playID %= numSources;
            }
        }

    }


    //Reset to the default values!
    public void Reset(){


            if( sources.Length>playID){
                if( sources[playID] != null ){
                    sources[playID].volume = 1;
                    sources[playID].pitch = 1;
                    sources[playID].time = 0.00000001f;
                    sources[playID].transform.position = Vector3.zero;
                    sources[playID].spatialize=false;
                    sources[playID].spatialBlend = 0;
                    sources[playID].dopplerLevel = 0;
                    sources[playID].outputAudioMixerGroup = defaultMixerGroup;
                }
            }
        
    }

    public void Play( AudioClip clip ){
        Reset();
        BasePlay(clip);
    }

    public void Play( AudioClip clip , float pitch){
        Reset();
        sources[playID].volume = 1;
        sources[playID].pitch = pitch;
        BasePlay(clip);
    }

    public void Play( AudioClip clip , float pitch , float volume){
        Reset();
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        BasePlay(clip);
    }

    public void Play( AudioClip clip , int step , float volume ){
        Reset();
        float p = Mathf.Pow( 1.05946f , (float)step );
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        BasePlay(clip);
    }

    public void Play(AudioClip clip , int step , float volume,float location){
        Reset();
        float p = Mathf.Pow( 1.05946f , (float)step );
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].time = location;
        BasePlay(clip);
    }

    public void Play(AudioClip clip , int step , float volume,float location,float length){
        Reset();
        float p = Mathf.Pow( 1.05946f , (float)step );
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].time = location;
        sources[playID].SetScheduledEndTime( AudioSettings.dspTime +.25f );
        BasePlay(clip);
    }

    public void Play( AudioClip clip , int step , float volume, AudioMixer mixer, string group ){
        Reset();
        float p = Mathf.Pow( 1.05946f , (float)step );
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].outputAudioMixerGroup = mixer.FindMatchingGroups(group)[0];
        BasePlay(clip);
    }


    public void Play(AudioClip clip , float pitch , float volume,float location,float length){
        Reset();
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        sources[playID].time = location;
        BasePlay(clip);
        sources[oPlayID].SetScheduledEndTime( AudioSettings.dspTime + length );
    }


    public void Play(AudioClip clip , float pitch , float volume,float location,float length, AudioMixer mixer, string group){
        
        Reset();
        sources[playID].volume = volume;
        sources[playID].pitch = pitch;
        sources[playID].time = location;
        sources[playID].outputAudioMixerGroup = mixer.FindMatchingGroups(group)[0];
        BasePlay(clip);
        sources[oPlayID].SetScheduledEndTime( AudioSettings.dspTime + length );
    }


    // plays sound at 3d location with mixer and default distances
    public void Play(AudioClip clip , float pitch , float volume,float location,float length, AudioMixer mixer, string group , Vector3 pos ){        sources[playID].volume = volume;
        
        Reset();
        sources[playID].clip = clip;
        sources[playID].pitch = pitch;
        sources[playID].time = location;

        if( location  < 0 || location > sources[playID].clip.length ){
            print( clip );
            print( "HI");
        }

        sources[playID].outputAudioMixerGroup = mixer.FindMatchingGroups(group)[0];
        sources[playID].transform.position = pos;
        sources[playID].spatialize= true;
        sources[playID].spatialBlend = 1;
        sources[playID].maxDistance = 50;
        sources[playID].minDistance = 1000;

        BasePlay(clip);

        sources[oPlayID].SetScheduledEndTime( AudioSettings.dspTime + length );
    }


        // plays sound at 3d location
      public void Play( AudioClip clip , int step , float volume , Vector3 location , float falloff ){

        Reset();
        float p = Mathf.Pow( 1.05946f , (float)step );
        sources[playID].volume = volume;
        sources[playID].pitch = p;
        sources[playID].spatialize = true;
        sources[playID].spatialBlend = 1;
        sources[playID].maxDistance = falloff;
        sources[playID].minDistance = falloff/10;
        objects[playID].transform.position = location;
        BasePlay(clip);
    }



        // plays random sound in array
        public void Play( AudioClip[] clips  ){
          Reset();
          int clipID = (int)Mathf.Floor(Random.Range(0, clips.Length));
          BasePlay(clips[clipID]);
      }

      
        // plays random sound in array with volume
      public void Play( AudioClip[] clips  , float volume ){
          Reset();
          sources[playID].volume = volume;
          int clipID = (int)Mathf.Floor(Random.Range(0, clips.Length));
          BasePlay(clips[clipID]);
      }

      

    // Plays sound with specific mixer
    public void Play( AudioClip[] clips  , float volume , string group ){
        Reset();
        sources[playID].volume = volume;
        sources[playID].outputAudioMixerGroup = defaultMixer.FindMatchingGroups(group)[0];
        int clipID = (int)Mathf.Floor(Random.Range(0, clips.Length-.0001f));
        BasePlay(clips[clipID]);
      }








    public void FadeIn( AudioClip clip , float volume , float time ){

        

    }


    

    
    public List<Loop> loops;


    public void DestroyLoop(Loop loop){

        Destroy(loop.source1);
        Destroy(loop.source2);
        loops.Remove( loop );

    }

    public Loop MakeLoop(AudioClip clip ){


        var loop = new Loop();

        loop.source1 = gameObject.AddComponent<AudioSource>();
        loop.source2 = gameObject.AddComponent<AudioSource>();
        loop.clip = clip;
        loop.source1.clip = clip;
        loop.source2.clip = clip;
        loop.length = clip.length;
        loop.enabled = false;
        loop.flipFlop = false;
        
        // Defaults
        loop.fadeSpeed = .01f;
        loop.maxVolume = 1;

        loop.player = this;
     

        loops.Add( loop );
    
        return loop;


    }


    public void EndLoop(Loop loop ){
        loop.fadeSpeed = -.03f;
    }




   

}

    public class Loop{
        public AudioClip clip;

        // some sources to ping pong
        public AudioSource source1;
        public AudioSource source2;

        public float fadeSpeed;

        public float length;

        public float timer;

        public float maxVolume;
        public float currentVolume;

        public AudioPlayer player;

        public bool flipFlop;
        public bool enabled;

        public bool shouldEndAtNextLoop;
        public bool shouldEnd;
        public virtual void Update(){

            timer += Time.deltaTime;


            if( !shouldEndAtNextLoop ){

                if( timer > length ){
                    timer -= length;
                    FlipFlop();
                }

            // if we want it to end after this loop!
            }else{
                if( !source1.isPlaying && !source2.isPlaying ){
                    player.DestroyLoop(this);
                }
            }

            currentVolume += fadeSpeed;
         

            if( currentVolume < 0  && shouldEnd ){
                player.DestroyLoop( this );
            }


            currentVolume = Mathf.Clamp( currentVolume ,0 , maxVolume);
            
            source1.volume = currentVolume;
            source2.volume = currentVolume;


        }


        // Stored Info!
        public void FlipFlop(){

            if( flipFlop ){
                source2.Play();
            }else{
                source1.Play();
            }
            
        }

        public void SetFadeSpeed( float f){
            fadeSpeed = f;
        }

        public void GenerateLengthByBPMBarsAndBeats( float bpm , float bars , float beats ){

            float totalBeats = beats * bars;
            float beatsPerSecond = bpm / 60;
            float secondsPerBeat = 1 / beatsPerSecond;
            float totalSeconds = secondsPerBeat * totalBeats;

            Debug.Log("Total Seconds : " + totalSeconds);
            length = totalSeconds;



        }

        public void Start(){
            source1.Play();
            Update();
            enabled = true;
        }


        public void End(){

        }

        public void EndAfterNextLoop(){
            shouldEndAtNextLoop = true;
        }

        public void FadeOut(float fadeOutSpeed){
            shouldEnd = true;
            fadeSpeed = fadeOutSpeed;
        }

    }