using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Collectable))]
[CanEditMultipleObjects]
public class CollectableEditor : Editor 
{


int paramID;
int oParamID;
    public override void OnInspectorGUI()
    {
        
        

        Collectable collectable = (Collectable)target;
        if(GUILayout.Button("FakePointPickup")){
           collectable.FakePointPickup();
        }



         if(GUILayout.Button("FakeDrop")){
           collectable.FakeDrop();
        }

        DrawDefaultInspector();



    }
}
