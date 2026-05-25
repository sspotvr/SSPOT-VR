using UnityEngine;
using Photon.Pun;
using SSPot;


public class GoingUpAndDownController : MonoBehaviourPun
{
    // GameObjects
    public GameObject instructionsInitial;          // Initial instructions blackboard GameObject
    public GameObject instructionsProgramming;      // Programming instructions blackboard GameObject  
    public GameObject plataformTeleport;
    public GameObject plataformTeleport2;

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
		if (PlayerSetup.Local != null)
        {
            var verticalMovement = PlayerSetup.Local.GetComponent<VerticalMovementPlayer>();
            if (verticalMovement != null)
            {
                verticalMovement.movement = Movement.Up;
            }
            else
            {
                Debug.LogWarning("PlayerSetup.Local não possui o componente VerticalMovementPlayer!");
            }
        }
        else
        {
            Debug.LogWarning("Tentou acionar GoUpRpc, mas PlayerSetup.Local está nulo!");
        }

        if (goingUpScript != null) goingUpScript.enabled = true;

        // Setup GameObjects
        if (instructionsInitial != null) instructionsInitial.SetActive(false);
        if (instructionsProgramming != null) instructionsProgramming.SetActive(true);

        if (plataformTeleport != null) plataformTeleport.SetActive(false);
        if (plataformTeleport2 != null) plataformTeleport2.SetActive(false);
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
        if (PlayerSetup.Local != null)
        {
            var verticalMovement = PlayerSetup.Local.GetComponent<VerticalMovementPlayer>();
            if (verticalMovement != null)
            {
                verticalMovement.movement = Movement.Down;
            }
            PlayerSetup.Local.isUp = false;
        }

        if (goingUpScript != null) goingUpScript.enabled = false;
        if (goingDownScript != null) goingDownScript.enabled = true;

        // Setup GameObjects
        if (instructionsProgramming != null) instructionsProgramming.SetActive(false);
        if (instructionsInitial != null) instructionsInitial.SetActive(true);
    }
}
