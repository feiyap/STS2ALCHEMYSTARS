using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 真理之主遗落赐福：生成属性格时填充空白格，或改写已满转色栏中的一格。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsTruthLordsBlessing : AlchemyStarsAncientRelicBase
{
    [ThreadStatic]
    private static bool _processing;

    public static void OnAttributeCellGenerated(Player player)
    {
        if (_processing)
            return;

        var relic = player.GetRelic<AlchemyStarsTruthLordsBlessing>();
        if (relic == null || !LightMechanic.IsMechanicActive(player))
            return;

        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return;

        _processing = true;
        try
        {
            relic.Flash();
            var cells = state.AttributeCells.Items.ToList();
            var slotLimit = state.AttributeCells.MaxSlots;
            var rng = player.RunState.Rng.Niche;

            if (cells.Count < slotLimit)
            {
                var fill = PickNonDominantElement(cells, rng);
                LightMechanic.TryAddAttributeCell(player, fill);
                return;
            }

            if (cells.Count == 0)
                return;

            var index = rng.NextInt(cells.Count);
            var kinds = new[]
            {
                AttributeCellKind.Enhanced,
                AttributeCellKind.Dark,
                AttributeCellKind.Prism,
            };
            var newElement = PickNonDominantElement(cells, rng);
            var newKind = AttributeCell.NormalizeKind(newElement, rng.NextItem(kinds));
            cells[index] = new AttributeCell(newElement, newKind);
            state.AttributeCells.ReplaceAll(cells);
            state.UpdateRainbowState();
            LightMechanicUiBootstrap.RefreshForPlayer(player);
        }
        finally
        {
            _processing = false;
        }
    }

    private static LightElement PickNonDominantElement(IReadOnlyList<AttributeCell> cells, Rng rng)
    {
        var bases = LightElementExtensions.BaseElements;
        var counts = bases.ToDictionary(e => e, _ => 0);
        foreach (var cell in cells)
        {
            if (cell.Element.IsBaseElement())
                counts[cell.Element]++;
        }

        var max = counts.Values.DefaultIfEmpty(0).Max();
        var candidates = bases.Where(e => counts[e] < max || max == 0).ToArray();
        if (candidates.Length == 0)
            candidates = bases;
        return rng.NextItem(candidates);
    }
}
