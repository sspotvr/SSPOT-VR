using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class ElevatorSync : MonoBehaviourPun
{
    #region Singleton
    public static ElevatorSync instance;
    public GoingUpAndDownController controller;

    void Awake()
    {
        if(!instance)
        {
            instance = this;
        }
    }
    #endregion


    public int playersOnElevator = 0;


    public void AddPlayerOnElevator()
    {
        photonView.RPC("AddPlayerOnElevatorRpc", RpcTarget.AllBuffered);
    }

    public void RemovePlayerOnElevator()
    {
        photonView.RPC("RemovePlayerOnElevatorRpc", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void AddPlayerOnElevatorRpc()
    {
        Debug.Log("+1 player");
        // Increase number of players on elevator
        playersOnElevator++;

        // If this number is equal to number of players, enable elevator button
        if(playersOnElevator == PhotonNetwork.PlayerList.Length)
        {
            controller.GoUp();
        }
    }

    [PunRPC]
    private void RemovePlayerOnElevatorRpc()
    {
        // Decrease number of players on elevator
        if(playersOnElevator > 0) playersOnElevator--;
        controller.GoDown();
    }
}
