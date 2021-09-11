using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

/// <summary>
/// �J�[�h�̃X�L���S�ʂ��Ǘ�����
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
    /// �t�@�C�g�Ɏg�p����J�[�h�̃X�L�������W����
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
    /// �w�肵���J�[�h�̎w�肵���W�������i���N�i�j�̃X�L�������݂��邩���ׂ�
    /// </summary>
    /// <param name="category">�� or�N or �i</param>
    /// <param name="cardID">�����������J�[�h�̌ŗLID</param>
    /// <returns>��������</returns>
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
    /// �N���\�͂��������m�F���Ă��甭������
    /// </summary>
    /// <param name="card">�N���\�͂𔭓��������J�[�h</param>
    /// <param name="skillIndex">�I�������\�͂̈ʒu�i�����\�͎����j</param>
    /// <returns>�N���\�͂̔����ɐ���������</returns>
    public async UniTask<bool> StartActivate(Card card, int skillIndex)
    {
        Skill skill = SearchSkill(CategoryType.Activated, card.ID)[skillIndex];
        //if (!abilityList.Any()) return false;
        if (!card.Parent.transform.tag.Contains(skill.place.ToString())) return false;

        if (!await costManager.PayCost(card, skill.cost)) return false;

        activateSkill.Activate(card, skill.skill);

        Debug.Log($"{card.Name} (id = {card.ID}) �̃X�L�������I {skill.place} �� {skill.cost[0].Type} �� {skill.cost[0].Count} ���s�����Ƃ� ��");
        Debug.Log($"  �� {skill.finish} �܂� {skill.skill[0].TargetCard} �� {skill.skill[0].Option} ���� {skill.skill[0].Type} �̌��ʂ�^����");

        return true;
    }
}
