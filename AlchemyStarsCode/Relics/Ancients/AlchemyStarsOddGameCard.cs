using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 奇怪的游戏卡：每回合往手牌加入 1 张角色卡池随机牌，本战斗费用为 0 且具有消耗。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsOddGameCard : AlchemyStarsAncientRelicBase
{
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner || Owner == null)
            return;

        // 必须经 CombatState.CreateCard（GetDistinctForCombat）登记，
        // CreateForReward 生成的牌不在 CombatState._allCards，打出时会报错。
        var created = CardFactory.GetDistinctForCombat(
            Owner,
            Owner.Character.CardPool.GetUnlockedCards(
                Owner.UnlockState,
                Owner.RunState.CardMultiplayerConstraint),
            1,
            Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (created == null)
            return;

        created.SetToFreeThisCombat();
        CardCmd.ApplyKeyword(created, CardKeyword.Exhaust);

        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, Owner);
    }
}
