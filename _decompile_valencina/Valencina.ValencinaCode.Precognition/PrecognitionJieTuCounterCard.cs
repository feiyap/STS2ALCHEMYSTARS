using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Precognition;

public sealed class PrecognitionJieTuCounterCard : ValencinaCard, IPrecognitionVirtualCounterCard, IInstantAttackCard
{
	private readonly record struct CounterAttackResult(int LandedHits, int UnblockedDamage)
	{
		public static CounterAttackResult None => new CounterAttackResult(0, 0);

		public static CounterAttackResult operator +(CounterAttackResult a, CounterAttackResult b)
		{
			return new CounterAttackResult(a.LandedHits + b.LandedHits, a.UnblockedDamage + b.UnblockedDamage);
		}
	}

	private const string CounterVfx = "vfx/vfx_attack_slash";

	private ValencinaCounterDefinition Definition
	{
		get
		{
			if (!((AbstractModel)this).IsMutable)
			{
				return ValencinaCounterLevelHelper.GetDefinition(0);
			}
			return ValencinaCounterLevelHelper.GetDefinition(((CardModel)this).Owner);
		}
	}

	public override bool CanBeGeneratedInCombat => false;

	public int InstantAmmoCost => Definition.AmmoCost;

	public override bool SpendsAmmo => Definition.AmmoCost > 0;

	public override int AmmoSpendPreviewAmount => Definition.AmmoCost;

	public override string CustomPortraitPath => Definition.Style switch
	{
		ValencinaCounterStyle.JieTu => "jie_tu.png".BigCardImagePath(), 
		ValencinaCounterStyle.JieLu => "jie_lu.png".BigCardImagePath(), 
		ValencinaCounterStyle.JieXiang => "jie_xiang.png".BigCardImagePath(), 
		_ => "basic_counter.png".BigCardImagePath(), 
	};

	public override string PortraitPath => Definition.Style switch
	{
		ValencinaCounterStyle.JieTu => "jie_tu.png".CardImagePath(), 
		ValencinaCounterStyle.JieLu => "jie_lu.png".CardImagePath(), 
		ValencinaCounterStyle.JieXiang => "jie_xiang.png".CardImagePath(), 
		_ => "basic_counter.png".CardImagePath(), 
	};

