using UnityEngine;

namespace SSPot
{
    public class SpeakAtStartTutorial : MonoBehaviour
    {
        [SerializeField] string[] clipsStart;
        [SerializeField] Door door;

        [SerializeField] string[] clipsCongrats1;
        [SerializeField] string[] clipsCongrats2;

		public void Start()
		{
			StartSpeaking();
		}

		public async void StartSpeaking()
        {
            await Voice.instance.Speak(clipsStart);
            door.GetComponent<Door>().Operate();
		}

        public void Congrats1()
        {
            Voice.instance.Speak(clipsCongrats1);
        }

        public void Congrats2()
        {
            Voice.instance.Speak(clipsCongrats2);
        }
    }
}
