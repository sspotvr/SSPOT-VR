using System.Threading.Tasks;
using Photon.Pun;
using SSPot;
using UnityEngine;

public class TeleportToObject : MonoBehaviourPun
{
    // Locations
    public MeshRenderer teleportLocationMesh;               // Teleport location mesh

    public bool isForPlayer1;

    // I will change this to be more elegant later
    public bool isElevator;
    public bool opensDoor;
    [SerializeField] Door door;

    private bool firstTime = true;

	[SerializeField] private AudioObject[] clips;

    [SerializeField] private bool playAudioOnTeleport = true;
    private AudioSource audioSource;


	private void Awake()
    {
        if(isForPlayer1 != PhotonNetwork.IsMasterClient)
            Destroy(gameObject);
        
        if (playAudioOnTeleport) audioSource = GetComponent<AudioSource>();
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
            door.GetComponent<Door>().Operate();
        }
        
        // if (playAudioOnTeleport) audioSource.Play();
        if (playAudioOnTeleport) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
    }
    
    
    [PunRPC]    
    private void DisableTeleportMesh()
    {
        // Disable elevator teleport button mesh
        teleportLocationMesh.enabled = false;
        gameObject.SetActive(false);
    }
}
