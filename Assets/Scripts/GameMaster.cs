using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class GameMaster : SingletonMonoBehaviour<GameMaster>
{
    public FightMode fightMode;

    public bool local = true;

    public string phase;

    public int phaseCount = 1;

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    [SerializeField]
    private PhotonController photonController;

    [SerializeField]
    private PlayerInput input;

    //public Player[] playerList;
    public GameObject fighter1Obj;
    public GameObject fighter2Obj;

    public IFighter fighter1;
    public IFighter fighter2;

    [field: SerializeField]
    private int MyNumber { get; set; } = -1;

    [field: SerializeField]
    private IFighter AttackFighter { get; set; }
    [field: SerializeField]
    private IFighter DefenceFighter { get; set; }

    public CancellationTokenSource tokenSource = new CancellationTokenSource();

    public void ResetScene()
    {
        // ���݂�Scene�����擾����
        Scene loadScene = SceneManager.GetActiveScene();
        // Scene�̓ǂݒ���
        SceneManager.LoadScene(loadScene.name);

        Debug.Log("RESET!");
    }

    //private void Awake()
    //{
    //    fighter1Obj.GetComponent<Fighter>().enabled = true;
    //    fighter2Obj.GetComponent<CPUFighter>().enabled = true;
    //}

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
    public void SetMyNumber(int myNumber)
    {
        MyNumber = myNumber;
    }

    private void SetFighters()
    {
        if(local)
        {
            switch (fightMode)
            {
                case FightMode.PVP:
                    fighter1 = fighter1Obj.GetComponent<LocalFighter>();
                    fighter2 = fighter2Obj.GetComponent<LocalFighter>();
                    fighter1.enabled = true;
                    fighter2.enabled = true;
                    break;
                case FightMode.PVE:
                    fighter1 = fighter1Obj.GetComponent<LocalFighter>();
                    fighter2 = fighter2Obj.GetComponent<CPULocalFighter>();
                    fighter1.enabled = true;
                    fighter2.enabled = true;
                    break;
                case FightMode.EVE:
                    fighter1 = fighter1Obj.GetComponent<CPULocalFighter>();
                    fighter2 = fighter2Obj.GetComponent<CPULocalFighter>();
                    fighter1.enabled = true;
                    fighter2.enabled = true;
                    break;
                default:
                    break;
            }
            fighter1.OpponentFighter = fighter2;
            fighter2.OpponentFighter = fighter1;
        }
        else
        {
            switch (fightMode)
            {
                case FightMode.PVP:
                    if (MyNumber == 0)
                    {
                        fighter1 = fighter1Obj.GetComponent<NetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<NetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    else
                    {
                        fighter1 = fighter1Obj.GetComponent<NetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<NetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    break;
                case FightMode.PVE:
                    if (MyNumber == 0)
                    {
                        fighter1 = fighter1Obj.GetComponent<NetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<CPUNetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    else
                    {
                        fighter1 = fighter1Obj.GetComponent<CPUNetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<NetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    break;
                case FightMode.EVE:
                    if (MyNumber == 0)
                    {
                        fighter1 = fighter1Obj.GetComponent<CPUNetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<CPUNetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    else
                    {
                        fighter1 = fighter1Obj.GetComponent<CPUNetworkFighter>();
                        fighter2 = fighter2Obj.GetComponent<CPUNetworkFighter>();
                        fighter1.enabled = true;
                        fighter2.enabled = true;
                    }
                    break;
                default:
                    break;
            }

            photonController.GameStart();
        }



    }


    async void Start()
    {
        SetFighters();
        NextController.Instance.local = local;

        fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(async _ => await End(fighter1));
        fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(async _ => await End(fighter2));

        print("init�J�n");
        await InitPhase();
        print("init�I��");

        //while (true)
        //{
        //    await StandPhase();
        //    await DrawPhase(tokenSource.Token);
        //    await RidePhase(tokenSource.Token);
        //    await MainPhase();
        //    await ButtlePhase(tokenSource.Token);
        //    await EndPhase();
        //}

        //bool result = await DrawPhase(tokenSource.Token).SuppressCancellationThrow();
        //if (result) Debug.Log("�L�����Z�����ꂽ");
        //else Debug.Log("�L�����Z������ĂȂ�");
        //await RidePhase(tokenSource.Token).SuppressCancellationThrow();
        this.ObserveEveryValueChanged(x => x.phase).Subscribe(phase =>
        {
            if (local) TextManager.Instance.SetPhaseText(phase);
            else photonController.SendState(phase);
        });
        WaitForCanceledAsync(tokenSource.Token).Forget();
        bool result = false;
        int save = 0;
        while (true)
        {
            await UniTask.NextFrame();

            switch (phaseCount)
            {
                case -1:
                    //Debug.Log("�ꎞ��~��");
                    continue;
                case 0:
                    Debug.Log($"{save} ����ĊJ����");
                    phaseCount = save;
                    result = false;
                    continue;
                case 1:
                    result = await StandPhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                case 2:
                    result = await DrawPhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                case 3:
                    result = await RidePhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                case 4:
                    result = await MainPhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                case 5:
                    result = await ButtlePhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                case 6:
                    result = await EndPhase(tokenSource.Token).SuppressCancellationThrow();
                    break;
                default:
                    phaseCount = 1;
                    continue;
            }

            if (result)
            {
                Debug.Log(phaseCount + "�L�����Z������");
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
                save = phaseCount;
                phaseCount = -1;
            }
            else phaseCount++;
        }

    }

    private async UniTaskVoid WaitForCanceledAsync(CancellationToken token)
    {
        await token.WaitUntilCanceled();
        Debug.Log("Canceled!");
    }

    void Update()
    {
        if (input.GetDown("Zoom"))
        {
            tokenSource.Cancel();
            SelectManager.Instance.ZoomCard();
        }
        else if (input.GetDown("Reset")) ResetScene();
    }

    async UniTask InitPhase()
    {
        phase = "����";

        fighter1.CreateDeck();
        fighter2.CreateDeck();
        var CardDic = fighter1.Deck.cardList.Concat(fighter2.Deck.cardList).ToDictionary(card => card.ID);
        fighter1.CardDic = CardDic;
        fighter2.CardDic = CardDic;

        // �����ŁA�J�[�h���������̂�1�t���[���҂��āACard��Start()���\�b�h�����s������
        await UniTask.NextFrame();

        if (fighter1.ActorNumber == 0)
        {
            AttackFighter = fighter1;
            DefenceFighter = fighter2;
        }
        else if (fighter1.ActorNumber == 1)
        {
            AttackFighter = fighter2;
            DefenceFighter = fighter1;
        }

        Debug.Log(AttackFighter.ActorNumber);
        Debug.Log(DefenceFighter.ActorNumber);

        await AttackFighter.SetFirstVanguard();
        await DefenceFighter.SetFirstVanguard();
        await AttackFighter.Deck.Shuffle();
        await DefenceFighter.Deck.Shuffle();

        if (local) await UniTask.WhenAll(fighter1.DrawCard(5), fighter2.DrawCard(5));
        else await fighter1.DrawCard(5);

        AttackFighter.DamageTriggerCheck(2).Forget();

        await fighter1.Mulligan();
        await fighter1.Deck.Shuffle();
        if (local)
        {
            await UniTask.NextFrame();
            await fighter2.Mulligan();
            await fighter2.Deck.Shuffle();
        }
        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());
        Debug.Log("�X�^���h�A�b�v���@���K�[�h");
        await UniTask.NextFrame();
    }

    async UniTask StandPhase(CancellationToken cancellationToken)
    {
        phase = "�X�^���h�t�F�C�Y";

        if (local || AttackFighter.ActorNumber == MyNumber) await AttackFighter.StandPhase(cancellationToken);
    }


    async UniTask DrawPhase(CancellationToken cancellationToken)
    {
        phase = "�h���[�t�F�C�Y";
        await UniTask.NextFrame();

        if (local || AttackFighter.ActorNumber == MyNumber) await AttackFighter.DrawPhase(cancellationToken);

        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
    }

    async UniTask RidePhase(CancellationToken cancellationToken)
    {
        phase = "���C�h�t�F�C�Y";

        if (local || AttackFighter.ActorNumber == MyNumber) await AttackFighter.RidePhase(cancellationToken);

        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
    }

    async UniTask MainPhase(CancellationToken cancellationToken)
    {
        phase = "���C���t�F�C�Y";

        if (local || AttackFighter.ActorNumber == MyNumber)
        {
            while (await AttackFighter.MainPhase(cancellationToken))
            {
                await UniTask.NextFrame();
            }
        }

        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
    }

    async UniTask ButtlePhase(CancellationToken cancellationToken)
    {

        while (true)

        {
            await UniTask.NextFrame();
            phase = "�o�g���t�F�C�Y";

            AttackFighter.SelectedAttackZone = null;
            AttackFighter.SelectedTargetZone = null;

            if (local || AttackFighter.ActorNumber == MyNumber)
                await AttackFighter.AttackStep();
            if (AttackFighter.SelectedAttackZone is null || AttackFighter.SelectedTargetZone is null) break;
            photonController.SendSyncNext(MyNumber);
            //int cancel = await UniTask.WhenAny(UniTask.WaitUntil(() => cancellationToken.IsCancellationRequested), UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext()));
            //if (cancel == 0) return; // �L�����Z�����ďI������
            //print("�L�����Z�����Ȃ�");

            if (local || DefenceFighter.ActorNumber == MyNumber)
                await DefenceFighter.GuardStep();

            photonController.SendSyncNext(MyNumber);
            await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());


            if ((local || AttackFighter.ActorNumber == MyNumber) && AttackFighter.SelectedAttackZone.V)
            {
                phase = "�h���C�u�g���K�[�`�F�b�N";
                int checkCount = AttackFighter.SelectedAttackZone.Card.Ability == Card.AbilityType.TwinDrive ? 2 : 1;
                await AttackFighter.DriveTriggerCheck(checkCount);
            }

            photonController.SendSyncNext(MyNumber);
            await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());

            //await AnimationManager.Instance.AttackEffect(AttackFighter.SelectedAttackZone.transform, AttackFighter.SelectedTargetZone.transform, 0);

            //print(selectedAttackZone.Card.Power);
            //print(selectedTargetZone.Card.Power);
            //print(DefenceFighter.Guardian.Shield);

            if (AttackFighter.SelectedAttackZone.Card.Power >= AttackFighter.SelectedTargetZone.Card.Power + DefenceFighter.Guardian.Shield)
            {
                if (AttackFighter.SelectedTargetZone.V)
                {
                    phase = "�_���[�W�g���K�[�`�F�b�N";

                    await AnimationManager.Instance.HitEffect(AttackFighter.SelectedTargetZone.Card.transform);

                    if (local || DefenceFighter.ActorNumber == MyNumber)
                        await DefenceFighter.DamageTriggerCheck(AttackFighter.SelectedAttackZone.Card.Critical);
                }
                else
                {
                    if (local || DefenceFighter.ActorNumber == MyNumber)
                        await DefenceFighter.RetireCard(AttackFighter.SelectedTargetZone.Card);
                }
            }

            photonController.SendSyncNext(MyNumber);
            await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());

            List<Card> guardian = new List<Card>(DefenceFighter.Guardian.cardList);
            //guardian.ForEach(async card => await CardManager.Instance.GuardianToDrop(DefenceFighter.Guardian, DefenceFighter.Drop, card));
            foreach (var card in guardian)
            {
                await CardManager.Instance.GuardianToDrop(DefenceFighter.Guardian, DefenceFighter.Drop, card);
            }
            //if (local || DefenceFighter.ActorNumber == MyNumber)
                await AttackFighter.EndStep();
                await DefenceFighter.EndStep();

            photonController.SendSyncNext(MyNumber);
            await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
        }
    }
    async UniTask EndPhase(CancellationToken cancellationToken)
    {
        phase = "�G���h�t�F�C�Y";

        await AttackFighter.EndPhase(cancellationToken);
        await DefenceFighter.EndPhase(cancellationToken);

        (AttackFighter, DefenceFighter) = (DefenceFighter, AttackFighter);
        Turn++;
    }

    async UniTask End(IFighter fighter)
    {
        await UniTask.Delay(100);
        Debug.Log($"<color=red>{fighter} �̕���</color>");
        Time.timeScale = 0;
    }

    public void PausePhase()
    {
        if (!tokenSource.IsCancellationRequested) tokenSource.Cancel();
    }


    public void RestartPhase() => phaseCount = 0;

    public void DebugShuffle() => fighter1.Deck.Shuffle().Forget();
}