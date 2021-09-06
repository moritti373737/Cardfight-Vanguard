using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    private int Turn { get; set; } = 1;

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

        while (true)
        {
            await StandPhase();
        }
        //TextManager.Instance.SetPhaseText("�G���h�t�F�C�Y");
    }

    void Update()
    {
        if (Input.GetButtonDown("Zoom"))
        {
            selectManager.ZoomCard();
        }
        else if (Input.GetButtonDown("Reset")) ResetScene();
    }

    async UniTask InitPhase()
    {
        TextManager.Instance.SetPhaseText("����");

        fighter1.CreateDeck();
        fighter2.CreateDeck();

        // �����ŁA�J�[�h���������̂�1�t���[���҂��āACard��Start()���\�b�h�����s������
        await UniTask.NextFrame();

        AttackFighter = fighter1;
        DefenceFighter = fighter2;


        await fighter1.SetFirstVanguard();
        await fighter2.SetFirstVanguard();
        fighter1.deck.Shuffle();
        fighter2.deck.Shuffle();

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await fighter1.DrawCard(5);
        await fighter2.DrawCard(1);

        await fighter1.Mulligan();

        //await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());

        await UniTask.NextFrame();
    }

    async UniTask StandPhase()
    {
        TextManager.Instance.SetPhaseText("�X�^���h�t�F�C�Y");

        await AttackFighter.StandPhase();

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

        await AttackFighter.RidePhase();
        await MainPhase();
    }

    async UniTask MainPhase()
    {
        TextManager.Instance.SetPhaseText("���C���t�F�C�Y");

        while (await AttackFighter.MainPhase())
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

            bool result = await AttackFighter.AttackPhase();
            if (!result)
                break;

            //TextManager.Instance.SetPhaseText("�h���C�u�g���K�[�`�F�b�N");

            //await AttackFighter.DriveTriggerCheck();

            //TextManager.Instance.SetPhaseText("�_���[�W�g���K�[�`�F�b�N");

            //await DefenceFighter.DamageTriggerCheck();

        }
        await EndPhase();
    }
    async UniTask EndPhase()
    {
        TextManager.Instance.SetPhaseText("�G���h�t�F�C�Y");

        Turn++;
        await AttackFighter.EndPhase();
        await DefenceFighter.EndPhase();

        (AttackFighter, DefenceFighter) = (DefenceFighter, AttackFighter);
        return;
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