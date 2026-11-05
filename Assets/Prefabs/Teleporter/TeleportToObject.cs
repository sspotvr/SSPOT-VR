using System;
using System.Threading.Tasks;
using Photon.Pun;
using SSPot;
using UnityEngine;

public class TeleportToObject : MonoBehaviourPun
{
    // Locations
    public MeshRenderer teleportLocationMesh;               // Teleport location mesh
    private MeshCollider teleportLocationMeshCollider;

    public bool isForPlayer1;

    public static event Action PlayerTeleported;

    // I will change this to be more elegant later
    public bool isElevator;
    public bool opensDoor;
    [SerializeField] private Door door;

    private bool firstTime = true;

	[SerializeField] private string[] clips;

    [SerializeField] private bool playAudioOnTeleport = true;
    private AudioSource audioSource;

	private void Awake()
    {
        if(isForPlayer1 != PhotonNetwork.IsMasterClient)
            Destroy(gameObject);
        
        if (playAudioOnTeleport) audioSource = GetComponent<AudioSource>();

        // Fetched here, not in Start: OnEnable (which already uses it) runs before Start when a
        // teleporter starts inactive and is enabled later in the level.
        teleportLocationMeshCollider = transform.GetComponent<MeshCollider>();
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
        // The player's root is at eye level (camera at local 0), so the teleporter marks where the eyes go.
        PlayerSetup.Local.transform.position = transform.position;
        Debug.Log("Teleportado!");
        PlayerTeleported?.Invoke();
        
        if (ElevatorSync.instance != null)
        {
            Debug.Log("Atualizando elevador");
            if (isElevator)
            {
                ElevatorSync.instance.AddPlayerOnElevator();
            }
            else
            {
                ElevatorSync.instance.RemovePlayerOnElevator();
            }
        }
        else
        {
            Debug.Log("Não encontrei o elevator sync");
        }

        Debug.Log("Tocando Audio!");
        if (playAudioOnTeleport) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
        if(firstTime && clips.Length != 0) await Voice.instance.Speak(clips);

        firstTime = false;
        if(opensDoor) door.GetComponent<Door>().Operate();
    }
    
    private void Rotate()
    {
        if (PlayerSetup.Local == null) return;
        transform.LookAt(PlayerSetup.Local.transform.position);
        transform.rotation *= Quaternion.Euler(90f, 0f, 0f); // rotaciona o prefab pra ficar perpendicular/de cara com o player
    }

    private void CheckVisibility()
    {
        if (PlayerSetup.Local == null) return;
        float distance = Vector3.Distance(transform.position, PlayerSetup.Local.transform.position);

        // Only the teleporter the player is standing on (the player is placed exactly on it)
        bool isHere = distance < 0.5f;
        if (teleportLocationMeshCollider) teleportLocationMeshCollider.enabled = !isHere;
        teleportLocationMesh.enabled = !isHere;  // esconde a seta caso o player esteja perto

    }
}
