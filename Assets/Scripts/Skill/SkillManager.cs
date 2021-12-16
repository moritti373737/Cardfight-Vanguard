using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// カードのスキル全般を管理する
/// </summary>
public class SkillManager : SingletonMonoBehaviour<SkillManager>
{
    [SerializeField]
    private PlayerInput input;

    IFighter fighter1;
    IFighter fighter2;

    private CostManager costManager;

    private ConditionManager conditionManager;

    private ActivateSkill activateSkill;

    private readonly List<Card> cardList = new List<Card>();

    public ReactiveCollection<Skill> AutomaticSkill = new ReactiveCollection<Skill>();
    public List<(int CardID, Skill Skill)> ActivatedSkill = new List<(int, Skill)>();
    public List<(int CardID, Skill Skill)> ContinuousSkill = new List<(int, Skill)>();

    public void Start()
    {
        ActionManager.Instance.ActionHistory.ObserveAdd().Subscribe(async x => await StartContinuous(x.Value));
        fighter1 = GameObject.Find("Fighter1").GetComponents<IFighter>().First(fighter => fighter.enabled);
        fighter2 = GameObject.Find("Fighter2").GetComponents<IFighter>().First(fighter => fighter.enabled);
        conditionManager = new ConditionManager(fighter1, fighter2);
        costManager = new CostManager(input);
        activateSkill = new ActivateSkill(input, fighter1, fighter2);
    }

    /// <summary>
    /// ファイトに使用するカードのスキルを収集する
    /// </summary>
    /// <param name="card"></param>
    public void InitSkill(Card card)
    {
        cardList.Add(card);
        if (card.Skill is null) return;
        foreach (var skill in card.Skill.SkillList)
        {
            if (skill.category == CategoryType.Automatic) AutomaticSkill.Add(skill);
            else if (skill.category == CategoryType.Activated) ActivatedSkill.Add((card.ID, skill));
            else if (skill.category == CategoryType.Continuous) ContinuousSkill.Add((card.ID, skill));

        }
        //print(ActivatedSkill.Count);
        //ActivatedSkill.ToList().ForEach(a => print(a.CardID));
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
            CategoryType.Continuous => ContinuousSkill.Where(skill => skill.CardID == cardID).Select(skill => skill.Skill).ToList(),
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

        await activateSkill.Activate(card, skill.skill);

        Debug.Log($"{card.Name} (id = {card.ID}) のスキル発動！ {skill.place} で {skill.cost[0].Cost} を {skill.cost[0].Count} 枚行うことで →");
        Debug.Log($"  → {skill.finish} まで {skill.skill[0].TargetCard} に {skill.skill[0].SkillOption} だけ {skill.skill[0].Skill} の効果を与える");

        return true;
    }

    public async UniTask StartContinuous(ActionData actionData)
    {
        Debug.Log($"{actionData.Card} が {actionData.Source} から、{actionData.Target} に移動した！");

        Card actionCard = actionData.Card;

        //List<Skill> skillList = SearchSkill(CategoryType.Continuous, actionData.Card.ID);
        List<Skill> skillList = ContinuousSkill.Select(skill => skill.Skill).ToList();

        if (!skillList.Any()) return;

        foreach ((int cardID, Skill skill) in ContinuousSkill)
        {
            if (!actionCard.Parent.transform.tag.Contains(skill.place.ToString())) continue;

            if (!conditionManager.CheckCondition(actionData, cardList[cardID], skill)) continue;

            if (!await costManager.PayCost(cardList[cardID], skill.cost)) break;

            await activateSkill.Activate(cardList[cardID], skill.skill);
        }

    }
}