using UnityEngine;

// Tunable behavior params for a PreyInterestPoint. Scene-bound references (the owning manager and the
// OnCollider Collider[] array) stay on the PreyInterestPoint component, which exposes same-named proxy
// properties forwarding to this asset.
[CreateAssetMenu( fileName = "PreyInterestPointConfigSO" , menuName = "Prey/PreyInterestPointConfigSO" , order = 3 )]
public class PreyInterestPointConfigSO : ScriptableObject
{
    [Header( "Common" )]
    public InterestPointType type                   = InterestPointType.NewCalm;
    [Tooltip( "Within this radius the bird flies toward the point (enters Searching)." )]
    public float             noticeRadius           = 40f;
    [Tooltip( "Within this radius (while Searching) the bird enters the point's state." )]
    public float             enterRadius            = 5f;
    public bool              alwaysInteresting      = false;
    public float             priority               = 1f;
    public float             timeToRemainInterested = 0f;
    [Tooltip( "Random ± variance applied to Time To Remain (e.g. perch duration, updraft ride time)." )]
    public float             timeToRemainVariance   = 0f;
    [Tooltip( "While calm and inside noticeRadius, speeds the calm-before-search countdown by (1 + this). " +
              "0 = no pull (only considered when search is normally called); high = pulled in almost immediately." )]
    public float             noticeUrgency          = 0f;

    [Header( "Search / Entrance" )]
    public SearchTargetType searchTargetType   = SearchTargetType.Center;
    [Tooltip( "Radius used when searchTargetType is RandomInRange." )]
    public float            searchRandomRadius = 10f;
    [Tooltip( "Random scatter (XZ radius) added to the entrance target for ALL search types, " +
              "so birds don't all aim at the exact same spot. 0 = exact target." )]
    public float            targetRandomness   = 0f;
    public EntranceShape    entranceShape      = EntranceShape.Sphere;

    [Header( "Perch" )]
    public PerchSubType            perchSubType    = PerchSubType.InArea;
    public PerchOnColliderSettings perchOnCollider = new PerchOnColliderSettings();   // colliders[] live on the component
    public PerchInAreaSettings     perchInArea     = new PerchInAreaSettings();
    public PerchFieldSettings      perchField      = new PerchFieldSettings();

    [Header( "Updraft" )]
    public UpdraftPointSettings updraftSettings = new UpdraftPointSettings();
}
