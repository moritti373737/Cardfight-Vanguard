using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// カードのスキルのうち、実際に発動して効果をもたらす部分を管理する
/// </summary>
public class ActivateSkill : MonoBehaviour
{
    IFighter fighter1;
    IFighter fighter2;

    private void Start()
    {
        //fighter1 = GameObject.Find("Fighter1").GetComponents<IFighter>().First(fighter => fighter.enabled);
        //fighter2 = GameObject.Find("Fighter2").GetComponents<IFighter>().First(fighter => fighter.enabled);
    }

    /// <summary>
    /// 実際にスキルを発動させる
    /// </summary>
    /// <param name="card">スキルを発動したカード</param>
    /// <param name="skill">発動させるスキル</param>
    public void Activate(Card card, List<MainSkillData> skill)
    {
        foreach (var data in skill)
        {
            Card targetCard = null;

            switch (data.TargetCard)
            {
                case ConditionType.None:
                    break;
                case ConditionType.Own:
                    targetCard = card;
                    break;
                default:
                    break;
            }

            switch (data.Type)
            {
                case SkillType.PowerUp:
                    PowerUp(targetCard, data.Option);
                    break;
                default:
                    break;
            }
        }
    }

    private void PowerUp(Card card, string power) => card.AddPower(int.Parse(power));

}
