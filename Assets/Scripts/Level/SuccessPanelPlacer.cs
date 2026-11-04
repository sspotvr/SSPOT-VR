using UnityEngine;

namespace SSpot.Level
{
    /// <summary>
    /// When the level-complete panel shows up, moves it next to Mark and moves the next-level arrow to the
    /// local player's right (taking as reference the player facing the computer), both turned to the player.
    /// Assumes the panel and arrow were authored readable by a player looking towards +Z.
    /// </summary>
    public class SuccessPanelPlacer : MonoBehaviour
    {
        [Tooltip("Board with the success message, placed above Mark. Defaults to this object.")]
        [SerializeField] private Transform panel;
        [SerializeField] private Vector3 panelOffsetFromMark = new(0f, 4.2f, 1f);
        [SerializeField] private float panelScaleMultiplier = .6f;

        [Tooltip("Next-level arrow, placed at the player's right.")]
        [SerializeField] private Transform nextLevelButton;
        [Tooltip("Offset from the player's eyes: x = to the right, y = up, z = towards the computer.")]
        [SerializeField] private Vector3 buttonOffsetFromPlayer = new(1.6f, -.4f, 1.6f);
        [SerializeField] private float buttonScaleMultiplier = .35f;

        private Quaternion _panelRotation, _buttonRotation;
        private Vector3 _panelScale, _buttonScale;
        private bool _initialized;

        private void Awake()
        {
            if (!panel) panel = transform;
            _panelRotation = panel.rotation;
            _panelScale = panel.localScale;
            if (nextLevelButton)
            {
                _buttonRotation = nextLevelButton.rotation;
                _buttonScale = nextLevelButton.localScale;
            }
            _initialized = true;
        }

        private void OnEnable()
        {
            if (!_initialized) return;

            var eyes = PlayerEyes();
            if (eyes == null) return;
            Vector3 eyePosition = eyes.position;

            var robot = LevelManager.Instance ? LevelManager.Instance.Robot : null;
            if (robot)
            {
                panel.position = robot.transform.position + panelOffsetFromMark;
                panel.rotation = FacingFrom(eyePosition, panel.position) * _panelRotation;
                panel.localScale = _panelScale * panelScaleMultiplier;
            }

            if (nextLevelButton)
            {
                Vector3 forward = Flat(ComputerPosition(eyePosition) - eyePosition);
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                nextLevelButton.position = eyePosition
                                           + right * buttonOffsetFromPlayer.x
                                           + Vector3.up * buttonOffsetFromPlayer.y
                                           + forward * buttonOffsetFromPlayer.z;
                nextLevelButton.rotation = FacingFrom(eyePosition, nextLevelButton.position) * _buttonRotation;
                nextLevelButton.localScale = _buttonScale * buttonScaleMultiplier;
            }
        }

        private static Transform PlayerEyes()
        {
            if (PlayerSetup.Local == null) return null;
            var cam = PlayerSetup.Local.GetComponentInChildren<Camera>();
            return cam ? cam.transform : PlayerSetup.Local.transform;
        }

        private static Vector3 ComputerPosition(Vector3 fallbackFrom)
        {
            var computer = LevelManager.Instance ? LevelManager.Instance.LastActiveComputer : null;
            if (!computer) computer = FindFirstObjectByType<CubeComputer>();
            return computer ? computer.transform.position : fallbackFrom + Vector3.forward;
        }

        /// <summary>Rotation that turns something authored for a +Z viewer into one facing the viewer.</summary>
        private static Quaternion FacingFrom(Vector3 viewer, Vector3 target) =>
            Quaternion.LookRotation(Flat(target - viewer), Vector3.up);

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }
    }
}
