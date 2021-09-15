using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonController : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
        // roomに入室する
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2 });
    }

    //public override void OnJoinRandomFailed(short returnCode, string message)
    //{
    //    // まだroomがない場合は作成する
    //    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    //}

    public override void OnJoinedRoom()
    {
        Debug.Log($"入室成功、マスター→{PhotonNetwork.IsMasterClient}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"誰かが入室しました、合計人数は{PhotonNetwork.CurrentRoom.PlayerCount}人です。");
        Debug.Log(newPlayer.ActorNumber);
    }

}
