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


[Serializable]
public class CostData
{
    public CostType Cost; // コストの種類
    public int Count;     // コストを支払う回数
    public string Option;
}

[Serializable]
public class MainSkillData
{
    public FighterType TargetFighter; // 効果対象のファイター
    public ConditionType TargetCard;  // 効果対象のカード
    public string TargetOption;
    public SkillType Skill;            // 効果内容
    public string SkillOption;             // 効果のオプション
}