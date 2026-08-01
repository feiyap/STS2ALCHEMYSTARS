using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Compat;

public static class CompatHoverTips
{
	public static IHoverTip FromPower<TPower>() where TPower : PowerModel
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return (IHoverTip)(object)((PowerModel)ModelDb.Power<TPower>()).DumbHoverTip;
	}
}
