using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// カードを管理する
/// </summary>
public class CardManager : SingletonMonoBehaviour<CardManager>
{
    /// <summary>
    /// カードをデッキから手札に移動
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="index">デッキから取り出すためのインデックス</param>
    /// <returns>コルーチン</returns>
    public IEnumerator DeckToHand(Deck deck, Hand hand, int index)
    {
        //MethodInfo method = this.GetType().GetMethod("DeckToHand");
        //var p = method.GetParameters();
        //var deck1 = p[0];

        // ログ出力
        //Debug.Log("1second");
        Card card = deck.Pull(index);

        //yield return StartCoroutine(AnimationManager.Instance.DeckToCard(card));

        card.TurnOver();
        hand.Add(card);

        //yield return StartCoroutine(AnimationManager.Instance.CardToHand(card));
        SetHistory(card: card, source:deck, target:hand);
        // 待つ
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// カードを手札から各種カードサークル(V,R)に移動
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="cardCircle"></param>
    /// <param name="card">手札から取り出すカード</param>
    /// <returns></returns>
    public async UniTask<Card> HandToField(Hand hand, ICardCircle cardCircle, Card card)
    {
        // ログ出力
        //Debug.Log("1second");
        Card pulledCard = hand.Pull(card);
        //card.TurnOver();
        Card removedCard = cardCircle.Pull();
        cardCircle.Add(pulledCard);
        card.SetState(Card.State.FaceUp, true);
        SetHistory(card: card, source:hand, target:cardCircle);
        //if (removedCard != null)
        //{
        return removedCard;
            //if (cardCircle.GetTransform().tag.Contains(Tag.Vanguard.ToString()))
            //    yield return StartCoroutine(CardToSoul(removedCardCircle, removedCard));
            //else if (cardCircle.GetTransform().tag.Contains(Tag.Rearguard.ToString()))
            //    yield return StartCoroutine(CardToDrop(removedCardCircle, removedCard));
        //}

    }

    /// <summary>
    /// カードをデッキから各種カードサークル(V,R)に移動
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

        SetHistory(card: card, source:deck, target:cardCircle);
        yield return null;
    }

    /// <summary>
    /// カードをRゾーンからRゾーンに移動
    /// </summary>
    /// <param name="cardCircle">移動元のR</param>
    /// <param name="targetCircle">移動先のR</param>
    /// <param name="card">移動対象のカード</param>
    /// <returns></returns>
    public IEnumerator RearToRear(Rearguard cardCircle, Rearguard targetCircle, Card card)
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
        SetHistory(card: card, source:cardCircle, target:targetCircle);
        yield return new WaitForSeconds(0.0f);
    }

    public async UniTask DeckToDrive(Deck deck, Drive drive)
    {
        Card card = deck.Pull(0);
        card.transform.parent = null;
        await AnimationManager.Instance.DeckToDrive(card, drive);
        drive.Add(card);
        card.TurnOver();

        SetHistory(card: card, source:deck, target:drive);
        //yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DriveToHand(Drive drive, Hand hand)
    {
        Card card = drive.Pull();
        hand.Add(card);

        SetHistory(card: card, source:drive, target:hand);
        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DriveToDamage(Drive drive, Damage damage)
    {
        Card card = drive.Pull();
        damage.Add(card);

        SetHistory(card: card, source:drive, target:damage);
        yield return new WaitForSeconds(0.0f);
    }

    //public IEnumerator DriveToDrop(Drive drive, Drop drop)
    //{
    //    Card card = drive.Pull();
    //    drop.Add(card);

    //    yield return new WaitForSeconds(0.0f);
    //}

    /// <summary>
    /// カードをソウルに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator CardToSoul(Soul soul, Card card)
    {
        soul.Add(card);

        SetHistory(card: card, source:null, target:soul);
        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// カードをドロップゾーンに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns>コルーチン</returns>
    public IEnumerator CardToDrop(Drop drop, Card card)
    {
        drop.Add(card);

        SetHistory(card: card, source:null, target:drop);
        // 待つ
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// 手札をデッキに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns></returns>
    public async UniTask HandToDeck(Hand hand, Deck deck, Card card)
    {
        hand.Pull(card);

        deck.Add(card);
        SetHistory(card: card, source:hand, target:deck);
    }

    /// <summary>
    /// 手札をガーディアンサークルに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns></returns>
    public async UniTask HandToGuardian(Hand hand, Guardian guardian, Card card)
    {
        hand.Pull(card);

        guardian.Add(card);
        SetHistory(card: card, source:hand, target:guardian);
    }

    public async UniTask GuardianToDrop(Guardian guardian, Drop drop, Card card)
    {

        guardian.Pull(card);
        drop.Add(card);

        SetHistory(card: card, source:guardian, target:drop);
    }

    public async UniTask DamageToDrop(Damage damage, Drop drop, Card card)
    {
        damage.Pull(card);
        drop.Add(card);
        SetHistory(card: card, source:damage, target:drop);
    }

    public async UniTask HandToDrop(Hand hand, Drop drop, Card card)
    {
        hand.Pull(card);

        drop.Add(card);
        SetHistory(card: card, source: hand, target: drop);
    }



    /// <summary>
    /// カードを裏返す
    /// </summary>
    /// <param name="card">カード</param>
    /// <returns>コルーチン</returns>
    public async UniTask RotateCard(Card card)
    {
        await AnimationManager.Instance.RotateFieldCard(card);
        SetHistory(card: card, source:card.GetComponentInParent<ICardZone>());
    }

    public async UniTask RestCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        if (card == null) return;
        //Card card = cardCircle.Pull();
        //card.transform.parent = null;
        await AnimationManager.Instance.RestCard(card, 15);
        //cardCircle.Add(card);
        card.SetState(Card.State.Stand, false);
        SetHistory(card: card, source: card.GetComponentInParent<ICardZone>());
    }

    public async UniTask StandCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        if (card == null) return;
        //card.transform.parent = null;
        await AnimationManager.Instance.StandCard(card, 15);
        //cardCircle.Add(card);
        card.SetState(Card.State.Stand, true);
        SetHistory(card: card, source: card.GetComponentInParent<ICardZone>());
    }

    public void SetHistory([CallerMemberName]string name=null, Card card=null, object source=null, object target =null) => ActionManager.Instance.ActionHistory.Add(new ActionData(name, card.FighterID, card, source, target));
}
