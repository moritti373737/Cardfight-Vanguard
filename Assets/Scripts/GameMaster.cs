using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    //public Player[] playerList;
    public Fighter fighter1;
    public Fighter fighter2;
    public SelectManager selectManager;


    private Fighter AttackFighter;
    private Fighter DefenceFighter;

    //[SerializeField] Animator animator;

    public void ResetScene()
    {
        // ���݂�Scene�����擾����
        Scene loadScene = SceneManager.GetActiveScene();
        // Scene�̓ǂݒ���
        SceneManager.LoadScene(loadScene.name);

        Debug.Log("RESET!");
    }

    private enum Phase
    {
        INIT,
        STAND,
        DRAW,
        BATTLE,
        END,
    };

    //Phase phase;


    async void Start()
    {
        //phase = Phase.INIT;
        await InitPhase();
        await StandPhase();
        //yield return StartCoroutine(StandPhase());
        //TextManager.Instance.SetPhaseText("�G���h�t�F�C�Y");
    }

    void Update()
    {
        if (Input.GetButtonDown("Zoom"))
        {
            selectManager.ZoomCard();
        }


        //switch (phase)
        //{
        //    case Phase.INIT:
        //        //InitPhase();
        //        break;
        //    case Phase.STAND:
        //        DrawPhase();
        //        break;
        //    case Phase.DRAW:
        //        StandPhase();
        //        break;
        //    case Phase.BATTLE:
        //        BattlePhase();
        //        break;
        //    case Phase.END:
        //        EndPhase();
        //        break;
        //}
    }

    async UniTask InitPhase()
    {
        TextManager.Instance.SetPhaseText("����");

        fighter1.CreateDeck();
        fighter2.CreateDeck();

        AttackFighter = fighter1;
        DefenceFighter = fighter2;

        await fighter1.SetFirstVanguard();
        await fighter2.SetFirstVanguard();
        fighter1.deck.Shuffle();
        fighter2.deck.Shuffle();

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await fighter1.DrawCard(5);
        await fighter2.DrawCard(1);

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());

        await UniTask.NextFrame();
        //while (true)
        //{
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleSelected());
        //selectManager.SingleSelected();
        //yield return null;

        //yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleConfirm("Vanguard"));
        //selectManager.SingleConfirm();
        //yield return null;
        //}

        //phase = Phase.DRAW;


    }

    async UniTask StandPhase()
    {
        TextManager.Instance.SetPhaseText("�X�^���h�t�F�C�Y");

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));
        await UniTask.NextFrame();

        await DrawPhase();
    }


    async UniTask DrawPhase()
    {
        TextManager.Instance.SetPhaseText("�h���[�t�F�C�Y");

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await AttackFighter.DrawCard(1);

        await UniTask.NextFrame();

        await RidePhase();
    }

    async UniTask RidePhase()
    {
        TextManager.Instance.SetPhaseText("���C�h�t�F�C�Y");

        while (!await AttackFighter.RidePhase())
        {
            await UniTask.NextFrame();
        }
        await MainPhase();
    }

    async UniTask MainPhase()
    {
        TextManager.Instance.SetPhaseText("���C���t�F�C�Y");

        while (!await AttackFighter.MainPhase())
        {
            await UniTask.NextFrame();
        }

        await ButtlePhase();
    }

    async UniTask ButtlePhase()
    {
        TextManager.Instance.SetPhaseText("�o�g���t�F�C�Y");

        while (true)
        {
            await UniTask.NextFrame();

            int result = await AttackFighter.AttackPhase();
            if (result == -1)
                break;
            else if (result == 0)
                continue;

            TextManager.Instance.SetPhaseText("�h���C�u�g���K�[�`�F�b�N");

            await AttackFighter.DriveTriggerCheck();

            TextManager.Instance.SetPhaseText("�_���[�W�g���K�[�`�F�b�N");

            await DefenceFighter.DamageTriggerCheck();

        }
        print("�I���");
    }

    //void StandbyPhase()
    //{
    //    Debug.Log("StandbyPhase");
    //    // ��D�̃J�[�h����ɏo��
    //    //currentPlayer.StandbyPhaseAction();
    //    phase = Phase.BATTLE;
    //}
    /*
    void BattlePhase()
    {
        Debug.Log("BattlePhase");

        currentPlayer.ButtlePhaseAction(waitPlayer);
        phase = Phase.END;
    }

    void CheckFieldCardHP()
    {
        currentPlayer.CheckFieldCardHP();
        waitPlayer.CheckFieldCardHP();
    }
    void EndPhase()
    {
        Debug.Log("EndPhase");
        (currentPlayer, waitPlayer) = (waitPlayer, currentPlayer);
        //phase = Phase.DRAW;
    }*/
}