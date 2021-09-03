using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Fighter : MonoBehaviour
{
    public FighterID ID;

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

    public async UniTask<bool> RidePhase()
    {
        int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));

        if (inputIndex == 0) // Enter入力時
        {
            if (!await SelectManager.Instance.SingleSelected(Tag.Hand, ID)) return false;
        }
        else if (inputIndex == 1) return true; // Submit入力時

        await UniTask.NextFrame();

        while (true)
        {
            inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

            if (inputIndex == 0)
            {
                if (await SelectManager.Instance.SingleConfirm(Tag.Vanguard, ID, Action.MOVE)) return true;
            }
            else if (inputIndex == 1)
            {
                SelectManager.Instance.SingleCansel();
                return false;
            };

            await UniTask.NextFrame();
        }

    }

    public async UniTask<bool> MainPhase()
    {
        print("メイン開始");

        Action action = Action.None;

        async UniTask<bool> Loop2()
        {
            int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

            if (inputIndex == 0)
            {
                if (action == Action.CALL && await SelectManager.Instance.SingleConfirm(Tag.Rearguard, ID, Action.MOVE)) return true;
                else if (action == Action.MOVE && await SelectManager.Instance.SingleConfirm(Tag.Rearguard, ID, Action.MOVE)) return true;
                else return false;
            }
            else if (inputIndex == 1)
            {
                SelectManager.Instance.SingleCansel();
                return true;
            };
            return true;
        }

        int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));

        if (inputIndex == 0) // Enter入力時
        {
            if (await SelectManager.Instance.SingleSelected(Tag.Hand, ID))
                action = Action.CALL;
            else if (await SelectManager.Instance.SingleSelected(Tag.Rearguard, ID))
                action = Action.MOVE;
            else return false;
        }
        else if (inputIndex == 1) return true; // Submit入力時
        await UniTask.NextFrame();

        while (!await Loop2())
        {
            await UniTask.NextFrame();
        }

        return false;

    }

    public async UniTask<int> AttackPhase()
    {
        int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
        if (inputIndex == 0) // Enter入力時
        {
            if (!await SelectManager.Instance.SingleSelected(Tag.Circle, ID)) return 0;
        }
        else if (inputIndex == 1) return -1; // Submit入力時

        await UniTask.NextFrame();

        while (true)
        {
            inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Cancel")));

            if (inputIndex == 0)
            {
                if (await SelectManager.Instance.SingleConfirm(Tag.Vanguard, FighterID.TWO, Action.ATTACK)) return 1;
            }
            else if (inputIndex == 1)
            {
                SelectManager.Instance.SingleCansel();
                return 0;
            }

            await UniTask.NextFrame();
        }
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
