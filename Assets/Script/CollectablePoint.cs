using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;

public class CollectablePoint : MonoBehaviour
{
    // Start is called before the first frame update
    public Collectable collectable;
    public bool collected;
    MeshRenderer renderer;
    LineRenderer lineRenderer;



    public UnityEvent OnCollectEvent;
    public UnityEvent SetCollectedEvent;
    public UnityEvent SetStartedEvent;
    public UnityEvent ResetEvent;



    public void OnCollect(){ 
    
        // AKA collecting in real time
        if( collected == false ){
           
            collected = true;
            God.audio.Play( God.sounds.pointCollectedSounds );
            collectable.PointCollected(this);
            God.smallSuccessSystem.transform.position = transform.position;
            God.smallSuccessSystem.Play();

            God.tween.AddTween( 3 , TweenOn);
        }

    }

    public void Uncollect(){
        
        collected = false;
        ResetEvent.Invoke();
    }

    public void OnEnable(){
        
        renderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void OnEnter(){
        renderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        

        collectFade = 0;

    }

    public void OnExit(){

    }

    void TweenOn( float v ){
        renderer.material.SetFloat("_On" , v);
        lineRenderer.material.SetFloat("_On" , v);
    }

    float collectFade;
    void Update(){

     

    }

    public void SetCollected(){
        collected = true;
        OnCollect();   
        lineRenderer.material.SetFloat("_On",1);
        renderer.material.SetFloat("_On",1);
    }


}
