using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 温德岚之日：从抽牌堆与弃牌堆各抽 1 张牌；升级后额外获得随机光能。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsUncommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");
    public AlchemyStarsUncommon3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1m, Owner);

        var discardPile = PileType.Discard.GetPile(Owner);
        var fromDiscard = Owner.RunState.Rng.CombatCardSelection.NextItem(discardPile.Cards);
        if (fromDiscard != null)
            await CardPileCmd.Add(fromDiscard, PileType.Hand);

        if (IsUpgraded)
            LightMechanic.TryGrantRandomBaseLightEnergy(Owner);
    }
}
