using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CinematicCameraManager : BaseCameraManager
{


    // TYPES
    // Orbit
    // Location
    // Lock to bird, 
    // soft lock

    [System.Serializable]
    public class CinematicCameraDescriptor
    {

        public enum CameraType
        {
            Orbit,
            Location,
            LockToBird,
            None
        }
        public enum LockToBirdLocation { Soul, Head, LeftEye, RightEye }

        [Header("General Info")]
        public CameraType cameraType = CameraType.Orbit;

        public Transform lookAtTarget;

        public Vector3 upVector = Vector3.up;

        public Vector3 lookOffset = Vector3.zero;

        public float FOV = 60;
        public float wiggleSize = 0;
        public Vector3 osscilationSize = new Vector3(0, 0, 0);
        public float osscilateSpeed = 0;


        [Header("Orbit Info")]
        // Orbit info
        public Transform orbitCenter;
        public float orbitRadius = 10;
        public float orbitSpeed = 1;
        public float orbitVerticalOffset = 0;

        public Vector3 orbitAxis = Vector3.up;


        [Header("Location Info")]

        public Transform locationTarget;
        public Vector3 locationOffset;



        [Header("Lock to Bird Info")]

        public bool localOffset;
        public bool useLocalUp;
        public Vector3 lockBirdOffset = new Vector3(0, 0, 0);
        public LockToBirdLocation locktoBirdLocation = LockToBirdLocation.Soul;



        [Header("Data")]

        public Vector3 finalLookPosition;




    }

    public CinematicCameraDescriptor currentDescriptor;//= new CinematicCameraDescriptor();


    // Start is called before the first frame update
    void Start()
    {

    }



    public void SetCamera(CinematicCameraDescriptor cam)
    {

        currentDescriptor = cam;
        RequestPriority();
    }


    public void SetCamera(CinematicCameraDescriptor cam, float speed)
    {

        //        print("SETTING");
        currentDescriptor = cam;
        RequestPriority(speed);
    }


    public void ReleaseCamera()
    {
        currentDescriptor = null;
        ReleasePriority();
    }



    // Update is called once per frame
    void Update()
    {

        if (currentDescriptor == null || God.wren == null)
        {
            return;
        }

        if (currentDescriptor.cameraType == CinematicCameraDescriptor.CameraType.None)
        {
            return;
        }

        Vector3 targetPosition = GetCameraPosition(currentDescriptor);
        Vector3 lookPosition = GetCameraLookPosition(currentDescriptor);
        Vector3 lookUp = GetCameraLookUp(currentDescriptor);

        transform.position = targetPosition;
        transform.LookAt(lookPosition, lookUp);



        FOV = currentDescriptor.FOV;


    }

    public Vector3 GetCameraPosition(CinematicCameraDescriptor cam)
    {
        Vector3 pos = Vector3.zero;

        switch (cam.cameraType)
        {
            case CinematicCameraDescriptor.CameraType.Orbit:
                pos = GetOrbitPosition(cam);
                break;
            case CinematicCameraDescriptor.CameraType.Location:
                pos = GetLocationPosition(cam);
                break;
            case CinematicCameraDescriptor.CameraType.LockToBird:
                pos = GetLockToBirdPosition(cam);
                break;

        }

        return pos;

    }

    public Vector3 GetCameraLookUp(CinematicCameraDescriptor cam)
    {

        Vector3 pos = cam.upVector;

        switch (cam.cameraType)
        {
            case CinematicCameraDescriptor.CameraType.Orbit:
                pos = cam.orbitAxis;
                break;
            case CinematicCameraDescriptor.CameraType.Location:
                break;
            case CinematicCameraDescriptor.CameraType.LockToBird:
                if (cam.localOffset)
                {
                    pos = God.wren.transform.TransformDirection(pos);
                }
                break;

        }

        return pos;
    }

    public Vector3 GetCameraLookPosition(CinematicCameraDescriptor cam)
    {
        Vector3 pos = Vector3.zero;

        pos += cam.lookOffset;

        switch (cam.cameraType)
        {
            case CinematicCameraDescriptor.CameraType.Orbit:
                pos = cam.orbitCenter.position;
                break;
            case CinematicCameraDescriptor.CameraType.Location:
                pos = GetBirdPosition(cam);
                break;
            case CinematicCameraDescriptor.CameraType.LockToBird:
                pos = GetBirdPosition(cam);

                Vector3 offset = cam.lookOffset;
                if (cam.localOffset)
                {
                    offset = God.wren.transform.TransformDirection(offset);
                }

                pos += offset;

                break;

        }




        return pos;
    }


    public Vector3 GetOrbitPosition(CinematicCameraDescriptor cam)
    {
        Vector3 pos = Vector3.zero;

        Vector3 center = cam.orbitCenter.position;
        Vector3 axis = cam.orbitAxis.normalized;

        float angle = Time.time * cam.orbitSpeed;

        pos = center + Quaternion.AngleAxis(angle, axis) * Vector3.forward * cam.orbitRadius;

        return pos;


    }


    public Vector3 GetLocationPosition(CinematicCameraDescriptor cam)
    {
        Vector3 pos = cam.locationTarget.position;

        pos += cam.locationOffset;

        return pos;
    }



    public Vector3 GetBirdPosition(CinematicCameraDescriptor cam)
    {
        Vector3 pos = God.wren.transform.position;

        switch (cam.locktoBirdLocation)
        {
            case CinematicCameraDescriptor.LockToBirdLocation.Soul:
                pos = God.wren.soul.transform.position;
                break;
            case CinematicCameraDescriptor.LockToBirdLocation.Head:
                pos = God.wren.bird.head.position;
                break;

            case CinematicCameraDescriptor.LockToBirdLocation.LeftEye:
                pos = God.wren.bird.leftEye.position;
                break;
            case CinematicCameraDescriptor.LockToBirdLocation.RightEye:
                pos = God.wren.bird.rightEye.position;
                break;

        }


        return pos;
    }

    public Vector3 GetLockToBirdPosition(CinematicCameraDescriptor cam)
    {

        if (God.wren == null)
        {
            return Vector3.zero;
        }


        Vector3 pos = GetBirdPosition(cam);

        Vector3 offset = cam.lockBirdOffset;



        offset += Vector3.right * God.wren.input.rightX * cam.wiggleSize;
        offset += Vector3.up * God.wren.input.rightY * cam.wiggleSize;

        offset += new Vector3(
            Mathf.Sin(Time.time * cam.osscilateSpeed * 1.2f + 12.313f) * cam.osscilationSize.x,
            Mathf.Sin(Time.time * cam.osscilateSpeed * .9f + 312.32f) * cam.osscilationSize.y,
            Mathf.Sin(Time.time * cam.osscilateSpeed * 1f + .31f) * cam.osscilationSize.z
            );

        if (cam.localOffset)
        {

            offset = God.wren.soul.transform.TransformDirection(offset);
        }

        offset += God.camera.transform.right * God.wren.input.leftX * cam.wiggleSize;
        offset += God.camera.transform.up * God.wren.input.leftY * cam.wiggleSize;

        pos += offset;

        return pos;
    }



    public void LerpCamera(CinematicCameraDescriptor a, CinematicCameraDescriptor b, float t)
    {
        SetCamera(LerpDescriptors(a, b, t));
    }



    public CinematicCameraDescriptor LerpDescriptors(CinematicCameraDescriptor a, CinematicCameraDescriptor b, float t)
    {
        CinematicCameraDescriptor c = new CinematicCameraDescriptor();

        if (a.cameraType != b.cameraType)
        {
            Debug.LogWarning("Camera types are different, cannot lerp");
            //return a;
        }

        c.cameraType = a.cameraType;

        c.FOV = Mathf.Lerp(a.FOV, b.FOV, t);
        c.wiggleSize = Mathf.Lerp(a.wiggleSize, b.wiggleSize, t);
        c.osscilationSize = Vector3.Lerp(a.osscilationSize, b.osscilationSize, t);
        c.osscilateSpeed = Mathf.Lerp(a.osscilateSpeed, b.osscilateSpeed, t);

        c.orbitCenter = a.orbitCenter;
        c.orbitRadius = Mathf.Lerp(a.orbitRadius, b.orbitRadius, t);
        c.orbitSpeed = Mathf.Lerp(a.orbitSpeed, b.orbitSpeed, t);
        c.orbitVerticalOffset = Mathf.Lerp(a.orbitVerticalOffset, b.orbitVerticalOffset, t);
        c.orbitAxis = Vector3.Lerp(a.orbitAxis, b.orbitAxis, t);

        c.locationTarget = a.locationTarget;
        c.locationOffset = Vector3.Lerp(a.locationOffset, b.locationOffset, t);

        c.localOffset = a.localOffset;
        c.lockBirdOffset = Vector3.Lerp(a.lockBirdOffset, b.lockBirdOffset, t);
        c.locktoBirdLocation = a.locktoBirdLocation;

        c.lookAtTarget = a.lookAtTarget;
        c.upVector = Vector3.Lerp(a.upVector, b.upVector, t);
        c.lookOffset = Vector3.Lerp(a.lookOffset, b.lookOffset, t);


        return c;
    }


    public IEnumerator LerpCamera(CinematicCamera from, CinematicCamera to, float time)
    {
        float cT = 0;
        while (cT < time)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            //cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, cT / time));
            LerpCamera(from.info, to.info, cT / time);


            cT += Time.unscaledDeltaTime;
            yield return null;
        }

        SetCamera(to.info, 1);
    }



}
