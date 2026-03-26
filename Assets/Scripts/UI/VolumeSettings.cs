using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace SSPot
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer myMixer;
        [SerializeField] private Slider volumeSlider;

        public void SetVolume()
        {
            float volume = volumeSlider.value;
            if(volume > 0) myMixer.SetFloat("TotalVol", Mathf.Log10(volume) * 20);
            else myMixer.SetFloat("TotalVol", -80);
        }
    }
}
