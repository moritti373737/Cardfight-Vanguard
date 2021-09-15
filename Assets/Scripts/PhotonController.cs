using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonController : MonoBehaviourPunCallbacks
{
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
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2 });
    }

    //public override void OnJoinRandomFailed(short returnCode, string message)
    //{
    //    // �܂�room���Ȃ��ꍇ�͍쐬����
    //    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    //}

    public override void OnJoinedRoom()
    {
        Debug.Log($"���������A�}�X�^�[��{PhotonNetwork.IsMasterClient}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"�N�����������܂����A���v�l����{PhotonNetwork.CurrentRoom.PlayerCount}�l�ł��B");
        Debug.Log(newPlayer.ActorNumber);
    }

}
