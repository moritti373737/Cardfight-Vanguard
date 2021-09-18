using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Photonによるデータの送受信を行う
/// </summary>
public class PhotonController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameMaster gameMaster;

    private IFighter fighter1;
    private IFighter fighter2;


    private void Awake()
    {
        //SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        PhotonNetwork.IsMessageQueueRunning = true;
        // PhotonServerSettingsの設定内容を使ってマスターサーバへ接続する
        //PhotonNetwork.ConnectUsingSettings();
        //GameStart();
    }


    /// <summary>
    /// ゲームを開始する
    /// roomに2人入室して人数が揃った時に呼ぶ
    /// </summary>
    public void GameStart()
    {
        gameMaster.SetMyNumber(PhotonNetwork.LocalPlayer.ActorNumber - 1);
        fighter1 = GameObject.Find("Fighter1").GetComponents<IFighter>().First(fighter => fighter.enabled);
        fighter2 = GameObject.Find("Fighter2").GetComponents<IFighter>().First(fighter => fighter.enabled);
        Debug.Log("2人揃いました。");
        Debug.Log($"あなたのID = {PhotonNetwork.LocalPlayer.ActorNumber}");
        Debug.Log($"相手のID = {PhotonNetwork.PlayerListOthers.First().ActorNumber}");
        fighter1.ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        fighter2.ActorNumber = PhotonNetwork.PlayerListOthers.First().ActorNumber - 1;
        if (fighter1.ActorNumber != 0 && fighter1.ActorNumber != 1) Debug.LogError($"Fighter1のActorNumber = {fighter1.ActorNumber}");
        if (fighter2.ActorNumber != 0 && fighter2.ActorNumber != 1) Debug.LogError($"Fighter2のActorNumber = {fighter2.ActorNumber}");
    }

    /// <summary>
    /// メインデータ（CardManagerで実行する関数の内容）を送信する
    /// </summary>
    /// <param name="funcname">実行したい関数名</param>
    /// <param name="card">引数に使用するカード</param>
    public void SendData(string funcname, Card card)
    {
        object[] args = new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber - 1,
            funcname,
            card.ID
        };
        photonView.RPC("ReceivedData", RpcTarget.All, args);
    }

    /// <summary>
    /// メインデータ（CardManagerで実行する関数の内容）を受信したときに呼び出される
    /// </summary>
    /// <param name="args">受信した引数</param>
    [PunRPC]
    public void ReceivedData(object[] args)
    {
        if (fighter1.ActorNumber == (int)args[0]) _ = fighter1.ReceivedData(args.ToList());
        else if (fighter2.ActorNumber == (int)args[0]) _ = fighter2.ReceivedData(args.ToList());
    }

    /// <summary>
    /// さまざまな処理（Attackなど）を送信する
    /// </summary>
    /// <param name="type">処理の種類</param>
    /// <param name="options">引数など</param>
    public void SendGeneralData(string type, object[] options)
    {
        object[] args = new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber - 1,
            type,
            options
        };
        photonView.RPC("ReceivedGeneralData", RpcTarget.All, args);
    }

    /// <summary>
    /// さまざまな処理（Attackなど）を受信したときに呼び出される
    /// </summary>
    /// <param name="args">受信した引数</param>
    [PunRPC]
    public void ReceivedGeneralData(object[] args)
    {
        if ((string)args[1] == "Cancel")
        {
            gameMaster.tokenSource.Cancel();
            return;
        }

        if (fighter1.ActorNumber == (int)args[0]) _ = fighter1.ReceivedGeneralData(args.ToList());
        else if (fighter2.ActorNumber == (int)args[0]) _ = fighter2.ReceivedGeneralData(args.ToList());
    }
    /// <summary>
    /// 処理を次に進めるためのデータを送信する
    /// </summary>
    /// <param name="actorNumber">送信元のプレイヤーID</param>
    public void SendProcessNext(int actorNumber)
    {
        photonView.RPC("ReceivedProcessNext", RpcTarget.All, (byte)actorNumber);
    }

    /// <summary>
    /// 処理を次に進めるためのデータを受信したときに呼び出される
    /// </summary>
    /// <param name="actorNumber">送信元のプレイヤーID</param>
    [PunRPC]
    public void ReceivedProcessNext(byte actorNumber)
    {
        NextController.Instance.SetProcessNext(actorNumber, true);
    }

    /// <summary>
    /// 処理を次に進めるためのデータを送信する
    /// </summary>
    /// <param name="actorNumber">送信元のプレイヤーID</param>
    public void SendSyncNext(int actorNumber)
    {
        if (gameMaster.local) return;
        photonView.RPC("ReceivedSyncNext", RpcTarget.All, (byte)actorNumber);
    }

    /// <summary>
    /// 処理を次に進めるためのデータを受信したときに呼び出される
    /// </summary>
    /// <param name="actorNumber">送信元のプレイヤーID</param>
    [PunRPC]
    public void ReceivedSyncNext(byte actorNumber)
    {
        NextController.Instance.SetSyncNext(actorNumber, true);
    }

    /// <summary>
    /// 状態データを送信する
    /// </summary>
    /// <param name="state">状態</param>
    public void SendState(string state)
    {
        photonView.RPC("ReceivedState", RpcTarget.All, state);
    }

    /// <summary>
    /// 状態データを受信したときに呼び出される
    /// </summary>
    /// <param name="state">状態</param>
    [PunRPC]
    public void ReceivedState(string state)
    {
        fighter1.ReceivedState(state);
        fighter2.ReceivedState(state);
    }
}