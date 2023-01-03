using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class FindMissingScripts 
{
    

    [MenuItem("Window/FindMissingScripts/ScriptsInProject")]
    static void FindMissingSCriptsInProjectMenuItem(){
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab",System.StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach( string path in prefabPaths ){
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab == null) 
            {
                Debug.Log("Can't load prefab at: " + path);
                continue;
            }
            
            foreach( Component component in prefab.GetComponentsInChildren<Component>() ){
            
                if( component== null){
                    Debug.Log("Prefab Found with missing Script" + path , prefab);
                }
            
            }
        }
    }


    [MenuItem("Window/FindMissingScripts/ScriptsInScene")]
        static void FindMissingScripsInSceneMenuObject(){

        foreach( GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true)){
            foreach( Component component in gameObject.GetComponentsInChildren<Component>() ){
            
                if( component== null){
                    Debug.Log("Prefab Found with missing Script" + gameObject.name , gameObject);
                }
            
            }
        }
    }


}
