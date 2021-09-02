using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードを管理する
/// </summary>
public class CardManager : SingletonMonoBehaviour<CardManager>
{
    public GameObject Field;

    private Soul soul;
    private Drop drop;

    void Start()
    {
        soul = Field.transform.FindWithChildTag(Tag.Soul).GetComponent<Soul>();
        drop = Field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
    }

    /// <summary>
    /// カードをデッキから手札に移動
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="index">デッキから取り出すためのインデックス</param>
    /// <returns>コルーチン</returns>
    public IEnumerator DeckToHand(Deck deck, Hand hand, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        Card card = deck.Pull(index);

        //yield return StartCoroutine(AnimationManager.Instance.DeckToCard(card));

        card.TurnOver();
        hand.Add(card);

        //yield return StartCoroutine(AnimationManager.Instance.CardToHand(card));

        // 待つ
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// カードを手札から各種カードサークル(V,R,G)に移動
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="cardCircle"></param>
    /// <param name="card">手札から取り出すカード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator HandToField(Hand hand, ICardCircle cardCircle, Card card)
    {
        // ログ出力
        //Debug.Log("1second");
        Card pulledCard = hand.Pull(card);
        //card.TurnOver();
        Card removedCard = cardCircle.GetCard();
        if (cardCircle.GetTransform().FindWithChildTag(Tag.Card))
        {
            if (cardCircle.GetTransform().tag.Contains(Tag.Vanguard.ToString()))
                yield return StartCoroutine(CardToSoul(removedCard));
            else if (cardCircle.GetTransform().tag.Contains(Tag.Rearguard.ToString()))
                yield return StartCoroutine(CardToDrop(removedCard));
        }
        cardCircle.Add(pulledCard);
        hand.DestroyEmpty(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// カードをデッキから各種カードサークル(V,R,G)に移動
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="cardCircle"></param>
    /// <param name="index">デッキから取り出すためのインデックス</param>
    /// <returns>コルーチン</returns>
    public IEnumerator DeckToCircle(Deck deck, ICardCircle cardCircle, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        Card card = deck.Pull(index);
        //card.TurnOver();
        cardCircle.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator RearToRear(ICardCircle cardCircle, ICardCircle targetCircle, Card card)
    {
        // ログ出力
        //Debug.Log("1second");
        //Card card = deck.Pull(index);
        //card.TurnOver();
        //Card card = cardCircle.GetTransform().FindWithChildTag(Tag.Card).GetComponent<Card>();
        Card targetCard = targetCircle.Pull();
        if (targetCard != null)
        {
            cardCircle.Add(targetCard);
        }

        targetCircle.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DeckToDrive(Deck deck, Drive drive)
    {
        Card card = deck.Pull(0);
        card.TurnOver();
        drive.Add(card);

        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator DriveToHand(Drive drive, Hand hand)
    {
        Card card = drive.Pull();
        hand.Add(card);

        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// カードをソウルに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator CardToSoul(Card card)
    {
        soul.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// カードをドロップゾーンに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator CardToDrop(Card card)
    {
        drop.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// カードを裏返す
    /// </summary>
    /// <param name="card">カード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator RotateCard(Card card)
    {
        yield return StartCoroutine(AnimationManager.Instance.RotateFieldCard(card));
    }
}
