using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Threading;

public class LocalFighter : MonoBehaviour, IFighter
{
    [SerializeField]
    private PlayerInput input;

    [field: SerializeField]
    public FighterID ID { get; private set; }

    [field: SerializeField]
    public int ActorNumber { get; set; }

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    public IFighter OpponentFighter { get; set; }

    public GameObject Field { get; private set; }

    public Hand Hand { get; private set; }
    public Deck Deck { get; private set; }
    public Vanguard Vanguard { get; private set; }
    public List<Rearguard> Rearguards { get; private set; } = new List<Rearguard>();
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

        OpponentFighter = GameObject.FindGameObjectsWithTag("Fighter")
                                    .Select(obj => obj.GetComponent<IFighter>())
                                    .First(fighter => fighter.ID != ID);
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
    public async UniTask SetFirstVanguard()
    {
        await CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    /// <summary>
    /// 手札のカードを交換する（マリガン）
    /// マリガン後のシャッフル処理は未実装
    /// </summary>
    public async UniTask Mulligan()
    {
        while (true)
        {
            await UniTask.NextFrame();

            int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));

            if (inputIndex == 0) SelectManager.Instance.NormalSelected(Tag.Hand, ID); // Enter入力時
            else if (inputIndex == 1 && SelectManager.Instance.SelectedCount == 0) return;  // Submit入力時
            else break;
        }

        int ToDeckCount = SelectManager.Instance.SelectedCount;

        await SelectManager.Instance.ForceConfirm(Tag.Deck, ID, Action.MOVE);

