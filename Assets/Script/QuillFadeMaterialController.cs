using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class QuillFadeMaterialController : MonoBehaviour
{
    public float fade;
    public Color color;

    public Transform fadeLocation;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    MaterialPropertyBlock mpb;
    Renderer renderer;

    void doUpdate(){
    if( mpb == null ){
                mpb = new MaterialPropertyBlock();
                renderer = GetComponent<Renderer>();
            }

            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Fade",fade);
            mpb.SetColor("_Color",color);

            if( fadeLocation != null ){
                mpb.SetVector( "_FadeLocation" , fadeLocation.transform.position );
            }else{
                mpb.SetVector("_FadeLocation", transform.position );
            }
            renderer.SetPropertyBlock(mpb);

    }

    // Update is called once per frame
    void Update()
    {
        doUpdate();
    }

    void OnEnable(){
        doUpdate();
    }
}
