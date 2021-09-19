using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CPULocalFighter : MonoBehaviour, IFighter
{
    [field: SerializeField]
    public FighterID ID { get; private set; }

    [field: SerializeField]
    public int ActorNumber { get; set; }

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    [field: SerializeField]
    public IFighter OpponentFighter { get; set; }

    public GameObject Field { get; private set; }

    public Hand Hand { get; private set; }
    public Deck Deck { get; private set; }
    public Vanguard Vanguard { get; private set; }
    public List<Rearguard> Rearguards { get; private set; } = new List<Rearguard>();
    private List<ICardCircle> Circle { get; set; }
    public Drop Drop { get; private set; }
    public Damage Damage { get; private set; }
    public Drive Drive { get; private set; }
    public Guardian Guardian { get; private set; }
    public Order Order { get; private set; }
    public Soul Soul { get; private set; }

    public Dictionary<int, Card> CardDic { get; set; }

    public ICardCircle SelectedAttackZone { get; set; } = null;
    public ICardCircle SelectedTargetZone { get; set; } = null;

    private void OnEnable()
    {
        //子オブジェクトを全て取得する
        //foreach (Transform childTransform in field.transform)
        //{
        //    if (childTransform.tag.Contains("Vanguard"))
        //        vanguard = childTransform.GetComponent<Vanguard>();
        //}
        Field = transform.FindWithChildTag(Tag.Field).gameObject;
        Hand = transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();
        Deck = Field.transform.FindWithChildTag(Tag.Deck).GetComponent<Deck>();
        Vanguard = Field.transform.FindWithChildTag(Tag.Vanguard).GetComponent<Vanguard>();
        Drop = Field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
        Damage = Field.transform.FindWithChildTag(Tag.Damage).GetComponent<Damage>();
        Drive = Field.transform.FindWithChildTag(Tag.Drive).GetComponent<Drive>();
        Guardian = Field.transform.FindWithChildTag(Tag.Guardian).GetComponent<Guardian>();
        Order = Field.transform.FindWithChildTag(Tag.Order).GetComponent<Order>();
        Soul = Field.transform.FindWithChildTag(Tag.Soul).GetComponent<Soul>();

        List<Transform> rearguards = Field.transform.FindWithAllChildTag(Tag.Rearguard);
        foreach (var rear in rearguards)
        {
            Rearguards.Add(rear.GetComponent<Rearguard>());
        }
        Circle = new List<ICardCircle>(Rearguards);
        Circle.Add(Vanguard);
    }

    /// <summary>
    /// デッキを作成する
    /// </summary>
    public void CreateDeck()
    {
        DeckGenerater.Instance.Generate(Deck, ID == FighterID.ONE ? 0 : 1);
    }

    /// <summary>
    /// ファーストヴァンガードをセットする
    /// </summary>
    /// <returns></returns>
    public void SetFirstVanguard()
    {
        _ = CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    /// <summary>
    /// 手札のカードを交換する（マリガン）
    /// マリガン後のシャッフル処理は未実装
    /// </summary>
    public async UniTask Mulligan()
    {
        print("マリガン開始");
        //return;
        // グレード1-3まで1枚づつ残す
        List<Card> cardList = Hand.cardList.Where(card => card.Grade == 0)
                                           .Union(Hand.cardList.Where(card => card.Grade == 1).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 2).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 3).Skip(1))
                                           .ToList();
        int count = cardList.Count;

        for (int i = 0; i < count; i++)
        {
            Card card = cardList.Pop();
            await CardManager.Instance.HandToDeck(Hand, Deck, card); // ここでcardListの要素数が変化するため通常のForEachは使用不可
        }

        await DrawCard(count);
    }

    /// <summary>
    /// カードをデッキから指定枚数引く
    /// </summary>
    /// <param name="count">引く枚数</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(Deck, Hand, Deck.GetCard(0));
        }

    }

    /// <summary>
    /// スタンドアップヴァンガード！
    /// </summary>
    public async UniTask StandUpVanguard()
    {
        var card = Vanguard.Card;
        card.SetState(Card.State.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard);
    }

    /// <summary>
    /// スタンドフェイズ
    /// </summary>
    public async UniTask StandPhase()
    {
        await UniTask.NextFrame();
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// ドローフェイズ
    /// </summary>
    public async UniTask DrawPhase()
    {
        await DrawCard(1);
    }

    /// <summary>
    /// ライドフェイズ
    /// </summary>
    public async UniTask RidePhase()
    {
        // ヴァンガードのグレード+1のカードの中でパワーが最大のカード
        Card card = Hand.cardList.Where(card => card.Grade == Vanguard.Card.Grade + 1)
                                 .OrderByDescending(card => card.Power)
                                 .FirstOrDefault();
        if (card == null) return; // Gアシストステップの実装

        Card removedCard = Vanguard.Card; // カードが既に存在する場合はソウルに移動させる
        await CardManager.Instance.HandToCircle(Hand, Vanguard, card);
        if (removedCard != null)
        {
            Debug.Log("ソウルに送るよ");
            await CardManager.Instance.CircleToSoul(Vanguard, Soul, removedCard);
        }
    }

    /// <summary>
    /// メインフェイズ
    /// </summary>
    public async UniTask<bool> MainPhase()
    {
        if (Turn == 1) return false;

        if (Vanguard.Card.Grade == 1)
        {
            List<Card> cardList = Hand.cardList.Where(card => card.Grade == 1)
                                               .OrderByDescending(card => card.Power)
                                               .ToList();

            Card card = cardList.Dequeue();
            if (card != null)
            {
                Rearguard target = IDToRearguard(22);
                if (target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            card = cardList.Dequeue();
            if (card != null)
            {
                //if (card.Power < OpponentFighter.Vanguard.Card.Power) return false;
                Rearguard target = IDToRearguard(11);
                if (target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            card = cardList.Dequeue();
            if (card != null)
            {
                //if (card.Power < OpponentFighter.Vanguard.Card.Power) return false;
                Rearguard target = IDToRearguard(13);
                if (target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }
        }
        else if (Vanguard.Card.Grade >= 2)
        {
            Card card = IDToRearguard(11).Card;
            if (card != null && card.Ability == Card.AbilityType.Boost)
            {
                await CardManager.Instance.CircleToCircle(IDToRearguard(11), IDToRearguard(21), card);
            }

            card = IDToRearguard(13).Card;
            if (card != null && card.Ability == Card.AbilityType.Boost)
            {
                await CardManager.Instance.CircleToCircle(IDToRearguard(13), IDToRearguard(23), card);
            }

            List<Card> cardList = Hand.cardList.Where(card => card.Grade == 2)
                                               .OrderByDescending(card => card.Power)
                                               .Union(Hand.cardList.Where(card => card.Grade == 3).OrderBy(card => card.Power).Skip(1))
                                               .ToList();

            card = cardList.Dequeue();
            if (card != null)
            {
                Rearguard target = IDToRearguard(11);
                if (target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            card = cardList.Dequeue();
            if (card != null)
            {
                Rearguard target = IDToRearguard(13);
                if (target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            cardList = Hand.cardList.Where(card => card.Grade == 1)
                                   .OrderByDescending(card => card.Power)
                                   .ToList();

            card = cardList.Dequeue();
            if (card != null)
            {
                Rearguard target = IDToRearguard(22);
                if (GetSameColumn(target).Card != null && target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            card = cardList.Dequeue();
            if (card != null)
            {
                //if (card.Power < OpponentFighter.Vanguard.Card.Power) return false;
                Rearguard target = IDToRearguard(21);
                if (GetSameColumn(target).Card != null && target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }

            card = cardList.Dequeue();
            if (card != null)
            {
                //if (card.Power < OpponentFighter.Vanguard.Card.Power) return false;
                Rearguard target = IDToRearguard(23);
                if (GetSameColumn(target).Card != null && target.Card == null) await CardManager.Instance.HandToCircle(Hand, target, card);
                else cardList.Insert(0, card);
            }
        }

        return false;
    }

    /// <summary>
    /// アタックフェイズ内のアタックステップ
    /// </summary>
    public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    {
        await UniTask.NextFrame();
        ICardCircle selectedBoostZone = null;
        SelectedAttackZone = null;
        SelectedTargetZone = null;

        if (Turn == 1) return (null, null);

        IEnumerable<ICardCircle> front = Rearguards.Where(rear => rear.Front)
                                                   .Where(rear => rear.HasCard())
                                                   .Where(rear => rear.Card.JudgeState(Card.State.Stand))
                                                   .OrderBy(circle => GetSameColumnPower(circle));
                                                   //.Where(rear => GetSameColumnPower(rear) >= OpponentFighter.Vanguard.Card.Power);

        if (front.Count() >= 2)
        {
            SelectedAttackZone = front.FirstOrDefault();
            var boost = GetSameColumn(SelectedAttackZone);
            selectedBoostZone = boost.HasCard() ? boost : null;
        }
        else if (front.Count() == 1)
        {
            if (Vanguard.Card.JudgeState(Card.State.Stand))
            {
                SelectedAttackZone = Vanguard;
                var boost = GetSameColumn(SelectedAttackZone);
                selectedBoostZone = boost.HasCard() ? boost : null;
            }
            else
            {
                SelectedAttackZone = front.First();
                var boost = GetSameColumn(SelectedAttackZone);
                selectedBoostZone = boost.HasCard() ? boost : null;
            }
        }
        else if(front.Count() == 0)
        {
            if (Vanguard.Card.JudgeState(Card.State.Stand))
            {
                SelectedAttackZone = Vanguard;
                var boost = GetSameColumn(SelectedAttackZone);
                selectedBoostZone = boost.HasCard() ? boost : null;
            }
        }

        if (SelectedAttackZone != null)
        {
            SelectedTargetZone = GetSameColumnPower(SelectedAttackZone) >= OpponentFighter.Vanguard.Card.Power ? (ICardCircle)OpponentFighter.Vanguard : (ICardCircle)OpponentFighter.Rearguards.Where(rear => rear.Front).Where(rear => rear.HasCard()).OrderBy(rear => rear.Card.Power).FirstOrDefault();
            if (SelectedTargetZone == null) SelectedTargetZone = OpponentFighter.Vanguard;
            await CardManager.Instance.RestCard(SelectedAttackZone);
        }
        if (selectedBoostZone != null)
        {
            await CardManager.Instance.RestCard(selectedBoostZone);
            SelectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
        }

        return (SelectedAttackZone, SelectedTargetZone);
    }

    /// <summary>
    /// アタックフェイズ内のガードステップ
    /// </summary>
    public async UniTask<bool> GuardStep()
    {
        await UniTask.NextFrame();
        int n = Damage.cardList.Count;
        int damageCost = n * (n + 1) / 2;
        int guardCost = OpponentFighter.SelectedAttackZone.Card.Power - OpponentFighter.SelectedTargetZone.Card.Power;
        if (guardCost < 0) return false;
        //IEnumerable<Card> card5000 = Hand.cardList.Where(card => card.Shield == 5000);
        //IEnumerable<Card> card10000 = Hand.cardList.Where(card => card.Shield == 10000);

        int requestSheild = (guardCost / 5000 + 1) * 5000;

        if (Hand.cardList.Sum(card => card.Shield) < requestSheild) return false;

        IOrderedEnumerable<Card> handCard = Hand.cardList.Where(card => card.Shield != 0).OrderByDescending(card => card.Shield).ThenBy(card => card.Grade);
        List<Card> sheildCard = new List<Card>();
        foreach (var card in handCard)
        {
            if (requestSheild == 5000)
            {
                sheildCard.Add(handCard.Last());
                requestSheild -= handCard.Last().Shield;
            }
            else
            {
                sheildCard.Add(card);
                requestSheild -= card.Shield;
            }

            if (requestSheild <= 0) break;
        }

        foreach (var card in sheildCard)
        {
            await CardManager.Instance.HandToGuardian(Hand, Guardian, card);
        }

        return false;
    }

    /// <summary>
    /// ドライブトリガーチェック
    /// </summary>
    public async UniTask DriveTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            if (Drive.GetCard().Trigger != Card.TriggerType.None)
                await GetDriveTrigger(Drive.GetCard());
            await CardManager.Instance.DriveToHand(Drive, Hand, Drive.Card);
        }

    }

    /// <summary>
    /// ダメージトリガーチェック
    /// </summary>
    public async UniTask DamageTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            if (Drive.GetCard().Trigger != Card.TriggerType.None)
                await GetDamageTrigger(Drive.GetCard());
            await CardManager.Instance.DriveToDamage(Drive, Damage, Drive.Card);
        }
    }

    /// <summary>
    /// トリガー取得
    /// </summary>
    private async UniTask GetDriveTrigger(Card triggerCard)
    {
        await UniTask.NextFrame();

        await TriggerTypeAction(triggerCard);

        Card powerUp = Circle.Where(circle => circle.Front).Where(circle => circle.HasCard()).Where(circle => circle.Card.JudgeState(Card.State.Stand)).OrderBy(circle => circle.Card.Power).FirstOrDefault()?.Card;
        powerUp ??= Vanguard.Card;
        powerUp.AddPower(triggerCard.TriggerPower);
    }

    /// <summary>
    /// トリガー取得
    /// </summary>
    private async UniTask GetDamageTrigger(Card triggerCard)
    {
        await UniTask.NextFrame();

        await TriggerTypeAction(triggerCard);

        Card powerUp = Vanguard.Card;
        powerUp.AddPower(triggerCard.TriggerPower);
    }

    private async UniTask TriggerTypeAction(Card triggerCard)
    {
        switch (triggerCard.Trigger)
        {
            //case Card.TriggerType.None:
            //    break;
            case Card.TriggerType.Critical:
                Vanguard.Card.AddCritical(1);
                break;
            case Card.TriggerType.Draw:
                await DrawCard(1);
                break;
            case Card.TriggerType.Front:
                break;
            case Card.TriggerType.Heal:
                if (Damage.Count < OpponentFighter.Damage.Count) return;
                if (Damage.Count == 0) return;
                Card damage = Damage.cardList.OrderBy(damage => damage.JudgeState(Card.State.FaceUp)).ThenBy(damage => damage.Grade).FirstOrDefault();
                if (damage != null) await CardManager.Instance.DamageToDrop(Damage, Drop, damage);
                break;
            case Card.TriggerType.Stand:
                Rearguard stand = Rearguards.Where(circle => circle.Front).Where(circle => circle.HasCard()).Where(circle => !circle.Card.JudgeState(Card.State.Stand)).OrderByDescending(circle => circle.Card.Power).FirstOrDefault();
                if (stand != null) await CardManager.Instance.StandCard(stand);
                break;
            case Card.TriggerType.Over:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// アタックフェイズ内のエンドステップ
    /// </summary>
    public async UniTask EndStep()
    {
        await UniTask.NextFrame();
        Circle.Where(circle => circle.Card != null).ToList().ForEach(circle => circle.Card.BoostedPower = 0);
    }

    /// <summary>
    /// エンドフェイズ
    /// </summary>
    public async UniTask EndPhase()
    {
        await UniTask.NextFrame();

        Circle.ForEach(circle => circle.Card?.Reset());

        Turn++;
    }

    /// <summary>
    /// 指定したサークルのカードを退却させる
    /// </summary>
    /// <param name="circle">退却させるサークル</param>

    public async UniTask RetireCard(ICardCircle circle)
    {
        await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    }

    /// <summary>
    /// メインデータ（CardManagerで実行する関数の内容）を受信したときの処理
    /// カードに関する処理が送られている
    /// </summary>
    /// <param name="args">送信元ID、関数名、カードID</param>
    public async UniTask ReceivedData(List<object> args) { }

    /// <summary>
    /// さまざまな処理（Attackなど）を受信した時の処理
    /// </summary>
    /// <param name="args">送信元ID、処理名、オプション(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args) { }

    /// <summary>
    /// Stateデータを受信したときの処理
    /// フェイズの管理に使う予定
    /// </summary>
    /// <param name="state">フェイズ名</param>
    public void ReceivedState(string state) { }

    private Rearguard IDToRearguard(int id) => Rearguards.Find(rear => rear.ID == id);
    private ICardCircle StringToCircle(string name) => name == "Vanguard" ? (ICardCircle)Vanguard : (ICardCircle)Rearguards.Find(rear => rear.Name == name);

    private ICardCircle GetSameColumn(ICardCircle cardCircle)
    {
        return Circle.Where(circle => cardCircle.IsSameColumn(circle)).First(circle => circle.ID != cardCircle.ID);
    }

    private int GetSameColumnPower(ICardCircle cardCircle)
    {
        ICardCircle circle = GetSameColumn(cardCircle);
        return (circle.HasCard() && circle.Card.JudgeState(Card.State.Stand) ? circle.Card.Power : 0) + (cardCircle.HasCard() && cardCircle.Card.JudgeState(Card.State.Stand) ? cardCircle.Card.Power : 0);
    }
}
