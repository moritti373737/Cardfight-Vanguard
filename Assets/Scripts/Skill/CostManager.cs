using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;



/// <summary>
/// カードのスキルのうち、コストの支払い部分を管理する
/// </summary>
public class CostManager
{
    private readonly PlayerInput input;

    public CostManager(PlayerInput input)
    {
        this.input = input;
    }

    //[SerializeField]
    //private Fighter Fighter1;

    //[SerializeField]
    //private Fighter Fighter2;

    /// <summary>
    /// コストを支払う
    /// </summary>
    /// <param name="card">スキルを発動したカード</param>
    /// <param name="costList"></param>
    /// <returns></returns>
    public async UniTask<bool> PayCost(Card card, List<CostData> costList)
    {
        foreach (var cost in costList)
        {
            if (cost.Cost == CostType.None) return true;

            Type type = this.GetType();
            MethodInfo method = type.GetMethod(cost.Cost.ToString());
            object[] args = { card, cost };
            bool ret = await (UniTask<bool>)method.Invoke(this, args);
            if (!ret) return false;
        }
        return true;
    }

    public async UniTask<bool> CounterBlast(Card card, CostData cost)
    {
        while (true)
        {
            Debug.Log(1);
            await UniTask.NextFrame();

            int result = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            if (result == 0)
            {
                Debug.Log(2);
                Card damageCard = SelectManager.Instance.GetSelectCard(Tag.Damage, card.FighterID);
                if (damageCard != null && damageCard.JudgeState(Card.StateType.FaceUp))
                {
                    SelectManager.Instance.NormalSelected(Tag.Damage, card.FighterID);
                }
            }
            else if (result == 1 && cost.Count == SelectManager.Instance.SelectedCount)
            {
                Debug.Log(3);
                await SelectManager.Instance.ForceConfirm(Tag.Damage, card.FighterID, Action.CounterBlast);
                return true;
            }
        }
    }


    /// <summary>
    /// 手札を指定枚数捨てるというコストを支払う
    /// </summary>
    /// <param name="card">スキルを発動したカード</param>
    /// <param name="cost">コストデータ</param>
    /// <returns></returns>
    public async UniTask<bool> HandToDrop(Card card, CostData cost)
    {
        while (true)
        {
            await UniTask.NextFrame();

            int result = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            if (result == 0) SelectManager.Instance.NormalSelected(Tag.Hand, card.FighterID);
            else if (result == 1 && cost.Count == SelectManager.Instance.SelectedCount)
            {
                await SelectManager.Instance.ForceConfirm(Tag.Drop, card.FighterID, Action.MOVE);
                return true;
            }
        }
    }

    //public async UniTask<bool> HandToDeck(Card card, CostData cost)
    //{
    //    while (true)
    //    {
    //        await UniTask.NextFrame();

    //        int result = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        if (result == 0) SelectManager.Instance.NormalSelected(Tag.Hand, card.FighterID);
    //        else if (result == 1 && cost.Count == SelectManager.Instance.SelectedCount)
    //        {
    //            await SelectManager.Instance.ForceConfirm(Tag.Drop, card.FighterID, Action.MOVE);
    //            return true;
    //        }
    //    }
    //}
}