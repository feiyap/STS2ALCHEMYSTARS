using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 纸伤：受到伤害时减少最大生命�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsPaperWoundPower : ModPowerTemplate
{
    private const decimal MaxHpLossPerStack = 2m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.PaperWound];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.WasFullyBlocked || result.UnblockedDamage <= 0m || Amount <= 0)
            return;

        await CreatureCmd.LoseMaxHp(
            choiceContext,
            Owner,
            MaxHpLossPerStack * Amount,
            isFromCard: cardSource != null);
    }
}
