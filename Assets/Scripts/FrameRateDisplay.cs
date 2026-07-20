using UnityEngine;

// Simple on-screen frame rate counter.
// Toggle it by enabling/disabling this GameObject, or with toggleKey at runtime.
public class FrameRateDisplay : MonoBehaviour
{
    [Header( "Toggle" )]
    public KeyCode toggleKey = KeyCode.F;
    public bool    show      = true;

    [Header( "Look" )]
    public int   fontSize = 24;
    public Color color    = Color.white;

    [Tooltip( "Seconds between the displayed number updating ( smooths the reading ).")]
    public float refreshInterval = 0.25f;

    private float accum;
    private int   frames;
    private float timeLeft;
    private float fps;

    private GUIStyle style;

    private void OnEnable()
    {
        timeLeft = refreshInterval;
        accum    = 0;
        frames   = 0;
        fps      = 0;
    }

    private void Update()
    {
        if ( Input.GetKeyDown( toggleKey ) ) {
            show = !show;
        }

        // Smoothed FPS: average over refreshInterval.
        timeLeft -= Time.unscaledDeltaTime;
        accum    += 1f / Mathf.Max( Time.unscaledDeltaTime , 0.00001f );
        frames++;

        if ( timeLeft <= 0f ) {
            fps      = accum / frames;
            timeLeft = refreshInterval;
            accum    = 0;
            frames   = 0;
        }
    }

    private void OnGUI()
    {
        if ( !show ) {
            return;
        }

        if ( style == null ) {
            style = new GUIStyle();
        }

        style.fontSize         = fontSize;
        style.normal.textColor = color;

        GUI.Label( new Rect( 10 , 10 , 300 , fontSize + 10 ) ,
            string.Format( "{0:0.} fps  ({1:0.0} ms)" , fps , 1000f / Mathf.Max( fps , 0.00001f ) ) ,
            style );
    }
}
