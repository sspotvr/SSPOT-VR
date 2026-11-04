using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace SSPot
{
    public class CameraChanger : MonoBehaviour
    {

        private bool isFirstPerson = true;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform firstPerson;
        [SerializeField] private Transform thirdPerson;

        [SerializeField] private Transform smiley;

        private void Start()
        {
            cam.position = firstPerson.position;
            cam.rotation = firstPerson.rotation;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) // change camera position
            {
                isFirstPerson = !isFirstPerson;

                if (isFirstPerson)
                {
                    cam.position = firstPerson.position;
                    cam.rotation = firstPerson.rotation;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    smiley.position = cam.position;
                    smiley.rotation = cam.rotation * Quaternion.Euler(0f, 180f, 0f); // emoji rotation offset
                }
                else // set to third person
                {
                    cam.position = thirdPerson.position;
                    cam.rotation = thirdPerson.rotation;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    smiley.position = firstPerson.position;
                    smiley.rotation = firstPerson.rotation * Quaternion.Euler(0f, 180f, 0f); // emoji rotation offset
                }
            }
        }
    }
}
