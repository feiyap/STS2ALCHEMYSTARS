using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using Valencina.ValencinaCode.Acts;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Potions;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Content;

public static class ValencinaRitsuContent
{
	private static bool _registered;

	public static void Register()
	{
		if (_registered)
		{
			return;
		}
		_registered = true;
		ValencinaKeywords.RegisterAll();
		ModContentRegistry registry = ModContentRegistry.For("Valencina");
		registry.RegisterCharacter(typeof(Valencina.ValencinaCode.Character.Valencina));
		registry.RegisterAct(typeof(ValencinaAct4));
		RegisterPoolContent<ValencinaCard>(delegate(Type type)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Type type2 = (IsGeneratedOrTokenCard((ValencinaCard)Activator.CreateInstance(type), type) ? typeof(TokenCardPool) : typeof(ValencinaCardPool));
			registry.RegisterCard(type2, type, LegacyEntry(type));
		});
		RegisterPoolContent<ValencinaRelic>(delegate(Type type)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (((ValencinaRelic)Activator.CreateInstance(type)).AutoAddToRelicPool)
			{
				registry.RegisterRelic(typeof(ValencinaRelicPool), type, LegacyEntry(type));
			}
		});
		RegisterPoolContent<ValencinaPotion>(delegate(Type type)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			registry.RegisterPotion(typeof(ValencinaPotionPool), type, LegacyEntry(type));
		});
		RegisterConcrete<PowerModel>((Action<Type>)registry.RegisterPower);
		RegisterConcrete<EnchantmentModel>((Action<Type>)registry.RegisterEnchantment);
		RegisterConcrete<MonsterModel>((Action<Type>)registry.RegisterMonster);
		RegisterConcrete<EncounterModel>(delegate(Type type)
		{
			if (type == typeof(UngezieferKaiserEncounter) || type == typeof(ValencinaAct4EliteEncounter))
			{
				registry.RegisterActEncounter(typeof(ValencinaAct4), type);
			}
			else
			{
				registry.RegisterGlobalEncounter(type);
			}
		});
		RegisterConcrete<EventModel>(delegate(Type type)
		{
			if (!typeof(AncientEventModel).IsAssignableFrom(type))
			{
				if (type == typeof(LucioChoiceEvent))
				{
					registry.RegisterActEvent(typeof(ValencinaAct4), type);
				}
				else
				{
					registry.RegisterSharedEvent(type);
				}
			}
		});
		RegisterConcrete<AncientEventModel>(delegate(Type type)
		{
			if (IsFollowUpAncientType(type))
			{
				MainFile.DiagnosticInfo("[RienSecondAncient] Follow-up Ancient " + type.Name + " is event-page only and was not registered into map Ancient pools.");
			}
			else
			{
				registry.RegisterSharedAncient(type);
			}
		});
		registry.RegisterCharacterStarterCard<Valencina.ValencinaCode.Character.Valencina, AccumulatedExperience>(1, 0);
		registry.RegisterCharacterStarterCard<Valencina.ValencinaCode.Character.Valencina, EnduredHumiliation>(1, 10);
		registry.RegisterCharacterStarterCard<Valencina.ValencinaCode.Character.Valencina, ValStrike>(4, 20);
		registry.RegisterCharacterStarterCard<Valencina.ValencinaCode.Character.Valencina, ValDefend>(4, 30);
		registry.RegisterCharacterStarterRelic<Valencina.ValencinaCode.Character.Valencina, ImperfectForesightEye>(1, 0);
		registry.RegisterCharacterStarterRelic<Valencina.ValencinaCode.Character.Valencina, BernoullitMemory>(1, 10);
		RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<ImperfectForesightEye, CompleteForesightEye>("Valencina");
	}

	private static bool IsGeneratedOrTokenCard(ValencinaCard card, Type type)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		if (!card.AutoAddToCardPool || !((CardModel)card).ShouldShowInCardLibrary)
		{
			return true;
		}
		CardRarity rarity = ((CardModel)card).Rarity;
		if (rarity - 7 <= 2)
		{
			return true;
		}
		return type.Namespace?.Contains(".Precognition", StringComparison.Ordinal) ?? false;
	}

	private static bool IsFollowUpAncientType(Type type)
	{
		if (!(type == typeof(ThumbAdvisor)) && !(type == typeof(LimbusCompanyHeadquarters)))
		{
			return type == typeof(Rien);
		}
		return true;
	}

	private static void RegisterPoolContent<TBase>(Action<Type> register)
	{
		foreach (Type item in ConcreteTypes<TBase>())
		{
			register(item);
		}
	}

	private static void RegisterConcrete<TBase>(Action<Type> register)
	{
		foreach (Type item in ConcreteTypes<TBase>())
		{
			register(item);
		}
	}

	private static IEnumerable<Type> ConcreteTypes<TBase>()
	{
		Type baseType = typeof(TBase);
		return from type in Assembly.GetExecutingAssembly().GetTypes()
			where !type.IsAbstract && !type.IsInterface && baseType.IsAssignableFrom(type)
			select type;
	}

	private static ModelPublicEntryOptions LegacyEntry(Type type)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return ModelPublicEntryOptions.FromFullPublicEntry("Valencina_" + ToStem(type.Name));
	}

	private static string ToStem(string typeName)
	{
		if (typeName == "ValencinaTaunt")
		{
			return "TAUNT";
		}
		return Regex.Replace(Regex.Replace(typeName, "([a-z0-9])([A-Z])", "$1_$2"), "([A-Z]+)([A-Z][a-z])", "$1_$2").ToUpperInvariant();
	}
}
