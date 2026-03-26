using System.Threading.Tasks;
using Photon.Pun;
using SSPot;
using UnityEngine;

public class TeleportToObject : MonoBehaviourPun
{
    // Locations
    public MeshRenderer teleportLocationMesh;               // Teleport location mesh

    public bool isForPlayer1 = false;

    // I will change this to be more elegant later
    public bool isElevator = false;
    public bool opensDoor = false;
    [SerializeField] Door door;

    private bool firstTime = true;

	[SerializeField] AudioObject[] clips;


	private void Awake()
    {
        if(isForPlayer1 != PhotonNetwork.IsMasterClient)
            Destroy(gameObject);
    }

    /// <summary>
    /// When player clicks on this object, it teleports the player to current GameObject position.
    /// </summary>
    public async Task OnPointerClick()
    {
        photonView.RPC(nameof(DisableTeleportMesh), RpcTarget.AllBuffered);
        PlayerSetup.Local.transform.position = transform.position;

        if(isElevator) ElevatorSync.instance.AddPlayerOnElevator();

        if(firstTime)
        {
            await Voice.instance.Speak(clips);
        }

        firstTime = false;
        if(opensDoor) 
        {
            door.Operate();
        }
    }
    
    
    [PunRPC]    
    private void DisableTeleportMesh()
    {
        // Disable elevator teleport button mesh
        teleportLocationMesh.enabled = false;
        gameObject.SetActive(false);
    }
}
