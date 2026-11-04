using SSpot.Level;
using UnityEngine;

namespace SSPot
{
    /// <summary>
    /// Plays a success fanfare and narrates the level-complete line when the level is completed.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LevelCompleteCelebration : MonoBehaviour
    {
        [SerializeField] private AudioClip fanfare;
        [SerializeField] private string[] clips = { "Level_complete.1" };

        private AudioSource _audioSource;

        private void Awake() => _audioSource = GetComponent<AudioSource>();

        private void OnEnable() => LevelManager.Instance.OnLevelCompleted.AddListener(Celebrate);

        private void OnDisable()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.OnLevelCompleted.RemoveListener(Celebrate);
        }

        private void Celebrate()
        {
            if (fanfare) _audioSource.PlayOneShot(fanfare);
            if (Voice.instance != null && clips.Length > 0) _ = Voice.instance.Speak(clips);
        }
    }
}
