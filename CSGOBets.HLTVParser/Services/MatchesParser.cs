using CSGOBets.Services.Interfaces;
using CSGOBets.Services.Models;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V112.WebAuthn;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

namespace CSGOBets.HLTVParser.Services;

public class MatchesParser : IMatchesLoader
{
    private List<string> _matchesUrls = new List<string>();
    public Task<IEnumerable<MatchResult>> GetLastResults(PreMatch preMatch, int weeks)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PreMatch>> GetPreMatches()
    {
        List<PreMatch> preMatches = new List<PreMatch>();
        using var chrome = new ChromeDriver();
        chrome.Url = "https://www.hltv.org/matches";
        var html = chrome.PageSource;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var upcomingMatchesSection = GetUpcomingMatchesSection(doc.DocumentNode);
        foreach (var section in upcomingMatchesSection)
        {

            var matchDate = GetMatchDate(section);
            if (matchDate is null)
            {
                continue;
            }
            var upcomingMatchNodes = GetUpcomingMatches(section);
            foreach (var match in upcomingMatchNodes)
            {
                var matchTime = GetMatchTime(match);
                if (matchTime is null)
                {
                    continue;
                }
                var martchDateTime = new DateTime(matchDate.Value.Year, matchDate.Value.Month, matchDate.Value.Day, matchTime.Value.Hour, matchTime.Value.Minute, 0);
                MatchMeta matchMeta = GetMatchMeta(match);
                if (matchMeta == MatchMeta.Underfined)
                {
                    continue;
                }
                var teamName1 = GetTeamName1(match);
                if (teamName1 is null)
                {
                    continue;
                }
                var teamName2 = GetTeamName2(match);
                if (teamName2 is null)
                {
                    continue;
                }
                var matchUrl = GetMatchInfoUrl(match);
                if (matchUrl is null)
                {
                    continue;
                }
                preMatches.Add(new PreMatch(teamName1, teamName2, martchDateTime, matchMeta));
                _matchesUrls.Add("https://www.hltv.org" + matchUrl);
            }
        }
        return Task.FromResult(preMatches.AsEnumerable());
    }

    private IEnumerable<HtmlNode> GetUpcomingMatchesSection(HtmlNode node)
    {
        return node.Descendants("div").Where(d => d.HasClass("upcomingMatchesSection"));
    }

    private DateTime? GetMatchDate(HtmlNode node)
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

    private IEnumerable<HtmlNode> GetUpcomingMatches(HtmlNode node)
    {
        return node.Descendants("div").Where(s => s.HasClass("upcomingMatch"));
    }

    private TimeOnly? GetMatchTime(HtmlNode node)
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

    private MatchMeta GetMatchMeta(HtmlNode node)
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
            default:
                return MatchMeta.Underfined;
        }
    }

    private string? GetTeamName1(HtmlNode node)
    {
        return GetTeamName(node, "team1");
    }

    private string? GetTeamName2(HtmlNode node)
    {
        return GetTeamName(node, "team2");
    }

    private string? GetTeamName(HtmlNode node, string className)
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

    private string? GetMatchInfoUrl(HtmlNode node)
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
