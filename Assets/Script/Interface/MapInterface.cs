using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using WrenUtils;

//[ExecuteAlways]
public class MapInterface : WrenInterface
{

 public bool active;

public GameObject terrainMap;

public Vector3[] selectablePositions;

public GameObject[] selectableReps;



public Vector2 mapCenter;
public Vector2 mapSize;
public float panSpeedMultiplier;
public float zoomSpeedMultiplier;
public float selectorSpeedMultiplier;


public GameObject mapCenterRep;

public float mapDepth;

public LineRenderer toClosest;


public FullInterface fullInterface;

public GameObject activeInterfaceIndicator;


MaterialPropertyBlock mpb;

 public override void Activate(){
    active = true;
    enabled = true;
    activeInterfaceIndicator.SetActive(true);
    BuildRaces();
         UpdateInterface();
         UpdateRaces();
    //gameObject.SetActive(true);


    // Get our current position and show it on that map
 }

 public void SetCenter(){
    if( God.wren != null ){
        SetCenterPos( God.wren.transform.position);
        selectorPos.x = 0;
        selectorPos.y = 0;
        selectorVel.x = 0;
        selectorVel.y = 0;
         
    }
 }

 public override void Deactivate(){
    active = false;
    enabled = false;
    
        activeInterfaceIndicator.SetActive(false);
    //gameObject.SetActive(false);
 }

 public void Update(){
     if( active){
         UpdateInterface();
         UpdateRaces();
     }
 }

 public Vector2 locationVel;
 public float zoomVel;

 public Vector2 selectorVel;
 public Vector2 selectorPos;

public GameObject closestSelectable;
public GameObject selectorSubMenu;

public bool canMove = true;


public Vector3 worldSelectorPos;



public void SubMenuSelect( int i ){
        if( i == 2 ){
            fullInterface.TeleportToLocation( worldSelectorPos );
        }else if( i == 1 ){
            fullInterface.PlaceBeacon( worldSelectorPos );
        }else{
            fullInterface.RemoveBeacon();
        }
}
 void UpdateInterface(){
     GetWrens();


    if(canMove){
     
        zoomVel += God.input.r1 * .00001f * zoomSpeedMultiplier;
        zoomVel -= God.input.l1 * .00001f * zoomSpeedMultiplier;
        mapSize += Vector2.one * zoomVel;

        mapSize = new Vector2( Mathf.Clamp( mapSize.x , .04f, 1 ),  
                            Mathf.Clamp( mapSize.y , .04f, 1 ));
                            
        zoomVel *= .9f;

        locationVel += God.input.alwaysRight * new Vector2(1,1) * .00001f* panSpeedMultiplier;

        mapCenter += locationVel;
        locationVel *= .9f;


/*
        for( int i = 0;  i < selectablePositions.Length; i++ ){
            Vector3 mapPos = GetMapPosHideSides( selectablePositions[0] );

            selectableReps[0].transform.localPosition = mapPos;
        
        }
*/

        selectorVel += God.input.alwaysLeft * new Vector2(1,1) * .0001f * selectorSpeedMultiplier;
        selectorPos += selectorVel;

        if(Mathf.Abs(selectorPos.x)  > .5f || Mathf.Abs(selectorPos.y)  > .5f ){
            mapCenter += selectorVel * .5f;
        }
        selectorVel *= .95f;

        selectorPos.x = Mathf.Clamp( selectorPos.x , -.5f,.5f);
        selectorPos.y = Mathf.Clamp( selectorPos.y , -.5f,.5f);


        mapCenter = new Vector2( Mathf.Clamp( mapCenter.x , mapSize.x * .5f, 1-mapSize.x*.5f ),  
                                        Mathf.Clamp( mapCenter.y , mapSize.y * .5f, 1-mapSize.y*.5f ));



        worldSelectorPos = new Vector3(
            ((mapCenter.x-.5f)+selectorPos.x* mapSize.x) * God.terrainData.size.x,
            0,
            ((mapCenter.y-.5f)+selectorPos.y * mapSize.y) * God.terrainData.size.z
        );

        worldSelectorPos.y = God.terrain.SampleHeight(worldSelectorPos);


        mapCenterRep.transform.localPosition = GetMapPos( worldSelectorPos );



        if( closestSelectable != null ){

        //closestSelectable = selectableReps[0];

            Vector3 c0 = mapCenterRep.transform.position;
            Vector3 c1 = mapCenterRep.transform.position + mapCenterRep.transform.forward;
            Vector3 c2 = closestSelectable.transform.position + closestSelectable.transform.forward;
            Vector3 c3 = closestSelectable.transform.position;

            for( int i = 0; i< toClosest.positionCount; i++ ){
                float v = (float)i/(float)toClosest.positionCount;

                Vector3 p = Helpers.cubicCurve( v , c0 , c1 ,c2,c3);
                toClosest.SetPosition( i , p );

            }
        }else{
            for( int i = 0; i< toClosest.positionCount; i++ ){
                Vector3 p = Vector3.zero;
                toClosest.SetPosition( i , p );
            }
        }

    }



    if( mpb == null ){ 
        mpb = new MaterialPropertyBlock();
        terrainMap.GetComponent<MeshRenderer>().SetPropertyBlock( mpb );
    }

    mpb.SetTexture( "_Terrain" , God.terrainData.heightmapTexture );
    mpb.SetVector("_Center", mapCenter);
    mpb.SetVector("_MapSize", mapSize);
    mpb.SetFloat("_MapDepth", mapDepth);

    terrainMap.GetComponent<MeshRenderer>().SetPropertyBlock( mpb );




    CheckSelecting();
 
 }

