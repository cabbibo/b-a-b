using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

 [System.Serializable]
 public class IntEvent : UnityEvent<int>{}
 public class IntVec3Event : UnityEvent<int,Vector3>{}

[ExecuteAlways]
public class SubMenu : MonoBehaviour
{

    public float selectionAngle;
    public float selectionMagnitude;

    public int numChoices;

    public float radius;
    public float upRatio;


    public Renderer[] selections;

    public MaterialPropertyBlock mpbSelected;
    public MaterialPropertyBlock mpbDeselected;

    public Color selectedColor;
    public Color deselectedColor;

    public LineRenderer lr;

    public int currentSelection;

    public IntEvent onSelect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable(){

        if( mpbSelected == null ){
            mpbSelected = new MaterialPropertyBlock();
        }

        if( mpbDeselected == null ){
            mpbDeselected = new MaterialPropertyBlock();
        }

        selectionTime = Time.time;
        UpdateCurrentSelection(-1);
            mpbSelected.SetColor("_Color", selectedColor);
            mpbDeselected.SetColor("_Color", deselectedColor);

        // Show menu items
    }

    void OnDisable(){
        if( currentSelection != -1 ){
            DoSelection();
        }

    }

    void DoSelection(){
        onSelect.Invoke( currentSelection );
    }

    public float selectionTime;

    // Update is called once per frame
    void LateUpdate()
    {
        selectionAngle = Vector2.SignedAngle(Vector2.right, God.input.left.normalized );
        selectionMagnitude = God.input.left.magnitude;

     

        transform.LookAt( Camera.main.transform.position , transform.parent.up);

        if( God.input.left.magnitude > .5f){
            if( selectionAngle > 0 ){

                float v = selectionAngle / 180;
                v *= (float)numChoices;
                v = Mathf.Floor(v);

                if( (int)v != currentSelection ){
                    UpdateCurrentSelection( (int)v);
                }
                currentSelection = (int)v;

                selectionTime = Time.time;

            }else{
                 UpdateCurrentSelection(-1);
            }
        }else{
            if( Time.time - selectionTime > .3f){
              UpdateCurrentSelection(-1);//  currentSelection = -1;
            }
        }
    }


    void UpdateCurrentSelection( int v ){

        for( int i = 0; i < selections.Length; i++ ){
            if( v == i ){
                selections[i].SetPropertyBlock(mpbSelected);
            }else{
                selections[i].SetPropertyBlock(mpbDeselected);
            }
        }

        currentSelection = v;
        
    }


    void UpdateLineRenderer(){
           lr.positionCount = 3 * numChoices + 2;

        

        for( int i = 0; i < numChoices; i++ ){

            float v = (float)i/(float)numChoices;

            v += .02f;
            v *= Mathf.PI;

             float v2 = ((float)i+1)/(float)numChoices;
             v2 -= .02f;
            v2 *= Mathf.PI;

            v -= Mathf.PI * .5f;
            v2 -= Mathf.PI * .5f;


            Vector3 vec1 = transform.TransformDirection(new Vector3( Mathf.Sin(v)  , Mathf.Cos(v),upRatio));
            Vector3 vec2 = transform.TransformDirection(new Vector3( Mathf.Sin(v2)  , Mathf.Cos(v2),upRatio));
            

            lr.SetPosition(i *3 + 0 , transform.position );
            lr.SetPosition(i *3 + 1 , transform.position + vec1 * radius);
            lr.SetPosition(i *3 + 2 , transform.position +vec2 * radius);

        } 
        
        
        lr.SetPosition( 3 * numChoices, transform.position   );
        lr.SetPosition( 3 * numChoices+1, transform.position + transform.TransformDirection(new Vector3( -God.input.left.x , God.input.left.y,upRatio) * radius * 1.1f)   );

    
    }


}
