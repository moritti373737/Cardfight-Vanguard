using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// �J�[�h�̃X�L���̂����A�R�X�g�̎x�����������Ǘ�����
/// </summary>
public class CostManager : MonoBehaviour
{
    //[SerializeField]
    //private Fighter Fighter1;

    //[SerializeField]
    //private Fighter Fighter2;

    /// <summary>
    /// �R�X�g���x����
    /// </summary>
    /// <param name="card">�X�L���𔭓������J�[�h</param>
    /// <param name="costList"></param>
    /// <returns></returns>
    public async UniTask<bool> PayCost(Card card, List<CostData> costList)
    {
        foreach (var cost in costList)
        {
            if (cost.Type == CostType.None) return true;

            Type type = this.GetType();
            MethodInfo method = type.GetMethod(cost.Type.ToString());
            object[] args = { card, cost };
            bool ret = await (UniTask<bool>)method.Invoke(this, args);
            if (!ret) return false;
        }
        return true;
    }

    /// <summary>
    /// ��D���w�薇���̂Ă�Ƃ����R�X�g���x����
    /// </summary>
    /// <param name="card">�X�L���𔭓������J�[�h</param>
    /// <param name="cost">�R�X�g�f�[�^</param>
    /// <returns></returns>
    public async UniTask<bool> HandToDrop(Card card, CostData cost)
    {
        while (true)
        {
            int result = await UniTask.WhenAny(UniTask.WaitUntil(() => Input.GetButtonDown("Enter")), UniTask.WaitUntil(() => Input.GetButtonDown("Submit")));
            if (result == 0) await SelectManager.Instance.NormalSelected(Tag.Hand, card.FighterID);
            else if (result == 1 && cost.Count == SelectManager.Instance.SelectedCount)
            {
                await SelectManager.Instance.ForceConfirm(Tag.Drop, card.FighterID, Action.MOVE);
                return true;
            }
        }
    }
}