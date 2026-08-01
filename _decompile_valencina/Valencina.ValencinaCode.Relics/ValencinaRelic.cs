using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Relics;

public abstract class ValencinaRelic(bool autoAdd = true) : ModRelicTemplate()
{
	public bool AutoAddToRelicPool { get; } = autoAdd;

	public override string? CustomIconPath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".RelicImagePath();
			}
			return text;
		}
	}

	public override string? CustomIconOutlinePath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + "_outline.png").RelicImagePath();
			if (ResourceLoader.Exists(text, ""))
			{
				return text;
			}
			string text2 = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").RelicImagePath();
			if (ResourceLoader.Exists(text2, ""))
			{
				return text2;
			}
			string text3 = "relic.png".RelicImagePath();
			if (!ResourceLoader.Exists(text3, ""))
			{
				return text2;
			}
			return text3;
		}
	}

	public override string? CustomBigIconPath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").BigRelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".BigRelicImagePath();
			}
			return text;
		}
	}
}
