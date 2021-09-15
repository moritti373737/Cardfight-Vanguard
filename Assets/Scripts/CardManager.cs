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
    public async UniTask DeckToHand(Deck deck, Hand hand, int index)
    {
        //MethodInfo method = this.GetType().GetMethod("DeckToHand");
        //var p = method.GetParameters();
        //var deck1 = p[0];

        // ログ出力
        //Debug.Log("1second");
        Card card = deck.Pull(index);

        await AnimationManager.Instance.DeckToCard(card);

        card.TurnOver();
        hand.Add(card);

        await AnimationManager.Instance.CardToHand(card);
        SetHistory(card: card, source:deck, target:hand);
        // 待つ
        await UniTask.Delay(100);
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

        await AnimationManager.Instance.HandToCircle(card, cardCircle);

        //card.TurnOver();
        Card removedCard = cardCircle.Pull();
        cardCircle.Add(pulledCard);
        hand.DestroyEmpty();

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
    public async UniTask RearToRear(Rearguard cardCircle, Rearguard targetCircle, Card card)
    {
        // ログ出力
        //Debug.Log("1second");
        //Card card = deck.Pull(index);
        //card.TurnOver();
        Card targetCard = targetCircle.Pull();
        if (targetCard != null)
        {
            //_ = AnimationManager.Instance.HandToCircle(targetCard, cardCircle);
            cardCircle.Add(targetCard);
        }

        //await AnimationManager.Instance.HandToCircle(card, targetCircle);

        targetCircle.Add(card);

        SetHistory(card: card, source:cardCircle, target:targetCircle);
    }

    public async UniTask DeckToDrive(Deck deck, Drive drive)
    {
        Card card = deck.Pull(0);
        card.transform.parent = null;
        await AnimationManager.Instance.DeckToDrive(card, drive);
        drive.Add(card);
        card.TurnOver();

        SetHistory(card: card, source:deck, target:drive);
    }

    public async UniTask DriveToHand(Drive drive, Hand hand)
    {
        Card card = drive.Pull();

        await UniTask.Delay(1000);

        await AnimationManager.Instance.DriveToCard(card);

        hand.Add(card);

        await AnimationManager.Instance.CardToHand(card);

        SetHistory(card: card, source:drive, target:hand);
    }

    public async UniTask DriveToDamage(Drive drive, Damage damage)
    {
        Card card = drive.Pull();

        await UniTask.Delay(1000);
        await AnimationManager.Instance.DriveToCard(card);

        damage.Add(card);

        await AnimationManager.Instance.CardToDamage(card);


        SetHistory(card: card, source:drive, target:damage);

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
    public async UniTask CardToDrop(Drop drop, Card card)
    {
        await AnimationManager.Instance.RetireCard(card);

        drop.Add(card);

        await AnimationManager.Instance.CardToHand(card);

        SetHistory(card: card, source:null, target:drop);
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
        hand.DestroyEmpty();
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
        hand.DestroyEmpty();
        SetHistory(card: card, source:hand, target:guardian);
    }

    public async UniTask GuardianToDrop(Guardian guardian, Drop drop, Card card)
    {


        await AnimationManager.Instance.RetireCard(card);

        guardian.Pull(card);

        drop.Add(card);

        await AnimationManager.Instance.CardToHand(card);

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
        hand.DestroyEmpty();
        SetHistory(card: card, source: hand, target: drop);
    }



    /// <summary>
    /// カードを裏返す
    /// </summary>
    /// <param name="card">カード</param>
    /// <returns>コルーチン</returns>
    public async UniTask RotateCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        card.transform.parent = null;
        await AnimationManager.Instance.RotateFieldCard(card);
        card.transform.SetParent(cardCircle.transform);
        SetHistory(card: card, source: cardCircle);
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
