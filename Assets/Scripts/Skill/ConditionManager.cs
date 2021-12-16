using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ConditionManager
{
    readonly IFighter fighter1;
    readonly IFighter fighter2;

    public ConditionManager(IFighter fighter1, IFighter fighter2)
    {
        this.fighter1 = fighter1;
        this.fighter2 = fighter2;
    }

    public bool CheckCondition(ActionData actionData, Card card, Skill skill)
    {
        List<ConditionData> conditionData = skill.condition.ToList();
        foreach (var condition in conditionData)
        {
            switch (condition.SourceFighter)
            {
                case FighterType.None:
                    break;
                case FighterType.Own:
                    if (card.FighterID != actionData.FighterID) return false;
                    break;
                case FighterType.Your:
                    if (card.FighterID == actionData.FighterID) return false;
                    break;
                default:
                    break;
            }

            switch (condition.SourceCard)
            {
                case ConditionType.None:
                    break;
                case ConditionType.Own:
                    if (card.ID != actionData.Card.ID) return false;
                    break;
                case ConditionType.Rearguard:
                    if (actionData.Source.GetType() != typeof(Rearguard)) return false;
                    break;
                case ConditionType.Vanguard:
                    if (actionData.Source.GetType() != typeof(Vanguard)) return false;
                    break;
                case ConditionType.Other:
                    break;
            }
        }

        return true;
    }
}