 void CheckSelecting(){

     if( God.input.x ){
         canMove = false;
         selectorSubMenu.SetActive(true);//enabled = true;
     }else{
         selectorSubMenu.SetActive(false);//enabled = true;
         canMove = true;
     }
 }



 void SetCenterPos( Vector3 p ){
    Vector3 pos = p;

     Vector3 s = God.terrainData.size;
    
     pos =  new Vector3( pos.x / s.x ,  pos.z / s.z,pos.y / s.y );//
     mapCenter.x = pos.x+.5f;
     mapCenter.y = pos.y+.5f;
 }

 Vector3 GetMapPos( Vector3  p){
     Vector3 pos = p;

     
     //pos.x += God.terrainData.size.x;
     //pos.z += God.terrainData.size.z;
     pos.y = God.terrain.SampleHeight(p);
     Vector3 s = God.terrainData.size;
    
     pos =  new Vector3( pos.x / s.x ,  pos.z / s.z,pos.y / s.y );

    

    float x = pos.x - (mapCenter.x-.5f);
    x /= mapSize.x;

    float y  = pos.y - (mapCenter.y-.5f);
    y /= mapSize.y;

    pos = new Vector3( -x , y , pos.z *  mapDepth * .5f );
    pos.x = Mathf.Clamp(pos.x , -.5f,.5f);
    pos.y = Mathf.Clamp(pos.y , -.5f,.5f);
     return pos;

 }

  Vector3 GetMapPosHideSides( Vector3  p){

     Vector3 pos = p;
     
     //pos.x += God.terrainData.size.x;
     //pos.z += God.terrainData.size.z;

     pos.y = God.terrain.SampleHeight(p);
     Vector3 s = God.terrainData.size;
    
     pos =  new Vector3( pos.x / s.x ,  pos.z / s.z,pos.y / s.y );

    

    float x = pos.x - (mapCenter.x-.5f);
    x /= mapSize.x;

    float y  = pos.y- (mapCenter.y-.5f);
    y /= mapSize.y;


    pos = new Vector3( -x , y , pos.z *  mapDepth * .5f );
    pos.x = Mathf.Clamp(pos.x , -.5f,.5f);
    pos.y = Mathf.Clamp(pos.y , -.5f,.5f);

    if( pos.x <= -.5f ||pos.x >= .5f  || pos.y <= -.5f ||pos.y >= .5f ){
        pos.z = 0;
    }
     return pos;

 }
 
 

int numWrens;
int oNumWrens;


int numRaces;


 public GameObject wrenPrefab;
  public GameObject[] wrens;
 public Renderer[] wrenRenderers;


public GameObject beaconPrefab;
 public GameObject[] beacons;
 public Renderer[] beaconRenderers;



public GameObject racePrefab;
 public GameObject[] races;
 public Renderer[] raceRenderers;
 public LineRenderer[] raceLines;

 public Transform[] raceStarts;
 public Transform[] raceEnds;

 
 void GetWrens(){

    if( God.wrens != null ){
        oNumWrens = numWrens;
        numWrens = God.wrens.Count;

        if( oNumWrens != numWrens ){
            RebuildPrefabs();
        }
    

        int i = 0;
        foreach( Wren wren in God.wrens){

            beacons[i].SetActive( wren.state.beaconOn );
            beacons[i].transform.localPosition = GetMapPos( wren.state.beaconPos );
            wren.colors.SetMat(beaconRenderers[i]);
            
            Vector3 wrenF = Vector3.Scale( wren.transform.forward , new Vector3(1,0,1));
            wrenF = new Vector3( wrenF.y , wrenF.x , wrenF.z );

            wrens[i].transform.localRotation = Quaternion.Euler(0, 0, wren.transform.localRotation.eulerAngles.z);// transform.GetPo wren.transform.localRotation;
            
            wrens[i].transform.localPosition = GetMapPos( wren.transform.position ) + Vector3.forward * .1f;
            wren.colors.SetMat( wrenRenderers[i]);
            i++;
        }
    
    
    
    }

 }


