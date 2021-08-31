using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject field;
    public Deck deck;
    public Hand hand;
    //private Card card;
    private Vanguard vanguard;



    //private bool isDraw = true;
    //public Field field;
    //public Graveyard graveyard;

    private void Start()
    {
        //子オブジェクトを全て取得する
        foreach (Transform childTransform in field.transform)
        {
            if (childTransform.tag.Contains("Vanguard"))
                vanguard = childTransform.GetComponent<Vanguard>();
        }

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

    public IEnumerator SetFirstVanguard()
    {
        yield return StartCoroutine(CardManager.Instance.DeckToCircle(deck, vanguard, 0));
    }

    public IEnumerator StandUpVanguard()
    {
        yield return StartCoroutine(CardManager.Instance.RotateCard(vanguard.transform.Find("Card0").GetComponent<Card>()));

    }

    public IEnumerator DrawCard()
    {
        //Debug.Log("Drawする");


        //CardManager.DeckToHand(deck, hand);
        yield return StartCoroutine(CardManager.Instance.DeckToHand(deck, hand, 0));

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
