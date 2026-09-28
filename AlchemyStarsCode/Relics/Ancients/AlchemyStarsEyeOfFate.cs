using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 命运之眼：战斗开始展示 5 张职业稀有牌，选 1 张入手牌（保留，费用 -1）。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsEyeOfFate : AlchemyStarsAncientRelicBase
{
    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner == null)
            return;

        if (Owner.PlayerCombatState.TurnNumber != 1)
            return;

        // 必须经 CombatState.CreateCard（GetDistinctForCombat）登记，
        // CreateForReward 生成的牌不在 CombatState._allCards，打出时会报错。
        var rarePool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity == CardRarity.Rare);
        var offered = CardFactory.GetDistinctForCombat(
            Owner,
            rarePool,
            5,
            Owner.RunState.Rng.CombatCardGeneration).ToList();

        if (offered.Count == 0)
            return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
        var picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            offered,
            Owner,
            prefs)).FirstOrDefault() ?? offered[0];

        CardCmd.ApplyKeyword(picked, CardKeyword.Retain);
        if (!picked.EnergyCost.CostsX)
            picked.EnergyCost.UpgradeBy(-1);

        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(picked, PileType.Hand, Owner);
    }
}
