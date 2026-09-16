using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace SSPot
{
    public class CameraHolder : MonoBehaviour
    {

        private bool isFirstPerson = true;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform firstPerson;
        [SerializeField] private Transform thirdPerson;

        private void Start()
        {
            cam.position = firstPerson.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) ChangeCameraPosition();
        }

        private void ChangeCameraPosition()
        {
            isFirstPerson = !isFirstPerson;
            cam.position = isFirstPerson ? firstPerson.position : thirdPerson.position;
        }
    }
}
