using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create CardData")]
public class CardData : ScriptableObject
{
    public string Name;     // カード名
    public string UnitType ; // ノーマル or トリガーユニット
    public string Clan;     // クラン名
    public string Race;     // 種族名
    public string Nation;   // 国家名
    public int Grade;
    public int DefaultPower;  // 元のパワー
    public int DefaultCritical;
    public int Shield;
    public Card.AbilityType Ability;
    public Card.TriggerType Trigger;
    public int TriggerPower;
    public SkillData Skill;
    public string Flavor;
    public string Number;
    public string Rarity;
}
