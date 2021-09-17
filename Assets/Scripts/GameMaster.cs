using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    [SerializeField]
    private PhotonController photonController;

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    [SerializeField]
    private PlayerInput input;

    //public Player[] playerList;
    public Fighter fighter1;
    public Fighter fighter2;


    [field: SerializeField]
    private Fighter AttackFighter { get; set; }
    [field: SerializeField]
    private Fighter DefenceFighter { get; set; }

    //[SerializeField] Animator animator;

    public void ResetScene()
    {
        // ���݂�Scene�����擾����
        Scene loadScene = SceneManager.GetActiveScene();
        // Scene�̓ǂݒ���
        SceneManager.LoadScene(loadScene.name);

        Debug.Log("RESET!");
    }


    //async void Start()
    //{
    //    fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter1} �̕���</color>"));
    //    fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter2} �̕���</color>"));

    //    await InitPhase();

    //    while (true)
    //    {
    //        await StandPhase();
    //    }
    //}

    public async UniTask GameStart()
    {
        fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter1} �̕���</color>"));
        fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter2} �̕���</color>"));

        await InitPhase();

        while (true)
        {
            await StandPhase();
        }
    }

    void Update()
    {
        if (input.GetDown("Zoom"))
        {
            SelectManager.Instance.ZoomCard();
        }
        else if (input.GetDown("Reset")) ResetScene();
    }

    async UniTask InitPhase()
    {
        TextManager.Instance.SetPhaseText("����");

        fighter1.CreateDeck();
        fighter2.CreateDeck();
        var CardDic = fighter1.Deck.cardList.Concat(fighter2.Deck.cardList).ToDictionary(card => card.ID);
        fighter1.CardDic = CardDic;
        fighter2.CardDic = CardDic;

        // �����ŁA�J�[�h���������̂�1�t���[���҂��āACard��Start()���\�b�h�����s������
        await UniTask.NextFrame();

        if (fighter1.ActorNumber == 1)
        {
            AttackFighter = fighter1;
            DefenceFighter = fighter2;
        }
        else if (fighter1.ActorNumber == 2)
        {
            AttackFighter = fighter2;
            DefenceFighter = fighter1;
        }


        await fighter1.SetFirstVanguard();
        await fighter2.SetFirstVanguard();
        fighter1.Deck.Shuffle();
        fighter2.Deck.Shuffle();

        await UniTask.WaitUntil(() => input.GetDown("Enter"));

        //await UniTask.WhenAll(fighter1.DrawCard(5), fighter2.DrawCard(5));
        await fighter1.DrawCard(5);

        await fighter1.Mulligan();
        photonController.SendNext(fighter1.ActorNumber);

        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllNext());

        //await UniTask.NextFrame();
        //await fighter2.Mulligan();

        //await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());

        await UniTask.NextFrame();
    }

    async UniTask StandPhase()
    {
        //TextManager.Instance.SetPhaseText("�X�^���h�t�F�C�Y");
        photonController.SendState("�X�^���h�t�F�C�Y");

        await AttackFighter.StandPhase();

        await DrawPhase();
    }


    async UniTask DrawPhase()
    {
        //TextManager.Instance.SetPhaseText("�h���[�t�F�C�Y");
        photonController.SendState("�h���[�t�F�C�Y");

        await UniTask.WaitUntil(() => input.GetDown("Enter"));

        await AttackFighter.DrawPhase();

        await UniTask.NextFrame();
        //await AttackFighter.GuardPhase();

        await RidePhase();
    }

    async UniTask RidePhase()
    {
        //TextManager.Instance.SetPhaseText("���C�h�t�F�C�Y");
        photonController.SendState("���C�h�t�F�C�Y");

        await AttackFighter.RidePhase();
        await MainPhase();
    }

    async UniTask MainPhase()
    {
        //TextManager.Instance.SetPhaseText("���C���t�F�C�Y");
        photonController.SendState("���C���t�F�C�Y");

        while (await AttackFighter.MainPhase())
        {
            await UniTask.NextFrame();
        }

        await ButtlePhase();
    }

    async UniTask ButtlePhase()
    {

        while (true)

        {
            await UniTask.NextFrame();
            //TextManager.Instance.SetPhaseText("�o�g���t�F�C�Y");
            photonController.SendState("�o�g���t�F�C�Y");

            (ICardCircle selectedAttackZone, ICardCircle selectedTargetZone) = await AttackFighter.AttackStep();
            if (selectedAttackZone == null) break;

            await DefenceFighter.GuardStep();

            if (selectedAttackZone.V)
            {
                TextManager.Instance.SetPhaseText("�h���C�u�g���K�[�`�F�b�N");
                int checkCount = selectedAttackZone.Card.Ability == Card.AbilityType.TwinDrive ? 2 : 1;
                await AttackFighter.DriveTriggerCheck(checkCount);
            }

            print(selectedAttackZone.Card.Power);
            print(selectedTargetZone.Card.Power);
            print(DefenceFighter.Guardian.Shield);

            if (selectedAttackZone.Card.Power >= selectedTargetZone.Card.Power + DefenceFighter.Guardian.Shield)
            {
                if (selectedTargetZone.V)
                {
                    TextManager.Instance.SetPhaseText("�_���[�W�g���K�[�`�F�b�N");
                    await DefenceFighter.DamageTriggerCheck(selectedAttackZone.Card.Critical);
                }
                else
                {
                    await DefenceFighter.RetireCard(selectedTargetZone);
                }
            }

            List<Card> guardian = new List<Card>(DefenceFighter.Guardian.cardList);
            //guardian.ForEach(async card => await CardManager.Instance.GuardianToDrop(DefenceFighter.Guardian, DefenceFighter.Drop, card));
            foreach (var card in guardian)
            {
                await CardManager.Instance.GuardianToDrop(DefenceFighter.Guardian, DefenceFighter.Drop, card);
            }
            await AttackFighter.EndStep();

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