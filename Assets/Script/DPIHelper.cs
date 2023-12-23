using System.Runtime.InteropServices;
using UnityEngine;
using System;
public class DPIHelper : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    const int LOGPIXELSX = 88;   // Used for the horizontal DPI
    const int LOGPIXELSY = 90;   // Used for the vertical DPI


    const int BASE_DPI = 96;  // Base DPI value

    public static Vector2 GetSystemDPI()
    {
        IntPtr desktop = GetDC(IntPtr.Zero);
        if (desktop != IntPtr.Zero)
        {
            int dpiX = GetDeviceCaps(desktop, LOGPIXELSX);
            int dpiY = GetDeviceCaps(desktop, LOGPIXELSY);
            return new Vector2(dpiX, dpiY);
        }
        return new Vector2(96, 96);  // Default DPI
    }


    public static Vector2 GetDisplayScaling()
    {
        Vector2 dpi = GetSystemDPI();
        return new Vector2(dpi.x / BASE_DPI, dpi.y / BASE_DPI);
    }
}