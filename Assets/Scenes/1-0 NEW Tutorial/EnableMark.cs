using SSpot.Level;
using UnityEngine;

namespace SSPot
{
    public class EnableMark : MonoBehaviour
    {
        void OnPointerClick()
		{
			LevelManager.Instance.resetStage();
			LevelManager.Instance.activateRobotMovement();
		}
	}
}
