using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create AbilityData")]
public class AbilityData : ScriptableObject
{
    public string CardNumber;
    public List<Ability> AbilityList = new List<Ability>();
    private void OnEnable()
    {
        CardNumber = name;
    }
}

[System.Serializable]
public class Ability
{
    public CategoryType category;
    public Tag place;
    public CostType cost;
    public string targetFighter;
    public targetType targetCard;
    public string ability;
    public string end;
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

public enum targetType
{
    None,
    Own,
}