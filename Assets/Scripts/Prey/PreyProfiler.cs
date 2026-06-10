using System.Diagnostics;

// Lightweight, allocation-free profiler for the prey system.
//
// The prey code feeds raw counters into the "accumulating" fields every frame
// (raycasts cast, ticks spent in PreyController.Update / PreyManager.Update).
// Once per frame the first caller into FrameGate() rolls the frame over:
// it publishes the just-finished frame's totals into the "last*" snapshot
// fields (which the HUD reads, stable for the whole frame) and zeroes the
// accumulators for the new frame.
//
// Everything is static so there's no instance to wire up and no GC churn:
// we use Stopwatch.GetTimestamp() deltas rather than `new Stopwatch()`.
public static class PreyProfiler
{
    // ── accumulating this frame (written by the prey code) ─────────────────
    public static int  raycastCount;        // synchronous Physics.Raycast calls
    public static int  spherecastCount;     // Physics.SphereCast calls (collision module)
    public static int  batchedRaycastCount; // rays issued via PreyRaycastBatcher (worker threads)
    public static int  splineQueryCount;    // SplineUtility.GetNearestPoint searches (throttled)
    public static long controllerTicks;     // time spent in PreyController.Update
    public static long managerTicks;        // time spent in PreyManager.Update
    public static long batchTicks;          // time spent in PreyRaycastBatcher.Update

    // ── snapshot of the last completed frame (read by the HUD) ─────────────
    public static int    lastRaycastCount;
    public static int    lastSpherecastCount;
    public static int    lastBatchedRaycastCount;
    public static int    lastSplineQueryCount;
    public static double lastControllerMs;
    public static double lastManagerMs;
    public static double lastBatchMs;
    public static int    lastFrameUpdated = -1;

    // frameCount we last rolled over on; -1 so the very first call publishes nothing real
    private static int _gatedFrame = -1;

    private static readonly double TicksToMs = 1000.0 / Stopwatch.Frequency;

    // Call at the very start of any prey update path (and from the HUD). The
    // first call in a new frame publishes the previous frame's totals and
    // resets; every later call in the same frame is a cheap no-op.
    public static void FrameGate( int frameCount )
    {
        if ( frameCount == _gatedFrame ) return;
        _gatedFrame = frameCount;

        lastRaycastCount        = raycastCount;
        lastSpherecastCount     = spherecastCount;
        lastBatchedRaycastCount = batchedRaycastCount;
        lastSplineQueryCount    = splineQueryCount;
        lastControllerMs        = controllerTicks * TicksToMs;
        lastManagerMs           = managerTicks    * TicksToMs;
        lastBatchMs             = batchTicks      * TicksToMs;
        lastFrameUpdated        = frameCount;

        raycastCount        = 0;
        spherecastCount     = 0;
        batchedRaycastCount = 0;
        splineQueryCount    = 0;
        controllerTicks     = 0;
        managerTicks        = 0;
        batchTicks          = 0;
    }

    public static long Now => Stopwatch.GetTimestamp();
}
