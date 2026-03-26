using UnityEngine;

namespace SSPot
{
    public class TeleportCount : MonoBehaviour
    {
        public void OnPointerClick()
        {
            TutorialHandler.Instance.Teleport();
		}
    }
}
