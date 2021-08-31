using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : SingletonMonoBehaviour<CardManager>
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator DeckToHand(Deck deck, Hand hand, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        var card = deck.Pull(index);

        //yield return StartCoroutine(AnimationManager.Instance.DeckToCard(card));

        card.TurnOver();
        hand.Add(card);

        //yield return StartCoroutine(AnimationManager.Instance.CardToHand(card));

        // 待つ
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator HandToField(Hand hand, ICardCircle cardCircle, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        var card = hand.Pull(index);
        //card.TurnOver();
        cardCircle.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DeckToCircle(Deck deck, ICardCircle cardCircle, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        var card = deck.Pull(index);
        //card.TurnOver();
        cardCircle.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }
}
