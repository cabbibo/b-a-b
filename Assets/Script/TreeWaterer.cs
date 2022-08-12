using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FantasyTree;

public class TreeWaterer : MonoBehaviour
{

    public ControlTreeMaterialValues treeController;
    public GameObject collider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(God.wren){
        if( God.wren.waterController.isWatering && God.wren.waterController.objectWatering == collider ){
            treeController.barkShown += .01f;
            treeController.barkShown = Mathf.Clamp( treeController.barkShown , 0 , 1 );

            if( treeController.barkShown > .95f ){
                treeController.flowersShown += .01f;
                treeController.flowersShown = Mathf.Clamp( treeController.flowersShown , 0 , 1 );
            }

        }
        }
        
    }
}
