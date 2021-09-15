using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create SkillData")]
public class SkillData : ScriptableObject
{
    public string CardNumber;
    public List<Skill> SkillList = new List<Skill>();
    private void OnEnable()
    {
        CardNumber = name;
        //var fullpath = UnityEditor.AssetDatabase.GetAssetPath(this).SplitEx('/');
        //string pack = fullpath[fullpath.Count - 2];
        //string number = fullpath[fullpath.Count - 1].Substring(0, fullpath[fullpath.Count - 1].Length - 11);
        //SkillList.ForEach(skill => skill.cardNumber = pack + "/" + number);
    }
}

[System.Serializable]
public class Skill
{
    public string cardNumber;             // ���̃X�L����ۗL����J�[�h�i�p�b�N����ID�j
    public CategoryType category;         // ���� or �N�� or �i��
    public Tag place;                     // �X�L�������\�ȏꏊ
    public List<ConditionData> condition; // ���� or �i���X�L���̔�������
    public List<CostData> cost;           // �X�L�������ɕK�v�ȃR�X�g
    public List<MainSkillData> skill;     // �X�L�����e
    public FinishType finish;             // �X�L���̌��ʏI���^�C�~���O
}

public enum CategoryType
{
    Automatic,
    Activated,
    Continuous
}

public enum CostType
{
    None,
    HandToDrop
}

public enum FighterType
{
    None,
    Own,
    Your,
}

public enum ConditionType
{
    None,
    Own,
    Rearguard,
    Vanguard,
    Other,
}

public enum ActionType
{
    None,
    DeckToHand,
    Attack,
    Hit,
}


public enum SkillType
{
    PowerUp,
}

public enum FinishType
{
    None,
    TurnEnd,
}

[System.Serializable]
public class ConditionData
{
    public FighterType SourceFighter; // �ω��O�̃t�@�C�^�[
    public ConditionType SourceCard;  // �ω��O�̃J�[�h
    public string SourceCardOption;   // ��L�̒ǉ��I�v�V����
    public FighterType TargetFighter; // �ω���̃t�@�C�^�[
    public ConditionType TargetCard;  // �ω���̃J�[�h
    public string TargetCardOption;   // ��L�̒ǉ��I�v�V����
    public ActionType Action;         // �ω��̓��e
    public ActionType Result;         // �ω��̌���
    public bool SameClan;             // ���N���������̗L��
}


[System.Serializable]
public class CostData
{
    public CostType Type; // �R�X�g�̎��
    public int Count;     // �R�X�g���x������
}

[System.Serializable]
public class MainSkillData
{
    public FighterType TargetFighter; // ���ʑΏۂ̃t�@�C�^�[
    public ConditionType TargetCard;  // ���ʑΏۂ̃J�[�h
    public SkillType Type;            // ���ʓ��e
    public string Option;             // ���ʂ̃I�v�V����
}