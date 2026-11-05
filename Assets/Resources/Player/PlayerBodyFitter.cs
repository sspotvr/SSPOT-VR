using UnityEngine;

namespace SSPot
{
    /// <summary>
    /// The player's root sits at eye level (the camera is at local 0), so spawn points, teleporters and
    /// elevators mark where the eyes go. This scales the avatar's body so its feet reach the floor of
    /// the current level, whose eye height is configured on the scene's <see cref="Spawner"/>.
    /// Runs on every client, for local and remote avatars alike.
    /// </summary>
    public class PlayerBodyFitter : MonoBehaviour
    {
        [Tooltip("Everything below the eyes that should scale with the avatar (model, neck, 3rd person cam).")]
        [SerializeField] private Transform body;
        [Tooltip("Eye height of the avatar at body scale 1 (distance from the camera down to the feet).")]
        [SerializeField] private float nativeEyeHeight = 3.28f;
        [Tooltip("Hand that holds cubes. Its position follows the body scale; its scale doesn't.")]
        [SerializeField] private Transform hand;
        [Tooltip("Objects outside the body that must keep their size relative to it (e.g. the emoji head).")]
        [SerializeField] private Transform[] scaleWithBody;

        private Vector3 _handPosition;
        private Vector3[] _scales;
        private bool _initialized;

        public float EyeHeight { get; private set; }

        private void Awake()
        {
            if (hand) _handPosition = hand.localPosition;
            _scales = new Vector3[scaleWithBody.Length];
            for (int i = 0; i < scaleWithBody.Length; i++)
                if (scaleWithBody[i]) _scales[i] = scaleWithBody[i].localScale;
            _initialized = true;

            var spawner = FindFirstObjectByType<Spawner>();
            Fit(spawner ? spawner.playerEyeHeight : nativeEyeHeight);
        }

        public void Fit(float eyeHeight)
        {
            if (!_initialized || !body || eyeHeight <= 0f) return;

            EyeHeight = eyeHeight;
            float factor = eyeHeight / nativeEyeHeight;
            body.localPosition = new Vector3(0f, -eyeHeight, 0f);
            body.localScale = Vector3.one * factor;
            if (hand) hand.localPosition = _handPosition * factor;
            for (int i = 0; i < scaleWithBody.Length; i++)
                if (scaleWithBody[i]) scaleWithBody[i].localScale = _scales[i] * factor;
        }
    }
}
