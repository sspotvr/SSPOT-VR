using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    // Mouse sensitivity
    public float sensitivity = 150f;

    // Player body transform
    public Transform playerBody;

    // Photon View reference
    private PhotonView photonView;

    // Mouse position
	private float pitch;
    private float yaw;


	// Start is called before the first frame update
	private void Start()
	{
        // Get Photon View component
        photonView = GetComponentInParent<PhotonView>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

	private void Update()
	{
		// Is this Photon View is not mine, exit
		if (!photonView.IsMine) return;
		
		// Get new mouse position
        pitch += Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // mouseY / X-axis rotation
		
        yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime; // mouseX / Y-axis rotation

        playerBody.eulerAngles = new Vector3(0f, yaw, 0f);
        transform.localEulerAngles = new Vector3(-pitch, 0f, 0f);

		// forces the correct orientation for the emoji
		// Vector3 angles = smiley.localEulerAngles;
		// angles.z = 0f;
		// smiley.localEulerAngles = angles;
	}
}
