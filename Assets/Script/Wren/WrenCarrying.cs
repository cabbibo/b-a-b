using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;

 using WrenUtils;

public class WrenCarrying : MonoBehaviour
{

    public List<Carryable> CarriedItems = new List<Carryable>();

    // TODO: don't use God, use info from Wren
    public int GetNormalClientId() {
        return God.wrenMaker.GetNormalClientId();
    }

    public bool PickUpItem(GameObject g) {
        Carryable carryable;
        if (g.TryGetComponent(out carryable)) {
            return PickUpItem(carryable);
        } else {
            Debug.LogWarning($"Trying to pick up Object {g.name}, but it doesn't have a Carryable component attached.");
        }
        return false;
    }

    public bool PickUpItem(Carryable c){
        var targetPosition = transform.position - transform.up * 1- transform.forward * 3f;
        if (c.TryToCarry(this, targetPosition)) {
            
            God.audio.Play(God.sounds.collectablePickedUpSounds);
            CarriedItems.Add( c );
            
            print(CarriedItems.Count);
        }else{

            print("no picky");

        }


        //var rb = g.GetComponent<Rigidbody>();
        //rb.position = transform.position - transform.up * 1- transform.forward * 3;
        //g.transform.localScale = new Vector3(4,4,4);
        //rb.drag = 3f;

        return true;
    }

    

    public void DropAllCarriedItems(Carryable.DropSettings dropSettings = null){
        for (var i=0; i<CarriedItems.Count; i++) {
            if (DropCarriedItemAtIndex(i, dropSettings)) {
                i--;
            }
        }
    }

    public bool DropFirstCarriedItem(Carryable.DropSettings dropSettings=null) {
        return DropCarriedItemAtIndex(0, dropSettings);
    }

    public bool DropLastCarriedItem(Carryable.DropSettings dropSettings=null) {
        return DropCarriedItemAtIndex(CarriedItems.Count-1, dropSettings);
    }

    public bool DropCarriedItemAtIndex(int index, Carryable.DropSettings dropSettings=null) {
        if (CarriedItems.IsIndexValid(index) && CarriedItems[index].TryToDrop(this, dropSettings)) { 
            God.audio.Play(God.sounds.collectableDroppedSounds);
            CarriedItems.RemoveAt(index);
            return true;
        }
        return false;
    }



    public void UpdateCarriedItems(){
        Vector3 targetPosition = transform.position - transform.up * 3- transform.forward * 4;
        foreach( var c in CarriedItems){
            c.UpdateCarriedPosition(this, targetPosition);

            //g.GetComponent<Rigidbody>().AddForce( -30 * (g.transform.position - targetPosition));
            targetPosition = c.GetAttachableTargetPos();
        }

    }


    public int CheckIfCarryingItem(Carryable carryable){
        int id = -1;

        int index = 0;
        foreach( var c  in CarriedItems ){
            if(  c == carryable ){
                id = index;
            }
            index ++;
        }

        return id;

    }

    public void DropIfCarrying( Carryable c ){
        int id = CheckIfCarryingItem(c);

        if( id >= 0 ){
            DropCarriedItemAtIndex(id);
        }
    }

    public void GroundHit(Carryable.DropSettings dropSettings=null){

        int index = 0;
        foreach( var c in CarriedItems ){
            if( c.dropOnGroundHit ){
                DropCarriedItemAtIndex(index,dropSettings);
            }
            index ++;
        }
    }

}
