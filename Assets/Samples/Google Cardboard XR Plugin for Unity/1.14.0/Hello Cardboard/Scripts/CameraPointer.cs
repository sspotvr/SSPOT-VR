//-----------------------------------------------------------------------
// <copyright file="CameraPointer.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

// using Photon.Pun;
using UnityEngine;

/// <summary>
/// Sends messages to gazed GameObject.
/// </summary>
public class CameraPointer : MonoBehaviour {
    
    private GameObject gazedAtObject;
    private const float MaxDistance = 100;
    [SerializeField] public Camera uiCamera;
    [SerializeField] private CrosshairController crosshairController;

    private AudioSource audioSource;
    [SerializeField] private AudioClip interactable;
    [SerializeField] private AudioClip successfulInteraction;

    // Root of the player that owns this camera. Its own colliders (body, the cube in its hand)
    // are skipped by the gaze raycast, so they never block what the player is looking at.
    private Transform playerRoot;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
        var player = GetComponentInParent<PlayerSetup>();
        playerRoot = player ? player.transform : transform.root;
    }

    /// <summary>
    /// Closest hit along the gaze that does not belong to the local player itself.
    /// </summary>
    private bool GazeRaycast(out RaycastHit closest)
    {
        closest = default;
        int count = Physics.RaycastNonAlloc(transform.position, transform.forward, hits, MaxDistance);
        float best = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].transform.IsChildOf(playerRoot)) continue;
            if (hits[i].distance >= best) continue;
            best = hits[i].distance;
            closest = hits[i];
        }
        return best < float.MaxValue;
    }

    public void Update() {
        if (!gazedAtObject)
            gazedAtObject = null;

        // raycasts to get the object the camera is looking at
        if (GazeRaycast(out RaycastHit hit))
        {
            // checks if the player looked at a different object than what's cached
            if (gazedAtObject != hit.transform.gameObject)
            {
                if (gazedAtObject)
                    gazedAtObject.SendMessage("OnPointerExit", SendMessageOptions.DontRequireReceiver);
                
                // caches the new object
                gazedAtObject = hit.transform.gameObject;
                gazedAtObject.SendMessage("OnPointerEnter", SendMessageOptions.DontRequireReceiver);

                // If gazed object is clickable, scale up crosshair
                if (gazedAtObject.CompareTag("Clickable"))
                {
                    crosshairController.SetCrosshairScale(new Vector3(1.75f, 1.75f, 1.75f));
                    audioSource.PlayOneShot(interactable);
                }
                else
                {
                    crosshairController.SetCrosshairScale(new Vector3(1f, 1f, 1f));
                }
            }
        }
        
        else // if no object reference
        {
            if (gazedAtObject)
                gazedAtObject.SendMessage("OnPointerExit", SendMessageOptions.DontRequireReceiver);

            gazedAtObject = null;
            crosshairController.SetCrosshairScale(new Vector3(1f, 1f, 1f));
        }

        if (Google.XR.Cardboard.Api.IsTriggerPressed || /*Input.GetTouch(0).phase == TouchPhase.Began ||*/
            Input.GetButtonDown("Fire1"))
        {
            // remove the cube from the player's hands if the player interacts with a non-clickable
            // or with a non-interactive
            if(!gazedAtObject || (!gazedAtObject.CompareTag("Clickable") && !gazedAtObject.CompareTag("NoPointerAction")) )
            {
                // invalid interaction and cube destruction SFXs inside the DestroyCubeOnHand function
                PlayerSetup.Local.DestroyCubeOnHand();
            }
            else // otherwise, if the player clicks on something interactable, call OnPointerClick method
            {
                if (gazedAtObject)
                    gazedAtObject.SendMessage("OnPointerClick", SendMessageOptions.DontRequireReceiver);

                audioSource.PlayOneShot(successfulInteraction);
            }
        }
    }
}
