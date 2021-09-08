using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �J�[�h���Ǘ�����
/// </summary>
public class CardManager : SingletonMonoBehaviour<CardManager>
{
    /// <summary>
    /// �J�[�h���f�b�L�����D�Ɉړ�
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="index">�f�b�L������o�����߂̃C���f�b�N�X</param>
    /// <returns>�R���[�`��</returns>
    public IEnumerator DeckToHand(Deck deck, Hand hand, int index)
    {
        // ���O�o��
        //Debug.Log("1second");
        Card card = deck.Pull(index);

        //yield return StartCoroutine(AnimationManager.Instance.DeckToCard(card));

        card.TurnOver();
        hand.Add(card);

        //yield return StartCoroutine(AnimationManager.Instance.CardToHand(card));

        // �҂�
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// �J�[�h����D����e��J�[�h�T�[�N��(V,R,G)�Ɉړ�
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="cardCircle"></param>
    /// <param name="card">��D������o���J�[�h</param>
    /// <returns></returns>
    public async UniTask<Card> HandToField(Hand hand, ICardCircle cardCircle, Card card)
    {
        // ���O�o��
        //Debug.Log("1second");
        Card pulledCard = hand.Pull(card);
        //card.TurnOver();
        Card removedCard = cardCircle.Pull();
        cardCircle.Add(pulledCard);
        card.SetState(Card.State.FaceUp, true);
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
    /// �J�[�h���f�b�L����e��J�[�h�T�[�N��(V,R,G)�Ɉړ�
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="cardCircle"></param>
    /// <param name="index">�f�b�L������o�����߂̃C���f�b�N�X</param>
    /// <returns>�R���[�`��</returns>
    public IEnumerator DeckToCircle(Deck deck, ICardCircle cardCircle, int index)
    {
        // ���O�o��
        //Debug.Log("1second");
        Card card = deck.Pull(index);
        //card.TurnOver();
        cardCircle.Add(card);

        yield return null;
    }

    public IEnumerator RearToRear(ICardCircle cardCircle, ICardCircle targetCircle, Card card)
    {
        // ���O�o��
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

        // �҂�
        yield return new WaitForSeconds(0.0f);
    }

    public async UniTask DeckToDrive(Deck deck, Drive drive)
    {
        Card card = deck.Pull(0);
        card.transform.parent = null;
        await AnimationManager.Instance.DeckToDrive(card, drive);
        drive.Add(card);
        card.TurnOver();

        //yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DriveToHand(Drive drive, Hand hand)
    {
        Card card = drive.Pull();
        hand.Add(card);

        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DriveToDamage(Drive drive, Damage damage)
    {
        Card card = drive.Pull();
        damage.Add(card);

        yield return new WaitForSeconds(0.0f);
    }

    public IEnumerator DriveToDrop(Drive drive, Drop drop)
    {
        Card card = drive.Pull();
        drop.Add(card);

        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// �J�[�h���\�E���Ɉړ�
    /// </summary>
    /// <param name="card">�ړ�������J�[�h</param>
    /// <returns>�R���[�`��</returns>
    public IEnumerator CardToSoul(Soul soul, Card card)
    {
        soul.Add(card);

        // �҂�
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// �J�[�h���h���b�v�]�[���Ɉړ�
    /// </summary>
    /// <param name="card">�ړ�������J�[�h</param>
    /// <returns>�R���[�`��</returns>
    public IEnumerator CardToDrop(Drop drop, Card card)
    {
        drop.Add(card);

        // �҂�
        yield return new WaitForSeconds(0.0f);
    }

    /// <summary>
    /// ��D���f�b�L�Ɉړ�
    /// </summary>
    /// <param name="card">�ړ�������J�[�h</param>
    /// <returns></returns>
    public async UniTask HandToDeck(Hand hand, Deck deck, Card card)
    {
        hand.Pull(card);

        deck.Add(card);
    }

    /// <summary>
    /// ��D���K�[�f�B�A���T�[�N���Ɉړ�
    /// </summary>
    /// <param name="card">�ړ�������J�[�h</param>
    /// <returns></returns>
    public async UniTask HandToGuardian(Hand hand, Guardian guardian, Card card)
    {
        hand.Pull(card);

        guardian.Add(card);
    }

    public async UniTask GuardianToDrop(Guardian guardian, Drop drop, Card card=null)
    {
        if (card == null)
        {
            List<Card> cardList = guardian.Clear();
            cardList.ForEach(card => drop.Add(card));
        }
        else
        {
            guardian.Pull(card);

            drop.Add(card);
        }
    }


    /// <summary>
    /// �J�[�h�𗠕Ԃ�
    /// </summary>
    /// <param name="card">�J�[�h</param>
    /// <returns>�R���[�`��</returns>
    public async UniTask RotateCard(Card card)
    {
        await AnimationManager.Instance.RotateFieldCard(card);
    }

    public async UniTask RestCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Card;
        //Card card = cardCircle.Pull();
        //card.transform.parent = null;
        await AnimationManager.Instance.RestCard(card, 15);
        //cardCircle.Add(card);
        card.SetState(Card.State.Stand, false);
    }

    public async UniTask StandCard(ICardCircle cardCircle)
    {
        Card card = cardCircle.Pull();
        if (card == null) return;
        //card.transform.parent = null;
        await AnimationManager.Instance.StandCard(card, 15);
        //cardCircle.Add(card);
        card.SetState(Card.State.Stand, true);
    }
}
