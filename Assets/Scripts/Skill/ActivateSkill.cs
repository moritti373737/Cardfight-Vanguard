using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// カードのスキルのうち、実際に発動して効果をもたらす部分を管理する
/// </summary>
public class ActivateSkill
{
    private PlayerInput input;

    readonly IFighter fighter1;
    readonly IFighter fighter2;

    public ActivateSkill(PlayerInput input, IFighter fighter1, IFighter fighter2)
    {
        this.input = input;
        this.fighter1 = fighter1;
        this.fighter2 = fighter2;
    }

        /// <summary>
    /// 実際にスキルを発動させる
    /// </summary>
    /// <param name="card">スキルを発動したカード</param>
    /// <param name="skillList">発動させるスキル</param>
    public async UniTask Activate(Card card, List<MainSkillData> skillList)
    {
        foreach (var skill in skillList)
        {
            Card targetCard = null;

            switch (skill.TargetCard)
            {
                case ConditionType.None:
                    break;
                case ConditionType.Own:
                    targetCard = card;
                    break;
                case ConditionType.Rearguard:
                    targetCard = await SelectRearguard(card.FighterID, skill.TargetFighter, skill.TargetCard, skill.TargetOption);
                    break;
                case ConditionType.Vanguard:
                    break;
                case ConditionType.Other:
                    break;
                default:
                    break;
            }

            switch (skill.Skill)
            {
                case SkillType.PowerUp:
                    PowerUp(targetCard, skill.SkillOption);
                    break;
                case SkillType.CriticalUp:
                    CriticalUp(targetCard, skill.SkillOption);
                    break;
                case SkillType.Retire:
                    Retire(targetCard);
                    break;
                default:
                    break;
            }
        }
    }

    private async UniTask<Card> SelectRearguard(FighterID cardFighter, FighterType targetFighter, ConditionType condition, string option)
    {
        while (true)
        {
            await UniTask.NextFrame();
            await UniTask.WaitUntil(() => input.GetDown("Enter"));

            FighterID fighterID = targetFighter == FighterType.Own ? cardFighter : 1 - cardFighter;
            var card = SelectManager.Instance.GetSelectCard(Tag.Rearguard, fighterID);
            if (card != null) return card;
        }
    }

    private void PowerUp(Card card, string power) => card.AddPower(int.Parse(power));
    private void CriticalUp(Card card, string critical) => card.AddCritical(int.Parse(critical));
    private void Retire(Card card)
    {
        if (card.FighterID == fighter1.ID) fighter1.RetireCard(card);
        else if (card.FighterID == fighter2.ID) fighter2.RetireCard(card);
    }

}
