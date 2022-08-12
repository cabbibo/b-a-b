using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenColors : MonoBehaviour
{

    public Wren wren;
    public Renderer[] renderers;

    public Texture2D icon;

    public MaterialPropertyBlock mpb;

    public void SetMat( int whichRenderer ){
        UpdateMPBValues();
        renderers[whichRenderer].SetPropertyBlock( mpb );

    }

    public void SetMat( int whichRenderer , int propertyIndex ){
        UpdateMPBValues();

        renderers[whichRenderer].SetPropertyBlock( mpb , propertyIndex);
        

    }


    public void SetMat( Renderer r ){

        UpdateMPBValues();

        for( int i = 0; i < r.materials.Length; i++ ){
            r.SetPropertyBlock( mpb ,i);
        }

    }
    
    public void UpdateMPBValues(){
            if( mpb == null){
            mpb = new MaterialPropertyBlock();
        }

        mpb.SetFloat("_Hue1", wren.state.hue1 );
        mpb.SetFloat("_Hue2", wren.state.hue2 );
        mpb.SetFloat("_Hue3", wren.state.hue3 );
        mpb.SetFloat("_Hue4", wren.state.hue4 );
        mpb.SetTexture("_IconTexture",icon);
    }

    public void MakeIcon(){

        // icon = new Texture2D(4,1);
        // icon.filterMode = FilterMode.Point;

        // icon.SetPixel(0,0, Color.HSVToRGB( wren.state.hue1 , 1, 1));
        // icon.SetPixel(1,0, Color.HSVToRGB( wren.state.hue2 , 1, 1));
        // icon.SetPixel(2,0, Color.HSVToRGB( wren.state.hue3 , 1, 1));
        // icon.SetPixel(3,0, Color.HSVToRGB( wren.state.hue4 , 1, 1));

        // icon.Apply();

        icon = MakeWrenIcon(wren.state.hue1, wren.state.hue2, wren.state.hue3, wren.state.hue4);

    }

    public static Texture2D MakeWrenIcon(float hue1, float hue2, float hue3, float hue4) {
        var tex = new Texture2D(4, 1);
        tex.filterMode = FilterMode.Point;

        tex.SetPixel(0,0, Color.HSVToRGB( hue1 , 1, 1));
        tex.SetPixel(1,0, Color.HSVToRGB( hue2 , 1, 1));
        tex.SetPixel(2,0, Color.HSVToRGB( hue3 , 1, 1));
        tex.SetPixel(3,0, Color.HSVToRGB( hue4 , 1, 1));

        tex.Apply();
        return tex;
    }

    public void updateAllRenderers(){
        for( int i = 0; i < renderers.Length; i++){
            SetMat(renderers[i]);
        }
    }

    public void OnColorUpdate(){
        MakeIcon();
        updateAllRenderers();
    }
}
