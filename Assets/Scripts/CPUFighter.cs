using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.InputSystem;
using System.Reflection;
public class CPUFighter : MonoBehaviour
{
    [SerializeField]
    private PhotonController photonController;

    [field: SerializeField]
    public FighterID ID { get; private set; }

    [SerializeField]
    public int ActorNumber;

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    private IFighter OpponentFighter { get; set; }

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

    public Dictionary<int, Card> CardDic;
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
        DeckGenerater.Instance.Generate(Deck, ActorNumber);
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
        //while (true)
        //{
        //    await UniTask.NextFrame();

        //    int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));

        //    if (inputIndex == 0) await SelectManager.Instance.NormalSelected(Tag.Hand, ID); // Enter入力時
        //    else if (inputIndex == 1 && SelectManager.Instance.SelectedCount == 0) return;  // Submit入力時
        //    else break;
        //}

        //int ToDeckCount = SelectManager.Instance.SelectedCount;

        //Action<string, Card> endAction = (funcname, card) => photonController.SendData(funcname, card); // データの送信処理を委譲する

        //await SelectManager.Instance.ForceConfirm(Tag.Deck, ID, Action.MOVE, endAction);

        //await DrawCard(ToDeckCount);
    }

    /// <summary>
    /// カードをデッキから指定枚数引く
    /// </summary>
    /// <param name="count">引く枚数</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Drawする");


        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            photonController.SendData("DeckToHand", card: Deck.GetCard(0));
            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
            Debug.Log("trueです");
            //await CardManager.Instance.DeckToHand(Deck, Hand, 0);
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
        //await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// ドローフェイズ
    /// </summary>
    public async UniTask DrawPhase()
    {
        //await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await DrawCard(1);
    }

    /// <summary>
    /// ライドフェイズ
    /// </summary>
    //public async UniTask RidePhase()
    //{
    //    await UniTask.NextFrame();

    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

    //    functions.Add(async () => {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        return resultInt.ToEnum("YES", "END");
    //    });
    //    //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
    //    functions.Add(async () => {
    //        Card selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
    //        if (selectedCard == null) return Result.NO;
    //        if (selectedCard.Grade > Vanguard.Card.Grade + 1) return Result.NO; // グレードのチェック
    //        var result = await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
    //        if (result != null)
    //            return Result.YES;
    //        else
    //            return Result.NO;
    //    });
    //    functions.Add(async () => {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functions.Add(async () => {
    //        (ICardCircle vanguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE);
    //        if (vanguard == null) return Result.NO;
    //        //Card removedCard = await CardManager.Instance.HandToCircle(Hand, circle, card.GetComponent<Card>());
    //        Card removedCard = vanguard.Card; // カードが既に存在する場合はソウルに移動させる
    //        photonController.SendData("HandToVanguard", card.GetComponent<Card>());
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        Debug.Log(removedCard);
    //        if (removedCard != null)
    //        {
    //            Debug.Log("ソウルに送るよ");
    //            photonController.SendData("VanguardToSoul", removedCard);
    //            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        }
    //        return Result.YES;
    //    });

    //    int i = 0;
    //    Result result = Result.NONE;
    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;    // 1つ次に進む
    //                break;
    //            case Result.NO:
    //                i -= 1; // 1つ前に戻る
    //                break;
    //            case Result.CANCEL:
    //                SelectManager.Instance.SingleCancel();
    //                i = 0;  // 最初に戻る
    //                break;
    //            case Result.END:
    //                return; // 終了する
    //        }
    //    }

    //    print("owari");

    //}

    ///// <summary>
    ///// メインフェイズ
    ///// </summary>
    //public async UniTask<bool> MainPhase()
    //{
    //    await UniTask.NextFrame();
    //    Card selectedCard = null;
    //    Debug.Log("メイン開始！");
    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsC = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsM = new List<Func<UniTask<Result>>>();

    //    var state = functionsV;

    //    functionsV.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        return resultInt.ToEnum("YES", "END");
    //    });
    //    functionsV.Add(async () =>
    //    {
    //        selectedCard = await SelectManager.Instance.GetSelect(Tag.None, ID);
    //        //ICardZone selectedZone = await SelectManager.Instance.GetZone(Tag.None, ID);
    //        if (selectedCard == null) return Result.NO;
    //        print(selectedCard);
    //        Transform action = TextManager.Instance.SetActionList(selectedCard);
    //        SelectManager.Instance.SetActionObj(action);
    //        state = functionsV2;
    //        functions.AddRange(functionsV2);
    //        return Result.YES;
    //    });

    //    functionsV2.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsV2.Add(async () =>
    //    {
    //        string act = SelectManager.Instance.ActionConfirm(ActorNumber);
    //        //return Result.YES;

    //        if (act == "Call")
    //        {
    //            selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
    //            if (selectedCard != null)
    //            {
    //                //if (selectedCard.Grade > Vanguard.Card.Grade) return Result.RESTART; // グレードのチェック
    //                await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
    //                state = functionsC;
    //                functions.AddRange(functionsC);
    //                return Result.YES;
    //            }

    //            selectedCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
    //            if (selectedCard != null)
    //            {
    //                await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
    //                state = functionsM;
    //                functions.AddRange(functionsM);
    //                return Result.YES;
    //            }
    //        }
    //        else if (act == "Skill")
    //        {
    //            print("スキル発動");
    //            await SkillManager.Instance.StartActivate(selectedCard, 0);
    //        }

    //        return Result.RESTART;

    //    });

    //    // ここまで共通処理

    //    // ここから分岐
    //    // 分岐1
    //    functionsC.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsC.Add(async () =>
    //    {
    //        (ICardCircle rearguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
    //        if (rearguard == null) return Result.NO;
    //        Card removedCard = rearguard.Card; // カードが既に存在する場合はソウルに移動させる
    //        photonController.SendData("HandToRearguard" + rearguard.ID, card.GetComponent<Card>());
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        if (removedCard != null)
    //        {
    //            photonController.SendData(rearguard.Name + "ToDrop", removedCard);
    //            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        }
    //        //Card removedCard = await CardManager.Instance.HandToCircle(Hand, circle, card.GetComponent<Card>());
    //        //if (removedCard != null) await CardManager.Instance.CardToDrop(Drop, removedCard);
    //        return Result.YES;
    //    });

    //    // 分岐2
    //    functionsM.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsM.Add(async () =>
    //    {
    //        //Card targetCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
    //        Rearguard targetZone = (Rearguard)await SelectManager.Instance.GetZone(Tag.Rearguard, ID);
    //        if (targetZone == null) return Result.NO;
    //        Rearguard selectedRearguard = selectedCard.transform.GetComponentInParent<Rearguard>();
    //        if (!selectedRearguard.IsSameColumn(targetZone)) return Result.NO;
    //        (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
    //        photonController.SendData(selectedRearguard.Name + "To" + circle.Name, card.GetComponent<Card>());
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        //await CardManager.Instance.CircleToCircle(selectedRearguard, (Rearguard)circle, card.GetComponent<Card>());
    //        return Result.YES;
    //    });

    //    functions.AddRange(functionsV);

    //    int i = 0;
    //    Result result = Result.NONE;
    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;      // 1つ次に進む
    //                break;
    //            case Result.NO:
    //                i -= 1;   // 1つ前に戻る
    //                break;
    //            case Result.CANCEL:
    //                functions.RemoveRange(functions.Count - state.Count, state.Count);
    //                i -= state.Count; // 直前の分岐点に戻る
    //                state = functionsV;
    //                SelectManager.Instance.SingleCancel();
    //                SelectManager.Instance.ActionConfirm(ActorNumber);
    //                break;
    //            case Result.RESTART:
    //                i = 0;
    //                functions.Clear();
    //                functions.AddRange(functionsV);
    //                break;
    //            case Result.END:
    //                return false;
    //        }
    //    }

    //    print("owari");
    //    return true;

    //}

    ///// <summary>
    ///// アタックフェイズ内のアタックステップ
    ///// </summary>
    //public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    //{
    //    await UniTask.NextFrame();
    //    print("開始");

    //    //if (Turn == 1) return true;

    //    Result result = Result.NONE;
    //    ICardCircle selectedAttackZone = null;
    //    ICardCircle selectedBoostZone = null;
    //    ICardCircle selectedTargetZone = null;

    //    int i = 0;

    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsB = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV3 = new List<Func<UniTask<Result>>>();

    //    var state = functionsV;

    //    functionsV.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        return resultInt.ToEnum("YES", "END");
    //    });
    //    functionsV.Add(async () =>
    //    {
    //        Card selectedAttackCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID); // こちらのサークルを選択したかどうか
    //        if (selectedAttackCard == null) return Result.NO;
    //        selectedAttackZone = selectedAttackCard.GetComponentInParent<ICardCircle>();

    //        //if (!selectedAttackZone.Card.JudgeStep(Card.State.Stand)) return Result.NO; // 攻撃可能なカードか判定
    //        if (!selectedAttackZone.Front) return Result.NO;
    //        state = functionsV2;
    //        functions.AddRange(functionsV2);
    //        var result = await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // 攻撃するカードを選択する
    //        if (result != null)
    //            return Result.YES;
    //        else
    //            return Result.NO;
    //    });


    //    functionsV2.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsV2.Add(async () =>
    //    {
    //        Card selectedBoostCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID); // ブーストする
    //        if (selectedBoostCard != null)
    //        {
    //            selectedBoostZone = selectedBoostCard.GetComponentInParent<Rearguard>();
    //            if (!selectedBoostZone.Card.JudgeState(Card.State.Stand)) return Result.NO; // ブースト可能なカードか判定
    //            if (!selectedBoostZone.IsSameColumn(selectedAttackZone)) return Result.NO;
    //            if (selectedBoostZone.Card.Ability != Card.AbilityType.Boost) return Result.NO;
    //            await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // ブーストするカードを選択する
    //            //selectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
    //            photonController.SendGeneralData("Boost", new object[2] { selectedBoostZone.Name, selectedAttackZone.Name });
    //            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //            ActionManager.Instance.ActionHistory.Add(new ActionData("Boost", ID, selectedBoostZone.Card, selectedBoostZone, selectedAttackZone));
    //            state = functionsB;
    //            functions.AddRange(functionsB);
    //            return Result.YES;
    //        }
    //        (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
    //        if (circle == null) return Result.NO;
    //        selectedTargetZone = circle;
    //        photonController.SendGeneralData("Attack", new object[2] { selectedAttackZone.Name, selectedTargetZone.Name });
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
    //        state = functionsV3;
    //        functions.AddRange(functionsV3);
    //        return Result.YES;
    //    });

    //    functionsB.Add(async () =>
    //    {
    //        print("T");
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsB.Add(async () => {
    //        (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
    //        if (circle == null) return Result.NO;
    //        selectedTargetZone = circle;
    //        photonController.SendGeneralData("Attack", new object[2] { selectedAttackZone.Name, selectedTargetZone.Name });
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
    //        state = functionsV3;
    //        functions.AddRange(functionsV3);
    //        return Result.YES;
    //    });

    //    functionsV3.Add(async () =>
    //    {
    //        print("V3");
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        print("V3Enter");
    //        await CardManager.Instance.RestCard(selectedAttackZone);
    //        if (selectedBoostZone != null) await CardManager.Instance.RestCard(selectedBoostZone);
    //        //if (selectedAttackZone.V) await DriveTriggerCheck();
    //        return Result.YES;
    //    });

    //    functions.AddRange(functionsV);

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        print($"{ i}, { result}, { SelectManager.Instance.SelectedCount}, {functions.Count}");
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;
    //                break;
    //            case Result.NO:
    //                i--;
    //                break;
    //            case Result.CANCEL:
    //                functions.RemoveRange(functions.Count - state.Count, state.Count);
    //                i -= state.Count;
    //                state = functionsV;
    //                SelectManager.Instance.SingleCancel();
    //                break;
    //            case Result.END:
    //                return (null, null);
    //        }
    //    }

    //    return (selectedAttackZone, selectedTargetZone);
    //}

    ///// <summary>
    ///// アタックフェイズ内のガードステップ
    ///// </summary>
    //public async UniTask<bool> GuardStep()
    //{
    //    await UniTask.NextFrame();
    //    Result result = Result.NONE;
    //    int i = 0;
    //    Card selectedCard = null;

    //    List<Func<UniTask<Result>>> functionsSelect = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsSubmit = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

    //    var state = functions;


    //    functionsSelect.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        if (resultInt == 1)
    //        {
    //            state = functionsSubmit;
    //            functions.Pop();
    //            functions.AddRange(functionsSubmit);
    //        }
    //        return Result.YES;
    //    });

    //    functionsSelect.Add(async () =>
    //    {
    //        selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
    //        if (selectedCard != null)
    //        {
    //            await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
    //            functions.AddRange(functionsSelect);
    //            return Result.YES;
    //        }
    //        return Result.NO;
    //    });

    //    functionsSubmit.Add(async () =>
    //    {
    //        print(SelectManager.Instance.SelectedCount);
    //        await SelectManager.Instance.ForceConfirm(Tag.Guardian, ID, Action.MOVE);
    //        return Result.YES;
    //    });

    //    functionsSubmit.Add(async () =>
    //    {
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        //await CardManager.Instance.GuardianToDrop(guardian, drop);
    //        return Result.YES;
    //    });

    //    functions.AddRange(functionsSelect);

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;      // 1つ次に進む
    //                break;
    //            case Result.NO:
    //                i--;
    //                break;
    //            case Result.CANCEL:
    //                functions.RemoveRange(functions.Count - state.Count, state.Count);
    //                i -= state.Count;
    //                state = functions;
    //                SelectManager.Instance.SingleCancel();
    //                break;
    //            case Result.END:
    //                return false;
    //        }
    //    }

    //    print("owari");
    //    return true;
    //}

    ///// <summary>
    ///// ドライブトリガーチェック
    ///// </summary>
    //public async UniTask DriveTriggerCheck(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        await CardManager.Instance.DeckToDrive(Deck, Drive);
    //        if (Drive.GetCard().Trigger != Card.TriggerType.None)
    //            await GetTrigger(Drive.GetCard());
    //        await CardManager.Instance.DriveToHand(Drive, Hand);
    //    }

    //}

    ///// <summary>
    ///// ダメージトリガーチェック
    ///// </summary>
    //public async UniTask DamageTriggerCheck(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        await CardManager.Instance.DeckToDrive(Deck, Drive);
    //        await CardManager.Instance.DriveToDamage(Drive, Damage);
    //    }
    //}

    ///// <summary>
    ///// トリガー取得
    ///// </summary>
    //public async UniTask GetTrigger(Card triggerCard)
    //{
    //    await UniTask.NextFrame();
    //    Result result = Result.NONE;
    //    int i = 0;
    //    Card selectedPowerUpCard = null;

    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

    //    switch (triggerCard.Trigger)
    //    {
    //        case Card.TriggerType.Critical:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedCriticalUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //                if (selectedCriticalUpCard == null) return Result.NO;
    //                selectedCriticalUpCard.AddCritical(1);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Draw:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                await DrawCard(1);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Front:
    //            break;
    //        case Card.TriggerType.Heal:
    //            functions.Add(async () => {
    //                if (Damage.Count == 0) return Result.YES;
    //                if (Damage.Count < OpponentFighter.Damage.Count) return Result.YES;
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedDamageCard = await SelectManager.Instance.GetSelect(Tag.Damage, ID);
    //                if (selectedDamageCard == null) return Result.NO;
    //                await CardManager.Instance.DamageToDrop(Damage, Drop, selectedDamageCard);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Stand:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedStandCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //                if (selectedStandCard == null) return Result.NO;
    //                var selectedStandCircle = selectedStandCard.GetComponentInParent<ICardCircle>();
    //                if (selectedStandCircle.R == true) await CardManager.Instance.StandCard(selectedStandCircle);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Over:
    //            break;
    //        default:
    //            break;
    //    }

    //    functions.Add(async () => {
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        return Result.YES;
    //    });
    //    //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
    //    functions.Add(async () => {
    //        selectedPowerUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //        if (selectedPowerUpCard == null) return Result.NO;
    //        selectedPowerUpCard.AddPower(triggerCard.TriggerPower);
    //        return Result.YES;
    //    });

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;    // 1つ次に進む
    //                break;
    //            case Result.NO:
    //                i -= 1; // 1つ前に戻る
    //                break;
    //            case Result.CANCEL:
    //                SelectManager.Instance.SingleCancel();
    //                i = 0;  // 最初に戻る
    //                break;
    //            case Result.END:
    //                return; // 終了する
    //        }
    //    }
    //}

    ///// <summary>
    ///// アタックフェイズ内のエンドステップ
    ///// </summary>
    //public async UniTask EndStep()
    //{
    //    Vanguard.Card.BoostedPower = 0;
    //    Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
    //}

    ///// <summary>
    ///// エンドフェイズ
    ///// </summary>
    //public async UniTask EndPhase()
    //{
    //    await UniTask.NextFrame();

    //    Vanguard.Card.Reset();
    //    Rearguards.ForEach(rear => rear.Card?.Reset());

    //    Turn++;
    //}

    ///// <summary>
    ///// 指定したサークルのカードを退却させる
    ///// </summary>
    ///// <param name="circle">退却させるサークル</param>

    //public async UniTask RetireCard(ICardCircle circle)
    //{
    //    await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    //}

    /// <summary>
    /// メインデータ（CardManagerで実行する関数の内容）を受信したときの処理
    /// カードに関する処理が送られている
    /// </summary>
    /// <param name="args">送信元ID、関数名、カードID</param>
    public async UniTask ReceivedData(List<object> args)
    {
        int actorNumber = (int)args[0];
        string funcname = ((string)args[1]);
        int toIndex = funcname.IndexOf("To");
        string source = funcname.Substring(0, toIndex);
        string target = funcname.Substring(toIndex + 2, funcname.Length - toIndex - 2);
        Debug.Log(ActorNumber);
        Debug.Log(args[0]);
        Debug.Log($"{source} to {target}");

        Debug.Log(CardDic[(int)args[2]]);

        funcname = funcname.Replace("Vanguard", "Circle");
        Debug.Log(funcname);

        object sourceObj = null, targetObj = null;
        Type type = this.GetType();
        if (source.Contains("Rearguard"))
        {
            Debug.Log(source.Substring(source.Length - 2));
            var rearguard = Rearguards.Find(rear => rear.ID.ToString() == source.Substring(source.Length - 2));
            Debug.Log(rearguard);
            sourceObj = rearguard;
            funcname = "CircleTo" + funcname.Substring(funcname.IndexOf("To") + 2, funcname.Length - funcname.IndexOf("To") - 2);
        }
        if (target.Contains("Rearguard"))
        {
            Debug.Log(target.Substring(target.Length - 2));
            var rearguard = Rearguards.Find(rear => rear.ID.ToString() == target.Substring(target.Length - 2));
            Debug.Log(rearguard);
            targetObj = rearguard;
            funcname = funcname.Substring(0, funcname.IndexOf("To")) + "ToCircle";
        }

        Debug.Log($"{source} to {target}");
        Debug.Log(funcname);

        if (sourceObj == null) sourceObj = type?.GetProperty(source)?.GetValue(this);
        if (targetObj == null) targetObj = type?.GetProperty(target)?.GetValue(this);

        if (sourceObj == null || targetObj == null) Debug.LogError("Nullエラー");
        Debug.Log($"{sourceObj} to {targetObj}");


        MethodInfo method = CardManager.Instance.GetType().GetMethod(funcname);
        if (method == null) Debug.LogError("Nullエラー");
        object[] args2 =
        {
            sourceObj,
            targetObj,
            CardDic[(int)args[2]],
        };
        await (UniTask)method.Invoke(CardManager.Instance, args2);

        print("終わったよ");
        NextController.Instance.SetProcessNext(actorNumber, true);
        //foreach (var arg in args)
        //{
        //    Debug.Log(arg);
        //}
    }

    /// <summary>
    /// さまざまな処理（Attackなど）を受信した時の処理
    /// </summary>
    /// <param name="args">送信元ID、処理名、オプション(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args)
    {
        int actorNumber = (int)args[0];
        string type = ((string)args[1]);
        object[] options = (object[])args[2];
        if (type == "Attack")
        {
            print($"{options[0]} から {options[1]} に攻撃した！");
        }
        else if (type == "Boost")
        {
            print($"{options[0]} が {options[1]} をブーストした！");
        }

        print("終わったよ");
        NextController.Instance.SetProcessNext(actorNumber, true);
    }

    /// <summary>
    /// Stateデータを受信したときの処理
    /// フェイズの管理に使う予定
    /// </summary>
    /// <param name="state">フェイズ名</param>
    public void ReceivedState(string state)
    {
        TextManager.Instance.SetPhaseText(state);
    }
}
