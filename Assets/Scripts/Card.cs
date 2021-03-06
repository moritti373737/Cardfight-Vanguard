using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;



public class Card : MonoBehaviour
{
    public int ID { get => int.Parse(transform.name.Substring(4)); } // カード固有のID

    [field: SerializeField]
    public FighterID FighterID { get; set; }

    [SerializeField]
    public ICardZone Parent { get => GetComponentInParent<ICardZone>(); }

    [field: SerializeField]
    public MeshRenderer Face { get; private set; }

    [field: SerializeField]
    public MeshRenderer Back { get; private set; }

    [field: Space(10)]
    [field: Header("↓カードデータ↓")]
    [field: Space(10)]

    [field: SerializeField]
    public string Name { get; private set; }     // カード名
    [field: SerializeField]
    public string UnitType { get; private set; } // ノーマル or トリガーユニット
    [field: SerializeField]
    public string Clan { get; private set; }     // クラン名
    [field: SerializeField]
    public string Race { get; private set; }     // 種族名
    [field: SerializeField]
    public string Nation { get; private set; }   // 国家名
    [field: SerializeField]
    public int Grade { get; private set; }
    [field: SerializeField]
    private int DefaultPower { get; set; }  // 元のパワー
    public int Power { get => DefaultPower + OffsetPower + BoostedPower; }
    [field: SerializeField]
    private int DefaultCritical { get; set; } // 元のクリティカル
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
    public int OffsetPower { get; private set; } = 0;
    [field: SerializeField]
    public int BoostedPower { get; set; } = 0;
    [field: SerializeField]
    public int OffsetCritical { get; private set; } = 0;

    public Transform Transform { get => transform; }

    [Flags]
    public enum StateType
    {
        Stand = 1 << 0,     //2進数だと0001　(10進数だと1) スタンド状態か
        FaceUp = 1 << 1,    //2進数だと0010　(10進数だと2) 表向きかどうか
        Hoimi = 1 << 2,     //2進数だと0100　(10進数だと4)
        Mera = 1 << 3       //2進数だと1000　(10進数だと8)
    }
    public StateType State { get; set; }

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
        State = StateType.Stand;
        transform.ObserveEveryValueChanged(x => x.parent)
                 .Skip(1)
                 .Where(parent => parent != null)
                 .Where(parent => parent.ExistTag(Tag.Circle))
                 .Where(_ => JudgeState(StateType.FaceUp))
                 .Subscribe(_ => TextManager.Instance.SetStatusText(this))
                 .AddTo(this);
        this.ObserveEveryValueChanged(x => x.State)
            .Skip(1)
            .Where(_ => transform.parent != null)
            .Where(_ => transform.parent.ExistTag(Tag.Circle))
            .Where(_ => JudgeState(StateType.FaceUp))
            .Subscribe(_ => TextManager.Instance.SetStatusText(this))
            .AddTo(this);
        this.ObserveEveryValueChanged(x => x.Power)
            .Skip(1)
            .Subscribe(_ => TextManager.Instance.SetStatusText(this))
            .AddTo(this);
        this.ObserveEveryValueChanged(x => x.Critical)
            .Skip(1)
            .Subscribe(_ => TextManager.Instance.SetStatusText(this))
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
        Flavor = cardData.Flavor;
        Number = cardData.Number;
        Rarity = cardData.Rarity;

        Skill = SkillDataJson.Instance.LoadSkillData(Number);
    }

    public void TurnOver()
    {
        transform.Rotate(0, 180, 0);
        //State = State ^ StateType.FaceUp;
    }

    public Texture GetTexture() => transform.Find("Face").GetComponent<Renderer>().material.mainTexture;

    public void SetState(StateType newState, bool t)
    {
        if (t) State |= newState;
        else State &= ~newState;
    }
    public bool JudgeState(StateType judgeState) => State == (State | judgeState);
    public void AddPower(int power) => OffsetPower += power;
    public void AddCritical(int critical) => OffsetCritical += critical;

    public void Reset()
    {
        OffsetCritical = 0;
        OffsetPower = 0;
    }
}
