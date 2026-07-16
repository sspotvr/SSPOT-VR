using UnityEngine;

namespace SSPot
{
    public class TutorialHandler : MonoBehaviour
    {
		public static TutorialHandler Instance { get; private set; }


		private int teleports = 0;
		[SerializeField] Door door;
		[SerializeField] GameObject teleporter;
		[SerializeField] GameObject teleporterP2;
		[SerializeField] GameObject panel;

		[SerializeField] GameObject[] otherTeleporters;
		
		private void Awake()
		{
			// If an instance already exists and it's not this one, destroy this object
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			// Set the instance and ensure it persists across scenes (optional)
			Instance = this;
		}

		public void Start()
		{
			PlayerSetup.Local.isUp = true;
		}

		public void Teleport()
		{
			teleports++;

			if(teleports == 3)
			{
				teleporter.SetActive(true);
				teleporterP2.SetActive(true);
				panel.SetActive(false);
				door.Operate();

				foreach (GameObject otherTeleporter in otherTeleporters)
				{
					otherTeleporter.SetActive(false);
				}
			}
		}
	}
}
