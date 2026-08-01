using System;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class UngezieferKaiserRoomIconFallback
{
	internal static bool ShouldUseKaiserIcon(MapPointType mapPointType, RoomType roomType, ModelId? modelId)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		if (!ValencinaModConfig.EnableKaiserContent)
		{
			return false;
		}
		if ((int)roomType != 3 && (int)mapPointType != 7)
		{
			return false;
		}
		string text = ((modelId != null) ? modelId.Entry : null);
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		if (text.Contains("UNGEZIEFER_KAISER", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}
}
