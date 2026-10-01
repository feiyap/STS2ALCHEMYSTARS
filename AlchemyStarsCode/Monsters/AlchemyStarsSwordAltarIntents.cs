using System.Collections.Generic;
using AlchemyStars.Mechanics;
using AlchemyStars.UI;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace AlchemyStars.Monsters;

/// <summary>
/// 显示本回合将切换到的属性，图标用该属性的角标。
/// </summary>
public sealed class AlchemyStarsElementSwitchIntent : AbstractIntent
{
    private readonly LightElement _element;

    public AlchemyStarsElementSwitchIntent(LightElement element) => _element = element;

    public override IntentType IntentType => IntentType.Buff;

    protected override string IntentPrefix => "BUFF";

    protected override string? SpritePath => null;

    public override Texture2D? GetTexture(IEnumerable<Creature> targets, Creature owner) =>
        LightMechanicUiAssets.Load(LightMechanicUiAssets.GetCardAttributeIconPath(_element));

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        var suffix = _element switch
        {
            LightElement.Thunder => "thunder",
            LightElement.Water => "water",
            LightElement.Fire => "fire",
            _ => "forest",
        };
        return new LocString(
            "monsters",
            $"ALCHEMY_STARS_MONSTER_ALCHEMY_STARS_SWORD_ALTAR.elementIntent.{suffix}.description");
    }
}

/// <summary>
/// 原版脆弱意图。通用减益图标分不清是哪种减益，这里直接用脆弱图标。
/// </summary>
public sealed class AlchemyStarsFrailIntent : AbstractIntent
{
    public override IntentType IntentType => IntentType.Debuff;

    protected override string IntentPrefix => "DEBUFF";

    protected override string? SpritePath => null;

    public override Texture2D? GetTexture(IEnumerable<Creature> targets, Creature owner) =>
        ModelDb.Power<FrailPower>().Icon;

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        ModelDb.Power<FrailPower>().Description;
}
