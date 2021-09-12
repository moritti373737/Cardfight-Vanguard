using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    [field: SerializeField]
    private int Turn { get; set; } = 1;


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
        // 現在のScene名を取得する
        Scene loadScene = SceneManager.GetActiveScene();
        // Sceneの読み直し
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
        fighter1.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter1} の負け</color>"));
        fighter2.Damage.cardList.ObserveCountChanged().Where(damage => damage >= 6).Subscribe(_ => Debug.Log($"<color=red>{fighter2} の負け</color>"));

        //phase = Phase.INIT;
        await InitPhase();

        while (true)
        {
            await StandPhase();
        }
        //TextManager.Instance.SetPhaseText("エンドフェイズ");
    }

    void Update()
    {
        if (Input.GetButtonDown("Zoom"))
        {
            SelectManager.Instance.ZoomCard();
        }
        else if (Input.GetButtonDown("Reset")) ResetScene();
    }

    async UniTask InitPhase()
    {
        TextManager.Instance.SetPhaseText("準備");

        fighter1.CreateDeck();
        fighter2.CreateDeck();

        // ここで、カード生成したので1フレーム待って、CardのStart()メソッドを実行させる
        await UniTask.NextFrame();

        AttackFighter = fighter1;
        DefenceFighter = fighter2;


        await fighter1.SetFirstVanguard();
        await fighter2.SetFirstVanguard();
        fighter1.Deck.Shuffle();
        fighter2.Deck.Shuffle();

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.DrawCard(5), fighter2.DrawCard(5));

        await fighter1.Mulligan();
        await UniTask.NextFrame();
        await fighter2.Mulligan();

        //await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await UniTask.WhenAll(fighter1.StandUpVanguard(), fighter2.StandUpVanguard());

        await UniTask.NextFrame();
    }

    async UniTask StandPhase()
    {
        TextManager.Instance.SetPhaseText("スタンドフェイズ");

        await AttackFighter.StandPhase();

        await DrawPhase();
    }


    async UniTask DrawPhase()
    {
        TextManager.Instance.SetPhaseText("ドローフェイズ");

        await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));

        await AttackFighter.DrawCard(1);

        await UniTask.NextFrame();
        //await AttackFighter.GuardPhase();

        await RidePhase();
    }

    async UniTask RidePhase()
    {
        TextManager.Instance.SetPhaseText("ライドフェイズ");

        await AttackFighter.RidePhase();
        await MainPhase();
    }

    async UniTask MainPhase()
    {
        TextManager.Instance.SetPhaseText("メインフェイズ");

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
            TextManager.Instance.SetPhaseText("バトルフェイズ");

            (ICardCircle selectedAttackZone, ICardCircle selectedTargetZone) = await AttackFighter.AttackStep();
            if (selectedAttackZone == null) break;

            await DefenceFighter.GuardStep();

            if (selectedAttackZone.V)
            {
                TextManager.Instance.SetPhaseText("ドライブトリガーチェック");
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
                    TextManager.Instance.SetPhaseText("ダメージトリガーチェック");
                    await DefenceFighter.DamageTriggerCheck(selectedAttackZone.Card.Critical);
                }
                else
                {
                    await DefenceFighter.RetireCard(selectedTargetZone);
                }
            }

            new List<Card>(DefenceFighter.Guardian.cardList).ForEach(async card => await CardManager.Instance.GuardianToDrop(DefenceFighter.Guardian, DefenceFighter.Drop, card));

            await AttackFighter.EndStep();

        }
        await EndPhase();
    }
    async UniTask EndPhase()
    {
        TextManager.Instance.SetPhaseText("エンドフェイズ");

        Turn++;
        await AttackFighter.EndPhase();
        await DefenceFighter.EndPhase();

        (AttackFighter, DefenceFighter) = (DefenceFighter, AttackFighter);
        return;
    }

    //void StandbyPhase()
    //{
    //    Debug.Log("StandbyPhase");
    //    // 手札のカードを場に出す
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