using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 奇怪的糖果：拾起时选 1 张能力牌注入 Imbued；每场战斗开始失去 1 点能量。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsWeirdCandy : AlchemyStarsAncientRelicBase
{
    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override bool IncludeEnergyHoverTip => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        // Imbued 原版仅允许技能；此处按设计强制对能力牌注能。
        var selected = (await CardSelectCmd.FromDeckGeneric(
            Owner,
            prefs,
            c => c.Type == CardType.Power && c.Enchantment == null)).FirstOrDefault();

        if (selected == null)
            return;

        var imbued = ModelDb.Enchantment<Imbued>().ToMutable();
        selected.EnchantInternal(imbued, 1m);
        imbued.ModifyCard();
        selected.FinalizeUpgradeInternal();
        CardCmd.Preview([selected]);
        Flash();
    }

    public override async Task AfterEnergyReset(Player player)
    {
        // 战斗开始能量结算后再扣 1，避免 BeforeCombatStart 时尚无战斗能量。
        if (player != Owner)
            return;

        if (Owner.PlayerCombatState.TurnNumber != 1)
            return;

        Flash();
        await PlayerCmd.LoseEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
}
