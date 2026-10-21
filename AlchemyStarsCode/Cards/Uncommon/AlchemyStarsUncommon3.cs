using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 温德岚之日：从弃牌堆抽 1 张；升级后再从抽牌堆抽 1 张。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsUncommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
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
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        // 乱战等自动打出会嵌套牌堆动画。快照弃牌堆并跳过演出，避免把正在打出的牌自己捞回来把补间卡死。
        var candidates = new List<CardModel>();
        foreach (var card in PileType.Discard.GetPile(Owner).Cards)
        {
            if (ReferenceEquals(card, this) || card.Pile?.Type != PileType.Discard)
                continue;

            candidates.Add(card);
        }

        var fromDiscard = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (fromDiscard != null)
        {
            await CardPileCmd.Add(
                fromDiscard,
                PileType.Hand,
                CardPilePosition.Bottom,
                null,
                cardPlay.IsAutoPlay);
        }

        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, 1m, Owner);
    }
}
