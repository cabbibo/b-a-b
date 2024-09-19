using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class WrenNetworkedModel{
    
    [RealtimeProperty(1, true, true)]
    private Color _color;
    
    [RealtimeProperty(2, true, true)]
    private bool _onGround;
    
    [RealtimeProperty(3, true, true)]
    private int _inRace;

    [RealtimeProperty(4, true, true)]
    private bool _exploded;

    [RealtimeProperty(5, false, true)]
    private float _timeValue1;

    
    [RealtimeProperty(6, true, true)]
    private string _name;


    [RealtimeProperty(7, true, true)]
    private Vector3 _interfaceValue1;


    [RealtimeProperty(8, true, true)]
    private Vector3 _interfaceValue2;


    [RealtimeProperty(9, true, true)]
    private Vector3 _interfaceValue3;
    
    [RealtimeProperty(10, true, true)]
    private Vector3 _interfaceValue4;


    [RealtimeProperty(11, true, true)]
    private Vector3 _beaconLocation;

    [RealtimeProperty(12, true, true)]
    private bool  _beaconOn;




    [RealtimeProperty(13, true, true)]
    private float _hue1;


    [RealtimeProperty(14, true, true)]
    private float _hue2;


    [RealtimeProperty(15, true, true)]
    private float _hue3;
    
    [RealtimeProperty(16, true, true)]
    private float _hue4;




   [RealtimeProperty(17, true, true)]
   private uint _playerID;


}


