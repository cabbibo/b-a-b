﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using IMMATERIA;

public class AddRebuildFunctionality {
 
   // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
// and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
[MenuItem("IMMATERIA/Rebuid Scene %b")]
static void RebuildScene()
{
  GameObject sceneGod = GameObject.Find("SceneGod");

  if( sceneGod ){
    GameObject.Find("SceneGod").GetComponent<God>().Rebuild();
  }

  
}






}
