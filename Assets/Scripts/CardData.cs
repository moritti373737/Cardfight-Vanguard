using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create CardData")]
public class CardData : ScriptableObject
{
    public string Name;     // �J�[�h��
    public string UnitType ; // �m�[�}�� or �g���K�[���j�b�g
    public string Clan;     // �N������
    public string Race;     // �푰��
    public string Nation;   // ���Ɩ�
    public int Grade;
    public int DefaultPower;  // ���̃p���[
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
