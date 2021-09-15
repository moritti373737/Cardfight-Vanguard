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
    public string cardNumber;             // このスキルを保有するカード（パック名とID）
    public CategoryType category;         // 自動 or 起動 or 永続
    public Tag place;                     // スキル発動可能な場所
    public List<ConditionData> condition; // 自動 or 永続スキルの発動条件
    public List<CostData> cost;           // スキル発動に必要なコスト
    public List<MainSkillData> skill;     // スキル内容
    public FinishType finish;             // スキルの効果終了タイミング
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
    public FighterType SourceFighter; // 変化前のファイター
    public ConditionType SourceCard;  // 変化前のカード
    public string SourceCardOption;   // 上記の追加オプション
    public FighterType TargetFighter; // 変化後のファイター
    public ConditionType TargetCard;  // 変化後のカード
    public string TargetCardOption;   // 上記の追加オプション
    public ActionType Action;         // 変化の内容
    public ActionType Result;         // 変化の結果
    public bool SameClan;             // 同クラン制限の有無
}


[System.Serializable]
public class CostData
{
    public CostType Type; // コストの種類
    public int Count;     // コストを支払う回数
}

[System.Serializable]
public class MainSkillData
{
    public FighterType TargetFighter; // 効果対象のファイター
    public ConditionType TargetCard;  // 効果対象のカード
    public SkillType Type;            // 効果内容
    public string Option;             // 効果のオプション
}