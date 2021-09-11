using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAbility : MonoBehaviour
{
    [SerializeField]
    Fighter Fighter1;
    [SerializeField]
    Fighter Fighter2;

    public void Activate(Card card, List<MainAbilityData> ability)
    {
        foreach (var data in ability)
        {
            Card targetCard = null;

            switch (data.TargetCard)
            {
                case TargetType.None:
                    break;
                case TargetType.Own:
                    targetCard = card;
                    break;
                default:
                    break;
            }

            switch (data.Type)
            {
                case AbilityType.PowerUp:
                    PowerUp(targetCard, data.Option);
                    break;
                default:
                    break;
            }
        }
    }

    private void PowerUp(Card card, int power) => card.AddPower(power);

}
