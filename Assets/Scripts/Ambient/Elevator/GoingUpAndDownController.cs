using UnityEngine;
using Photon.Pun;
using SSPot;


public class GoingUpAndDownController : MonoBehaviourPun
{
    // GameObjects
    public GameObject instructionsCodingPlatform;   // Elevator instructions blackboard GameObject
    public GameObject instructionsInitial;          // Initial instructions blackboard GameObject
    public GameObject instructionsProgramming;      // Programming instructions blackboard GameObject  
    public GameObject plataformTeleport;
    public GameObject plataformTeleport2;

	private bool firstTime = true;
	[SerializeField] AudioObject[] clips;

    private GoingDown goingDownScript;
    private GoingUp goingUpScript;


    private void Awake()
    {
        goingDownScript = GetComponent<GoingDown>();
        goingUpScript = GetComponent<GoingUp>();
    }

    private void OnEnable()
    {
        if (goingDownScript != null)
        {
            goingDownScript.OnReachedDestination += ActivateLocation1;
        }

        if(goingUpScript != null)
        {
            goingUpScript.OnReachedDestination += ActivateCoding;
        }
    }

    private void OnDisable()
    {
        if (goingDownScript != null)
        {
            goingDownScript.OnReachedDestination -= ActivateLocation1;
        }

        if(goingUpScript != null)
        {
            goingUpScript.OnReachedDestination -= ActivateCoding;
        }
    }

    private void ActivateLocation1()
    {
        if (plataformTeleport != null)
        {
            plataformTeleport.SetActive(true);
        }

        if (plataformTeleport2 != null)
        {
            plataformTeleport2.SetActive(true);
        }
    }

    private void ActivateCoding()
    {
        PlayerSetup.Local.isUp = true;
    }


    /// <summary>
    /// Go up the elevator.
    /// </summary>
    public void GoUp()
    {
		photonView.RPC(nameof(GoUpRpc), RpcTarget.AllBuffered);
	}

    [PunRPC]
    private void GoUpRpc()
    {
		if (firstTime)
		{
			Voice.instance.Speak(clips);
		}

		firstTime = false;

		// Enable GoingUp
        PlayerSetup.Local.GetComponent<VerticalMovementPlayer>().movement = Movement.Up;
        goingUpScript.enabled = true;

        // Setup GameObjects
        instructionsCodingPlatform.SetActive(false);
        instructionsInitial.SetActive(false);
        instructionsProgramming.SetActive(true);

        plataformTeleport.SetActive(false);
        plataformTeleport2.SetActive(false);
    }

    /// <summary>
    /// Go down the elevator.
    /// </summary>
    public void GoDown()
    {
        photonView.RPC(nameof(GoDownRpc), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void GoDownRpc()
    {
        // Enable GoingDown
        PlayerSetup.Local.GetComponent<VerticalMovementPlayer>().movement = Movement.Down;
        PlayerSetup.Local.isUp = false;
        goingUpScript.enabled = false;
        goingDownScript.enabled = true;

        // Setup GameObjects
        instructionsCodingPlatform.SetActive(true);
        instructionsProgramming.SetActive(false);
        instructionsInitial.SetActive(true);
    }
}
