using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class AbilityManager : SingletonMonoBehaviour<AbilityManager>
{
    [SerializeField]
    CostManager costManager;

    [SerializeField]
    ActivateAbility activateAbility;

    [SerializeField]
    Fighter Fighter1;
    [SerializeField]
    Fighter Fighter2;

    public ReactiveCollection<Ability> AutomaticAbility = new ReactiveCollection<Ability>();
    public List<(int CardID, Ability Ability)> ActivatedAbility = new List<(int, Ability)>();
    public ReactiveCollection<Ability> ContinuousAbility = new ReactiveCollection<Ability>();

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitAbility(Card card)
    {
        if (card.Ability == null) return;
        Ability ability = card.Ability.AbilityList[0];
        if (ability.category == CategoryType.Automatic) AutomaticAbility.Add(ability);
        else if (ability.category == CategoryType.Activated) ActivatedAbility.Add((card.ID, ability));
        else if (ability.category == CategoryType.Continuous) ContinuousAbility.Add(ability);
        print(ActivatedAbility.Count);
        ActivatedAbility.ToList().ForEach(a => print(a.CardID));
    }

    public List<Ability> SearchAbility(CategoryType category, int cardID)
    {
        return category switch
        {
            CategoryType.Automatic => AutomaticAbility.ToList(),
            CategoryType.Activated => ActivatedAbility.Where(ability => ability.CardID == cardID).Select(ability => ability.Ability).ToList(),
            CategoryType.Continuous => ContinuousAbility.ToList(),
            _ => null,
        };
    }

    public bool StartActivate(Card card, int skillIndex)
    {
        Ability ability = SearchAbility(CategoryType.Activated, card.ID)[skillIndex];
        //if (!abilityList.Any()) return false;
        if (!card.Parent.transform.tag.Contains(ability.place.ToString())) return false;

        if (!costManager.PayCost(ability.cost)) return false;

        activateAbility.Activate(card, ability.ability);

        Debug.Log($"{card.Name} (id = {card.ID}) のスキル発動！ {ability.place} で {ability.cost[0].Type} を {ability.cost[0].Count} 枚行うことで {ability.ability[0].TargetCard} に {ability.ability[0].Option} だけ {ability.ability[0].Type} の効果を与える");

        return true;
    }
}
