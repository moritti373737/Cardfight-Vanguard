using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Card : MonoBehaviour
{
    public int ID { get; private set; } // カード固有のID

    public FighterID FighterID { get; set; }

    public ICardZone Parent { get => GetComponentInParent<ICardZone>(); }

    public string Name { get; private set; }     // カード名
    public string UnitType { get; private set; } // ノーマル or トリガーユニット
    public string Clan { get; private set; }     // クラン名
    public string Race { get; private set; }     // 種族名
    public string Nation { get; private set; }   // 国家名
    public int Grade { get; private set; }
    private int DefaultPower { get; set; }  // 元のパワー
    public int Power { get => DefaultPower + OffsetPower + BoostedPower; }
    private int DefaultCritical { get; set; }
    public int Critical { get => DefaultCritical + OffsetCritical; }
    public int Shield { get; private set; }
    public SkillType Skill { get; private set; }
    public TriggerType Trigger { get; private set; }
    public int TriggerPower { get; private set; } = 0;
    public string Effect { get; private set; }
    public string Flavor { get; private set; }
    public string Number { get; private set; }
    public string Rarity { get; private set; }

    public int OffsetPower { get; set; } = 0;
    public int BoostedPower { get; set; } = 0;
    public int OffsetCritical { get; set; } = 0;

    public Transform Transform { get => transform; }

    [Flags]
    public enum State
    {
        Stand = 1 << 0,     //2進数だと0001　(10進数だと1) スタンド状態か
        FaceUp = 1 << 1,    //2進数だと0010　(10進数だと2) 表向きかどうか
        Hoimi = 1 << 2,     //2進数だと0100　(10進数だと4)
        Mera = 1 << 3       //2進数だと1000　(10進数だと8)
    }
    public State state { get; set; }

    public enum SkillType
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
        ID = int.Parse(transform.name.Substring(4));
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
    }


    public void SetStatus(TextAsset cardText)
    {
        List<string> cardTextList = cardText.text.Replace("\r\n", "\n").SplitEx('\n');

        Name = cardTextList[1].SplitEx(',')[1];
        UnitType = cardTextList[2].SplitEx(',')[1];
        Clan = cardTextList[3].SplitEx(',')[1];
        Race = cardTextList[4].SplitEx(',')[1];
        Nation = cardTextList[5].SplitEx(',')[1];
        Grade = int.Parse(cardTextList[6].SplitEx(',')[1]);
        DefaultPower = int.Parse(cardTextList[7].SplitEx(',')[1]);
        DefaultCritical = int.Parse(cardTextList[8].SplitEx(',')[1]);
        Shield = int.Parse(cardTextList[9].SplitEx(',')[1].Replace("-", "0"));
        var skillText = cardTextList[10].SplitEx(',')[1];
        if (skillText == "ブースト") Skill = SkillType.Boost;
        else if (skillText == "インターセプト") Skill = SkillType.Intercept;
        else if (skillText == "ツインドライブ") Skill = SkillType.TwinDrive;
        else if (skillText == "トリプルドライブ") Skill = SkillType.TripleDrive;
        var triggerText = cardTextList[11].SplitEx(',')[1];
        if (triggerText == "-") Trigger = TriggerType.None;
        else
        {
            var text = triggerText.SplitEx('+');
            if (text[0] == "クリティカルトリガー") Trigger = TriggerType.Critical;
            else if (text[0] == "ドロートリガー") Trigger = TriggerType.Draw;
            else if (text[0] == "フロントトリガー") Trigger = TriggerType.Front;
            else if (text[0] == "ヒールトリガー") Trigger = TriggerType.Heal;
            else if (text[0] == "スタンドトリガー") Trigger = TriggerType.Stand;
            else if (text[0] == "オーバートリガー") Trigger = TriggerType.Over;
            TriggerPower = int.Parse(text[1]);
        }
        Effect = cardTextList[12].SplitEx(',')[1];
        Flavor = cardTextList[13].SplitEx(',')[1];
        Number = cardTextList[14].SplitEx(',')[1].Replace("/", "-");
        Rarity = cardTextList[15].SplitEx(',')[1];

        //if (ID == 0) print(cardText);
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
