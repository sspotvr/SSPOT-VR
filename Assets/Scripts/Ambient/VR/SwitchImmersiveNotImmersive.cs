using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class SwitchImmersiveNotImmersive : MonoBehaviour
{

    public float spinForce;
    private bool authorization = true;
    private bool immersiveVR = true;

    
    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(0, spinForce * Time.deltaTime, 0);
    }

    public void Authorization()
    {
        authorization = true;
    }

    public void ChangeVRMode()
    {

        Debug.Log("chamou a funcao");
        //Debug.Log(GvrIntent.IsLaunchedFromVr());

        if(authorization)
        {

            Debug.Log("authorization true");

            if(immersiveVR)
            {
                Debug.Log("immersive vr true");

                StartCoroutine(SwitchToNotImmersive());
                immersiveVR = false;
                spinForce = -spinForce;

            }
            else
            {

                Debug.Log("immersive vr false");

                StartCoroutine(SwitchToImmersive());
                immersiveVR = true;
                spinForce = -spinForce;

            }
        }

    }

    // Call via `StartCoroutine(SwitchToVR())` from your code. Or, use
    // `yield SwitchToVR()` if calling from inside another coroutine.
    IEnumerator SwitchToImmersive()
    {
        // Device names are lowercase, as returned by `XRSettings.supportedDevices`.
        string desiredDevice = "cardboard"; // Or "cardboard".

        // Some VR Devices do not support reloading when already active, see
        // https://docs.unity3d.com/ScriptReference/XR.XRSettings.LoadDeviceByName.html
        if(String.Compare(XRSettings.loadedDeviceName, desiredDevice, StringComparison.OrdinalIgnoreCase) != 0)
        {
            XRSettings.LoadDeviceByName(desiredDevice);

            // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
            yield return null;
        }

        // Now it's ok to enable VR mode.
        XRSettings.enabled = true;
    }


    // Call via `StartCoroutine(SwitchTo2D())` from your code. Or, use
    // `yield SwitchTo2D()` if calling from inside another coroutine.
    IEnumerator SwitchToNotImmersive()
    {
        // Empty string loads the "None" device.
        XRSettings.LoadDeviceByName("");

        // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
        yield return null;

        // Not needed, since loading the None (`""`) device takes care of this.
        XRSettings.enabled = false;

        // Restore 2D camera settings.
        ResetCameras();
    }

    // Resets camera transform and settings on all enabled eye cameras.
    private static void ResetCameras()
    {
        // Camera looping logic copied from GvrEditorEmulator.cs
        foreach (Camera cam in Camera.allCameras)
        {
            if(cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
            {

                // Reset local position.
                // Only required if you change the camera's local position while in 2D mode.
                cam.transform.localPosition = Vector3.zero;

                // Reset local rotation.
                // Only required if you change the camera's local rotation while in 2D mode.
                cam.transform.localRotation = Quaternion.identity;

                // No longer needed, see issue github.com/googlevr/gvr-unity-sdk/issues/628.
                // cam.ResetAspect();

                // No need to reset `fieldOfView`, since it's reset automatically.
            }
        }
    }



}
