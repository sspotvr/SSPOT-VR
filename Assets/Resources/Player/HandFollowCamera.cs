using UnityEngine;

namespace SSPot
{
    /// <summary>
    /// Pivot at the player's eyes that holds the hand: it turns with the camera (pitch included), so a held
    /// cube stays in front of where the player is looking instead of at a fixed height.
    /// While the camera is away from the eyes (third-person customization view), it keeps the body's
    /// facing instead, so the cube doesn't swing behind the avatar.
    /// </summary>
    public class HandFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float maxCameraDistance = .5f;

        private void LateUpdate()
        {
            if (!playerCamera) return;

            bool firstPerson = (playerCamera.position - transform.position).sqrMagnitude
                               <= maxCameraDistance * maxCameraDistance;
            transform.rotation = firstPerson ? playerCamera.rotation : transform.parent.rotation;
        }
    }
}
