using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Fighter : MonoBehaviour
{
    public FighterID ID;
    private FighterID OpponentID { get; set; }

    public GameObject field { get; private set; }

    public Hand hand { get; private set; }
    public Deck deck { get; private set; }
    public Vanguard vanguard { get; private set; }
    public Drop drop { get; private set; }
    public Damage damage { get; private set; }
    public Drive drive { get; private set; }
    public Guardian guardian { get; private set; }
    public Order order { get; private set; }

    private void Start()
    {
        //子オブジェクトを全て取得する
        //foreach (Transform childTransform in field.transform)
        //{
        //    if (childTransform.tag.Contains("Vanguard"))
        //        vanguard = childTransform.GetComponent<Vanguard>();
        //}
        field = transform.FindWithChildTag(Tag.Field).gameObject;
        hand = transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();
        deck = field.transform.FindWithChildTag(Tag.Deck).GetComponent<Deck>();
        vanguard = field.transform.FindWithChildTag(Tag.Vanguard).GetComponent<Vanguard>();
        drop = field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
        damage = field.transform.FindWithChildTag(Tag.Damage).GetComponent<Damage>();
        drive = field.transform.FindWithChildTag(Tag.Drive).GetComponent<Drive>();
        guardian = field.transform.FindWithChildTag(Tag.Guardian).GetComponent<Guardian>();
        order = field.transform.FindWithChildTag(Tag.Order).GetComponent<Order>();

        OpponentID = FighterID.ONE == ID ? FighterID.TWO : FighterID.ONE;

    }

    public void CreateDeck()
    {
        DeckGenerater.Instance.Generate(deck);
    }

    public async UniTask SetFirstVanguard()
    {
        await CardManager.Instance.DeckToCircle(deck, vanguard, 0);
    }

    public async UniTask Mulligan()
    {
        while (true)
        {
            await UniTask.NextFrame();

            int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));

            if (inputIndex == 0) await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            else if (inputIndex == 1 && SelectManager.Instance.SelectedCount() == 0) return; // Submit入力時
            else break;
        }

        int ToDeckCount = SelectManager.Instance.SelectedCount();
        await SelectManager.Instance.ForceConfirm(Tag.Hand, ID, Action.MOVE);

        await DrawCard(ToDeckCount);

    }

    public async UniTask StandUpVanguard()
    {
        await CardManager.Instance.RotateCard(vanguard.GetCard());
    }

    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Drawする");


        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(deck, hand, 0);
        }

    }

    public async UniTask RidePhase()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        ICardZone selectedEmpty = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
        functions.Add(async () => {
            selectedEmpty = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedEmpty == null) return Result.NO;
            if (selectedEmpty.GetCard().Grade > vanguard.GetCard().Grade + 1) return Result.NO; // グレードのチェック
            var result = await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            if (result != null)
                return Result.YES;
            else
                return Result.NO;
        });
        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functions.Add(async () => {
            (Transform area, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE);
            if (area == null) return Result.NO;
            await CardManager.Instance.HandToField(hand, area.GetComponent<ICardCircle>(), card.GetComponent<Card>());
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
                    SelectManager.Instance.SingleCansel();
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
        ICardZone selectedEmpty = null;
        Transform selectedTransform = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsC = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsM = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            selectedEmpty = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedEmpty != null)
            {
                if (selectedEmpty.GetCard().Grade > vanguard.GetCard().Grade) return Result.NO; // グレードのチェック
                await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                state = functionsC;
                functions.AddRange(functionsC);
                return Result.YES;
            }

            selectedTransform = await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
            if (selectedTransform != null)
            {
                state = functionsM;
                functions.AddRange(functionsM);
                return Result.YES;
            }
            return Result.NO;
        });

        // ここまで共通処理

        // ここから分岐
        // 分岐1
        functionsC.Add(async () =>
        {
            print("C");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsC.Add(async () =>
        {
            (Transform area, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            if (area == null) return Result.NO;
            await CardManager.Instance.HandToField(hand, area.GetComponent<ICardCircle>(), card.GetComponent<Card>());
            return Result.YES;
        });

        // 分岐2
        functionsM.Add(async () =>
        {
            print("M");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsM.Add(async () =>
        {
            (Transform area, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            if (area == null) return Result.NO;
            await CardManager.Instance.RearToRear(selectedTransform.GetComponent<Rearguard>(), area.GetComponent<Rearguard>(), card.GetComponent<Card>());
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
                    SelectManager.Instance.SingleCansel();
                    break;
                case Result.END:
                    return false;
            }
        }

        print("owari");
        return true;

     }

    public async UniTask<bool> AttackPhase()
    {
        await UniTask.NextFrame();
        print("開始");
        Result result = Result.NONE;
        ICardCircle selectedAttackZone = null;
        ICardCircle selectedBoostZone = null;

        int i = 0;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsO = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsT = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            selectedAttackZone = (ICardCircle)await SelectManager.Instance.GetSelect(Tag.Circle, ID); // こちらのサークルを選択したかどうか
            if (selectedAttackZone == null) return Result.NO;

            if (!selectedAttackZone.GetCard().JudgeState(Card.State.Stand)) return Result.NO; // 攻撃可能なカードか判定
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
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            selectedBoostZone = (ICardCircle)await SelectManager.Instance.GetSelect(Tag.Rearguard, ID); // ブーストする
            if (selectedBoostZone != null)
            {
                if (!selectedBoostZone.GetCard().JudgeState(Card.State.Stand)) return Result.NO; // ブースト可能なカードか判定
                if (!selectedBoostZone.IsSameColumn(selectedAttackZone)) return Result.NO;
                if (selectedBoostZone.GetCard().Skill != Card.SkillType.Boost) return Result.NO;
                state = functionsT;
                functions.AddRange(functionsT);
                await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // ブーストするカードを選択する
                return Result.YES;
            }
            var result = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentID, Action.ATTACK); // 相手に攻撃する
            if (result.Item1 != null)
            {
                if (selectedAttackZone.V) await DriveTriggerCheck();
                state = functionsO;
                functions.AddRange(functionsO);
                return Result.YES;
            }
            return Result.NO;
        });

        functionsT.Add(async () =>
        {
            print("T");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsT.Add(async () => {
            (Transform area, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, OpponentID, Action.ATTACK); // 相手に攻撃する
            if (area == null) return Result.NO;
            return Result.YES;
        });

        functions.AddRange(functionsV);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            print($"{ i}, { result}, { SelectManager.Instance.SelectedCount()}, {functions.Count}, {state}");
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;
                    break;
                case Result.NO:
                    i -= 1;
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    state = functionsV;
                    SelectManager.Instance.SingleCansel();
                    break;
                case Result.END:
                    return false;
            }
        }

        return true;

        print("owari");
    }


    public async UniTask DriveTriggerCheck()
    {
        await CardManager.Instance.DeckToDrive(deck, drive);
        if (drive.GetCard().Trigger != Card.TriggerType.None)
            await GetDriveTrigger(drive.GetCard());
        await CardManager.Instance.DriveToHand(drive, hand);
    }

    public async UniTask DamageTriggerCheck()
    {
        await CardManager.Instance.DeckToDrive(deck, drive);
        await CardManager.Instance.DriveToDamage(drive, damage);
    }

    public async UniTask GetDriveTrigger(Card triggerCard)
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        ICardCircle selectedPowerUpCircle = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        switch (triggerCard.Trigger)
        {
            case Card.TriggerType.Critical:
                break;
            case Card.TriggerType.Draw:
                break;
            case Card.TriggerType.Front:
                break;
            case Card.TriggerType.Heal:
                break;
            case Card.TriggerType.Stand:
                break;
            case Card.TriggerType.Over:
                break;
            default:
                break;
        }

        functions.Add(async () => {
            await UniTask.WaitUntil(() => Input.GetButtonDown("Enter"));
            return Result.YES;
        });
        //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
        functions.Add(async () => {
            selectedPowerUpCircle = (ICardCircle)await SelectManager.Instance.GetSelect(Tag.Circle, ID);
            if (selectedPowerUpCircle == null) return Result.NO;
            selectedPowerUpCircle.ChangeCardPower(triggerCard.TriggerPower);
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
                    SelectManager.Instance.SingleCansel();
                    i = 0;  // 最初に戻る
                    break;
                case Result.END:
                    return; // 終了する
            }
        }
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