        await DrawCard(ToDeckCount);
    }

    /// <summary>
    /// カードをデッキから指定枚数引く
    /// </summary>
    /// <param name="count">引く枚数</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Drawする");

        await UniTask.WaitUntil(() => input.GetDown("Enter"));

        //CardManager.DeckToHand(deck, hand);
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
        card.SetState(Card.StateType.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard.Card);
    }

    /// <summary>
    /// スタンドフェイズ
    /// </summary>
    public async UniTask StandPhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();
        await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// ドローフェイズ
    /// </summary>
    public async UniTask DrawPhase(CancellationToken cancellationToken)
    {
        await UniTask.WaitUntil(() => input.GetDown("Enter"), cancellationToken: cancellationToken);
        await DrawCard(1);
    }

    /// <summary>
    /// ライドフェイズ
    /// </summary>
    public async UniTask RidePhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter"), cancellationToken: cancellationToken), UniTask.WaitUntil(() => input.GetDown("Submit"), cancellationToken: cancellationToken));
            return resultInt.ToEnum("YES", "END");
        });
        functions.Add(async () => {
            Card selectedCard = SelectManager.Instance.GetSelectCard(Tag.Hand, ID);
            if (selectedCard is null) return Result.NO;
            if (selectedCard.Grade > Vanguard.Card.Grade + 1) return Result.NO; // グレードのチェック
            SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            return Result.YES;
        });
        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functions.Add(async () => {
            (ICardCircle vanguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE);
            if (vanguard is null) return Result.NO;
            Card removedCard = vanguard.Card; // カードが既に存在する場合はソウルに移動させる
            await CardManager.Instance.HandToCircle(Hand, vanguard, card.GetComponent<Card>());
            Debug.Log(removedCard);
            if (removedCard != null)
            {
                Debug.Log("ソウルに送るよ");
                await CardManager.Instance.CircleToSoul(vanguard, Soul, removedCard);
            }
            return Result.YES;
        });

        int i = 0;
        Result result = Result.NONE;
        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;    // 1つ次に進む
                    break;
                case Result.NO:
                    i -= 1; // 1つ前に戻る
                    break;
                case Result.CANCEL:
                    SelectManager.Instance.SingleCancel();
                    i = 0;  // 最初に戻る
                    break;
                case Result.END:
                    return; // 終了する
            }
        }

        print("owari");

    }

    /// <summary>
    /// メインフェイズ
    /// </summary>
    public async UniTask<bool> MainPhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();
        Card selectedCard = null;
        Debug.Log("メイン開始！");
        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsC = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsM = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsS = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            Debug.Log("Enter1開始");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            Debug.Log("Enter1終了");
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            selectedCard = SelectManager.Instance.GetSelectCard(Tag.None, ID);
            //ICardZone selectedZone = await SelectManager.Instance.GetZone(Tag.None, ID);
            if (selectedCard is null) return Result.NO;
            print(selectedCard);
            SelectManager.Instance.SetActionCard(selectedCard);
            state = functionsV2;
            functions.AddRange(functionsV2);
            return Result.YES;
        });

        functionsV2.Add(async () =>
        {
            Debug.Log("Enter2開始");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter"), cancellationToken: cancellationToken), UniTask.WaitUntil(() => input.GetDown("Cancel"), cancellationToken: cancellationToken));
            Debug.Log("Enter2終了");
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            string act = SelectManager.Instance.ActionConfirm(ActorNumber);
            //return Result.YES;

            if (act == "Call")
            {
                selectedCard = SelectManager.Instance.GetSelectCard(Tag.Hand, ID);
                if (selectedCard != null)
                {
                    //if (selectedCard.Grade > Vanguard.Card.Grade) return Result.RESTART; // グレードのチェック
                    SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                    state = functionsC;
                    functions.AddRange(functionsC);
                    return Result.YES;
                }

                selectedCard = SelectManager.Instance.GetSelectCard(Tag.Rearguard, ID);
                if (selectedCard != null)
                {
                    SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
                    state = functionsM;
                    functions.AddRange(functionsM);
                    return Result.YES;
                }
            }
            else if (act == "Skill")
            {
                print("スキル発動");
                //state = functionsS;
                //functions.AddRange(functionsS);
                await SkillManager.Instance.StartActivate(selectedCard, 0);
                return Result.YES;
            }

            return Result.RESTART;

        });

        // ここまで共通処理

        // ここから分岐
        // 分岐1
        functionsC.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsC.Add(async () =>
        {
            (ICardCircle rearguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            if (rearguard is null) return Result.NO;
            Card removedCard = rearguard.Card; // カードが既に存在する場合は移動させる
            await CardManager.Instance.HandToCircle(Hand, rearguard, card.GetComponent<Card>());
            if (removedCard != null) await CardManager.Instance.CircleToDrop(rearguard, Drop, removedCard);
            return Result.YES;
        });

        // 分岐2
        functionsM.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsM.Add(async () =>
        {
            //Card targetCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
            Rearguard targetZone = (Rearguard)SelectManager.Instance.GetZone(Tag.Rearguard, ID);
            if (targetZone is null) return Result.NO;
            Rearguard selectedRearguard = selectedCard.transform.GetComponentInParent<Rearguard>();
            if (!selectedRearguard.IsSameColumn(targetZone)) return Result.NO;
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            await CardManager.Instance.CircleToCircle(selectedRearguard, (Rearguard)circle, card.GetComponent<Card>());
            return Result.YES;
        });

        functions.AddRange(functionsV);

        int i = 0;
        Result result = Result.NONE;
        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;      // 1つ次に進む
                    break;
                case Result.NO:
                    i -= 1;   // 1つ前に戻る
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count; // 直前の分岐点に戻る
                    state = functionsV;
                    SelectManager.Instance.SingleCancel();
                    SelectManager.Instance.ActionConfirm(ActorNumber);
                    break;
                case Result.RESTART:
                    i = 0;
                    functions.Clear();
                    functions.AddRange(functionsV);
                    break;
                case Result.END:
                    return false;
            }
        }

        print("owari");
        return true;

     }

    /// <summary>
    /// アタックフェイズ内のアタックステップ
    /// </summary>
    public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    {
        await UniTask.NextFrame();
        print("開始");

        //if (Turn == 1) return true;

        Result result = Result.NONE;
        ICardCircle selectedBoostZone = null;

        int i = 0;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsB = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV3 = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            await UniTask.NextFrame();
            Card selectedAttackCard = SelectManager.Instance.GetSelectCard(Tag.Circle, ID); // こちらのサークルを選択したかどうか
            if (selectedAttackCard is null) return Result.NO;
            SelectedAttackZone = selectedAttackCard.GetComponentInParent<ICardCircle>();

            //if (!selectedAttackZone.Card.JudgeStep(Card.StateType.Stand)) return Result.NO; // 攻撃可能なカードか判定
            if (!SelectedAttackZone.Front) return Result.NO;
            state = functionsV2;
            functions.AddRange(functionsV2);
            SelectManager.Instance.NormalSelected(Tag.Circle, ID); // 攻撃するカードを選択する
            return Result.YES;
        });


        functionsV2.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            Card selectedBoostCard = SelectManager.Instance.GetSelectCard(Tag.Rearguard, ID); // ブーストする
            if (selectedBoostCard != null)
            {
                selectedBoostZone = selectedBoostCard.GetComponentInParent<Rearguard>();
                if (!selectedBoostZone.Card.JudgeState(Card.StateType.Stand)) return Result.NO; // ブースト可能なカードか判定
                if (!selectedBoostZone.IsSameColumn(SelectedAttackZone)) return Result.NO;
                if (selectedBoostZone.Card.Ability != Card.AbilityType.Boost) return Result.NO;
                SelectManager.Instance.NormalSelected(Tag.Circle, ID); // ブーストするカードを選択する
                SelectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
                ActionManager.Instance.ActionHistory.Add(new ActionData("Boost", ID, selectedBoostZone.Card, selectedBoostZone, SelectedAttackZone));
                state = functionsB;
                functions.AddRange(functionsB);
                return Result.YES;
            }
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
            if (circle is null) return Result.NO;
            SelectedTargetZone = circle;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, SelectedAttackZone.Card, SelectedAttackZone, SelectedTargetZone));
            state = functionsV3;
            functions.AddRange(functionsV3);
            return Result.YES;
        });

        functionsB.Add(async () =>
        {
            print("T");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsB.Add(async () => {
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
            if (circle is null) return Result.NO;
            SelectedTargetZone = circle;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, SelectedAttackZone.Card, SelectedAttackZone, SelectedTargetZone));
            state = functionsV3;
            functions.AddRange(functionsV3);
            return Result.YES;
        });

        functionsV3.Add(async () =>
        {
            print("V3");
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            print("V3Enter");
            await CardManager.Instance.RestCard(SelectedAttackZone);
            if (selectedBoostZone != null) await CardManager.Instance.RestCard(selectedBoostZone);
            return Result.YES;
        });

        functions.AddRange(functionsV);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            //print($"{ i}, { result}, { SelectManager.Instance.SelectedCount}, {functions.Count}");
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;
                    break;
                case Result.NO:
                    i--;
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    state = functionsV;
                    SelectManager.Instance.SingleCancel();
                    break;
                case Result.END:
                    return (null, null);
            }
        }

        if (SelectedAttackZone != null && SelectedTargetZone != null)
        {
            AnimationManager.Instance.SelectButtleCard(SelectedAttackZone.transform, SelectedTargetZone.transform);
        }

        return (SelectedAttackZone, SelectedTargetZone);
    }

    /// <summary>
    /// アタックフェイズ内のガードステップ
    /// </summary>
    public async UniTask<bool> GuardStep()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedCard = null;

        List<Func<UniTask<Result>>> functionsSelect = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsSubmit = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        var state = functions;


        functionsSelect.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            if (resultInt == 1)
            {
                state = functionsSubmit;
                functions.Pop();
                functions.AddRange(functionsSubmit);
            }
            return Result.YES;
        });

        functionsSelect.Add(async () =>
        {
            selectedCard = SelectManager.Instance.GetSelectCard(Tag.Hand, ID);
            if (selectedCard != null)
            {
                SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                functions.AddRange(functionsSelect);
                return Result.YES;
            }
            return Result.NO;
        });

        functionsSubmit.Add(async () =>
        {
            await SelectManager.Instance.ForceConfirm(Tag.Guardian, ID, Action.MOVE);
            return Result.YES;
        });

        functionsSubmit.Add(async () =>
        {
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            //await CardManager.Instance.GuardianToDrop(guardian, drop);
            return Result.YES;
        });

        functions.AddRange(functionsSelect);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;      // 1つ次に進む
                    break;
                case Result.NO:
                    i--;
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    state = functions;
                    SelectManager.Instance.SingleCancel();
                    break;
                case Result.END:
                    return false;
            }
        }

        return true;
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
                await GetTrigger(Drive.GetCard());
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
            await CardManager.Instance.DriveToDamage(Drive, Damage, Drive.Card);
        }
    }

    /// <summary>
    /// トリガー取得
    /// </summary>
    public async UniTask GetTrigger(Card triggerCard)
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedPowerUpCard = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        switch (triggerCard.Trigger)
        {
            case Card.TriggerType.Critical:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedCriticalUpCard = SelectManager.Instance.GetSelectCard(Tag.Circle, ID);
                    if (selectedCriticalUpCard is null) return Result.NO;
                    selectedCriticalUpCard.AddCritical(1);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Draw:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    await DrawCard(1);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Front:
                break;
            case Card.TriggerType.Heal:
                functions.Add(async () => {
                    if (Damage.Count == 0) return Result.YES;
                    if (Damage.Count < OpponentFighter.Damage.Count) return Result.YES;
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedDamageCard = SelectManager.Instance.GetSelectCard(Tag.Damage, ID);
                    if (selectedDamageCard is null) return Result.NO;
                    await CardManager.Instance.DamageToDrop(Damage, Drop, selectedDamageCard);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Stand:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedStandCard = SelectManager.Instance.GetSelectCard(Tag.Circle, ID);
                    if (selectedStandCard is null) return Result.NO;
                    var selectedStandCircle = selectedStandCard.GetComponentInParent<ICardCircle>();
                    if (selectedStandCircle.R == true) await CardManager.Instance.StandCard(selectedStandCircle);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Over:
                break;
            default:
                break;
        }

        functions.Add(async () => {
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            return Result.YES;
        });
        //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
        functions.Add(async () => {
            selectedPowerUpCard = SelectManager.Instance.GetSelectCard(Tag.Circle, ID);
            if (selectedPowerUpCard is null) return Result.NO;
            selectedPowerUpCard.AddPower(triggerCard.TriggerPower);
            return Result.YES;
        });

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;    // 1つ次に進む
                    break;
                case Result.NO:
                    i -= 1; // 1つ前に戻る
                    break;
                case Result.CANCEL:
                    SelectManager.Instance.SingleCancel();
                    i = 0;  // 最初に戻る
                    break;
                case Result.END:
                    return; // 終了する
            }
        }
    }

    /// <summary>
    /// アタックフェイズ内のエンドステップ
    /// </summary>
    public async UniTask EndStep()
    {
        await UniTask.NextFrame();

        Vanguard.Card.BoostedPower = 0;
        Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
        AnimationManager.Instance.KillSequence();
    }

    /// <summary>
    /// エンドフェイズ
    /// </summary>
    public async UniTask EndPhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();

        Vanguard.Card.Reset();
        Rearguards.ForEach(rear => rear.Card?.Reset());

        Turn++;
    }

    /// <summary>
    /// 指定したカードを退却させる
    /// </summary>
    /// <param name="card">退却させるカード</param>

    public async UniTask RetireCard(Card card)
    {
        await CardManager.Instance.CardToDrop(Drop, card);
    }

    /// <summary>
    /// メインデータ（CardManagerで実行する関数の内容）を受信したときの処理
    /// カードに関する処理が送られている
    /// </summary>
    /// <param name="args">送信元ID、関数名、カードID</param>
    public async UniTask ReceivedData(List<object> args) { await UniTask.NextFrame(); }

    /// <summary>
    /// さまざまな処理（Attackなど）を受信した時の処理
    /// </summary>
    /// <param name="args">送信元ID、処理名、オプション(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args) { await UniTask.NextFrame(); }

    /// <summary>
    /// Stateデータを受信したときの処理
    /// フェイズの管理に使う予定
    /// </summary>
    /// <param name="state">フェイズ名</param>
    public void ReceivedState(string state) { }

    private ICardCircle StringToCircle(string name) => name == "Vanguard" ? (ICardCircle)Vanguard : (ICardCircle)Rearguards.Find(rear => rear.Name == name);
}
