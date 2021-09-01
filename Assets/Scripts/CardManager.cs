using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : SingletonMonoBehaviour<CardManager>
{
    public GameObject Field;

    private Soul soul;
    private Drop drop;

    // Start is called before the first frame update
    void Start()
    {
        soul = Field.transform.FindWithChildTag(Tag.Soul).GetComponent<Soul>();
        drop = Field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
    }

    // Update is called once per frame
    void Update()
    {

    }

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

    public IEnumerator HandToField(Hand hand, ICardCircle cardCircle, int index)
    {
        // ログ出力
        //Debug.Log("1second");
        Card card = hand.Pull(index);
        //card.TurnOver();
        Card removeCard = cardCircle.GetCard();
        if (cardCircle.GetTransform().FindWithChildTag(Tag.Card))
        {
            if (cardCircle.GetTransform().tag.Contains(Tag.Vanguard.ToString()))
                yield return StartCoroutine(CardToSoul(removeCard));
            else if (cardCircle.GetTransform().tag.Contains(Tag.Rearguard.ToString()))
                yield return StartCoroutine(CardToDrop(removeCard));
        }
        cardCircle.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

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

    public IEnumerator CardToSoul(Card card)
    {
        soul.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator CardToDrop(Card card)
    {
        drop.Add(card);

        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator RotateCard(Card card)
    {
        yield return StartCoroutine(AnimationManager.Instance.RotateFieldCard(card));
    }
}
