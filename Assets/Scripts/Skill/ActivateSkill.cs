using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �J�[�h�̃X�L���̂����A���ۂɔ������Č��ʂ������炷�������Ǘ�����
/// </summary>
public class ActivateSkill : MonoBehaviour
{
    [SerializeField]
    Fighter Fighter1;
    [SerializeField]
    Fighter Fighter2;

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
