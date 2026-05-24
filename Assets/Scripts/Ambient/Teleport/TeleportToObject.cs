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
        Rotate();
        CheckVisibility();
    }

    private void OnEnable()
    {
        PlayerTeleported += Rotate;
        PlayerTeleported += CheckVisibility;
        
        Rotate();
        CheckVisibility();
    }

    private void OnDisable()
    {
        PlayerTeleported -= Rotate;
        PlayerTeleported -= CheckVisibility;
    }

    /// <summary>
    /// When player clicks on this object, it teleports the player to current GameObject position.
    /// </summary>
    public async Task OnPointerClick()
    {
        // photonView.RPC(nameof(DisableTeleportMesh), RpcTarget.AllBuffered);
        PlayerSetup.Local.transform.position = transform.position;
        print("Teleported!!");
        PlayerTeleported?.Invoke();
        print("Invocou!");
        
        if(isElevator){ 
            ElevatorSync.instance.AddPlayerOnElevator();
        }
        else
        {
            ElevatorSync.instance.RemovePlayerOnElevator();
        }

        if (playAudioOnTeleport) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);


        if(firstTime && clips.Length != 0) await Voice.instance.Speak(clips);
        firstTime = false;
        if(opensDoor) door.GetComponent<Door>().Operate();
        // if (playAudioOnTeleport) audioSource.Play(); // stops when gameObject.SetActive(false)
    }
    
    private void Rotate()
    {
        if (PlayerSetup.Local == null) return;
        transform.LookAt(PlayerSetup.Local.transform.position);
        transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
    }

    private void CheckVisibility()
    {
        if (PlayerSetup.Local == null) return;
        float distance = Vector3.Distance(transform.position, PlayerSetup.Local.transform.position);
        
        if (distance < 0.1f)
        {
            teleportLocationMesh.enabled = false; // Player está aqui, esconde a seta
        }
        else
        {
            teleportLocationMesh.enabled = true; // Player saiu, mostra a seta
        }
    }
}
