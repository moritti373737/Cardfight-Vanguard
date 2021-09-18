using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Photon�ɂ��f�[�^�̑���M���s��
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
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�֐ڑ�����
        //PhotonNetwork.ConnectUsingSettings();
        //GameStart();
    }


    /// <summary>
    /// �Q�[�����J�n����
    /// room��2�l�������Đl�������������ɌĂ�
    /// </summary>
    public void GameStart()
    {
        gameMaster.SetMyNumber(PhotonNetwork.LocalPlayer.ActorNumber - 1);
        fighter1 = GameObject.Find("Fighter1").GetComponents<IFighter>().First(fighter => fighter.enabled);
        fighter2 = GameObject.Find("Fighter2").GetComponents<IFighter>().First(fighter => fighter.enabled);
        Debug.Log("2�l�����܂����B");
        Debug.Log($"���Ȃ���ID = {PhotonNetwork.LocalPlayer.ActorNumber}");
        Debug.Log($"�����ID = {PhotonNetwork.PlayerListOthers.First().ActorNumber}");
        fighter1.ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        fighter2.ActorNumber = PhotonNetwork.PlayerListOthers.First().ActorNumber - 1;
        if (fighter1.ActorNumber != 0 && fighter1.ActorNumber != 1) Debug.LogError($"Fighter1��ActorNumber = {fighter1.ActorNumber}");
        if (fighter2.ActorNumber != 0 && fighter2.ActorNumber != 1) Debug.LogError($"Fighter2��ActorNumber = {fighter2.ActorNumber}");
    }

    /// <summary>
    /// ���C���f�[�^�iCardManager�Ŏ��s����֐��̓��e�j�𑗐M����
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
    /// ���C���f�[�^�iCardManager�Ŏ��s����֐��̓��e�j����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="args">��M��������</param>
    [PunRPC]
    public void ReceivedData(object[] args)
    {
        if (fighter1.ActorNumber == (int)args[0]) _ = fighter1.ReceivedData(args.ToList());
        else if (fighter2.ActorNumber == (int)args[0]) _ = fighter2.ReceivedData(args.ToList());
    }

    /// <summary>
    /// ���܂��܂ȏ����iAttack�Ȃǁj�𑗐M����
    /// </summary>
    /// <param name="type">�����̎��</param>
    /// <param name="options">�����Ȃ�</param>
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
    /// ���܂��܂ȏ����iAttack�Ȃǁj����M�����Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="args">��M��������</param>
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
        if (gameMaster.local) return;
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