using CSGOBets.Services.Interfaces;
using CSGOBets.Services.Models;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V112.WebAuthn;
using System.Text.RegularExpressions;

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
        List<PreMatch> preMatches= new List<PreMatch>();  
        using var chrome = new ChromeDriver();
        chrome.Url = "https://www.hltv.org/matches";
        var html = chrome.PageSource;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var upcomingMatchesSection = doc.DocumentNode.Descendants("div").Where(d => d.HasClass("upcomingMatchesSection"));
        foreach (var section in upcomingMatchesSection)
        {

            var matchDayHeadlineNodes = section.Descendants("div").FirstOrDefault(s => s.HasClass("matchDayHeadline"));
            if (matchDayHeadlineNodes is null)
            {
                continue;
            }
            var strMatchDate = matchDayHeadlineNodes.InnerText;
            Regex regex = new Regex(@"\d{4}-\d{2}-\d{2}");
            var regexMatchDate = regex.Match(strMatchDate);
            if (!DateTime.TryParseExact(regexMatchDate.Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var matchDate))
            {
                continue;
            }
            var upcomingMatchNodes = section.Descendants("div").Where(s => s.HasClass("upcomingMatch"));
            foreach (var match in upcomingMatchNodes)
            {
                var matchTimeNode = match.Descendants("div").FirstOrDefault(s => s.HasClass("matchTime"));
                if (matchTimeNode is null)
                {
                    continue;
                }
                if (!TimeOnly.TryParse(matchTimeNode.InnerText, out var matchTime))
                {
                    continue;
                }
                var martchDateTime = new DateTime(matchDate.Year, matchDate.Month, matchDate.Day, matchTime.Hour, matchTime.Minute, 0);
                var matchMetaNode = match.Descendants("div").FirstOrDefault(s => s.HasClass("matchMeta"));
                if (matchMetaNode is null)
                {
                    continue;
                }
                MatchMeta matchMeta;
                switch (matchMetaNode.InnerText)
                {
                    case "bo1":
                        matchMeta = MatchMeta.Bo1;
                        break;
                    case "bo3":
                        matchMeta = MatchMeta.Bo3;
                        break;
                    default:
                        continue;
                }
                var team1Node = match.Descendants("div").FirstOrDefault(s => s.HasClass("team1"));
                var team2Node = match.Descendants("div").FirstOrDefault(s => s.HasClass("team2"));
                if (team1Node is null || team2Node is null)
                {
                    continue;
                }
                var matchTeamName1Node = team1Node.Descendants("div").FirstOrDefault(s => s.HasClass("matchTeamName"));
                var matchTeamName2Node = team2Node.Descendants("div").FirstOrDefault(s => s.HasClass("matchTeamName"));
                if (matchTeamName1Node is null || matchTeamName2Node is null)
                {
                    continue;
                }
                var matchHrefNode = match.Descendants("a").FirstOrDefault(s => s.HasClass("match"));
                if (matchHrefNode is null)
                {
                    continue;
                }
                var hrefAttribute = matchHrefNode.GetAttributes().FirstOrDefault(a => a.Name == "href");
                if (hrefAttribute is null)
                {
                    continue;
                }
                preMatches.Add(new PreMatch(matchTeamName1Node.InnerText, matchTeamName2Node.InnerText, martchDateTime, matchMeta));
                _matchesUrls.Add("https://www.hltv.org" + hrefAttribute.Value);
            }
        }
        return Task.FromResult(preMatches.AsEnumerable());
    }
}
