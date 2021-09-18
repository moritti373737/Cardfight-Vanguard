using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �J�[�h�̃X�L���̂����A���ۂɔ������Č��ʂ������炷�������Ǘ�����
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
    /// ���ۂɃX�L���𔭓�������
    /// </summary>
    /// <param name="card">�X�L���𔭓������J�[�h</param>
    /// <param name="skill">����������X�L��</param>
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
