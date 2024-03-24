using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class FullBird : MonoBehaviour
{





   public float eyeSize;
   public float beakSize;
   public Wren wren;
   public ComputeShader bodyShader;
   public ComputeShader wingShader;

   public Material bodyMaterial;
   public Material wingMaterial;

   public Material wingDebugMaterial;
   public Material bodyDebugMaterial;


   public Material wingDebugLineMaterial;
   public Material bodyDebugLineMaterial;

   public Transform specialTarget;

   [Range(0, 1)]
   public float percentageRendered;


   // Params for shaders

   public float _MiddleSecondaryFeatherScaleMultiplier;
   public float _BaseSecondaryFeatherScale;

   public float _MiddlePrimaryFeatherScaleMultiplier;
   public float _BasePrimaryFeatherScale;

   public float _MiddleCovertsFeatherScaleMultiplier;
   public float _BaseCovertsFeatherScale;

   public float _MiddleScapularFeatherScaleMultiplier;
   public float _BaseScapularFeatherScale;

   public float _MiddleTailFeatherScaleMultiplier;
   public float _BaseTailFeatherScale;



   public float _BackAmountOverlapping;
   public float _BaseDirectionLeftRightNoise;
   public float _BaseDirectionUpNoise;
   public float _BaseNoiseSize;
   public float _BaseNoiseScale;

   public float _NoiseSizeForFlutter;
   public float _MaxFlutter;
   public float _MinFlutter;
   public float _MaxFlutterSpeed;
   public float _MinFlutterSpeed;


   public float _ReturnToLockTime;
   public float _ReturnToLockForce;
   public float _ReturnToLockTimeMultiplier;

   public float _LockDistance;
   public float _LockLerp;



   public float _GroundLockForce;
   public float _GroundLockHeight;
   public float _ExplosionOutForce;
   public float _ExplosionUpForce;
   public float _ExplosionVelForce;



   public float _VortexInForce;
   public float _VortexCurlForce;
   public float _VortexNoiseForce;
   public float _VortexNoiseSize;


   public float _GroundVortexForce;
   public float _GroundVortexHeight;
   public float _ResetValue;


   public bool _OnGround;
   public float _LockedValue;
   public float _ExplosionValue;
   public Vector3 _ExplosionVector;
   public float _LockStartTime;
   public Vector3 _Velocity;


   public int _NumPrimaryFeathers;
   public int _NumPrimaryCoverts;
   public int _NumLesserCovertsRows;
   public int _NumLesserCovertsCols;


   public int _NumScapularRows;
   public int _NumScapularColumns;

   public int _NumMantleRows;
   public int _NumMantleColumns;

   public int _NumTailFeathers;

   public Mesh primaryFeather;
   public Mesh secondaryFeather;
   public Mesh primaryCovert;
   public Mesh secondaryCovert;
   public Mesh lesserCovert;

   public Mesh tailFeather;
   public Mesh scapularFeather;

   public Pose pose;


   // wings now in charge of positioning, 
   // but not the rendering
   public Wing1 leftWing;
   public Wing1 rightWing;
   public GPUWing leftWing_gpu;
   public GPUWing rightWing_gpu;


   public GPUTrailFromFeathers leftWingTrailFromFeathers_gpu;
   public GPUTrailFromFeathers rightWingTrailFromFeathers_gpu;
   public GPUBody body_gpu;

   // tail rendinger part of body now
   public Tail tail;



   public Transform head;
   public Transform neck;
   public Transform shoulder;
   public Transform spine;
   public Transform hip;

   public Transform leftHip;
   public Transform leftKnee;
   public Transform leftFoot;


   public Transform rightHip;
   public Transform rightKnee;
   public Transform rightFoot;

   public float shoulderWidth;

   public float hipToTail;
   public float hipToSpine;
   public float spineToShoulder;
   public float shoulderToNeck;
   public float neckToHead;

   public float chestToShoulder;
   public float shoulderToElbow;
   public float elbowToHand;
   public float handToFinger;



   public float hipSpread;
   public float hipToKnee;
   public float kneeToFoot;




   public Transform leftEye;
   public Transform rightEye;
   public Transform beak;


   private Vector3 leftEyeOPos;
   private Vector3 rightEyeOPos;
   private Vector3 beakOPos;

   private Vector3 leftEyeOScale;
   private Vector3 rightEyeOScale;
   private Vector3 beakOScale;


   private Quaternion leftEyeORot;
   private Quaternion rightEyeORot;
   private Quaternion beakORot;

   private Transform leftEyeOParent;
   private Transform rightEyeOParent;
   private Transform beakOParent;


   public Vector3 _ResetLocation;

   public int totalShards;

   void OnEnable()
   {

      // Gets our original position
      leftEyeOPos = leftEye.localPosition;
      rightEyeOPos = rightEye.localPosition;
      beakOPos = beak.localPosition;

      leftEyeORot = leftEye.localRotation;
      rightEyeORot = rightEye.localRotation;
      beakORot = beak.localRotation;


      leftEyeOScale = new Vector3(10, 10, 10);// leftEye.localScale;
      rightEyeOScale = new Vector3(10, 10, 10);//rightEye.localScale;
      beakOScale = new Vector3(10, 10, 10);///beak.localScale;


      leftEyeOParent = leftEye.parent;
      rightEyeOParent = rightEye.parent;
      beakOParent = beak.parent;


      leftWing_gpu.shader = wingShader;
      leftWing_gpu.featherDebugMaterial = wingDebugMaterial;
      leftWing_gpu.featherDebugLineMaterial = wingDebugLineMaterial;
      leftWing_gpu.featherMaterial = wingMaterial;

      leftWing_gpu.primaryFeather = primaryFeather;
      leftWing_gpu.secondaryFeather = secondaryFeather;
      leftWing_gpu.primaryCovert = primaryCovert;
      leftWing_gpu.secondaryCovert = secondaryCovert;
      leftWing_gpu.lesserCovert = lesserCovert;

      leftWing_gpu.numberPrimaryFeathers = _NumPrimaryFeathers;
      leftWing_gpu.numberPrimaryCoverts = _NumPrimaryCoverts;
      leftWing_gpu.numberLesserCovertsRows = _NumLesserCovertsRows;
      leftWing_gpu.numberLesserCovertsCols = _NumLesserCovertsCols;

      leftWing_gpu.wing = leftWing;
      leftWing_gpu.bird = this;



      rightWing_gpu.shader = wingShader;
      rightWing_gpu.featherDebugMaterial = wingDebugMaterial;
      rightWing_gpu.featherDebugLineMaterial = wingDebugLineMaterial;
      rightWing_gpu.featherMaterial = wingMaterial;

      rightWing_gpu.primaryFeather = primaryFeather;
      rightWing_gpu.secondaryFeather = secondaryFeather;
      rightWing_gpu.primaryCovert = primaryCovert;
      rightWing_gpu.secondaryCovert = secondaryCovert;
      rightWing_gpu.lesserCovert = lesserCovert;

      rightWing_gpu.numberPrimaryFeathers = _NumPrimaryFeathers;
      rightWing_gpu.numberPrimaryCoverts = _NumPrimaryCoverts;
      rightWing_gpu.numberLesserCovertsRows = _NumLesserCovertsRows;
      rightWing_gpu.numberLesserCovertsCols = _NumLesserCovertsCols;

      rightWing_gpu.wing = rightWing;
      rightWing_gpu.bird = this;

      body_gpu.shader = bodyShader;
      body_gpu.featherMaterial = bodyMaterial;
      body_gpu.featherDebugMaterial = bodyDebugMaterial;
      body_gpu.featherDebugLineMaterial = bodyDebugLineMaterial;

      body_gpu.scapularFeather = scapularFeather;
      body_gpu.tailFeather = tailFeather;

      body_gpu.numberScapularColumns = _NumScapularColumns;
      body_gpu.numberScapularRows = _NumScapularRows;
      body_gpu.numberTailFeathers = _NumTailFeathers;

      body_gpu.bird = this;

      leftWing.Create();
      rightWing.Create();

      leftWingTrailFromFeathers_gpu.Create();
      rightWingTrailFromFeathers_gpu.Create();

      leftWing_gpu.Create();
      rightWing_gpu.Create();

      body_gpu.Create();



      totalShards = body_gpu.totalFeatherPoints + leftWing_gpu.totalFeathers + rightWing_gpu.totalFeathers;

      ResetFeatherValues();
      SetMaterialProperties();


   }

   void OnDisable()
   {

      leftWing.Destroy();
      rightWing.Destroy();
      leftWing_gpu.Destroy();
      rightWing_gpu.Destroy();
      body_gpu.Destroy();



      leftWingTrailFromFeathers_gpu.Destroy();
      rightWingTrailFromFeathers_gpu.Destroy();


   }

   // Update is called once per frame
   void FixedUpdate()
   {

      leftWing.limbLengths[0] = wren._ScaleMultiplier * chestToShoulder;
      leftWing.limbLengths[1] = wren._ScaleMultiplier * shoulderToElbow;
      leftWing.limbLengths[2] = wren._ScaleMultiplier * elbowToHand;
      leftWing.limbLengths[3] = wren._ScaleMultiplier * handToFinger;

      leftWing.rots[0] = pose.wingRot1_L;
      leftWing.rots[1] = pose.wingRot2_L;
      leftWing.rots[2] = pose.wingRot3_L;
      leftWing.rots[3] = pose.wingRot4_L;

      rightWing.limbLengths[0] = wren._ScaleMultiplier * chestToShoulder;
      rightWing.limbLengths[1] = wren._ScaleMultiplier * shoulderToElbow;
      rightWing.limbLengths[2] = wren._ScaleMultiplier * elbowToHand;
      rightWing.limbLengths[3] = wren._ScaleMultiplier * handToFinger;

      rightWing.rots[0] = pose.wingRot1_R;
      rightWing.rots[1] = pose.wingRot2_R;
      rightWing.rots[2] = pose.wingRot3_R;
      rightWing.rots[3] = pose.wingRot4_R;

      tail.rots[0] = pose.tailRot1;
      tail.rots[1] = pose.tailRot2;
      tail.rots[2] = pose.tailRot3;

      tail.transform.localRotation = Quaternion.Euler(pose.tailRot);
      hip.localRotation = Quaternion.Euler(pose.hipRot);
      spine.localRotation = Quaternion.Euler(pose.spineRot);
      shoulder.localRotation = Quaternion.Euler(pose.shoulderRot);
      neck.localRotation = Quaternion.Euler(pose.neckRot);
      head.localRotation = Quaternion.Euler(pose.headRot);

      spine.localPosition = Vector3.forward * wren._ScaleMultiplier * hipToSpine;
      shoulder.localPosition = Vector3.forward * wren._ScaleMultiplier * spineToShoulder;
      neck.localPosition = Vector3.forward * wren._ScaleMultiplier * shoulderToNeck;
      head.localPosition = Vector3.forward * wren._ScaleMultiplier * neckToHead;

      tail.transform.localPosition = Vector3.forward * wren._ScaleMultiplier * -hipToTail;

      leftWing.transform.localPosition = -Vector3.right * wren._ScaleMultiplier * shoulderWidth;
      rightWing.transform.localPosition = Vector3.right * wren._ScaleMultiplier * shoulderWidth;





      leftWing.UpdatePositions();
      rightWing.UpdatePositions();
      tail.UpdatePositions();

      leftWing_gpu.lockedValue = _LockedValue;
      rightWing_gpu.lockedValue = _LockedValue;
      body_gpu.lockedValue = _LockedValue;


      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      leftWingTrailFromFeathers_gpu.UpdateFeathers();
      rightWingTrailFromFeathers_gpu.UpdateFeathers();


      if (_OnGround)
      {
         //leftEye.position = Vector3.Lerp( leftEye.position , transform.position , .01f );
         //rightEye.position = Vector3.Lerp( rightEye.position , transform.position , .01f );
         //beak.position = Vector3.Lerp( beak.position , transform.position , .01f );

         leftEye.localScale = Vector3.Lerp(leftEye.localScale, Vector3.one * .00001f, .03f);
         rightEye.localScale = Vector3.Lerp(rightEye.localScale, Vector3.one * .00001f, .03f);
         beak.localScale = Vector3.Lerp(beak.localScale, Vector3.one * .00001f, .03f);
      }
      else
      {
         leftEye.localScale = Vector3.Lerp(leftEye.localScale, wren._ScaleMultiplier * Vector3.one * eyeSize, .05f);
         rightEye.localScale = Vector3.Lerp(rightEye.localScale, wren._ScaleMultiplier * Vector3.one * eyeSize, .05f);
         beak.localScale = Vector3.Lerp(beak.localScale, wren._ScaleMultiplier * Vector3.one * beakSize, .05f);
      }


      SetUpDebug();
      SetUpDraw();

   }

   public void LateUpdate()
   {


      leftWing.UpdatePositions();
      rightWing.UpdatePositions();
      tail.UpdatePositions();


      body_gpu.UpdateFeathers();
      body_gpu.DrawFeathers();


      leftWing_gpu.UpdateFeathers();
      leftWing_gpu.DrawFeathers();

      rightWing_gpu.UpdateFeathers();
      rightWing_gpu.DrawFeathers();


      //SetUpDebug();
      //SetUpDraw();
   }


   public void TakeOff()
   {
      _LockedValue = 1;
      _LockStartTime = Time.time;

      _OnGround = false;

      //leftEye.localPosition = leftEyeOPos;
      //leftEye.localRotation = leftEyeORot;
      //leftEye.parent = leftEyeOParent;

      //rightEye.localPosition = rightEyeOPos;
      //rightEye.localRotation = rightEyeORot;
      //rightEye.parent = rightEyeOParent;

      //beak.localPosition = beakOPos;
      //beak.localRotation = beakORot;
      //beak.parent = beakOParent;




   }

   public void ResetFeatherValues()
   {

      _ResetValue = 1;

      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      _ResetValue = 0;
   }

   public void ResetAtLocation(Vector3 v)
   {


      _ResetValue = 2;
      _ResetLocation = v;


      _LockedValue = 1;
      _ExplosionValue = 0;
      _ExplosionVector = Vector3.zero;


      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();



      _LockedValue = 0;
      _ExplosionValue = 0;
      _ExplosionVector = Vector3.zero;

      _ResetValue = 0;

   }


   public void Teleport()
   {

      print("Teleporting: " + head.transform.position);
      _ResetValue = 2;



      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      _ResetValue = 0;
   }


   public void PhaseShift(Vector3 v)
   {

      _ResetValue = 3;
      _ResetLocation = v;

      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      _ResetValue = 0;

   }


   public void HitGround()
   {

      _OnGround = true;

      _LockedValue = 0;
      _ExplosionValue = 1;
      _ExplosionVector = Vector3.zero;

      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      // leftEye.parent = transform;
      // rightEye.parent = transform;
      // beak.parent = transform;



      _LockedValue = 0;
      _ExplosionValue = 0;
      _ExplosionVector = Vector3.zero;

   }



   public void HitGround(Collision c)
   {

      _OnGround = true;

      _LockedValue = 0;
      _ExplosionValue = 1;
      _ExplosionVector = c.impulse;


      print(c.impulse);
      print(wren.physics.vel);

      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();

      // leftEye.parent = transform;
      // rightEye.parent = transform;
      // beak.parent = transform;



      _LockedValue = 0;
      _ExplosionValue = 0;
      _ExplosionVector = Vector3.zero;

   }

   public void Explode()
   {

      _LockedValue = 0;
      _ExplosionValue = 1;
      _ExplosionVector = wren.physics.vel;


      leftWing_gpu.UpdateFeathers();
      rightWing_gpu.UpdateFeathers();
      body_gpu.UpdateFeathers();


      _LockedValue = 1;
      _ExplosionValue = 0;
      _ExplosionVector = Vector3.zero;

      _LockStartTime = Time.time;

   }




   public void SetBirdParameters(ComputeShader shader)
   {



      /*
         Setting some targets that *ARENT* the bird!
      */
      if (specialTarget != null)
      {
         shader.SetMatrix("_SpecialTarget", specialTarget.localToWorldMatrix);
      }




      //---
      shader.SetFloat("_ResetValue", _ResetValue);
      shader.SetVector("_ResetLocation", _ResetLocation);

      shader.SetFloat("_NoiseSizeForFlutter", _NoiseSizeForFlutter);
      shader.SetFloat("_MaxFlutter", _MaxFlutter);
      shader.SetFloat("_MinFlutter", _MinFlutter);
      shader.SetFloat("_MaxFlutterSpeed", _MaxFlutterSpeed);
      shader.SetFloat("_MinFlutterSpeed", _MinFlutterSpeed);

      shader.SetFloat("_ReturnToLockTime", _ReturnToLockTime);
      shader.SetFloat("_ReturnToLockForce", _ReturnToLockForce);
      shader.SetFloat("_ReturnToLockTimeMultiplier", _ReturnToLockTimeMultiplier);

      shader.SetFloat("_LockDistance", _LockDistance);
      shader.SetFloat("_LockLerp", _LockLerp);

      shader.SetFloat("_ExplosionOutForce", _ExplosionOutForce);
      shader.SetFloat("_ExplosionUpForce", _ExplosionUpForce);
      shader.SetFloat("_ExplosionVelForce", _ExplosionVelForce);
      shader.SetFloat("_BackAmountOverlapping", _BackAmountOverlapping);
      shader.SetFloat("_BaseDirectionLeftRightNoise", _BaseDirectionLeftRightNoise);

      shader.SetFloat("_BaseDirectionUpNoise", _BaseDirectionUpNoise);
      shader.SetFloat("_BaseNoiseScale", _BaseNoiseScale);

      shader.SetFloat("_ScaleMultiplier", wren._ScaleMultiplier);


      shader.SetFloat("_VortexInForce", _VortexInForce);
      shader.SetFloat("_VortexCurlForce", _VortexCurlForce);
      shader.SetFloat("_VortexNoiseForce", _VortexNoiseForce);
      shader.SetFloat("_VortexNoiseSize", _VortexNoiseSize);

      shader.SetFloat("_GroundVortexHeight", _GroundVortexHeight);
      shader.SetFloat("_GroundVortexForce", _GroundVortexForce);

      shader.SetFloat("_GroundLockHeight", _GroundLockHeight);
      shader.SetFloat("_GroundLockForce", _GroundLockForce);

      shader.SetFloat("_MiddleSecondaryFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleSecondaryFeatherScaleMultiplier);
      shader.SetFloat("_BaseSecondaryFeatherScale", wren._ScaleMultiplier * _BaseSecondaryFeatherScale);


      shader.SetFloat("_MiddlePrimaryFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddlePrimaryFeatherScaleMultiplier);
      shader.SetFloat("_BasePrimaryFeatherScale", wren._ScaleMultiplier * _BasePrimaryFeatherScale);

      shader.SetFloat("_MiddleCovertsFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleCovertsFeatherScaleMultiplier);
      shader.SetFloat("_BaseCovertsFeatherScale", wren._ScaleMultiplier * _BaseCovertsFeatherScale);


      shader.SetFloat("_MiddleScapularFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleScapularFeatherScaleMultiplier);
      shader.SetFloat("_BaseScapularFeatherScale", wren._ScaleMultiplier * _BaseScapularFeatherScale);


      shader.SetFloat("_MiddleTailFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleTailFeatherScaleMultiplier);
      shader.SetFloat("_BaseTailFeatherScale", wren._ScaleMultiplier * _BaseTailFeatherScale);


      shader.SetFloat("_LockStartTime", _LockStartTime);
      shader.SetFloat("_Locked", _LockedValue);
      shader.SetFloat("_Explosion", _ExplosionValue);
      shader.SetVector("_ExplosionVector", _ExplosionVector);
      shader.SetVector("_Velocity", _Velocity);


      shader.SetInt("_NumScapularColumns", _NumScapularColumns);
      shader.SetInt("_NumScapularRows", _NumScapularRows);
      shader.SetInt("_NumTailFeathers", _NumTailFeathers);


      shader.SetFloat("_Locked", _LockedValue);



      shader.SetInt("_NumPrimaryFeathers", _NumPrimaryFeathers);
      shader.SetInt("_NumPrimaryCoverts", _NumPrimaryCoverts);
      shader.SetInt("_NumLesserCovertRows", _NumLesserCovertsRows);
      shader.SetInt("_NumLesserCovertCols", _NumLesserCovertsCols);


      shader.SetFloat("_Time", Time.time);
      shader.SetFloat("_DT", Time.deltaTime);


   }



   public void SetBirdParameters(MaterialPropertyBlock shader)
   {



      /*
         Setting some targets that *ARENT* the bird!
      */
      if (specialTarget != null)
      {
         shader.SetMatrix("_SpecialTarget", specialTarget.localToWorldMatrix);
      }




      //---
      shader.SetFloat("_ResetValue", _ResetValue);
      shader.SetVector("_ResetLocation", _ResetLocation);

      shader.SetFloat("_NoiseSizeForFlutter", _NoiseSizeForFlutter);
      shader.SetFloat("_MaxFlutter", _MaxFlutter);
      shader.SetFloat("_MinFlutter", _MinFlutter);
      shader.SetFloat("_MaxFlutterSpeed", _MaxFlutterSpeed);
      shader.SetFloat("_MinFlutterSpeed", _MinFlutterSpeed);

      shader.SetFloat("_ReturnToLockTime", _ReturnToLockTime);
      shader.SetFloat("_ReturnToLockForce", _ReturnToLockForce);
      shader.SetFloat("_ReturnToLockTimeMultiplier", _ReturnToLockTimeMultiplier);

      shader.SetFloat("_LockDistance", _LockDistance);
      shader.SetFloat("_LockLerp", _LockLerp);

      shader.SetFloat("_ExplosionOutForce", _ExplosionOutForce);
      shader.SetFloat("_ExplosionUpForce", _ExplosionUpForce);
      shader.SetFloat("_ExplosionVelForce", _ExplosionVelForce);
      shader.SetFloat("_BackAmountOverlapping", _BackAmountOverlapping);
      shader.SetFloat("_BaseDirectionLeftRightNoise", _BaseDirectionLeftRightNoise);

      shader.SetFloat("_BaseDirectionUpNoise", _BaseDirectionUpNoise);
      shader.SetFloat("_BaseNoiseScale", _BaseNoiseScale);

      shader.SetFloat("_ScaleMultiplier", wren._ScaleMultiplier);


      shader.SetFloat("_VortexInForce", _VortexInForce);
      shader.SetFloat("_VortexCurlForce", _VortexCurlForce);
      shader.SetFloat("_VortexNoiseForce", _VortexNoiseForce);
      shader.SetFloat("_VortexNoiseSize", _VortexNoiseSize);

      shader.SetFloat("_GroundVortexHeight", _GroundVortexHeight);
      shader.SetFloat("_GroundVortexForce", _GroundVortexForce);

      shader.SetFloat("_GroundLockHeight", _GroundLockHeight);
      shader.SetFloat("_GroundLockForce", _GroundLockForce);

      shader.SetFloat("_MiddleSecondaryFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleSecondaryFeatherScaleMultiplier);
      shader.SetFloat("_BaseSecondaryFeatherScale", wren._ScaleMultiplier * _BaseSecondaryFeatherScale);


      shader.SetFloat("_MiddlePrimaryFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddlePrimaryFeatherScaleMultiplier);
      shader.SetFloat("_BasePrimaryFeatherScale", wren._ScaleMultiplier * _BasePrimaryFeatherScale);

      shader.SetFloat("_MiddleCovertsFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleCovertsFeatherScaleMultiplier);
      shader.SetFloat("_BaseCovertsFeatherScale", wren._ScaleMultiplier * _BaseCovertsFeatherScale);


      shader.SetFloat("_MiddleScapularFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleScapularFeatherScaleMultiplier);
      shader.SetFloat("_BaseScapularFeatherScale", wren._ScaleMultiplier * _BaseScapularFeatherScale);


      shader.SetFloat("_MiddleTailFeatherScaleMultiplier", wren._ScaleMultiplier * _MiddleTailFeatherScaleMultiplier);
      shader.SetFloat("_BaseTailFeatherScale", wren._ScaleMultiplier * _BaseTailFeatherScale);


      shader.SetFloat("_LockStartTime", _LockStartTime);
      shader.SetFloat("_Locked", _LockedValue);
      shader.SetFloat("_Explosion", _ExplosionValue);
      shader.SetVector("_ExplosionVector", _ExplosionVector);
      shader.SetVector("_Velocity", _Velocity);


      shader.SetInt("_NumScapularColumns", _NumScapularColumns);
      shader.SetInt("_NumScapularRows", _NumScapularRows);
      shader.SetInt("_NumTailFeathers", _NumTailFeathers);


      shader.SetFloat("_Locked", _LockedValue);



      shader.SetInt("_NumPrimaryFeathers", _NumPrimaryFeathers);
      shader.SetInt("_NumPrimaryCoverts", _NumPrimaryCoverts);
      shader.SetInt("_NumLesserCovertRows", _NumLesserCovertsRows);
      shader.SetInt("_NumLesserCovertCols", _NumLesserCovertsCols);


      shader.SetFloat("_Time", Time.time);
      shader.SetFloat("_DT", Time.deltaTime);


   }




   public void SetMaterialProperties()
   {

      if (leftWing_gpu.mpb == null)
      {
         OnEnable();
      }


      leftWing_gpu.mpb.SetFloat("_Hue1", wren.state.hue1);
      leftWing_gpu.mpb.SetFloat("_Hue2", wren.state.hue2);
      leftWing_gpu.mpb.SetFloat("_Hue3", wren.state.hue3);
      leftWing_gpu.mpb.SetFloat("_Hue4", wren.state.hue4);




      rightWing_gpu.mpb.SetFloat("_Hue1", wren.state.hue1);
      rightWing_gpu.mpb.SetFloat("_Hue2", wren.state.hue2);
      rightWing_gpu.mpb.SetFloat("_Hue3", wren.state.hue3);
      rightWing_gpu.mpb.SetFloat("_Hue4", wren.state.hue4);





      body_gpu.mpb.SetFloat("_Hue1", wren.state.hue1);
      body_gpu.mpb.SetFloat("_Hue2", wren.state.hue2);
      body_gpu.mpb.SetFloat("_Hue3", wren.state.hue3);
      body_gpu.mpb.SetFloat("_Hue4", wren.state.hue4);


      leftWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue1", wren.state.hue1);
      leftWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue2", wren.state.hue2);
      leftWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue3", wren.state.hue3);
      leftWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue4", wren.state.hue4);

      rightWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue1", wren.state.hue1);
      rightWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue2", wren.state.hue2);
      rightWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue3", wren.state.hue3);
      rightWingTrailFromFeathers_gpu.mpb.SetFloat("_Hue4", wren.state.hue4);



   }


   public bool debugHierarchyConnections;
   public bool debugHierarchyBasis;
   public DebugHierarchy debugHierarchy;

   public void SetUpDebug()
   {

      debugHierarchy.debugConnections = debugHierarchyConnections;
      debugHierarchy.debugBasis = debugHierarchyBasis;

   }



   public bool drawBodyFeather;
   public bool drawLeftWingFeathers;
   public bool drawRightWingFeathers;


   public bool drawLeftFeatherLines;
   public bool drawRightFeatherLines;

   // debug values
   public bool drawLeftWingFeatherPoints;
   public bool drawRightWingFeatherPoints;

   public bool drawBodyPoints;
   public bool drawBodyFeatherPoints;





   public void SetUpDraw()
   {
      leftWing_gpu.drawFeathers = drawLeftWingFeathers;
      rightWing_gpu.drawFeathers = drawRightWingFeathers;
      body_gpu.drawFeathers = drawBodyFeather;

      leftWing_gpu.debugFeatherPoints = drawLeftWingFeatherPoints;
      rightWing_gpu.debugFeatherPoints = drawRightWingFeatherPoints;
      body_gpu.debugFeatherPoints = drawBodyFeatherPoints;

      leftWing_gpu.debugLinePoints = drawLeftFeatherLines;
      rightWing_gpu.debugLinePoints = drawRightFeatherLines;
      body_gpu.debugLinePoints = drawBodyPoints;




   }



}
