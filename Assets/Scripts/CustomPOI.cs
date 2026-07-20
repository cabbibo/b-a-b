using UnityEngine;

// Put this on any object tagged "CustomPOI".
// It feeds a custom color into the ping/pointer system (InterfacePointer).
// Starts simple: just a color. The color is passed to the pointer shaders as HSV
// through the pointer's extraData channel ( x = hue, y = saturation, z = value ).
public class CustomPOI : MonoBehaviour
{
    [ColorUsage( false , false )]
    public Color color = Color.white;

    // Pointer type index understood by InterfacePointer / the pointer shaders.
    public const int PointerType = 11;

    // Packs the color as HSV for the shader extraData buffer.
    // w is left at 0 ( w == 1 would hide the sky column in the shader ).
    public Vector4 GetPointerData()
    {
        Color.RGBToHSV( color , out float h , out float s , out float v );
        return new Vector4( h , s , v , 0 );
    }
}
