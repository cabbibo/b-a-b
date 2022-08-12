using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Normal.Realtime.Serialization;
using Normal.Realtime;

[RealtimeModel]
public partial class CarryableModel
{
    [RealtimeProperty(1, true, true)]
    private bool _beingCarried;

    [RealtimeProperty(2, true, true)]
    private int _lastCarrierId;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class CarryableModel : RealtimeModel {
    public bool beingCarried {
        get {
            return _beingCarriedProperty.value;
        }
        set {
            if (_beingCarriedProperty.value == value) return;
            _beingCarriedProperty.value = value;
            InvalidateReliableLength();
            FireBeingCarriedDidChange(value);
        }
    }
    
    public int lastCarrierId {
        get {
            return _lastCarrierIdProperty.value;
        }
        set {
            if (_lastCarrierIdProperty.value == value) return;
            _lastCarrierIdProperty.value = value;
            InvalidateReliableLength();
            FireLastCarrierIdDidChange(value);
        }
    }
    
    public delegate void PropertyChangedHandler<in T>(CarryableModel model, T value);
    public event PropertyChangedHandler<bool> beingCarriedDidChange;
    public event PropertyChangedHandler<int> lastCarrierIdDidChange;
    
    public enum PropertyID : uint {
        BeingCarried = 1,
        LastCarrierId = 2,
    }
    
    #region Properties
    
    private ReliableProperty<bool> _beingCarriedProperty;
    
    private ReliableProperty<int> _lastCarrierIdProperty;
    
    #endregion
    
    public CarryableModel() : base(null) {
        _beingCarriedProperty = new ReliableProperty<bool>(1, _beingCarried);
        _lastCarrierIdProperty = new ReliableProperty<int>(2, _lastCarrierId);
    }
    
    protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
        _beingCarriedProperty.UnsubscribeCallback();
        _lastCarrierIdProperty.UnsubscribeCallback();
    }
    
    private void FireBeingCarriedDidChange(bool value) {
        try {
            beingCarriedDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    private void FireLastCarrierIdDidChange(int value) {
        try {
            lastCarrierIdDidChange?.Invoke(this, value);
        } catch (System.Exception exception) {
            UnityEngine.Debug.LogException(exception);
        }
    }
    
    protected override int WriteLength(StreamContext context) {
        var length = 0;
        length += _beingCarriedProperty.WriteLength(context);
        length += _lastCarrierIdProperty.WriteLength(context);
        return length;
    }
    
    protected override void Write(WriteStream stream, StreamContext context) {
        var writes = false;
        writes |= _beingCarriedProperty.Write(stream, context);
        writes |= _lastCarrierIdProperty.Write(stream, context);
        if (writes) InvalidateContextLength(context);
    }
    
    protected override void Read(ReadStream stream, StreamContext context) {
        var anyPropertiesChanged = false;
        while (stream.ReadNextPropertyID(out uint propertyID)) {
            var changed = false;
            switch (propertyID) {
                case (uint) PropertyID.BeingCarried: {
                    changed = _beingCarriedProperty.Read(stream, context);
                    if (changed) FireBeingCarriedDidChange(beingCarried);
                    break;
                }
                case (uint) PropertyID.LastCarrierId: {
                    changed = _lastCarrierIdProperty.Read(stream, context);
                    if (changed) FireLastCarrierIdDidChange(lastCarrierId);
                    break;
                }
                default: {
                    stream.SkipProperty();
                    break;
                }
            }
            anyPropertiesChanged |= changed;
        }
        if (anyPropertiesChanged) {
            UpdateBackingFields();
        }
    }
    
    private void UpdateBackingFields() {
        _beingCarried = beingCarried;
        _lastCarrierId = lastCarrierId;
    }
    
}
/* ----- End Normal Autogenerated Code ----- */
