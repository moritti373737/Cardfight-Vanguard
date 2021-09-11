using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create AbilityData")]
public class AbilityData : ScriptableObject
{
    public string CardNumber;
    public List<Ability> AbilityList = new List<Ability>();
    private void OnEnable()
    {
        CardNumber = name;
        var fullpath = UnityEditor.AssetDatabase.GetAssetPath(this).SplitEx('/');
        string pack = fullpath[fullpath.Count - 2];
        string number = fullpath[fullpath.Count - 1].Substring(0, fullpath[fullpath.Count - 1].Length - 13);
        AbilityList.ForEach(ability => ability.cardNumber = pack + "/" + number);
    }
}

[System.Serializable]
public class Ability
{
    public string cardNumber;
    public CategoryType category;
    public Tag place;
    public ConditionType condition;
    public List<CostData> cost;
    public List<MainAbilityData> ability;
    public FinishType finish;
}

public enum CategoryType
{
    Automatic,
    Activated,
    Continuous
}

public enum ConditionType
{
    None,

}

public enum CostType
{
    None,
    HandToDrop
}

public enum TargetType
{
    None,
    Own,
    Your,
}
public enum AbilityType
{
    PowerUp,
}

public enum FinishType
{
    None,
    TurnEnd,
}

[System.Serializable]
public class CostData
{
    public CostType Type;
    public int Count;
}

[System.Serializable]
public class MainAbilityData
{
    public TargetType TargetFighter;
    public TargetType TargetCard;
    public AbilityType Type;
    public int Option;
}