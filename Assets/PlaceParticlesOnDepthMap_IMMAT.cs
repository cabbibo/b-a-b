
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;
using IMMATERIA;
using WrenUtils;


public class PlaceParticlesOnDepthMap_IMMAT : Simulation
{


    public Camera camera;

    RenderTexture depthTexture;
    RenderTexture colorTexture;
    public Vector2 renderSize = new Vector2(1920, 1080);
    public override void Create()
    {

        print("hi");

        // Create a color texture
        colorTexture = new RenderTexture((int)renderSize.x, (int)renderSize.y, 0, RenderTextureFormat.ARGB32);
        colorTexture.Create();

        // Create a depth texture
        depthTexture = new RenderTexture((int)renderSize.x, (int)renderSize.y, 24, RenderTextureFormat.Depth);
        depthTexture.Create();

    }

    public override void OnDie()
    {

        if (depthTexture != null)
        {
            depthTexture.Release();
            colorTexture.Release();
        }
    }



    public override void Bind()
    {

        life.BindMatrix("_CameraViewMatrix", () => Camera.main.worldToCameraMatrix);
        life.BindMatrix("_CameraViewMatrixInverse", () => Camera.main.worldToCameraMatrix.inverse);
        life.BindMatrix("_CameraProjectionMatrix", () => Camera.main.projectionMatrix);
        life.BindMatrix("_CameraProjectionMatrixInverse", () => Camera.main.projectionMatrix.inverse);
        life.BindMatrix("_CameraViewProjectionMatrixInverse", () => Camera.main.worldToCameraMatrix.inverse * Camera.main.projectionMatrix.inverse);
        life.BindVector3("_CameraPosition", () => Camera.main.transform.position);
        life.BindFloat("_CameraNear", () => Camera.main.nearClipPlane);
        life.BindFloat("_CameraFar", () => Camera.main.farClipPlane);

        life.BindTexture("_DepthTexture", () => depthTexture);
        life.BindTexture("_ColorTexture", () => colorTexture);

        // Bind Wren God
        //WrenUtils.God.instance.SetWrenCompute(kernel, shader);

    }

    // Update is called once per frame
    public override void WhileLiving(float v)
    {

        // Set out depth camera properties to be the same as the main camera
        camera.fieldOfView = Camera.main.fieldOfView;
        camera.nearClipPlane = Camera.main.nearClipPlane;
        camera.farClipPlane = Camera.main.farClipPlane;
        camera.aspect = Camera.main.aspect;



        // set our camera to have the same transform as the main camera
        camera.transform.position = Camera.main.transform.position;
        camera.transform.rotation = Camera.main.transform.rotation;

        camera.SetTargetBuffers(depthTexture.colorBuffer, depthTexture.depthBuffer);
        camera.Render();

        camera.targetTexture = colorTexture;
        camera.Render();

        /*  debugDepthRenderer.sharedMaterial.SetTexture("_MainTex", depthTexture);

          shader.SetMatrix("_CameraViewMatrix", Camera.main.worldToCameraMatrix);
          shader.SetMatrix("_CameraViewMatrixInverse", Camera.main.worldToCameraMatrix.inverse);
          shader.SetMatrix("_CameraProjectionMatrix", Camera.main.projectionMatrix);
          shader.SetMatrix("_CameraProjectionMatrixInverse", Camera.main.projectionMatrix.inverse);
          shader.SetMatrix("_CameraViewProjectionMatrixInverse", Camera.main.worldToCameraMatrix.inverse * Camera.main.projectionMatrix.inverse);
          shader.SetVector("_CameraPosition", Camera.main.transform.position);
          shader.SetFloat("_CameraNear", Camera.main.nearClipPlane);
          shader.SetFloat("_CameraFar", Camera.main.farClipPlane);

          God.instance.SetWrenCompute(kernel, shader);

          float x = 1 - (Camera.main.farClipPlane / Camera.main.nearClipPlane);
          float y = Camera.main.farClipPlane / Camera.main.nearClipPlane;
          float z = x / Camera.main.farClipPlane;
          float w = y / Camera.main.farClipPlane;

          shader.SetVector("_ZBufferParams", new Vector4(x, y, z, w));
          shader.SetTexture(kernel, "_DepthTexture", depthTexture);
          shader.SetTexture(kernel, "_ColorTexture", colorTexture);

          shader.SetFloat("_Time", Time.time);

          shader.SetBuffer(kernel, "_VertBuffer", _VertBuffer);
          shader.SetInt("_VertBuffer_COUNT", count);

          shader.Dispatch(kernel, numGroups, 1, 1);

          if (mpb == null)
          {
              mpb = new MaterialPropertyBlock();
          }

          mpb.SetBuffer("_VertBuffer", _VertBuffer);
          mpb.SetInt("_Count", count);

          Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one * 500000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
    */
    }

}
