using CSGOBets.Services.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CSGOBets.HLTVParser.Services;

public static class PreMatchParserHelper
{
    public static IEnumerable<HtmlNode> GetUpcomingMatchesSection(HtmlNode node)
    {
        return node.Descendants("div").Where(d => d.HasClass("upcomingMatchesSection"));
    }

    public static DateTime? GetMatchDate(HtmlNode node)
    {
        var matchDayHeadlineNodes = node.Descendants("div").FirstOrDefault(s => s.HasClass("matchDayHeadline"));
        if (matchDayHeadlineNodes is null)
        {
            return null;
        }
        var strMatchDate = matchDayHeadlineNodes.InnerText;
        Regex regex = new Regex(@"\d{4}-\d{2}-\d{2}");
        var regexMatchDate = regex.Match(strMatchDate);
        if (!DateTime.TryParseExact(regexMatchDate.Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var matchDate))
        {
            return null;
        }
        return matchDate;
    }

    public static IEnumerable<HtmlNode> GetUpcomingMatches(HtmlNode node)
    {
        return node.Descendants("div").Where(s => s.HasClass("upcomingMatch"));
    }

    public static TimeOnly? GetMatchTime(HtmlNode node)
    {
        var matchTimeNode = node.Descendants("div").FirstOrDefault(s => s.HasClass("matchTime"));
        if (matchTimeNode is null)
        {
            return null;
        }
        if (!TimeOnly.TryParse(matchTimeNode.InnerText, out var matchTime))
        {
            return null;
        }
        return matchTime;
    }

    public static MatchMeta GetMatchMeta(HtmlNode node)
    {
        var matchMetaNode = node.Descendants("div").FirstOrDefault(s => s.HasClass("matchMeta"));
        if (matchMetaNode is null)
        {
            return MatchMeta.Underfined;
        }
        switch (matchMetaNode.InnerText)
        {
            case "bo1":
                return MatchMeta.Bo1;
            case "bo3":
                return MatchMeta.Bo3;
            case "bo5":
                return MatchMeta.Bo3;
            default:
                return MatchMeta.Underfined;
        }
    }

    public static string? GetTeamName1(HtmlNode node)
    {
        return GetTeamName(node, "team1");
    }

    public static string? GetTeamName2(HtmlNode node)
    {
        return GetTeamName(node, "team2");
    }

    public static string? GetTeamName(HtmlNode node, string className)
    {
        var teamNode = node.Descendants("div").FirstOrDefault(s => s.HasClass(className));
        if (teamNode is null)
        {
            return null;
        }
        var matchTeamNameNode = teamNode.Descendants("div").FirstOrDefault(s => s.HasClass("matchTeamName"));
        if (matchTeamNameNode is null)
        {
            return null;
        }
        return matchTeamNameNode.InnerText;
    }

    public static string? GetMatchInfoUrl(HtmlNode node)
    {
        var matchHrefNode = node.Descendants("a").FirstOrDefault(s => s.HasClass("match"));
        if (matchHrefNode is null)
        {
            return null;
        }
        var hrefAttribute = matchHrefNode.GetAttributes().FirstOrDefault(a => a.Name == "href");
        if (hrefAttribute is null)
        {
            return null;
        }
        return hrefAttribute.Value;
    }
}
