using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PhotonController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameMaster gameMaster;

    [SerializeField]
    private Fighter fighter1;
    [SerializeField]
    private Fighter fighter2;

    void Start()
    {
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�[�֐ڑ�����
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
        // room�ɓ�������
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2, PublishUserId = true });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"���������A�}�X�^�[��{PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.PlayerList.Count() == 2) GameStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"�N�����������܂����A���v�l����{PhotonNetwork.CurrentRoom.PlayerCount}�l�ł��B");
        Debug.Log($"ID = { PhotonNetwork.LocalPlayer.ActorNumber}");
        PhotonNetwork.PlayerListOthers.ToList().ForEach(player => Debug.Log(player.ActorNumber));
        if (PhotonNetwork.PlayerList.Count() == 2) GameStart();
    }

    private void GameStart()
    {
        Debug.Log("2�l�����܂����B");
        Debug.Log($"���Ȃ���ID = {PhotonNetwork.LocalPlayer.ActorNumber}");
        Debug.Log($"�����ID = {PhotonNetwork.PlayerListOthers.First().ActorNumber}");
        fighter1.ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        fighter2.ActorNumber = PhotonNetwork.PlayerListOthers.First().ActorNumber;
        if (fighter1.ActorNumber != 1 && fighter1.ActorNumber != 2) Debug.LogError($"Fighter1��ActorNumber = {fighter1.ActorNumber}");
        if (fighter2.ActorNumber != 1 && fighter2.ActorNumber != 2) Debug.LogError($"Fighter2��ActorNumber = {fighter2.ActorNumber}");
        _ = gameMaster.GameStart();
    }

    public void SendData(string funcname, Card card)
    {
        object[] args = new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber,
            funcname,
            card.ID
        };
        photonView.RPC("ReceivedData", RpcTarget.All, args);
    }

    [PunRPC]
    public void ReceivedData(object[] args)
    {
        if (fighter1.ActorNumber == (int)args[0]) _ = fighter1.ReceivedData(args.Skip(1).ToList());
        else if (fighter2.ActorNumber == (int)args[0]) _ = fighter2.ReceivedData(args.Skip(1).ToList());
    }

    public void SendNext(int actorNumber)
    {
        photonView.RPC("ReceivedNext", RpcTarget.All, actorNumber);
    }

    [PunRPC]
    public void ReceivedNext(int actorNumber)
    {
        NextController.Instance.SetNext(actorNumber, true);
    }

    public void SendState(string state)
    {
        photonView.RPC("ReceivedState", RpcTarget.All, state);
    }

    [PunRPC]
    public void ReceivedState(string state)
    {
        fighter1.ReceivedState(state);
        fighter2.ReceivedState(state);
    }
}
