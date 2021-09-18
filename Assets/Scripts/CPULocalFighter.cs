using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CPULocalFighter : MonoBehaviour, IFighter
{
    [field: SerializeField]
    public FighterID ID { get; private set; }

    [field: SerializeField]
    public int ActorNumber { get; set; }

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    private IFighter OpponentFighter { get; set; }

    public GameObject Field { get; private set; }

    public Hand Hand { get; private set; }
    public Deck Deck { get; private set; }
    public Vanguard Vanguard { get; private set; }
    public List<Rearguard> Rearguards { get; private set; } = new List<Rearguard>();
    public Drop Drop { get; private set; }
    public Damage Damage { get; private set; }
    public Drive Drive { get; private set; }
    public Guardian Guardian { get; private set; }
    public Order Order { get; private set; }
    public Soul Soul { get; private set; }

    public Dictionary<int, Card> CardDic { get; set; }

    public ICardCircle SelectedAttackZone { get; set; } = null;
    public ICardCircle SelectedTargetZone { get; set; } = null;

    private void OnEnable()
    {
        //�q�I�u�W�F�N�g��S�Ď擾����
        //foreach (Transform childTransform in field.transform)
        //{
        //    if (childTransform.tag.Contains("Vanguard"))
        //        vanguard = childTransform.GetComponent<Vanguard>();
        //}
        Field = transform.FindWithChildTag(Tag.Field).gameObject;
        Hand = transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();
        Deck = Field.transform.FindWithChildTag(Tag.Deck).GetComponent<Deck>();
        Vanguard = Field.transform.FindWithChildTag(Tag.Vanguard).GetComponent<Vanguard>();
        Drop = Field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
        Damage = Field.transform.FindWithChildTag(Tag.Damage).GetComponent<Damage>();
        Drive = Field.transform.FindWithChildTag(Tag.Drive).GetComponent<Drive>();
        Guardian = Field.transform.FindWithChildTag(Tag.Guardian).GetComponent<Guardian>();
        Order = Field.transform.FindWithChildTag(Tag.Order).GetComponent<Order>();
        Soul = Field.transform.FindWithChildTag(Tag.Soul).GetComponent<Soul>();

        List<Transform> rearguards = Field.transform.FindWithAllChildTag(Tag.Rearguard);
        foreach (var rear in rearguards)
        {
            Rearguards.Add(rear.GetComponent<Rearguard>());
        }

        OpponentFighter = GameObject.FindGameObjectsWithTag("Fighter")
                                    .Select(obj => obj.GetComponent<IFighter>())
                                    .First(fighter => fighter.ID != ID);
    }

    /// <summary>
    /// �f�b�L���쐬����
    /// </summary>
    public void CreateDeck()
    {
        DeckGenerater.Instance.Generate(Deck, ID == FighterID.ONE ? 0 : 1);
    }

    /// <summary>
    /// �t�@�[�X�g���@���K�[�h���Z�b�g����
    /// </summary>
    /// <returns></returns>
    public void SetFirstVanguard()
    {
        _ = CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    /// <summary>
    /// ��D�̃J�[�h����������i�}���K���j
    /// �}���K����̃V���b�t�������͖�����
    /// </summary>
    public async UniTask Mulligan()
    {
        print("�}���K���J�n");
        // �O���[�h1-3�܂�1���Âc��
        List<Card> cardList = Hand.cardList.Where(card => card.Grade == 0)
                                           .Union(Hand.cardList.Where(card => card.Grade == 1).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 2).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 3).Skip(1))
                                           .ToList();
        int count = cardList.Count;

        for (int i = 0; i < count; i++)
        {
            Card card = cardList.Pop();
            await CardManager.Instance.HandToDeck(Hand, Deck, card); // ������cardList�̗v�f�����ω����邽�ߒʏ��ForEach�͎g�p�s��
        }

        await DrawCard(count);
    }

    /// <summary>
    /// �J�[�h���f�b�L����w�薇������
    /// </summary>
    /// <param name="count">��������</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(Deck, Hand, Deck.GetCard(0));
        }

    }

    /// <summary>
    /// �X�^���h�A�b�v���@���K�[�h�I
    /// </summary>
    public async UniTask StandUpVanguard()
    {
        var card = Vanguard.Card;
        card.SetState(Card.State.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard);
    }

    /// <summary>
    /// �X�^���h�t�F�C�Y
    /// </summary>
    public async UniTask StandPhase()
    {
        await UniTask.NextFrame();
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// �h���[�t�F�C�Y
    /// </summary>
    public async UniTask DrawPhase()
    {
        await DrawCard(1);
    }

    /// <summary>
    /// ���C�h�t�F�C�Y
    /// </summary>
    public async UniTask RidePhase()
    {
        // ���@���K�[�h�̃O���[�h+1�̃J�[�h�̒��Ńp���[���ő�̃J�[�h
        Card card = Hand.cardList.Where(card => card.Grade == Vanguard.Card.Grade + 1)
                                 .OrderByDescending(card => card.Power)
                                 .FirstOrDefault();
        if (card == null) return; // G�A�V�X�g�X�e�b�v�̎���

        Card removedCard = Vanguard.Card; // �J�[�h�����ɑ��݂���ꍇ�̓\�E���Ɉړ�������
        await CardManager.Instance.HandToCircle(Hand, Vanguard, card);
        if (removedCard != null)
        {
            Debug.Log("�\�E���ɑ����");
            await CardManager.Instance.CircleToSoul(Vanguard, Soul, removedCard);
        }
    }

    /// <summary>
    /// ���C���t�F�C�Y
    /// </summary>
    public async UniTask<bool> MainPhase()
    {
        return false;
    }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃A�^�b�N�X�e�b�v
    /// </summary>
    public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    {
        await UniTask.NextFrame();
        return (null, null);
    }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃K�[�h�X�e�b�v
    /// </summary>
    public async UniTask<bool> GuardStep()
    {
        return false;
    }

    /// <summary>
    /// �h���C�u�g���K�[�`�F�b�N
    /// </summary>
    public async UniTask DriveTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            if (Drive.GetCard().Trigger != Card.TriggerType.None)
                await GetTrigger(Drive.GetCard());
            await CardManager.Instance.DriveToHand(Drive, Hand, Drive.Card);
        }

    }

    /// <summary>
    /// �_���[�W�g���K�[�`�F�b�N
    /// </summary>
    public async UniTask DamageTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            await CardManager.Instance.DriveToDamage(Drive, Damage, Drive.Card);
        }
    }

    /// <summary>
    /// �g���K�[�擾
    /// </summary>
    public async UniTask GetTrigger(Card triggerCard)
    {
    }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃G���h�X�e�b�v
    /// </summary>
    public async UniTask EndStep()
    {
        Vanguard.Card.BoostedPower = 0;
        Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
    }

    /// <summary>
    /// �G���h�t�F�C�Y
    /// </summary>
    public async UniTask EndPhase()
    {
        await UniTask.NextFrame();

        Vanguard.Card.Reset();
        Rearguards.ForEach(rear => rear.Card?.Reset());

        Turn++;
    }

    /// <summary>
    /// �w�肵���T�[�N���̃J�[�h��ދp������
    /// </summary>
    /// <param name="circle">�ދp������T�[�N��</param>

    public async UniTask RetireCard(ICardCircle circle)
    {
        await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    }

    /// <summary>
    /// ���C���f�[�^�iCardManager�Ŏ��s����֐��̓��e�j����M�����Ƃ��̏���
    /// �J�[�h�Ɋւ��鏈���������Ă���
    /// </summary>
    /// <param name="args">���M��ID�A�֐����A�J�[�hID</param>
    public async UniTask ReceivedData(List<object> args) { }

    /// <summary>
    /// ���܂��܂ȏ����iAttack�Ȃǁj����M�������̏���
    /// </summary>
    /// <param name="args">���M��ID�A�������A�I�v�V����(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args) { }

    /// <summary>
    /// State�f�[�^����M�����Ƃ��̏���
    /// �t�F�C�Y�̊Ǘ��Ɏg���\��
    /// </summary>
    /// <param name="state">�t�F�C�Y��</param>
    public void ReceivedState(string state) { }

    private ICardCircle StringToCircle(string name) => name == "Vanguard" ? (ICardCircle)Vanguard : (ICardCircle)Rearguards.Find(rear => rear.Name == name);

}
