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

    //private bool isDraw = true;
    //public Field field;
    //public Graveyard graveyard;

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
        //this.UpdateAsObservable()
        //    .Where(_ => FirstStateController.Instance.firstState == FirstState.Draw);
        //.Subscribe(_ => Debug.Log("draw"));
        /*

        this.UpdateAsObservable()
            .Where(_ => firstManager.firstState.Draw == FirstState.End)
            .Subscribe(_ => Debug.Log("draw"));

        this.UpdateAsObservable()
            .Where(_ => firstManager.firstState.Draw == FirstState.Draw)
            .Where(_ => Input.GetKeyDown(KeyCode.Alpha1))
            .Subscribe(_ => Debug.Log("p button"));
        */
    }

    public void CreateDeck()
    {
        DeckGenerater deckGenerater = GetComponent<DeckGenerater>();

        deckGenerater.Generate(deck);
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

        //while (true)
        //{
        //    inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

        //    if (inputIndex == 0)
        //    {
        //        if (await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE)) return false;
        //    }
        //    else if (inputIndex == 1)
        //    {
        //        SelectManager.Instance.SingleCansel();
        //        return false;
        //    };

        //    await UniTask.NextFrame();
        //}
    }

    public async UniTask StandUpVanguard()
    {
        await CardManager.Instance.RotateCard(vanguard.transform.FindWithChildTag(Tag.Card).GetComponent<Card>());

    }

    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Drawする");


        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(deck, hand, 0);
        }

        /*if (!isDraw)
            return;
        card = deck.Pull(0);
        var sequence = DOTween.Sequence();
        RectTransform transform = card.GetComponent<RectTransform>();
        Image image = card.GetComponent<Image>();

        sequence.Append(transform.DOMove(new Vector3(0, 0, -0.5f), 0.5f)
                        .SetRelative());
        sequence.Join(
            DOTween.ToAlpha(
                    () => image.color,
                    color => image.color = color,
                    0f, // 目標値
                    0.5f // 所要時間
            )
        );
        sequence.OnStart(() => { isDraw = false; });
        sequence.OnComplete(() =>
        {
            hand.Add(card);
            isDraw = true;
        });
        */
        //card.GetComponent<Animator>().SetBool("IsDraw", true);

        //hand.Add(card);
    }

    public async UniTask RidePhase()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functions.Add(async () => await SelectManager.Instance.CanSelect(Tag.Hand, ID));
        functions.Add(async () => {
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
        //inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));


        //if (inputIndex == 0)
        //    functions.Add(() => SelectManager.Instance.NormalSelected(Tag.Hand, ID));
        //else if (inputIndex == 1) return true;

        //while (i < functions.Count)
        //{
        //    bool result = await functions[i]();
        //    if (result) i++;
        //}

        //foreach (Func<Cysharp.Threading.Tasks.UniTask<bool>> func in functions)
        //    await func();




        //int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));

        //if (inputIndex == 0) // Enter入力時
        //{
        //    if (!await SelectManager.Instance.NormalSelected(Tag.Hand, ID)) return false;
        //}
        //else if (inputIndex == 1) return true; // Submit入力時

        //await UniTask.NextFrame();

        //while (true)
        //{
        //    inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

        //    if (inputIndex == 0)
        //    {
        //        if (await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE)) return true;
        //    }
        //    else if (inputIndex == 1)
        //    {
        //        SelectManager.Instance.SingleCansel();
        //        return false;
        //    };

        //    await UniTask.NextFrame();
        //}

    }

    public async UniTask<bool> MainPhase()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
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
            var result = await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            if (result != null)
            {
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

        //Action action = Action.None;

        //async UniTask<bool> Loop2()
        //{
        //    int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

        //    if (inputIndex == 0)
        //    {
        //        if (action == Action.CALL && Result.YES == await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE)) return true;
        //        else if (action == Action.MOVE && Result.YES == await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE)) return true;
        //        else return false;
        //    }
        //    else if (inputIndex == 1)
        //    {
        //        SelectManager.Instance.SingleCansel();
        //        return true;
        //    };
        //    return true;
        //}

        //int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));

        //if (inputIndex == 0) // Enter入力時
        //{
        //    if (Result.YES == await SelectManager.Instance.NormalSelected(Tag.Hand, ID))
        //        action = Action.CALL;
        //    else if (Result.YES == await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID))
        //        action = Action.MOVE;
        //    else return false;
        //}
        //else if (inputIndex == 1) return true; // Submit入力時
        //await UniTask.NextFrame();

        //while (!await Loop2())
        //{
        //    await UniTask.NextFrame();
        //}

        //return false;

    }

    public async UniTask<bool> AttackPhase()
    {
        await UniTask.NextFrame();
        print("開始");
        Result result = Result.NONE;
        Transform selectedAttackTransform = null;
        Transform selectedBoostTransform = null;

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
            state = functionsV2;
            functions.AddRange(functionsV2);
            selectedAttackTransform = await SelectManager.Instance.NormalSelected(Tag.Circle, ID);
                if (selectedAttackTransform != null)
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
            var result = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, OpponentID, Action.ATTACK);
            if (result.Item1 != null)
            {
                state = functionsO;
                functions.AddRange(functionsO);
                return Result.YES;
            }
            selectedBoostTransform = await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
            if (selectedBoostTransform != null)
            {
                state = functionsT;
                functions.AddRange(functionsT);
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
            (Transform area, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, OpponentID, Action.ATTACK);
            if (area == null) return Result.NO;
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
                    i++;
                    break;
                case Result.NO:
                    i -= 1;
                    break;
                case Result.CANCEL:
                    print(functions.Count);
                    print(state.Count);
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    print(functions.Count);
                    print(state.Count);
                    state = functionsV;
                    SelectManager.Instance.SingleCansel();
                    break;
                case Result.END:
                    return false;
            }
        }

        return true;

        print("owari");


        //int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
        //if (inputIndex == 0) // Enter入力時
        //{
        //    if (Result.NO == await SelectManager.Instance.NormalSelected(Tag.Circle, ID)) return 0;
        //}
        //else if (inputIndex == 1) return -1; // Submit入力時

        //await UniTask.NextFrame();

        //while (true)
        //{
        //    inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

        //    if (inputIndex == 0)
        //    {
        //        if (Result.YES == await SelectManager.Instance.NormalConfirm(Tag.Vanguard, OpponentID, Action.ATTACK)) return 1;
        //    }
        //    else if (inputIndex == 1)
        //    {
        //        SelectManager.Instance.SingleCansel();
        //        return 0;
        //    }

        //    await UniTask.NextFrame();
        //}
    }


    public async UniTask DriveTriggerCheck()
    {
        await CardManager.Instance.DeckToDrive(deck, drive);
        await CardManager.Instance.DriveToHand(drive, hand);
    }

    public async UniTask DamageTriggerCheck()
    {
        await CardManager.Instance.DeckToDrive(deck, drive);
        await CardManager.Instance.DriveToDamage(drive, damage);
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
