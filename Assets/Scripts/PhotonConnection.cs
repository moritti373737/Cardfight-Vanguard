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
        // PhotonServerSettingsの設定内容を使ってマスターサーバへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// このクライアントがマスターサーバに接続されたとき
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // roomを作成して入室する
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2, PublishUserId = true });
    }

    /// <summary>
    /// このクライアントがroomに入室したとき
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log($"入室成功、マスター→{PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.PlayerList.Count() == 2) GameStart();
    }

    /// <summary>
    /// 他のクライアントがroomに入室したとき
    /// </summary>
    /// <param name="newPlayer">roomに入室したクライアント</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"誰かが入室しました、合計人数は{PhotonNetwork.CurrentRoom.PlayerCount}人です。");
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
