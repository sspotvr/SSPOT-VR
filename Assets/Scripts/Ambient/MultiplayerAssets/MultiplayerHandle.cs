using Photon.Pun;
using SSPot;
using UnityEngine;

namespace SSPot
{
    public class MultiplayerHandle : MonoBehaviour
    {
        public bool isForPlayer1;

        private void Awake()
        {
            if (isForPlayer1 != PhotonNetwork.IsMasterClient)
                Destroy(gameObject);
        }
    }
}
