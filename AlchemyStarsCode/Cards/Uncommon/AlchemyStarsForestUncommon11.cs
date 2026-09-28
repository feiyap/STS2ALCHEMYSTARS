using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Minions;
using MinionLib.Commands;
using MinionLib.Minion;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 光之剑甲·海蒂：为所有队友召唤光之剑甲（MinionLib 守护召唤物）。多人模式卡。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon11 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(AlchemyStarsLightSwordArmorMinion.DefaultHp),
        AlchemyStarsKeywordText.InlineTitleVar("LightSwordArmor", AlchemyStarsKeywordIds.LightSwordArmor),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightSwordArmor)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon)
    ];

    public AlchemyStarsForestUncommon11()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        if (CombatState == null)
            return;

        var allies = CombatState.PlayerCreatures
            .Where(creature => creature.IsAlive && creature.IsPlayer)
            .ToList();

        foreach (var ally in allies)
        {
            var allyPlayer = ally.Player;
            if (allyPlayer == null)
                continue;

            var pet = await MinionCmd.AddMinion<AlchemyStarsLightSwordArmorMinion>(
                choiceContext,
                allyPlayer,
                new MinionSummonOptions(
                    MaxHp: DynamicVars.Summon.BaseValue,
                    Source: this,
                    Position: MinionPosition.Front));

            // 与奥提斯相同：守护召唤物显示主人的格挡环。
            NCombatRoom.Instance?.GetCreatureNode(pet)?.TrackBlockStatus(ally);
        }
    }
}
