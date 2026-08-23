using Photon.Pun;
using UnityEngine;

namespace SSPot.Network
{
    /// <summary>
    /// Lets a level scene be entered directly in the Editor (e.g. via Play Mode Start Scene) without going
    /// through MainMenu/Lobby first. Spawner.Awake() requires an active Photon room to instantiate the player;
    /// without this, testing a level scene standalone throws before anything spawns. Runs before other scene
    /// scripts' Awake() via DefaultExecutionOrder so the room exists by the time Spawner needs it.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class OfflineTestBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (PhotonNetwork.InRoom) return;

            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }
    }
}
