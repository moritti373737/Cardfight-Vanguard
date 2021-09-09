using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Card : MonoBehaviour
{
    public int ID { get; private set; } // �J�[�h�ŗL��ID

    public FighterID FighterID { get; set; }

    public ICardZone Parent { get => GetComponentInParent<ICardZone>(); }

    public string Name { get; private set; }     // �J�[�h��
    public string UnitType { get; private set; } // �m�[�}�� or �g���K�[���j�b�g
    public string Clan { get; private set; }     // �N������
    public string Race { get; private set; }     // �푰��
    public string Nation { get; private set; }   // ���Ɩ�
    public int Grade { get; private set; }
    private int DefaultPower { get; set; }  // ���̃p���[
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
        Stand = 1 << 0,     //2�i������0001�@(10�i������1) �X�^���h��Ԃ�
        FaceUp = 1 << 1,    //2�i������0010�@(10�i������2) �\�������ǂ���
        Hoimi = 1 << 2,     //2�i������0100�@(10�i������4)
        Mera = 1 << 3       //2�i������1000�@(10�i������8)
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
        if (skillText == "�u�[�X�g") Skill = SkillType.Boost;
        else if (skillText == "�C���^�[�Z�v�g") Skill = SkillType.Intercept;
        else if (skillText == "�c�C���h���C�u") Skill = SkillType.TwinDrive;
        else if (skillText == "�g���v���h���C�u") Skill = SkillType.TripleDrive;
        var triggerText = cardTextList[11].SplitEx(',')[1];
        if (triggerText == "-") Trigger = TriggerType.None;
        else
        {
            var text = triggerText.SplitEx('+');
            if (text[0] == "�N���e�B�J���g���K�[") Trigger = TriggerType.Critical;
            else if (text[0] == "�h���[�g���K�[") Trigger = TriggerType.Draw;
            else if (text[0] == "�t�����g�g���K�[") Trigger = TriggerType.Front;
            else if (text[0] == "�q�[���g���K�[") Trigger = TriggerType.Heal;
            else if (text[0] == "�X�^���h�g���K�[") Trigger = TriggerType.Stand;
            else if (text[0] == "�I�[�o�[�g���K�[") Trigger = TriggerType.Over;
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
