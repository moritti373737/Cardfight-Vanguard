using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;



public class PhotonConnection : MonoBehaviourPunCallbacks
{
    public void StartConnection()
    {
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�֐ڑ�����
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// ���̃N���C�A���g���}�X�^�[�T�[�o�ɐڑ����ꂽ�Ƃ�
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // room���쐬���ē�������
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2, PublishUserId = true });
    }

    /// <summary>
    /// ���̃N���C�A���g��room�ɓ��������Ƃ�
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log($"���������A�}�X�^�[��{PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.PlayerList.Count() == 2) GameStart();
    }

    /// <summary>
    /// ���̃N���C�A���g��room�ɓ��������Ƃ�
    /// </summary>
    /// <param name="newPlayer">room�ɓ��������N���C�A���g</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"�N�����������܂����A���v�l����{PhotonNetwork.CurrentRoom.PlayerCount}�l�ł��B");
        Debug.Log($"ID = { PhotonNetwork.LocalPlayer.ActorNumber}");
        PhotonNetwork.PlayerListOthers.ToList().ForEach(player => Debug.Log(player.ActorNumber));
        if (PhotonNetwork.PlayerList.Count() == 2) GameStart();
    }

    private void GameStart()
    {
        PhotonNetwork.IsMessageQueueRunning = false;

        SceneManager.LoadScene("MainScene");
    }
}
