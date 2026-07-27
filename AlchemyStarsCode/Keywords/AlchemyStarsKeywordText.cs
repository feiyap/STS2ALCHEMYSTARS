using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Keywords;

namespace AlchemyStars.Keywords;

/// <summary>
/// 将已注册关键词渲染为卡牌描述可用�?BBCode 文本�?/// </summary>
public static class AlchemyStarsKeywordText
{
    public static string InlineTitle(string keywordId) =>
        "[gold]" + ModKeywordRegistry.GetTitle(keywordId).GetFormattedText() + "[/gold]";

    public static StringVar InlineTitleVar(string name, string keywordId) =>
        new(name, InlineTitle(keywordId));
}
