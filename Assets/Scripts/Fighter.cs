using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class Fighter : MonoBehaviour
{
    [SerializeField]
    private PlayerInput input;

    [field: SerializeField]
    public FighterID ID { get; private set; }
    [field: SerializeField]
    private int Turn { get; set; } = 1;

    private Fighter OpponentFighter { get; set; }

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

    private void Start()
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
                                    .Select(obj => obj.GetComponent<Fighter>())
                                    .First(fighter => fighter.ID != ID);
    }

    public void CreateDeck()
    {
        DeckGenerater.Instance.Generate(Deck, ID);
    }

    public async UniTask SetFirstVanguard()
    {
        await CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    public async UniTask Mulligan()
    {
        while (true)
        {
            await UniTask.NextFrame();

            int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));

            if (inputIndex == 0) await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            else if (inputIndex == 1 && SelectManager.Instance.SelectedCount == 0) return; // Submit入力時
            else break;
        }

        int ToDeckCount = SelectManager.Instance.SelectedCount;
        await SelectManager.Instance.ForceConfirm(Tag.Deck, ID, Action.MOVE);

        await DrawCard(ToDeckCount);

    }

    public async UniTask StandUpVanguard()
    {
        var card = Vanguard.Card;
        card.SetState(Card.State.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard);
        print("standup");
    }

    public async UniTask StandPhase()
    {
        print("stand");
        await UniTask.NextFrame();
        await UniTask.WaitUntil(() => input.GetDown("Enter"));
        print("enter");
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Drawする");


        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(Deck, Hand, 0);
        }

    }

    public async UniTask RidePhase()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedCard = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
        functions.Add(async () => {
            selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedCard == null) return Result.NO;
            if (selectedCard.Grade > Vanguard.Card.Grade + 1) return Result.NO; // グレードのチェック
            var result = await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            if (result != null)
                return Result.YES;
            else
                return Result.NO;
        });
        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functions.Add(async () => {
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE);
            if (circle == null) return Result.NO;
            Card removedCard = await CardManager.Instance.HandToField(Hand, circle, card.GetComponent<Card>());
            if (removedCard != null) await CardManager.Instance.CardToSoul(Soul, removedCard);
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

        print("owari");

    }

    public async UniTask<bool> MainPhase()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedCard = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsC = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsM = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            selectedCard = await SelectManager.Instance.GetSelect(Tag.None, ID);
            //ICardZone selectedZone = await SelectManager.Instance.GetZone(Tag.None, ID);
            if (selectedCard == null) return Result.NO;
            print(selectedCard);
            Transform action = TextManager.Instance.SetActionList(selectedCard);
            SelectManager.Instance.SetActionObj(action);
            state = functionsV2;
            functions.AddRange(functionsV2);
            return Result.YES;
        });

        functionsV2.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            string act = SelectManager.Instance.ActionConfirm();
            //return Result.YES;

            if (act == "Call")
            {
                selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
                if (selectedCard != null)
                {
                    //if (selectedCard.Grade > Vanguard.Card.Grade) return Result.RESTART; // グレードのチェック
                    await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                    state = functionsC;
                    functions.AddRange(functionsC);
                    return Result.YES;
                }


                selectedCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
                if (selectedCard != null)
                {
                    await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
                    state = functionsM;
                    functions.AddRange(functionsM);
                    return Result.YES;
                }
            }
            else if (act == "Skill")
            {
                print("スキル発動");
                await SkillManager.Instance.StartActivate(selectedCard, 0);
            }

            return Result.RESTART;

        });

        // ここまで共通処理

        // ここから分岐
        // 分岐1
        functionsC.Add(async () =>
        {
            print("C");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsC.Add(async () =>
        {
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            if (circle == null) return Result.NO;
            Card removedCard = await CardManager.Instance.HandToField(Hand, circle, card.GetComponent<Card>());
            if (removedCard != null) await CardManager.Instance.CardToDrop(Drop, removedCard);
            return Result.YES;
        });

        // 分岐2
        functionsM.Add(async () =>
        {
            print("M");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsM.Add(async () =>
        {
            //Card targetCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
            Rearguard targetZone = (Rearguard)await SelectManager.Instance.GetZone(Tag.Rearguard, ID);
            if (targetZone == null) return Result.NO;
            Rearguard selectedRearguard = selectedCard.transform.GetComponentInParent<Rearguard>();
            if (!selectedRearguard.IsSameColumn(targetZone)) return Result.NO;
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            await CardManager.Instance.RearToRear(selectedRearguard, (Rearguard)circle, card.GetComponent<Card>());
            return Result.YES;
        });

        functions.AddRange(functionsV);

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
                    SelectManager.Instance.ActionConfirm();
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

    public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    {
        await UniTask.NextFrame();
        print("開始");

        //if (Turn == 1) return true;

        Result result = Result.NONE;
        ICardCircle selectedAttackZone = null;
        ICardCircle selectedBoostZone = null;
        ICardCircle selectedTargetZone = null;

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
            Card selectedAttackCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID); // こちらのサークルを選択したかどうか
            if (selectedAttackCard == null) return Result.NO;
            selectedAttackZone = selectedAttackCard.GetComponentInParent<ICardCircle>();

            //if (!selectedAttackZone.Card.JudgeState(Card.State.Stand)) return Result.NO; // 攻撃可能なカードか判定
            if (!selectedAttackZone.Front) return Result.NO;
            state = functionsV2;
            functions.AddRange(functionsV2);
            var result = await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // 攻撃するカードを選択する
                if (result != null)
                    return Result.YES;
                else
                    return Result.NO;
        });


        functionsV2.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            Card selectedBoostCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID); // ブーストする
            if (selectedBoostCard != null)
            {
                selectedBoostZone = selectedBoostCard.GetComponentInParent<Rearguard>();
                if (!selectedBoostZone.Card.JudgeState(Card.State.Stand)) return Result.NO; // ブースト可能なカードか判定
                if (!selectedBoostZone.IsSameColumn(selectedAttackZone)) return Result.NO;
                if (selectedBoostZone.Card.Ability != Card.AbilityType.Boost) return Result.NO;
                await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // ブーストするカードを選択する
                selectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
                ActionManager.Instance.ActionHistory.Add(new ActionData("Boost", ID, selectedBoostZone.Card, selectedBoostZone, selectedAttackZone));
                state = functionsB;
                functions.AddRange(functionsB);
                return Result.YES;
            }
            var result = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
            if (result.Item1 == null) return Result.NO;
            selectedTargetZone = result.Item1;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
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
            var result = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // 相手に攻撃する
            if (result.Item1 == null) return Result.NO;
            selectedTargetZone = result.Item1;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
            state = functionsV3;
            functions.AddRange(functionsV3);
            return Result.YES;
        });

        functionsV3.Add(async () =>
        {
            print("V3");
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            print("V3Enter");
            await CardManager.Instance.RestCard(selectedAttackZone);
            if (selectedBoostZone != null) await CardManager.Instance.RestCard(selectedBoostZone);
            //if (selectedAttackZone.V) await DriveTriggerCheck();
            return Result.YES;
        });

        functions.AddRange(functionsV);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            print($"{ i}, { result}, { SelectManager.Instance.SelectedCount}, {functions.Count}");
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

        return (selectedAttackZone, selectedTargetZone);

        print("owari");
    }

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
            selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedCard != null)
            {
                await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                functions.AddRange(functionsSelect);
                return Result.YES;
            }
            return Result.NO;
        });

        functionsSubmit.Add(async () =>
        {
            print(SelectManager.Instance.SelectedCount);
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

        print("owari");
        return true;
    }


    public async UniTask DriveTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive);
            if (Drive.GetCard().Trigger != Card.TriggerType.None)
                await GetTrigger(Drive.GetCard());
            await CardManager.Instance.DriveToHand(Drive, Hand);
        }

    }

    public async UniTask DamageTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive);
            await CardManager.Instance.DriveToDamage(Drive, Damage);
        }
    }

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
                    Card selectedCriticalUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
                    if (selectedCriticalUpCard == null) return Result.NO;
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
                    Card selectedDamageCard = await SelectManager.Instance.GetSelect(Tag.Damage, ID);
                    if (selectedDamageCard == null) return Result.NO;
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
                    Card selectedStandCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
                    if (selectedStandCard == null) return Result.NO;
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
            selectedPowerUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
            if (selectedPowerUpCard == null) return Result.NO;
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

    public async UniTask EndStep()
    {
        Vanguard.Card.BoostedPower = 0;
        Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
    }

    public async UniTask EndPhase()
    {
        await UniTask.NextFrame();

        Vanguard.Card.Reset();
        Rearguards.ForEach(rear => rear.Card?.Reset());

        Turn++;
    }


    public async UniTask RetireCard(ICardCircle circle)
    {
        await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    }

    //public IEnumerator Attack()
    //{
    //    SelectManager.Instance.
    //}

    /*
    public void onDamage(int _at)
    {
        hp -= _at;
        if (hp < 0)
            hp = 0;
        Debug.Log(hp);
    }
    public void Draw()
    {
        Card card = deck.Pull(0);
        hand.Add(card);
    }

    public void StandbyPhaseAction()
    {
        Card card = hand.Pull(0);
        field.Add(card);

    }

    public void ButtlePhaseAction(Player _enemyPlayer)
    {
        // アタックカードを選ぶ
        Card card = SelectAttacker();
        if (card == null)
            return;
        // 相手フィールドにカードがあればカードを攻撃
        if (_enemyPlayer.field.cardList.Count > 0)
        {
            // 敵のカードを取得して攻撃
            Card enemyCard = SelectTarget(_enemyPlayer.field);
            card.Attack(enemyCard);
        }
        else
        {
            // なければプレイヤーを攻撃
            card.Attack(_enemyPlayer);
        }
    }

    public void CheckFieldCardHP()
    {
        for(int i = 0; i < field.cardList.Count; i++)
        {
            Card card = field.cardList[i];
            if (card.hp <= 0)
            {
                SendGraveyard(card);
            }
        }
    }

    void SendGraveyard(Card _card)
    {
        field.Pull(_card);
        graveyard.Add(_card);
    }

    Card SelectAttacker()
    {
        return field.Get(0);

    }

    Card SelectTarget(Field enemyField)
    {
        return enemyField.Get(0);

    }
    */
}
