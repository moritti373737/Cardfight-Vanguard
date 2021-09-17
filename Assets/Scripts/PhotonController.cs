using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

/// <summary>
/// Photon�ɂ��f�[�^�̑���M���s��
/// </summary>
public class PhotonController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameMaster gameMaster;

    [SerializeField]
    private Fighter fighter1;

    [SerializeField]
    private Fighter fighter2;

    private void Start()
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

    /// <summary>
    /// �Q�[�����J�n����
    /// room��2�l�������Đl�������������ɌĂ�
    /// </summary>
    private void GameStart()
    {
        Debug.Log("2�l�����܂����B");
        Debug.Log($"���Ȃ���ID = {PhotonNetwork.LocalPlayer.ActorNumber}");
        Debug.Log($"�����ID = {PhotonNetwork.PlayerListOthers.First().ActorNumber}");
        fighter1.ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        fighter2.ActorNumber = PhotonNetwork.PlayerListOthers.First().ActorNumber - 1;
        if (fighter1.ActorNumber != 0 && fighter1.ActorNumber != 1) Debug.LogError($"Fighter1��ActorNumber = {fighter1.ActorNumber}");
        if (fighter2.ActorNumber != 0 && fighter2.ActorNumber != 1) Debug.LogError($"Fighter2��ActorNumber = {fighter2.ActorNumber}");
        _ = gameMaster.GameStart(PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    /// <summary>
    /// ���C���f�[�^�𑗐M����
    /// </summary>
    /// <param name="funcname">���s�������֐���</param>
    /// <param name="card">�����Ɏg�p����J�[�h</param>
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
    /// ���C���f�[�^����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="args">��M��������</param>
    [PunRPC]
    public void ReceivedData(object[] args)
    {
        if (fighter1.ActorNumber == (int)args[0]) _ = fighter1.ReceivedData(args.ToList());
        else if (fighter2.ActorNumber == (int)args[0]) _ = fighter2.ReceivedData(args.ToList());
    }

    /// <summary>
    /// ���������ɐi�߂邽�߂̃f�[�^�𑗐M����
    /// </summary>
    /// <param name="actorNumber">���M���̃v���C���[ID</param>
    public void SendProcessNext(int actorNumber)
    {
        photonView.RPC("ReceivedProcessNext", RpcTarget.All, (byte)actorNumber);
    }

    /// <summary>
    /// ���������ɐi�߂邽�߂̃f�[�^����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="actorNumber">���M���̃v���C���[ID</param>
    [PunRPC]
    public void ReceivedProcessNext(byte actorNumber)
    {
        NextController.Instance.SetProcessNext(actorNumber, true);
    }

    /// <summary>
    /// ���������ɐi�߂邽�߂̃f�[�^�𑗐M����
    /// </summary>
    /// <param name="actorNumber">���M���̃v���C���[ID</param>
    public void SendSyncNext(int actorNumber)
    {
        photonView.RPC("ReceivedSyncNext", RpcTarget.All, (byte)actorNumber);
    }

    /// <summary>
    /// ���������ɐi�߂邽�߂̃f�[�^����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="actorNumber">���M���̃v���C���[ID</param>
    [PunRPC]
    public void ReceivedSyncNext(byte actorNumber)
    {
        NextController.Instance.SetSyncNext(actorNumber, true);
    }

    /// <summary>
    /// ��ԃf�[�^�𑗐M����
    /// </summary>
    /// <param name="state">���</param>
    public void SendState(string state)
    {
        photonView.RPC("ReceivedState", RpcTarget.All, state);
    }

    /// <summary>
    /// ��ԃf�[�^����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="state">���</param>
    [PunRPC]
    public void ReceivedState(string state)
    {
        fighter1.ReceivedState(state);
        fighter2.ReceivedState(state);
    }
}