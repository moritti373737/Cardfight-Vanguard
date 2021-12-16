using System;
using System.Collections.Generic;

//[CreateAssetMenu(menuName = "MyScriptable/Create SkillData")]
//public class SkillData : ScriptableObject
//{
//    public string CardNumber;
//    public List<Skill> SkillList = new List<Skill>();
//    private void OnEnable()
//    {
//        CardNumber = name;
//        //var fullpath = UnityEditor.AssetDatabase.GetAssetPath(this).SplitEx('/');
//        //string pack = fullpath[fullpath.Count - 2];
//        //string number = fullpath[fullpath.Count - 1].Substring(0, fullpath[fullpath.Count - 1].Length - 11);
//        //SkillList.ForEach(skill => skill.cardNumber = pack + "/" + number);
//    }
//}
[Serializable]
public class SkillData
{
    public string CardNumber;
    public List<Skill> SkillList;
    //private void OnEnable()
    //{
    //    //var fullpath = UnityEditor.AssetDatabase.GetAssetPath(this).SplitEx('/');
    //    //string pack = fullpath[fullpath.Count - 2];
    //    //string number = fullpath[fullpath.Count - 1].Substring(0, fullpath[fullpath.Count - 1].Length - 11);
    //    //SkillList.ForEach(skill => skill.cardNumber = pack + "/" + number);
    //}
}

[Serializable]
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
    CounterBlast,
    HandToDrop,
    HandToDeck,
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
    CriticalUp,
    Retire,
}

public enum FinishType
{
    None,
    TurnEnd,
}

[Serializable]
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


[Serializable]
public class CostData
{
    public CostType Cost; // �R�X�g�̎��
    public int Count;     // �R�X�g���x������
    public string Option;
}

[Serializable]
public class MainSkillData
{
    public FighterType TargetFighter; // ���ʑΏۂ̃t�@�C�^�[
    public ConditionType TargetCard;  // ���ʑΏۂ̃J�[�h
    public string TargetOption;
    public SkillType Skill;            // ���ʓ��e
    public string SkillOption;             // ���ʂ̃I�v�V����
}