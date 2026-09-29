using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Events;

/// <summary>
/// 星辰纹章：第 2 回合起每回合生成随机旧印；满 4 张后沉睡（祭剑座未实装前不再生成）。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsStarCrest : ModRelicTemplate
{
    private const int SealsBeforeSleep = 4;

    private int _sealsGeneratedThisCombat;
    private bool _asleepThisCombat;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsAllowed(IRunState runState) => false;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _asleepThisCombat ? 0 : SealsBeforeSleep - _sealsGeneratedThisCombat;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override Task BeforeCombatStart()
    {
        _sealsGeneratedThisCombat = 0;
        _asleepThisCombat = false;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || _asleepThisCombat)
            return;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        if (turn < 2)
            return;

        if (_sealsGeneratedThisCombat >= SealsBeforeSleep)
        {
            // 祭剑座 Boss 卡未实装：达到上限后沉睡。
            _asleepThisCombat = true;
            Flash();
            InvokeDisplayAmountChanged();
            return;
        }

        Flash();
        var seal = CreateRandomOldSeal(player);
        if (seal == null)
            return;

        await CardPileCmd.AddGeneratedCardToCombat(seal, PileType.Hand, player);
        _sealsGeneratedThisCombat++;

        if (_sealsGeneratedThisCombat >= SealsBeforeSleep)
            _asleepThisCombat = true;

        InvokeDisplayAmountChanged();
    }

    private static CardModel? CreateRandomOldSeal(Player player)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return null;

        var candidates = new List<CardModel>
        {
            combatState.CreateCard<AlchemyStarsOldSealFrost>(player),
            combatState.CreateCard<AlchemyStarsOldSealGuard>(player),
            combatState.CreateCard<AlchemyStarsOldSealRhyme>(player),
            combatState.CreateCard<AlchemyStarsOldSealRequiem>(player)
        };

        return player.RunState.Rng.CombatCardGeneration.NextItem(candidates);
    }
}
