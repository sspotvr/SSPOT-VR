using System.Collections.Generic;
using Photon.Pun;
using SSPot.Utilities;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    #region Global
    
    private static readonly List<PlayerSetup> _players = new();
    public static IReadOnlyList<PlayerSetup> Players => _players;
    
    public static PlayerSetup Local { get; private set; }
    
    public static PlayerSetup GetPlayer(int id) => _players.Find(p => p.photonView.ViewID == id);
    
    #endregion

    public int ViewId => photonView.ViewID;
    
    public int PlayerIndex => Players.IndexOf(this);
    
    public GameObject Hand => playerHand;

    public bool IsLocal => photonView.IsMine;
    public bool isUp;

    // Player hand
    [SerializeField] private GameObject playerHand;

    // Cameras (UI and player's)
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject uiCamera;

    private AudioSource audioSource;
    [SerializeField] private AudioClip notInteractable;
    [SerializeField] private AudioClip destroyCube;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        //Register global
        _players.Add(this);
        if (photonView.IsMine)
            Local = this;
        
        // If it is not local player disable Camera, CameraPointer, AudioListener, PlayerSetup and destroy UI Camera
        if (photonView.IsMine) return;
        
        playerCamera.GetComponent<Camera>().enabled = false;
        playerCamera.GetComponent<CameraPointer>().enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;
        Destroy(uiCamera);

        // Local-input components must not run on another player's avatar: they would read this device's
        // mouse/gyro/head pose/keyboard and move or toggle the remote avatar's camera and menus.
        DestroyIfPresent<MouseController>();
        DestroyIfPresent<GyroController>();
        DestroyIfPresent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        DestroyIfPresent<SSPot.CameraChanger>();
        DestroyIfPresent<SSPot.CanvasToggler>();

        enabled = false;
    }
    
    private void DestroyIfPresent<T>() where T : Component
    {
        foreach (var component in GetComponentsInChildren<T>(true))
            Destroy(component);
    }

    private void OnDestroy()
    {
        _players.Remove(this);

        if (photonView.IsMine)
            Local = null;
    }

    /// <summary>
    /// Call RPC destroy cube on hand method.
    /// </summary>
    public void DestroyCubeOnHand()
    {
        if(playerHand.transform.childCount > 0) // if there is a cube on hand
        {
            // Call DestroyCubeOnHandRpc using this View ID
            photonView.RPC(nameof(DestroyCubeOnHandRpc), RpcTarget.AllBuffered);
            audioSource.PlayOneShot(destroyCube);
        }
        else // if empty-handed
        {
            audioSource.PlayOneShot(notInteractable);
        }
    }

    /// <summary>
    /// Destroy the cube this player is holding via RPC. It is important to use RPC
    /// because it needs to be synced.
    /// </summary>
    [PunRPC]
    private void DestroyCubeOnHandRpc()
    {
        // Destroy every cube on the player's hand. They are detached right away because Destroy only
        // happens at the end of the frame: a cube cloned in the same frame would otherwise leave two
        // cubes in the hand, and coding cells only accept a hand holding exactly one.
        for (int i = playerHand.transform.childCount - 1; i >= 0; i--)
        {
            var cube = playerHand.transform.GetChild(i).gameObject;
            cube.SetActive(false);
            cube.transform.SetParent(null, false);
            Destroy(cube);
        }
    }
}
