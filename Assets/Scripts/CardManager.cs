using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;



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
    public async UniTask DeckToHand(Deck deck, Hand hand, Card card)
    {
        deck.Pull(card);

        await AnimationManager.Instance.DeckToCard(card);

        card.TurnOver();
        hand.Add(card);

        await AnimationManager.Instance.CardToHand(card);
        SetHistory(card: card, source: deck, target: hand);
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
    public async UniTask HandToCircle(Hand hand, ICardCircle cardCircle, Card card)
    {
        Card pulledCard = hand.Pull(card);

        await AnimationManager.Instance.HandToCircle(card, cardCircle);

        cardCircle.Pull();
        cardCircle.Add(pulledCard);


        card.SetState(Card.StateType.FaceUp, true);
        SetHistory(card: card, source: hand, target: cardCircle);
        await UniTask.Delay(100);

    }

    /// <summary>
    /// カードをデッキから各種カードサークル(V,R)に移動
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="cardCircle"></param>
    /// <param name="index">デッキから取り出すためのインデックス</param>
    /// <returns>コルーチン</returns>
    public async UniTask DeckToCircle(Deck deck, ICardCircle cardCircle, int index)
    {
        await UniTask.NextFrame();

        Card card = deck.Pull(index);
        cardCircle.Add(card);

        SetHistory(card: card, source: deck, target: cardCircle);
    }

    /// <summary>
    /// カードをRゾーンからRゾーンに移動
    /// </summary>
    /// <param name="cardCircle">移動元のR</param>
    /// <param name="targetCircle">移動先のR</param>
    /// <param name="card">移動対象のカード</param>
    /// <returns></returns>
    public async UniTask CircleToCircle(ICardCircle cardCircle, ICardCircle targetCircle, Card card)
    {
        await UniTask.NextFrame();

        Card targetCard = targetCircle.Pull();
        if (targetCard != null)
        {
            cardCircle.Add(targetCard);
        }

        targetCircle.Add(card);

        SetHistory(card: card, source: cardCircle, target: targetCircle);
    }

    public async UniTask DeckToDrive(Deck deck, Drive drive, Card card)
    {
        card.transform.parent = null;
        await AnimationManager.Instance.DeckToDrive(card, drive);
        drive.Add(card);
        card.TurnOver();
        card.SetState(Card.StateType.FaceUp, true);
        SetHistory(card: card, source: deck, target: drive);
    }

    public async UniTask DriveToHand(Drive drive, Hand hand, Card card)
    {
        await UniTask.Delay(100);

        await AnimationManager.Instance.DriveToCard(card);

        hand.Add(card);

        await AnimationManager.Instance.CardToHand(card);

        SetHistory(card: card, source: drive, target: hand);
    }

    public async UniTask DriveToDamage(Drive drive, Damage damage, Card card)
    {
        await UniTask.Delay(100);
        await AnimationManager.Instance.DriveToCard(card);

        damage.Add(card);

        await AnimationManager.Instance.CardToDamage(card);

        SetHistory(card: card, source: drive, target: damage);
    }

    public async UniTask CircleToSoul(ICardCircle circle, Soul soul, Card card)
    {
        await UniTask.NextFrame();

        circle.Pull();
        soul.Add(card);

        SetHistory(card: card, source: circle, target: soul);
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

        SetHistory(card: card, source: null, target: drop);
    }

    public async UniTask CircleToDrop(ICardCircle circle, Drop drop, Card card)
    {
        circle.Pull();

        await AnimationManager.Instance.RetireCard(card);

        drop.Add(card);

        await AnimationManager.Instance.CardToHand(card);

        SetHistory(card: card, source: circle, target: drop);
    }

    /// <summary>
    /// 手札をデッキに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns></returns>
    public async UniTask HandToDeck(Hand hand, Deck deck, Card card)
    {
        await UniTask.NextFrame();

        hand.Pull(card);

        deck.Add(card);

        SetHistory(card: card, source: hand, target: deck);
    }

    /// <summary>
    /// 手札をガーディアンサークルに移動
    /// </summary>
    /// <param name="card">移動させるカード</param>
    /// <returns></returns>
    public async UniTask HandToGuardian(Hand hand, Guardian guardian, Card card)
    {
        hand.Pull(card);

        await AnimationManager.Instance.HandToGuardian(card, guardian);

        guardian.Add(card);

        SetHistory(card: card, source: hand, target: guardian);
    }

    public async UniTask GuardianToDrop(Guardian guardian, Drop drop, Card card)
    {
        await AnimationManager.Instance.RetireCard(card);

        guardian.Pull(card);

        drop.Add(card);

        await AnimationManager.Instance.CardToHand(card);

        SetHistory(card: card, source: guardian, target: drop);
    }

    public async UniTask DamageToDrop(Damage damage, Drop drop, Card card)
    {
        await AnimationManager.Instance.RetireCard(card);

        damage.Pull(card);

        drop.Add(card);

        await AnimationManager.Instance.CardToDrop(card);

        SetHistory(card: card, source: damage, target: drop);
    }

    public async UniTask HandToDrop(Hand hand, Drop drop, Card card)
    {
        await UniTask.NextFrame();

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
        SetHistory(card: card, source: card.Parent);
    }

    public async UniTask RestCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        if (card == null) return;
        await AnimationManager.Instance.RestCard(card);
        card.SetState(Card.StateType.Stand, false);
        SetHistory(card: card, source: card.GetComponentInParent<ICardZone>());
    }

    public async UniTask StandCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        if (card == null) return;
        await AnimationManager.Instance.StandCard(card);
        card.SetState(Card.StateType.Stand, true);
        SetHistory(card: card, source: card.GetComponentInParent<ICardZone>());
    }

    public async UniTask CounterBlast(Card card)
    {
        await AnimationManager.Instance.RotateFieldCard(card);
        card.SetState(Card.StateType.FaceUp, false);
        SetHistory(card: card, source: card.Parent);
    }

    public void SetHistory([CallerMemberName] string name = null, Card card = null, ICardZone source = null, ICardZone target = null) => ActionManager.Instance.ActionHistory.Add(new ActionData(name, card.FighterID, card, source, target));
}