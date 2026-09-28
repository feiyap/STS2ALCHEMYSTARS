using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 灵魂的价值：选 2 张牌变化为灵魂。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsValueOfSoul : AlchemyStarsAncientRelicBase
{
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 2)
        {
            Cancelable = true,
        };

        var selected = (await CardSelectCmd.FromDeckForTransformation(Owner, prefs)).ToList();
        foreach (var card in selected)
            await CardCmd.TransformTo<Soul>(card);

        Flash();
    }
}
