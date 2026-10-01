using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 四色燃尽：自身不抵消减益。战斗开始和每 4 回合由祭剑座获得人工制品。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFourColorBurnoutPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsFourColorBurnoutPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<ArtifactPower>()];
}
