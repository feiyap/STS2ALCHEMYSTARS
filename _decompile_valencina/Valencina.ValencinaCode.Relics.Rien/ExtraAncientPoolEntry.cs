using System;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Relics.Rien;

internal sealed record ExtraAncientPoolEntry(Type RelicType, Func<RelicModel> CreateCanonical);
