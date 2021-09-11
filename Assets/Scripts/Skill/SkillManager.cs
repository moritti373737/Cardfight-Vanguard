using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

/// <summary>
/// カードのスキル全般を管理する
/// </summary>
public class SkillManager : SingletonMonoBehaviour<SkillManager>
{
    [SerializeField]
    CostManager costManager;

    [SerializeField]
    ActivateSkill activateSkill;

    [SerializeField]
    Fighter Fighter1;
    [SerializeField]
    Fighter Fighter2;

    public ReactiveCollection<Skill> AutomaticSkill = new ReactiveCollection<Skill>();
    public List<(int CardID, Skill Skill)> ActivatedSkill = new List<(int, Skill)>();
    public ReactiveCollection<Skill> ContinuousSkill = new ReactiveCollection<Skill>();

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// ファイトに使用するカードのスキルを収集する
    /// </summary>
    /// <param name="card"></param>
    public void InitSkill(Card card)
    {
        if (card.Skill == null) return;
        Skill skill = card.Skill.SkillList[0];
        if (skill.category == CategoryType.Automatic) AutomaticSkill.Add(skill);
        else if (skill.category == CategoryType.Activated) ActivatedSkill.Add((card.ID, skill));
        else if (skill.category == CategoryType.Continuous) ContinuousSkill.Add(skill);
        print(ActivatedSkill.Count);
        ActivatedSkill.ToList().ForEach(a => print(a.CardID));
    }

    /// <summary>
    /// 指定したカードの指定したジャンル（自起永）のスキルが存在するか調べる
    /// </summary>
    /// <param name="category">自 or起 or 永</param>
    /// <param name="cardID">検索したいカードの固有ID</param>
    /// <returns>検索結果</returns>
    public List<Skill> SearchSkill(CategoryType category, int cardID)
    {
        return category switch
        {
            CategoryType.Automatic => AutomaticSkill.ToList(),
            CategoryType.Activated => ActivatedSkill.Where(skill => skill.CardID == cardID).Select(skill => skill.Skill).ToList(),
            CategoryType.Continuous => ContinuousSkill.ToList(),
            _ => null,
        };
    }

    /// <summary>
    /// 起動能力を条件を確認してから発動する
    /// </summary>
    /// <param name="card">起動能力を発動したいカード</param>
    /// <param name="skillIndex">選択した能力の位置（複数能力持ち）</param>
    /// <returns>起動能力の発動に成功したか</returns>
    public async UniTask<bool> StartActivate(Card card, int skillIndex)
    {
        Skill skill = SearchSkill(CategoryType.Activated, card.ID)[skillIndex];
        //if (!abilityList.Any()) return false;
        if (!card.Parent.transform.tag.Contains(skill.place.ToString())) return false;

        if (!await costManager.PayCost(card, skill.cost)) return false;

        activateSkill.Activate(card, skill.skill);

        Debug.Log($"{card.Name} (id = {card.ID}) のスキル発動！ {skill.place} で {skill.cost[0].Type} を {skill.cost[0].Count} 枚行うことで →");
        Debug.Log($"  → {skill.finish} まで {skill.skill[0].TargetCard} に {skill.skill[0].Option} だけ {skill.skill[0].Type} の効果を与える");

        return true;
    }
}
