using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSPot
{
    public class SpeakAtStart : MonoBehaviour
    {
		[SerializeField] string[] clips;
		public static SpeakAtStart instance;

		public void Awake()
		{
			if(instance != null)
			{
				Destroy(this);
			}

			instance = this;
		}

		public void Start()
		{
			StartSpeaking();
		}

		public void StartSpeaking()
        {
			Voice.instance.Speak(clips);
		}
	}
}