	public override string BetaPortraitPath => Definition.Style switch
	{
		ValencinaCounterStyle.JieTu => "beta/jie_tu.png".CardImagePath(), 
		ValencinaCounterStyle.JieLu => "beta/jie_lu.png".CardImagePath(), 
		ValencinaCounterStyle.JieXiang => "beta/jie_xiang.png".CardImagePath(), 
		_ => "beta/basic_counter.png".CardImagePath(), 
	};

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			HashSet<CardKeyword> emitted = new HashSet<CardKeyword>();
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				if (emitted.Add(canonicalKeyword))
				{
					yield return canonicalKeyword;
				}
			}
			if (emitted.Add(ValencinaKeywords.Counter))
			{
				yield return ValencinaKeywords.Counter;
			}
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(Definition.Damage, (ValueProp)((Definition.Style == ValencinaCounterStyle.BaseCounter) ? 12 : 8)),
		new DynamicVar("Amount", (decimal)Definition.AmmoCost)
	});

	public PrecognitionJieTuCounterCard()
		: base(0, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	public Task<bool> TriggerFromPrecognition(PrecognitionCounterContext context)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return ExecuteCounterEffectAsync((PlayerChoiceContext)new BlockingPlayerChoiceContext(), context.Attacker, context.IsActiveTrigger, context.FastAnimation);
	}

	private async Task<bool> ExecuteCounterEffectAsync(PlayerChoiceContext choiceContext, Creature target, bool isActiveTrigger, bool fastAnimation)
	{
		ValencinaCounterDefinition definition = ValencinaCounterLevelHelper.GetDefinition(((CardModel)this).Owner);
		Player owner = ((CardModel)this).Owner;
		Creature ownerCreature = ((owner != null) ? owner.Creature : null);
		if (ownerCreature == null)
		{
			return false;
		}
		if (definition.AmmoCost > 0)
		{
			await AmmoSystem.TryConsumeAsync(ownerCreature, definition.AmmoCost, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		}
		int breathingMethodToPreserve = ((definition.Style != ValencinaCounterStyle.BaseCounter) ? BreathingMethodStateHelper.GetAmount(ownerCreature) : 0);
		if (breathingMethodToPreserve > 0)
		{
			InstantAttackBreathingMethodRegistry.Begin(ownerCreature, breathingMethodToPreserve);
		}
		try
		{
			Player owner2 = ((CardModel)this).Owner;
			decimal damageMultiplier = ((((owner2 != null) ? owner2.GetRelic<RevengeLedgerAppendix>() : null) != null) ? 2m : 1m);
			IReadOnlyList<ValencinaCounterDefinition> stackedDefinitions = ValencinaCounterLevelHelper.GetStackedDefinitions(((CardModel)this).Owner);
			decimal maxDamage = ((stackedDefinitions.Count > 0) ? stackedDefinitions.Max((ValencinaCounterDefinition styleDefinition) => styleDefinition.Damage) : definition.Damage);
			int hitCount = ((stackedDefinitions.Count > 0) ? stackedDefinitions.Max((ValencinaCounterDefinition styleDefinition) => styleDefinition.BaseHitCount) : definition.BaseHitCount);
			bool unpowered = stackedDefinitions.All((ValencinaCounterDefinition styleDefinition) => styleDefinition.Style == ValencinaCounterStyle.BaseCounter);
			bool hadBurnOrTremor = StatusSystem.HasBurnOrTremor(target);
			CounterAttackResult total = await ExecuteCounterHitsAsync(choiceContext, target, maxDamage * damageMultiplier, hitCount, fastAnimation, unpowered);
			if (stackedDefinitions.FirstOrDefault((ValencinaCounterDefinition styleDefinition) => styleDefinition.Style == ValencinaCounterStyle.JieTu) != null && hadBurnOrTremor && target.IsAlive)
			{
				CounterAttackResult counterAttackResult = total;
				total = counterAttackResult + await ExecuteCounterHitsAsync(choiceContext, target, maxDamage * damageMultiplier, 1, fastAnimation: true, unpowered: false);
			}
			using (IEnumerator<ValencinaCounterDefinition> enumerator = stackedDefinitions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.Style)
					{
					case ValencinaCounterStyle.JieLu:
						if (total.UnblockedDamage > 0 && target.IsAlive)
						{
							await ApplyRandomBurnOrTremorAsync(choiceContext, target, total.UnblockedDamage);
						}
						break;
					case ValencinaCounterStyle.JieXiang:
						await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: true, choiceContext);
						break;
					}
				}
			}
			return total.LandedHits > 0;
		}
		finally
		{
			if (breathingMethodToPreserve > 0)
			{
				InstantAttackBreathingMethodRegistry.End(ownerCreature);
			}
		}
	}

	private async Task ApplyRandomBurnOrTremorAsync(PlayerChoiceContext choiceContext, Creature target, int amount)
	{
		Player owner = ((CardModel)this).Owner;
		int num;
		if (owner == null)
		{
			num = 0;
		}
		else
		{
			IRunState runState = owner.RunState;
			int? obj;
			if (runState == null)
			{
				obj = null;
			}
			else
			{
				RunRngSet rng = runState.Rng;
				obj = ((rng != null) ? new int?(rng.Niche.NextInt(0, 2)) : ((int?)null));
			}
			num = ((obj == 0) ? 1 : 0);
		}
		if (num != 0)
		{
			await StatusSystem.ApplyTremorAsync(target, amount, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
		}
		else
		{
			await StatusSystem.ApplyBurnAsync(target, amount, (CardModel?)(object)this, choiceContext);
		}
	}

	private async Task<CounterAttackResult> ExecuteCounterHitsAsync(PlayerChoiceContext choiceContext, Creature? target, decimal damage, int hitCount, bool fastAnimation, bool unpowered)
	{
		if (target == null || target.IsDead || !target.IsAlive)
		{
			return CounterAttackResult.None;
		}
		hitCount = Math.Max(1, hitCount);
		AttackCommand command = CommonActions.CardAttack((CardModel)(object)this, target, damage, hitCount, "vfx/vfx_attack_slash");
		if (unpowered)
		{
			command.Unpowered();
		}
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount, playOnEveryHit: true, fastAnimation ? new ulong?(180uL) : ((ulong?)null));
		await ExecuteAttackCommandAsync(choiceContext, command);
		int num = 0;
		int num2 = 0;
		foreach (DamageResult item in from result in CommonActions.DamageResults(command)
			where result.Receiver == target
			select result)
		{
			num++;
			num2 += item.UnblockedDamage;
		}
		return new CounterAttackResult(num, num2);
	}
}
