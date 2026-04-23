using System;
using System.Threading.Tasks;
using Photon.Pun;
using SSPot;
using UnityEngine;

public class TeleportToObject : MonoBehaviourPun
{
    // Locations
    public MeshRenderer teleportLocationMesh;               // Teleport location mesh

    public bool isForPlayer1;

    public static event Action PlayerTeleported;

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

    private void Start()
    {
        PlayerTeleported += Rotate;
        Rotate();
    }

    /// <summary>
    /// When player clicks on this object, it teleports the player to current GameObject position.
    /// </summary>
    public async Task OnPointerClick()
    {
        firstTime = false;
        
        photonView.RPC(nameof(DisableTeleportMesh), RpcTarget.AllBuffered);
        PlayerSetup.Local.transform.position = transform.position;
        PlayerTeleported?.Invoke();
        
        if(isElevator) ElevatorSync.instance.AddPlayerOnElevator();
        if(opensDoor) door.GetComponent<Door>().Operate();
        if(firstTime && clips.Length != 0) await Voice.instance.Speak(clips);
        if (playAudioOnTeleport) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
        // if (playAudioOnTeleport) audioSource.Play(); // stops when gameObject.SetActive(false)
    }
    
    private void Rotate()
    {
        transform.LookAt(PlayerSetup.Local.transform.position);
        transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
    }
    
        
    private void OnDisable()
    {
        PlayerTeleported -= Rotate;
    }
    
    
    [PunRPC]    
    private void DisableTeleportMesh()
    {
        // Disable elevator teleport button mesh
        teleportLocationMesh.enabled = false;
        gameObject.SetActive(false);
    }
}
