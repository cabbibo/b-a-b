using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class WrenNetworked : RealtimeComponent {

    private WrenNetworkedModel _model;
    public Wren wren;

    private void Start() {
     
    }

    private WrenNetworkedModel model {

     
         set {
             

            
            if (_model != null) {
                // Unregister from events
                _model.colorDidChange -= ColorDidChange;
                _model.onGroundDidChange -= OnGroundDidChange;
                _model.inRaceDidChange -= InRaceDidChange;
                _model.explodedDidChange -= ExplodedDidChange;
                _model.timeValue1DidChange -= TimeValue1DidChange;
                _model.nameDidChange -= NameDidChange;
                _model.interfaceValue1DidChange -= InterfaceValue1DidChange;
                _model.interfaceValue2DidChange -= InterfaceValue2DidChange;
                _model.interfaceValue3DidChange -= InterfaceValue3DidChange;
                _model.interfaceValue4DidChange -= InterfaceValue4DidChange;
                _model.beaconLocationDidChange -= BeaconLocationDidChange;
                _model.beaconOnDidChange -= BeaconOnDidChange;
                
                _model.hue1DidChange       -= Hue1DidChange       ;
                _model.hue2DidChange       -= Hue2DidChange       ;
                _model.hue3DidChange       -= Hue3DidChange       ;
                _model.hue4DidChange       -= Hue4DidChange       ;

                _model.playerIDDidChange -= PlayerIDDidChange;

            }



            // Store the model
            _model = value;

            if (_model != null) {

              NewValues(_model);


            ColorDidChange      ( _model , _model.color      );
            OnGroundDidChange   ( _model , _model.onGround   );
            InRaceDidChange     ( _model , _model.inRace     );
            ExplodedDidChange   ( _model , _model.exploded   );
            TimeValue1DidChange ( _model , _model.timeValue1 );
            NameDidChange       ( _model , _model.name );
            Hue1DidChange       ( _model , _model.hue1 );
            Hue2DidChange       ( _model , _model.hue2 );
            Hue3DidChange       ( _model , _model.hue3 );
            Hue4DidChange       ( _model , _model.hue4 );
            
            InterfaceValue1DidChange       ( _model , _model.interfaceValue1 );
            InterfaceValue2DidChange       ( _model , _model.interfaceValue2 );
            InterfaceValue3DidChange       ( _model , _model.interfaceValue3 );
            InterfaceValue4DidChange       ( _model , _model.interfaceValue4 );


            BeaconLocationDidChange( _model , _model.beaconLocation );
            BeaconOnDidChange( _model , _model.beaconOn );

            PlayerIDDidChange   ( _model , _model.playerID );
            
            // Register for events so we'll know if the color changes later
            _model.colorDidChange      += ColorDidChange      ;
            _model.onGroundDidChange   += OnGroundDidChange   ;
            _model.inRaceDidChange     += InRaceDidChange     ;
            _model.explodedDidChange   += ExplodedDidChange   ;
            _model.timeValue1DidChange += TimeValue1DidChange ;
            _model.nameDidChange       += NameDidChange       ;

              _model.interfaceValue1DidChange += InterfaceValue1DidChange;
                _model.interfaceValue2DidChange += InterfaceValue2DidChange;
                _model.interfaceValue3DidChange += InterfaceValue3DidChange;
                _model.interfaceValue4DidChange += InterfaceValue4DidChange;
            _model.hue1DidChange       += Hue1DidChange       ;
            _model.hue2DidChange       += Hue2DidChange       ;
            _model.hue3DidChange       += Hue3DidChange       ;
            _model.hue4DidChange       += Hue4DidChange       ;
            _model.beaconLocationDidChange += BeaconLocationDidChange;
            _model.beaconOnDidChange += BeaconOnDidChange;
            _model.playerIDDidChange += PlayerIDDidChange;
                
            }
        }
    }

    private void NewValues(WrenNetworkedModel model){
    }

    private void ColorDidChange(WrenNetworkedModel model, Color value) {
    }

    private void OnGroundDidChange(WrenNetworkedModel model, bool value) {
        wren.state.GroundChange( value );
    }
    private void InRaceDidChange(WrenNetworkedModel model, int value) {
        wren.state.RaceChange( value );
    }

    private void TimeValue1DidChange(WrenNetworkedModel model, float value) {
        wren.state.TimeValue1Change(value);
    }   

    private void ExplodedDidChange(WrenNetworkedModel model, bool value) {
        //wren.state.ExplodeDidChange( value );
    }

    private void Hue1DidChange( WrenNetworkedModel model, float value) {

        wren.state.Hue1Change(value);
    }

    private void Hue2DidChange( WrenNetworkedModel model, float value) {
        wren.state.Hue2Change(value);
    }

    private void Hue3DidChange( WrenNetworkedModel model, float value) {
        wren.state.Hue3Change(value);
    }

    private void Hue4DidChange( WrenNetworkedModel model, float value) {
        wren.state.Hue4Change(value);
    }

    private void NameDidChange( WrenNetworkedModel model,string value){

    }

    private void InterfaceValue1DidChange( WrenNetworkedModel model,Vector3 value){
       // print("hey interface 1 did change");
    }

    private void InterfaceValue2DidChange( WrenNetworkedModel model,Vector3 value){
        //print("hey interface 2 did change");
    }

    private void InterfaceValue3DidChange( WrenNetworkedModel model,Vector3 value){
        //print("hey interface 3 did change");
    }

    private void InterfaceValue4DidChange( WrenNetworkedModel model,Vector3 value){
        //print("hey interface 4 did change");
    }

    private void BeaconLocationDidChange( WrenNetworkedModel model,Vector3 value){
        wren.state.BeaconLocationDidChange(value);
    }
    
    private void BeaconOnDidChange( WrenNetworkedModel model,bool value){
        wren.state.BeaconOnDidChange(value);
    }

    private void PlayerIDDidChange( WrenNetworkedModel model, uint value) {
        wren.state.PlayerIDDidChange(value);
    }


    public void SetColor( Color c ){
        _model.color = c;
    }

    public void SetOnGround( bool b ){
        if( _model != null ){
            _model.onGround = b;
        }else{
            wren.state.GroundChange(b);
        }
    }

    public void SetInRace( int b ){
         if( _model != null ){
            _model.inRace = b;
        }else{
            wren.state.RaceChange(b);
        }
    }
    
    public void SetTimeValue1( float v ){
        if( _model != null ){
            _model.timeValue1 = v;
        }else{
            wren.state.TimeValue1Change(v);
        }
    }

    public void SetBeaconOn( bool v ){
        if( _model != null ){
            _model.beaconOn = v;
        }else{
            wren.state.BeaconOnDidChange(v);
        }
    }

    public void SetBeaconLocation( Vector3 v ){

        if( _model != null ){
            _model.beaconLocation = v;
        }else{
            wren.state.BeaconLocationDidChange(v);
        }
    }

    public void SetInterface1Value( Vector3 v ){
        _model.interfaceValue1 = v;
    }

    public void SetInterface2Value( Vector3 v ){
        _model.interfaceValue2 = v;
    }

    public void SetInterface3Value( Vector3 v ){
        _model.interfaceValue3 = v;
    }

    public void SetInterface4Value( Vector3 v ){
        _model.interfaceValue4 = v;
    }


    public void SetHue1( float v ){ if( _model != null ){ _model.hue1 = v; }else{ wren.state.Hue1Change(v); } }
    public void SetHue2( float v ){ if( _model != null ){ _model.hue2 = v; }else{ wren.state.Hue2Change(v); } }
    public void SetHue3( float v ){ if( _model != null ){ _model.hue3 = v; }else{ wren.state.Hue3Change(v); } }
    public void SetHue4( float v ){ if( _model != null ){ _model.hue4 = v; }else{ wren.state.Hue4Change(v); } }

    public void SetPlayerID( uint v) { if(_model != null ){ _model.playerID = v; }else{ wren.state.PlayerIDDidChange(v);} }

}

