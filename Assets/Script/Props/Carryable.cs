using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Normal.Realtime.Serialization;

public class Carryable : RealtimeComponent<CarryableModel>
{
    public class DropSettings
    {
        public Vector3? ExplosiveDirection;
        public Vector3? ExplosivePosition;
        public float? ExplosiveForce;

        public static DropSettings FromCrash(Collision collision)
        {
            return new DropSettings
            {
                ExplosiveDirection = collision.contacts[0].normal,
                ExplosivePosition = collision.contacts[0].point,
                ExplosiveForce = collision.impulse.magnitude * 0f//50f + collision.impulse.magnitude * .1f,
            };
        }
    }

    public bool setPositionOnPickup = false;
    public bool dropOnGroundHit = true;
    public float carryingDrag = 3f;
    public float releasedDrag = .25f;

    public bool gravityOnDrop = false;
    private const float CarryCooldown = 0.5f;

    private Rigidbody _rigidbody;
    private Transform _transform;
    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;

    private Vector3 _initialScale;


    public bool BeingCarried
    {
        get
        {
            if (model != null)
            {
                return model.beingCarried;
            }
            else
            {
                return false;
            }
        }
    }

    public int IdOfLastCarrier
    {
        get { return model.lastCarrierId; }
    }

    private float _lastCarryTime = -CarryCooldown;
    private float TimeSinceLastCarried
    {
        get { return Time.time - _lastCarryTime; }
    }

    private WrenCarrying _carrier;
    public WrenCarrying carrier => _carrier;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _transform = transform;
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        _initialScale = _transform.localScale;
    }

    public bool CheckAvailableToCarry(WrenCarrying carrier)
    {
        return !BeingCarried && (IdOfLastCarrier != carrier.GetNormalClientId() || TimeSinceLastCarried >= CarryCooldown);
    }

    protected override void OnRealtimeModelReplaced(CarryableModel previousModel, CarryableModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.beingCarriedDidChange -= OnBeingCarriedChanged;
            previousModel.lastCarrierIdDidChange -= OnLastCarrierIdChanged;
        }

        if (currentModel != null)
        {
            currentModel.beingCarriedDidChange += OnBeingCarriedChanged;
            currentModel.lastCarrierIdDidChange += OnLastCarrierIdChanged;
        }
    }

    private void OnBeingCarriedChanged(CarryableModel model, bool carried)
    {
        _lastCarryTime = Time.time;
    }

    private void OnLastCarrierIdChanged(CarryableModel model, int id)
    {

    }

    public bool TryToCarry(WrenCarrying carrier, Vector3 targetPosition)
    {
        if (!CheckAvailableToCarry(carrier))
        {
            return false;
        }

        print("TRying to carry here");

        _realtimeView.RequestOwnership();
        _realtimeTransform.RequestOwnership();


        // TODO: Ask jacob if we need this line
        if (setPositionOnPickup)
        {
            _rigidbody.position = targetPosition;
        }

        _rigidbody.drag = carryingDrag;

        model.beingCarried = true;
        model.lastCarrierId = carrier.GetNormalClientId();
        _carrier = carrier;

        return true;
    }

    public void UpdateCarriedPosition(WrenCarrying carrier, Vector3 targetPosition)
    {
        _rigidbody.AddForce(-30f * (transform.position - targetPosition));
    }

    public bool TryToDrop(WrenCarrying carrier, DropSettings dropSettings = null)
    {

        model.beingCarried = false;
        _carrier = null;

        _rigidbody.drag = releasedDrag;
        _rigidbody.useGravity = gravityOnDrop;
        //_transform.localScale = new Vector3(4,4,4);

        if (dropSettings != null)
        {
            if (dropSettings.ExplosiveDirection.HasValue)
            {
                // _rigidbody.AddForceAtPosition(dropSettings.ExplosiveDirection.Value * dropSettings.ExplosiveForce.Value, dropSettings.ExplosivePosition.Value, ForceMode.Impulse);
                _rigidbody.AddForce(dropSettings.ExplosiveDirection.Value * dropSettings.ExplosiveForce.Value, ForceMode.Impulse);
                print("adding force: " + dropSettings.ExplosiveDirection.Value * dropSettings.ExplosiveForce.Value);
            }
        }
        else
        {
            print("no drop settings lol");
        }

        return true;
    }

    public Vector3 GetAttachableTargetPos()
    {
        return _transform.position + transform.forward * (transform.localScale.y + .5f);
    }



}