 void BuildRaces(){

 if( races != null ){
        for( int i = 0; i < races.Length; i++ ){
            DestroyImmediate( races[i] );
        }
    }

    numRaces = God.races.Count;
    
    
    races = new GameObject[ numRaces ];
    raceRenderers = new Renderer[ numRaces ];
    raceLines = new LineRenderer[ numRaces ];

    raceStarts = new Transform[ numRaces ];
    raceEnds = new Transform[ numRaces ];


    for(int i = 0; i < numRaces; i++ ){
        races[i]  = Instantiate( racePrefab );
        races[i].SetActive( true );
        races[i].transform.parent = terrainMap.transform;
        races[i].transform.localRotation = Quaternion.identity;
        races[i].transform.localPosition =  Vector3.zero;;
        races[i].transform.localScale =  racePrefab.transform.localScale;;
        raceRenderers[i] = races[i].GetComponent<Renderer>();
        raceLines[i] = races[i].GetComponent<LineRenderer>();
        raceLines[i].positionCount = God.races[i].rings.Count;
        raceLines[i].material.SetColor("_Color", God.races[i].color);

        raceStarts[i] = races[i].transform.GetChild(0);
        raceEnds[i] = races[i].transform.GetChild(1);

        
        raceStarts[i].GetComponent<Renderer>().material.SetColor("_Color", God.races[i].color);
        raceEnds[i].GetComponent<Renderer>().material.SetColor("_Color", God.races[i].color);
     
    }

 }

 void UpdateRaces(){
         for(int i = 0; i < numRaces; i++ ){

            //races[i].transform.localPosition = GetMapPos( God.races[i].startingPlatform.transform.position );
            int id = 0;
            foreach( Transform t  in  God.races[i].rings ){
                raceLines[i].SetPosition( id ,GetMapPosHideSides(t.position) + Vector3.forward * .1f );
                id ++;
            }


            raceStarts[i].localPosition = GetMapPosHideSides( God.races[i].startingPlatform.transform.position )+ Vector3.forward * .1f;
            raceEnds[i].localPosition = GetMapPosHideSides( God.races[i].endingPlatform.transform.position )+ Vector3.forward * .1f;
         
        }
 }

 void RebuildPrefabs(){

    if( beacons != null ){
        for( int i = 0; i < beacons.Length; i++ ){
            Destroy( beacons[i] );
        }
    }

    if( wrens != null ){
        for( int i = 0; i < wrens.Length; i++ ){
            Destroy( wrens[i] );
        }
    }

   


    beacons = new GameObject[ numWrens ];
    beaconRenderers = new Renderer[ numWrens ];

    for(int i = 0; i < numWrens; i++ ){
        beacons[i]  = Instantiate( beaconPrefab );
        beacons[i].SetActive( true );
        beacons[i].transform.parent = terrainMap.transform;
        beacons[i].transform.localRotation = Quaternion.identity;
        beacons[i].transform.localPosition =  Vector3.zero;;
        beacons[i].transform.localScale =  beaconPrefab.transform.localScale;;
        beaconRenderers[i] = beacons[i].transform.GetChild(0).GetComponent<Renderer>();
        beaconRenderers[i].materials[0].SetInt( "_WhichHue" , 4 );
        beaconRenderers[i].materials[1].SetInt( "_WhichHue" , 3 );
        beaconRenderers[i].materials[2].SetInt( "_WhichHue" , 2 );
        beaconRenderers[i].materials[3].SetInt( "_WhichHue" , 1 );
        beaconRenderers[i].materials[4].SetInt( "_WhichHue" , 0 );
    }


    
    wrens = new GameObject[ numWrens ];
    wrenRenderers = new Renderer[ numWrens ];


    for(int i = 0; i < numWrens; i++ ){
        wrens[i]  = Instantiate( wrenPrefab );
        wrens[i].SetActive( true );
        wrens[i].transform.parent = terrainMap.transform;
        wrens[i].transform.localRotation = Quaternion.identity;
        wrens[i].transform.localPosition =  Vector3.zero;;
        wrens[i].transform.localScale =  wrenPrefab.transform.localScale;;
        wrenRenderers[i] = wrens[i].GetComponent<Renderer>();
        wrenRenderers[i].materials[0].SetInt( "_WhichHue" , 0 );
        wrenRenderers[i].materials[1].SetInt( "_WhichHue" , 1 );
        wrenRenderers[i].materials[2].SetInt( "_WhichHue" , 2 );
        wrenRenderers[i].materials[3].SetInt( "_WhichHue" , 3 );
    }



 }



}
