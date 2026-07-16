using UnityEngine;
using Photon.Pun;
using NaughtyAttributes;

public class CloningCube : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [BoxGroup("CubeInfo")]
    public CubeClass Cube;

    /// <summary>
    /// When player clicks on this object, it clones the selected cube to player's hand
    /// </summary>
    public void OnPointerClick()
    {
        // Destroy cube on hand, if there is any
        PlayerSetup.Local.DestroyCubeOnHand();

        // Debug.Log("Clicou no cubo");

        if(!PlayerSetup.Local.isUp) return;

        // Attach the selected cube to the player's hand
        AttachCubeToHand();
    }

    private void AttachCubeToHand()
    {
        // Instantiate the selected object
        object[] instantiationData = { Cube.type, PlayerSetup.Local.ViewId };
        PhotonNetwork.Instantiate(gameObject.name, Vector3.zero, Quaternion.identity, data: instantiationData);
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView.InstantiationData;
        if (data == null || data.Length == 0) return;

        if (data.Length == 2 && data[0] is Cube.CubeType cube && data[1] is int playerId)
        {
            SetupCubeOnHand(cube, playerId);
        }
    }
    
    private void SetupCubeOnHand(Cube.CubeType cubeType, int playerId)
    {
        Cube = new(cubeType);
        
        var player = PlayerSetup.GetPlayer(playerId);
        
        // Attach to player's hand
        transform.SetParent(player.Hand.transform);

		transform.localPosition = new Vector3(0f, -0.5f, 0.75f);
		transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
		transform.rotation = player.transform.rotation;
	}
}
