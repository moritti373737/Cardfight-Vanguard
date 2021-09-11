using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class Card : MonoBehaviour
{
    [field: SerializeField]
    public int ID { get => int.Parse(transform.name.Substring(4)); } // �J�[�h�ŗL��ID

    [field: SerializeField]
    public FighterID FighterID { get; set; }

    [SerializeField]
    public ICardZone Parent { get => GetComponentInParent<ICardZone>(); }

    [field: Space(10)]
    [field: Header("���J�[�h�f�[�^��")]
    [field: Space(10)]

    [field: SerializeField]
    public string Name { get; private set; }     // �J�[�h��
    [field: SerializeField]
    public string UnitType { get; private set; } // �m�[�}�� or �g���K�[���j�b�g
    [field: SerializeField]
    public string Clan { get; private set; }     // �N������
    [field: SerializeField]
    public string Race { get; private set; }     // �푰��
    [field: SerializeField]
    public string Nation { get; private set; }   // ���Ɩ�
    [field: SerializeField]
    public int Grade { get; private set; }
    [field: SerializeField]
    private int DefaultPower { get; set; }  // ���̃p���[
    public int Power { get => DefaultPower + OffsetPower + BoostedPower; }
    [field: SerializeField]
    private int DefaultCritical { get; set; } // ���̃N���e�B�J��
    public int Critical { get => DefaultCritical + OffsetCritical; }
    [field: SerializeField]
    public int Shield { get; private set; }
    [field: SerializeField]
    public AbilityType Ability { get; private set; }
    [field: SerializeField]
    public TriggerType Trigger { get; private set; }
    [field: SerializeField]
    public int TriggerPower { get; private set; } = 0;
    [field: SerializeField]
    public SkillData Skill { get; private set; }
    [field: SerializeField]
    public string Flavor { get; private set; }
    [field: SerializeField]
    public string Number { get; private set; }
    [field: SerializeField]
    public string Rarity { get; private set; }

    [field: SerializeField]
    public int OffsetPower { get; set; } = 0;
    [field: SerializeField]
    public int BoostedPower { get; set; } = 0;
    [field: SerializeField]
    public int OffsetCritical { get; set; } = 0;

    public Transform Transform { get => transform; }

    [Flags]
    public enum State
    {
        Stand = 1 << 0,     //2�i������0001�@(10�i������1) �X�^���h��Ԃ�
        FaceUp = 1 << 1,    //2�i������0010�@(10�i������2) �\�������ǂ���
        Hoimi = 1 << 2,     //2�i������0100�@(10�i������4)
        Mera = 1 << 3       //2�i������1000�@(10�i������8)
    }
    public State state { get; set; }

    public enum AbilityType
    {
        Boost,
        Intercept,
        TwinDrive,
        TripleDrive,
    }

    public enum TriggerType
    {
        None,
        Critical,
        Draw,
        Front,
        Heal,
        Stand,
        Over
    }

    void Start()
    {
        state = State.Stand;
        transform.ObserveEveryValueChanged(x => x.parent)
                 .Skip(1)
                 .Where(parent => parent != null)
                 .Where(parent => parent.ExistTag(Tag.Circle))
                 .Where(_ => JudgeState(State.FaceUp))
                 .Subscribe(_ => TextManager.Instance.SetStatusText(transform.GetComponentInParent<ICardCircle>()))
                 .AddTo(this);
        this.ObserveEveryValueChanged(x => x.state)
            .Skip(1)
            .Where(_ => transform.parent != null)
            .Where(_ => transform.parent.ExistTag(Tag.Circle))
            .Where(_ => JudgeState(State.FaceUp))
            .Subscribe(_ => TextManager.Instance.SetStatusText(transform.GetComponentInParent<ICardCircle>()))
            .AddTo(this);
        this.ObserveEveryValueChanged(x => x.OffsetPower)
            .Skip(1)
            .Subscribe(_ => TextManager.Instance.SetStatusText(transform.GetComponentInParent<ICardCircle>()))
            .AddTo(this);
        this.ObserveEveryValueChanged(x => x.OffsetCritical)
            .Skip(1)
            .Subscribe(_ => TextManager.Instance.SetStatusText(transform.GetComponentInParent<ICardCircle>()))
            .AddTo(this);

        //List<CardData> cardData = Resources.LoadAll<CardData>("TD01").ToList();
        //print(cardData[0].Name);
    }


    public void SetStatus(string filename)

    {
        CardData cardData = Resources.Load<CardData>(filename + "data");
        Name = cardData.Name;
        UnitType = cardData.UnitType;
        Clan = cardData.Clan;
        Race = cardData.Race;
        Nation = cardData.Nation;
        Grade = cardData.Grade;
        DefaultPower = cardData.DefaultPower;
        DefaultCritical = cardData.DefaultCritical;
        Shield = cardData.Shield;
        Ability = cardData.Ability;
        Trigger = cardData.Trigger;
        TriggerPower = cardData.TriggerPower;
        Skill = cardData.Skill;
        Flavor = cardData.Flavor;
        Number = cardData.Number;
        Rarity = cardData.Rarity;
    }

    public void TurnOver()
    {
        transform.Rotate(0, 180, 0);
        //state = state ^ State.FaceUp;
    }

    public Texture GetTexture() => transform.Find("Face").GetComponent<Renderer>().material.mainTexture;

    public void SetState(State newState, bool t)
    {
        if (t) state |= newState;
        else state &= ~newState;
    }
    public bool JudgeState(State judgeState) => state == (state | judgeState);
    public void AddPower(int power) => OffsetPower += power;
    public void AddCritical(int critical) => OffsetCritical += critical;

    public void Reset()
    {
        OffsetCritical = 0;
        OffsetPower = 0;
    }
}
