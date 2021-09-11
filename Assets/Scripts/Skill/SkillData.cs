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
        var fullpath = UnityEditor.AssetDatabase.GetAssetPath(this).SplitEx('/');
        string pack = fullpath[fullpath.Count - 2];
        string number = fullpath[fullpath.Count - 1].Substring(0, fullpath[fullpath.Count - 1].Length - 11);
        SkillList.ForEach(skill => skill.cardNumber = pack + "/" + number);
    }
}

[System.Serializable]
public class Skill
{
    public string cardNumber;
    public CategoryType category;
    public Tag place;
    public List<ConditionData> condition;
    public List<CostData> cost;
    public List<MainSkillData> skill;
    public FinishType finish;
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
}

public enum ActionType
{
    None,
    DeckToHand,
    Attack,
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
    public FighterType SourceFighter;
    public ConditionType SourceCard;
    public FighterType TargetFighter;
    public ConditionType TargetCard;
    public ActionType Action;
}


[System.Serializable]
public class CostData
{
    public CostType Type;
    public int Count;
}

[System.Serializable]
public class MainSkillData
{
    public FighterType TargetFighter;
    public ConditionType TargetCard;
    public SkillType Type;
    public int Option;
}