using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灵杖庇佑：每回合开始时选择消耗 1 张牌，获得 1 层灼燃并恢复 2 点体力。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSpiritStaffBlessingPower : ModPowerTemplate
{
    private const int HealAmount = 2;
    private const int IgnitionAmount = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(HealAmount),
        new PowerVar<AlchemyStarsIgnitionPower>(IgnitionAmount)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        var hand = PileType.Hand.GetPile(player).Cards.ToList();
        if (hand.Count == 0)
            return;

        Flash();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        var selected = (await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this)).ToList();
        foreach (var card in selected)
            await CardCmd.Exhaust(choiceContext, card);

        if (selected.Count == 0)
            return;

        await PowerCmd.Apply<AlchemyStarsIgnitionPower>(
            choiceContext,
            Owner,
            IgnitionAmount,
            Owner,
            null);
        await CreatureCmd.Heal(Owner, HealAmount);
    }
}
