using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public GameObject fighter1Obj;
    public GameObject fighter2Obj;

    public IFighter fighter1;
    public IFighter fighter2;

    [field: SerializeField]
    private int MyNumber { get; set; }

    [field: SerializeField]
    private IFighter AttackFighter { get; set; }
    [field: SerializeField]
    private IFighter DefenceFighter { get; set; }

    public readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    public void ResetScene()
    {
        // 現在のScene名を取得する
        Scene loadScene = SceneManager.GetActiveScene();
        // Sceneの読み直し
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
    //    fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter1} の負け</color>"));
    //    fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter2} の負け</color>"));

    //    await InitPhase();

    //    while (true)
    //    {
    //        await StandPhase();
    //    }
    //}

    public void SetFighters(int myNumber)
    {
        MyNumber = myNumber;
        if (myNumber == 0)
        {
            fighter1 = fighter1Obj.GetComponent<Fighter>();
            fighter2 = fighter2Obj.GetComponent<Fighter>();
            fighter1.enabled = true;
            fighter2.enabled = true;
        }
        else
        {
            fighter1 = fighter1Obj.GetComponent<Fighter>();
            fighter2 = fighter2Obj.GetComponent<Fighter>();
            fighter1.enabled = true;
            fighter2.enabled = true;
        }

        //if (myNumber == 0)
        //{
        //    fighter1 = fighter1Obj.GetComponent<Fighter>();
        //    fighter2 = fighter2Obj.GetComponent<CPUFighter>();
        //    fighter1.enabled = true;
        //    fighter2.enabled = true;
        //}
        //else
        //{
        //    fighter1 = fighter1Obj.GetComponent<CPUFighter>();
        //    fighter2 = fighter2Obj.GetComponent<Fighter>();
        //    fighter1.enabled = true;
        //    fighter2.enabled = true;
        //}

        //if (myNumber == 0)
        //{
        //    fighter1 = fighter1Obj.GetComponent<CPUFighter>();
        //    fighter2 = fighter2Obj.GetComponent<CPUFighter>();
        //    fighter1.enabled = true;
        //    fighter2.enabled = true;
        //}
        //else
        //{
        //    fighter1 = fighter1Obj.GetComponent<CPUFighter>();
        //    fighter2 = fighter2Obj.GetComponent<CPUFighter>();
        //    fighter1.enabled = true;
        //    fighter2.enabled = true;
        //}
    }


    async void Start()
    {
        fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter1} の負け</color>"));
        fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter2} の負け</color>"));

        print("init開始");
        await InitPhase();
        print("init終了");

        while (true)
        {
            await StandPhase();
            await DrawPhase();
            await RidePhase();
            await MainPhase();
            await ButtlePhase(tokenSource.Token);
            await EndPhase();
        }
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
        TextManager.Instance.SetPhaseText("準備");

        fighter1.CreateDeck();
        fighter2.CreateDeck();
        var CardDic = fighter1.Deck.cardList.Concat(fighter2.Deck.cardList).ToDictionary(card => card.ID);
        fighter1.CardDic = CardDic;
        fighter2.CardDic = CardDic;

        // ここで、カード生成したので1フレーム待って、CardのStart()メソッドを実行させる
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

        AttackFighter.SetFirstVanguard();
        DefenceFighter.SetFirstVanguard();
        AttackFighter.Deck.Shuffle();
        DefenceFighter.Deck.Shuffle();

        //await UniTask.WhenAll(fighter1.DrawCard(5), fighter2.DrawCard(5));
        await fighter1.DrawCard(5);

        await fighter1.Mulligan();

        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());

        //await UniTask.NextFrame();
        //await fighter2.Mulligan();

        //await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());
        Debug.Log("スタンドアップヴァンガード");
        await UniTask.NextFrame();
    }

    async UniTask StandPhase()
    {
        //TextManager.Instance.SetPhaseText("スタンドフェイズ");
        photonController.SendState("スタンドフェイズ");

        //await AttackFighter.StandPhase();
    }


    async UniTask DrawPhase()
    {
        //TextManager.Instance.SetPhaseText("ドローフェイズ");
        photonController.SendState("ドローフェイズ");

        if(AttackFighter.ActorNumber == MyNumber) await AttackFighter.DrawPhase();

        await UniTask.NextFrame();
        //await AttackFighter.GuardPhase();

        NextController.Instance.SetSyncNext(MyNumber, true);
        photonController.SendSyncNext(MyNumber);

        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
    }

    async UniTask RidePhase()
    {
        Debug.Log("ライドフェイズ");
        //TextManager.Instance.SetPhaseText("ライドフェイズ");
        photonController.SendState("ライドフェイズ");
        if (AttackFighter.ActorNumber == MyNumber) await AttackFighter.RidePhase();

        NextController.Instance.SetSyncNext(MyNumber, true);
        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());

    }

    async UniTask MainPhase()
    {
        //TextManager.Instance.SetPhaseText("メインフェイズ");
        photonController.SendState("メインフェイズ");

        if (AttackFighter.ActorNumber == MyNumber)
        {
            while (await AttackFighter.MainPhase())
            {
                await UniTask.NextFrame();
            }
        }

        NextController.Instance.SetSyncNext(MyNumber, true);
        photonController.SendSyncNext(MyNumber);
        await UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext());
    }

    async UniTask ButtlePhase(CancellationToken cancellationToken)
    {

        while (true)

        {
            await UniTask.NextFrame();
            //TextManager.Instance.SetPhaseText("バトルフェイズ");
            photonController.SendState("バトルフェイズ");

            ICardCircle selectedAttackZone = null, selectedTargetZone = null;

            if (AttackFighter.ActorNumber == MyNumber)
                    (selectedAttackZone, selectedTargetZone) = await AttackFighter.AttackStep();
            //if (selectedAttackZone == null) break;
            NextController.Instance.SetSyncNext(MyNumber, true);
            photonController.SendSyncNext(MyNumber);
            int cancel = await UniTask.WhenAny(UniTask.WaitUntil(() => cancellationToken.IsCancellationRequested), UniTask.WaitUntil(() => NextController.Instance.JudgeAllSyncNext()));
            if (cancel == 0) return; // キャンセルして終了する
            print("キャンセルしない");

            if (DefenceFighter.ActorNumber == MyNumber)
                await DefenceFighter.GuardStep();

            if (AttackFighter.ActorNumber == MyNumber && selectedAttackZone.V)
            {
                //TextManager.Instance.SetPhaseText("ドライブトリガーチェック");
                photonController.SendState("ドライブトリガーチェック");
                int checkCount = selectedAttackZone.Card.Ability == Card.AbilityType.TwinDrive ? 2 : 1;
                await AttackFighter.DriveTriggerCheck(checkCount);
            }

            //print(selectedAttackZone.Card.Power);
            //print(selectedTargetZone.Card.Power);
            //print(DefenceFighter.Guardian.Shield);

            if (selectedAttackZone.Card.Power >= selectedTargetZone.Card.Power + DefenceFighter.Guardian.Shield)
            {
                if (selectedTargetZone.V)
                {
                    //TextManager.Instance.SetPhaseText("ダメージトリガーチェック");
                    photonController.SendState("ダメージトリガーチェック");
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
    }
    async UniTask EndPhase()
    {
        TextManager.Instance.SetPhaseText("エンドフェイズ");

        Turn++;
        await AttackFighter.EndPhase();
        await DefenceFighter.EndPhase();

        (AttackFighter, DefenceFighter) = (DefenceFighter, AttackFighter);
    }
}