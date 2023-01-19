using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

public class RingSet : MonoBehaviour

{

    public Color color;
    
    public int raceID;
    public GameObject ActiveRing;


    public LineRenderer lineRenderer;

    public StartingPlatform startingPlatform;
    public GameObject endingPlatform;

    [SerializeField] private RaceLeaderboard raceLeaderboard;

    public DebugLines lines;
   



   public Transform ringBase;

    public List<Transform> rings;
    
    public int currentRingIndex = -1;

    public RingSetParticles particles;


    public Renderer startingBase;
    public Renderer endingBase;


    public float leavingSize = 100;
    public float leavingTime = 10;

    private void OnEnable() {
        currentRingIndex = -1;
        SetupRings();
        startingPlatform.race = this;

        startingBase.material.SetColor("_Color", color);
        endingBase.material.SetColor("_Color", color);
        
        canStartRace = false;
        
        finishedRace = false;
        inRace = false;
        canStartRace = false;
        particles.End();
        currentTime = 0;
        
        
        ActiveRing.SetActive(false);
    }

    private void OnDisable(){

    }

    private void SetupLines() {

        int totalPoints = (rings.Count-1) * 40;
        var points = new Vector3[totalPoints];
        
        for (var i=0; i<rings.Count-1; i++) {

            Vector3 p1 = rings[i].position;
            Vector3 f1 = rings[i].forward;
            Vector3 p2 = rings[i+1].position;
            Vector3 f2 = rings[i+1].forward;

            float d = (p1-p2).magnitude;
    
            Vector3 c0 = p1;
            Vector3 c1 = p1 + d*f1/3;
            Vector3 c2 = p2 - d*f2/3;
            Vector3 c3 = p2;

            for( int j = 0; j < 40; j++ ){
                float lVal = (float)j / 40;
                points[i * 40  + j ] = Helpers.cubicCurve( lVal , c0,c1 ,c2,c3);//rings[i].transform.position;
            }


        }

        lineRenderer.positionCount = totalPoints;

        lineRenderer.SetPositions(points);



    }

    private void SetupRings() {
        rings = new List<Transform>(ringBase.childCount);
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        for(var i=0; i<ringBase.childCount; i++) {

           // ring
            var ring = ringBase.GetChild(i);//.GetOrAddComponent<Ring>();
            rings.Add(ring);
            
            if( i == 0 ){ 
                ring.GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor("_Color", color);
            }

            ring.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
            //ring.localPlayerHitRing += OnLocalPlayerHitRing;
        
        }
        

        DeactivateRings();



        ///SetupLines();
    }

    private void OnLocalPlayerHitRing() {

    }


   public bool inRacePlatform;
   public bool canStartRace;
   public bool inRace;
   public bool finishedRace;


    public int currentRing;
    public float distToRing;

    public float raceStartTime;

    public float currentTime;


    public float distToStart;

    public float inRaceVal;


    public void Update(){

        if( inRacePlatform ){
            if( God.wren.state.onGround    ){
                canStartRace = true;
                ActiveRing.SetActive(true);
            }

        }        
        
        if( canStartRace ){
            CheckForStart();
        }

        if( inRace ){
            CheckIfRingsHit();
            UpdateTime();
        }


       //lineRenderer.material.SetFloat("_InRace" , (float)currentRing / (float)rings.Count );

    }

    public void CheckForStart(){
      Vector3 p = rings[0].InverseTransformPoint( God.wren.transform.position );//, rings[0].forward);
        
      //  lines.SetLine( 0 ,rings[0].TransformPoint( Vector3.Scale(p,new Vector3(1,1,0))) , rings[0].position,  1, 1 );

        float newDist = -p.z;
        if( distToStart > 0 && newDist <= 0 ){
            HitRing();
            StartRace();
        }

        distToStart = newDist;
    }


    public void CheckIfRingsHit(){

        
        Vector3 p = rings[currentRing].InverseTransformPoint( God.wren.transform.position );//, rings[currentRing].forward);
    
       // lines.SetLine( 0 ,rings[currentRing].TransformPoint( Vector3.Scale(p,new Vector3(1,1,0))) , rings[currentRing].position,  1, 1 );


        float newDist = -p.z;
        if( distToStart > 0 && newDist <= 0 ){

            if(  p.magnitude > .5f){
                God.wren.state.TransportToPosition( rings[currentRing].position,  rings[currentRing].forward );
                HitRing();
            }else{
                HitRing();
            }


        }

        distToStart = newDist;


    }

    public void HitRing(){

        if( currentRing == rings.Count-1 ){
            FinishRace();
            inRaceVal = 1;
            God.audio.Play(God.sounds.raceEndSound);
        }else{
            currentRing ++;
            
            God.audio.Play(God.sounds.ringHitSounds );
            ActiveRing.transform.parent =  rings[currentRing];
            ActiveRing.transform.localRotation = Quaternion.identity;
            ActiveRing.transform.localPosition = Vector3.zero;
            ActiveRing.transform.localScale = Vector3.one * 50;
            inRaceVal = (float)currentRing / (float)rings.Count;
        }


        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();    
        rings[currentRing].GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat("_Active", 1);
        rings[currentRing].GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
    }


    public void StartPlatformHit(){
        EnterRacePlatform();
    }
    
    public void EndPlatformHit(){

    }

    public void StartPlatformExit(){
        inRacePlatform = false;
        if( inRace == false){
            ExitRace();
        }
    }

    public void EndPlatformExit(){
        inRacePlatform = false;
        if( finishedRace == true ){
            ExitRace();
        }   
    }




public void EnterRacePlatform(){

    currentRing = -1;
    currentTime = 0;
    HitRing();
    inRacePlatform = true;
    inRace = false;
    finishedRace = false;
    canStartRace = false;
    God.wren.state.SetInRace(-1);

    ActivateRings();


}

    public void StartRace(){
        currentTime = 0;
        inRace = true;
        raceStartTime = Time.time;
        canStartRace = false;
        finishedRace = false;
        God.wren.state.SetInRace(raceID);
        particles.Begin();
    }


    public void FinishRace(){
        God.wren.state.HitGround();
        God.wren.state.SetInRace(-raceID);
        inRace = false;
        finishedRace = true;
        ActiveRing.SetActive(false);
        
        if (raceLeaderboard) {
            var id = UserIdService.GetLocalUserId();
            raceLeaderboard.AddEntry(id, currentTime);
            raceLeaderboard.RefreshUI();
        }
    }

    public void ExitRace(){
        currentTime = 0;
        God.wren.state.SetRaceTime( currentTime );      
        God.wren.state.SetInRace(-1);
        DeactivateRings();
    }

    public void AbortRace(){
        currentTime = 0;
        inRace = false;
        finishedRace = false;
        canStartRace = false;
        particles.End();
        ActiveRing.SetActive(false);
        God.wren.state.SetInRace(-1);
        DeactivateRings();
    }

    public void UpdateTime(){
        currentTime = Time.time - raceStartTime;
        God.wren.state.SetRaceTime( currentTime );
    }




public void ActivateRings(){


        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
      
          
          foreach( var ring in rings ){  
            
          
            ring.GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_Active", 1);
            ring.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
        
        }



}

public void DeactivateRings(){

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
      
          
          foreach( var ring in rings ){  
            
          
            ring.GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_Active", 0);
            ring.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
        
        }
}


}
