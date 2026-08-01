using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Relics;

public sealed class BernoullitMemory : ValencinaRelic
{
	private int _counterStyle;

	private bool _counterStyleUpgraded;

	private int _stackedStyleMask;

	private int _stackedUpgradedMask;

	private int _stackedStyleOrder;

	public override RelicRarity Rarity => (RelicRarity)1;

	public ValencinaCounterDefinition CurrentCounterDefinition => CreateDefinitionFromSavedState();

	[SavedProperty]
	public int StackedStyleMask
	{
		get
		{
			return _stackedStyleMask;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_stackedStyleMask = value;
		}
	}

	[SavedProperty]
	public int StackedUpgradedMask
	{
		get
		{
			return _stackedUpgradedMask;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_stackedUpgradedMask = value;
		}
	}

	[SavedProperty]
	public int StackedStyleOrder
	{
		get
		{
			return _stackedStyleOrder;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_stackedStyleOrder = value;
		}
	}

	[SavedProperty]
	public int CounterStyleValue
	{
		get
		{
			return _counterStyle;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_counterStyle = (Enum.IsDefined(typeof(ValencinaCounterStyle), value) ? value : 0);
			RefreshCounterDisplay();
		}
	}

	[SavedProperty]
	public bool CounterStyleUpgraded
	{
		get
		{
			return _counterStyleUpgraded;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_counterStyleUpgraded = value;
			RefreshCounterDisplay();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[7]
	{
		new DynamicVar("CounterDamage", 3m),
		new DynamicVar("CounterAmmo", 0m),
		new DynamicVar("IsBase", 1m),
		new DynamicVar("IsJieTu", 0m),
		new DynamicVar("IsJieLu", 0m),
		new DynamicVar("IsJieXiang", 0m),
		new DynamicVar("IsPalermo", 0m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			SyncCounterDescriptionVars();
			yield return HoverTipFactory.FromKeyword(ValencinaKeywords.Counter);
			yield return (IHoverTip)new CardHoverTip(CreateCurrentCounterHoverCard());
			IReadOnlyList<ValencinaCounterDefinition> activeCounterDefinitions;
			try
			{
				activeCounterDefinitions = GetActiveCounterDefinitions();
			}
			catch
			{
				yield break;
			}
			ValencinaCounterStyle currentStyle = CurrentCounterDefinition.Style;
			bool skippedCurrentStyle = false;
			foreach (ValencinaCounterDefinition item in activeCounterDefinitions)
			{
				if (!skippedCurrentStyle && item.Style == currentStyle)
				{
					skippedCurrentStyle = true;
					continue;
				}
				CardModel val = TryCreateStyleHoverCard(item);
				if (val != null)
				{
					yield return (IHoverTip)new CardHoverTip(val);
				}
			}
		}
	}

	private static CardModel? TryCreateStyleHoverCard(ValencinaCounterDefinition definition)
	{
		try
		{
			CardModel val = ValencinaCounterLevelHelper.GetPreviewCardForStyle(definition.Style).ToMutable();
			if (definition.Upgraded)
			{
				val.UpgradeInternal();
				val.FinalizeUpgradeInternal();
			}
			return val;
		}
		catch
		{
			return null;
		}
	}

	private void SyncCounterDescriptionVars()
	{
		try
		{
			ValencinaCounterDefinition currentCounterDefinition = CurrentCounterDefinition;
			((RelicModel)this).DynamicVars["CounterDamage"].BaseValue = currentCounterDefinition.Damage;
			((RelicModel)this).DynamicVars["CounterAmmo"].BaseValue = currentCounterDefinition.AmmoCost;
			((RelicModel)this).DynamicVars["IsBase"].BaseValue = ((currentCounterDefinition.Style == ValencinaCounterStyle.BaseCounter) ? 1m : 0m);
			((RelicModel)this).DynamicVars["IsJieTu"].BaseValue = ((currentCounterDefinition.Style == ValencinaCounterStyle.JieTu) ? 1m : 0m);
			((RelicModel)this).DynamicVars["IsJieLu"].BaseValue = ((currentCounterDefinition.Style == ValencinaCounterStyle.JieLu) ? 1m : 0m);
			((RelicModel)this).DynamicVars["IsJieXiang"].BaseValue = ((currentCounterDefinition.Style == ValencinaCounterStyle.JieXiang) ? 1m : 0m);
			((RelicModel)this).DynamicVars["IsPalermo"].BaseValue = ((currentCounterDefinition.Style == ValencinaCounterStyle.PalermoSwordplaySecret) ? 1m : 0m);
		}
		catch
		{
		}
	}

	public void ReplaceCounterStyle(ICounterStyleCard counterStyleCard, bool upgraded)
	{
		((AbstractModel)this).AssertMutable();
		_counterStyle = (int)counterStyleCard.Style;
		_counterStyleUpgraded = upgraded;
		int num = 1 << (int)counterStyleCard.Style;
		_stackedStyleMask |= num;
		AppendStyleToOrder(counterStyleCard.Style);
		if (upgraded)
		{
			_stackedUpgradedMask |= num;
		}
		RefreshCounterDisplay();
		((RelicModel)this).Flash();
	}

	public void UpgradeAllCounterStyles()
	{
		((AbstractModel)this).AssertMutable();
		_counterStyleUpgraded = true;
		_stackedUpgradedMask |= _stackedStyleMask;
		RefreshCounterDisplay();
		((RelicModel)this).Flash();
	}

	public IReadOnlyList<ValencinaCounterDefinition> GetActiveCounterDefinitions()
	{
		List<ValencinaCounterDefinition> list = new List<ValencinaCounterDefinition>();
		Player? obj = TryGetOwner();
		if (((obj != null) ? obj.GetRelic<RecordOfThatDay>() : null) == null)
		{
			list.Add(CurrentCounterDefinition);
			return list;
		}
		HashSet<ValencinaCounterStyle> hashSet = new HashSet<ValencinaCounterStyle>();
		foreach (ValencinaCounterStyle orderedStackedStyle in GetOrderedStackedStyles())
		{
			if (orderedStackedStyle != ValencinaCounterStyle.BaseCounter && hashSet.Add(orderedStackedStyle))
			{
				int num = 1 << (int)orderedStackedStyle;
				if ((_stackedStyleMask & num) != 0)
				{
					bool upgraded = _counterStyleUpgraded || (_stackedUpgradedMask & num) != 0;
					list.Add(CreateDefinitionFor(orderedStackedStyle, upgraded));
				}
			}
		}
		ValencinaCounterStyle valencinaCounterStyle = (Enum.IsDefined(typeof(ValencinaCounterStyle), _counterStyle) ? ((ValencinaCounterStyle)_counterStyle) : ValencinaCounterStyle.BaseCounter);
		if (valencinaCounterStyle == ValencinaCounterStyle.BaseCounter || hashSet.Add(valencinaCounterStyle))
		{
			list.Add(CurrentCounterDefinition);
		}
		if (list.Count == 0)
		{
			list.Add(CurrentCounterDefinition);
		}
		return list;
	}

	private void AppendStyleToOrder(ValencinaCounterStyle style)
	{
		if (style == ValencinaCounterStyle.BaseCounter || HasStyleInOrder(style))
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			int num = i * 4;
			if (((_stackedStyleOrder >> num) & 0xF) == 0)
			{
				_stackedStyleOrder |= (int)style << num;
				break;
			}
		}
	}

	private bool HasStyleInOrder(ValencinaCounterStyle style)
	{
		return GetStylesFromOrder().Contains(style);
	}

	private IEnumerable<ValencinaCounterStyle> GetOrderedStackedStyles()
	{
		HashSet<ValencinaCounterStyle> emitted = new HashSet<ValencinaCounterStyle>();
		foreach (ValencinaCounterStyle item in GetStylesFromOrder())
		{
			if (emitted.Add(item))
			{
				yield return item;
			}
		}
		ValencinaCounterStyle[] values = Enum.GetValues<ValencinaCounterStyle>();
		foreach (ValencinaCounterStyle valencinaCounterStyle in values)
		{
			if (valencinaCounterStyle != ValencinaCounterStyle.BaseCounter && !emitted.Contains(valencinaCounterStyle))
			{
				int num = 1 << (int)valencinaCounterStyle;
				if ((_stackedStyleMask & num) != 0 && emitted.Add(valencinaCounterStyle))
				{
					yield return valencinaCounterStyle;
				}
			}
		}
	}

	private IEnumerable<ValencinaCounterStyle> GetStylesFromOrder()
	{
		for (int slot = 0; slot < 8; slot++)
		{
			int num = (_stackedStyleOrder >> slot * 4) & 0xF;
			if (num != 0 && Enum.IsDefined(typeof(ValencinaCounterStyle), num))
			{
				yield return (ValencinaCounterStyle)num;
			}
		}
	}

	public override bool ShouldAddToDeck(CardModel card)
	{
		if (((RelicModel)this).Owner == null || card.Owner != ((RelicModel)this).Owner || !(card is ICounterStyleCard counterStyleCard))
		{
			return true;
		}
		bool upgraded = card.CurrentUpgradeLevel > 0 || ((RelicModel)this).Owner.GetRelic<RevengeLedgerAppendix>() != null;
		ReplaceCounterStyle(counterStyleCard, upgraded);
		return false;
	}

	public override Task AfterAddToDeckPrevented(CardModel card)
	{
		if (card is ICounterStyleCard)
		{
			Player owner = ((RelicModel)this).Owner;
			if (owner != null)
			{
				IRunState runState = owner.RunState;
				if (((runState != null) ? new bool?(runState.ContainsCard(card)) : ((bool?)null)) == true)
				{
					((ICardScope)((RelicModel)this).Owner.RunState).RemoveCard(card);
				}
			}
		}
		return Task.CompletedTask;
	}

	private ValencinaCounterDefinition CreateDefinitionFromSavedState()
	{
		return CreateDefinitionFor(Enum.IsDefined(typeof(ValencinaCounterStyle), _counterStyle) ? ((ValencinaCounterStyle)_counterStyle) : ValencinaCounterStyle.BaseCounter, _counterStyleUpgraded);
	}

	private static ValencinaCounterDefinition CreateDefinitionFor(ValencinaCounterStyle style, bool upgraded)
	{
		return style switch
		{
			ValencinaCounterStyle.JieTu => new ValencinaCounterDefinition(style, "JieTu", 0, upgraded ? 5m : 4m, 1, upgraded), 
			ValencinaCounterStyle.JieLu => new ValencinaCounterDefinition(style, "JieLu", 0, upgraded ? 4m : 3m, 1, upgraded), 
			ValencinaCounterStyle.JieXiang => new ValencinaCounterDefinition(style, "JieXiang", 0, upgraded ? 6m : 5m, 1, upgraded), 
			ValencinaCounterStyle.PalermoSwordplaySecret => new ValencinaCounterDefinition(style, "PalermoSwordplaySecret", upgraded ? 3 : 2, 3m, 1, upgraded), 
			_ => new ValencinaCounterDefinition(ValencinaCounterStyle.BaseCounter, "BaseCounter", 0, 3m, 1), 
		};
	}

	private void RefreshCounterDisplay()
	{
		SyncCounterDescriptionVars();
		((RelicModel)this).InvokeDisplayAmountChanged();
	}

	private CardModel CreateCurrentCounterHoverCard()
	{
		Player? player = TryGetOwner();
		ValencinaCounterDefinition currentCounterDefinition = CurrentCounterDefinition;
		CardModel val = ValencinaCounterLevelHelper.GetPreviewCard(player).ToMutable();
		if (currentCounterDefinition.Upgraded)
		{
			val.UpgradeInternal();
			val.FinalizeUpgradeInternal();
		}
		return val;
	}

	private Player? TryGetOwner()
	{
		try
		{
			return ((AbstractModel)this).IsMutable ? ((RelicModel)this).Owner : null;
		}
		catch
		{
			return null;
		}
	}
}
