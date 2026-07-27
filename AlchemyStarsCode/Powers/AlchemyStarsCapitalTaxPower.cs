using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Powers;

/// <summary>
/// ????????????????????????????????
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCapitalTaxPower : ModPowerTemplate
{
    private const int TaxGold = 20;
    private const decimal DamageIncreaseRate = 0.2m;

    private static readonly AttachedState<CardModel, int> TaxPaidGold = new(_ => 0);
    private static readonly AttachedState<CardModel, decimal> DamageBonusRate = new(_ => 0m);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => ["on_capital"];

    public static int TaxAmount => TaxGold;

    public static decimal DamageIncreasePerTax => DamageIncreaseRate;

    public static void RecordTax(CardModel card, int goldPaid)
    {
        TaxPaidGold[card] += goldPaid;
        DamageBonusRate[card] += DamageIncreaseRate;
    }

    public static decimal GetDamageBonusRate(CardModel card) => DamageBonusRate[card];

    public static async Task RefundOnVictory(Player player, CardModel card, bool playerSurvived)
    {
        var paid = TaxPaidGold[card];
        if (paid <= 0)
            return;

        var refundRate = playerSurvived ? 1.2m : 1.0m;
        await PlayerCmd.GainGold((int)(paid * refundRate), player);
        TaxPaidGold[card] = 0;
        DamageBonusRate[card] = 0m;
    }
}
