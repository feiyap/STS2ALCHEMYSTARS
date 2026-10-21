using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Events;

/// <summary>
/// 星辰纹章：第 2 回合起每回合生成一张本轮还没出现过的旧印。
/// 四种旧印都生成过一次后的下一回合生成祭剑座并沉睡。
/// 打出祭剑座后唤醒，开始下一轮，并让后续旧印永久降费。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsStarCrest : ModRelicTemplate
{
    private enum OldSealKind
    {
        Frost,
        Guard,
        Rhyme,
        Requiem
    }

    private const int SealsBeforeSleep = 4;

    private int _sealsGeneratedThisCombat;
    private bool _asleepThisCombat;
    private int _permanentCostReduction;
    private readonly List<OldSealKind> _remainingSeals = new();

    /// <summary>
    /// 祭剑座打出后累计的旧印降费。跨战斗保留。
    /// </summary>
    [SavedProperty]
    public int PermanentCostReduction
    {
        get => _permanentCostReduction;
        set
        {
            AssertMutable();
            _permanentCostReduction = value;
        }
    }

    /// <summary>
    /// 打出祭剑座：结束沉睡，并让后续旧印耗能永久 -1。
    /// </summary>
    public void AwakenAfterAltarPlayed()
    {
        AssertMutable();
        _sealsGeneratedThisCombat = 0;
        _asleepThisCombat = false;
        _remainingSeals.Clear();
        PermanentCostReduction++;
        Flash();
        InvokeDisplayAmountChanged();
    }

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
        _remainingSeals.Clear();
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

        Flash();
        if (_sealsGeneratedThisCombat >= SealsBeforeSleep)
        {
            var altar = CreateAltar(player);
            if (altar != null)
                await CardPileCmd.AddGeneratedCardToCombat(altar, PileType.Hand, player);

            _asleepThisCombat = true;
            InvokeDisplayAmountChanged();
            return;
        }

        var seal = CreateNextOldSeal(player);
        if (seal == null)
            return;

        await CardPileCmd.AddGeneratedCardToCombat(seal, PileType.Hand, player);
        _sealsGeneratedThisCombat++;
        InvokeDisplayAmountChanged();
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (PermanentCostReduction <= 0 || Owner == null || card.Owner != Owner)
            return false;

        if (card is not IAlchemyStarsOldSeal)
            return false;

        modifiedCost = Math.Max(0m, originalCost - PermanentCostReduction);
        return modifiedCost != originalCost;
    }

    private CardModel? CreateNextOldSeal(Player player)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return null;

        if (_remainingSeals.Count == 0)
        {
            _remainingSeals.Add(OldSealKind.Frost);
            _remainingSeals.Add(OldSealKind.Guard);
            _remainingSeals.Add(OldSealKind.Rhyme);
            _remainingSeals.Add(OldSealKind.Requiem);
        }

        var rng = player.RunState.Rng.CombatCardGeneration;
        var index = rng.NextInt(_remainingSeals.Count);
        var kind = _remainingSeals[index];
        _remainingSeals.RemoveAt(index);

        return kind switch
        {
            OldSealKind.Frost => combatState.CreateCard<AlchemyStarsOldSealFrost>(player),
            OldSealKind.Guard => combatState.CreateCard<AlchemyStarsOldSealGuard>(player),
            OldSealKind.Rhyme => combatState.CreateCard<AlchemyStarsOldSealRhyme>(player),
            _ => combatState.CreateCard<AlchemyStarsOldSealRequiem>(player)
        };
    }

    private static CardModel? CreateAltar(Player player)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return null;

        return combatState.CreateCard<AlchemyStarsOldSealAltar>(player);
    }
}
