using CSGOBets.Services.Interfaces;
using CSGOBets.Services.Models;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace CSGOBets.HLTVParser.Services;

public class MatchesParser : IMatchesLoader
{
    private Dictionary<PreMatch, string> _matchesUrls = new Dictionary<PreMatch, string>();

    public MatchesParser()
    {

    }

    public async Task<IEnumerable<MatchResult>> GetLastResults(PreMatch preMatch, int weeks)
    {
        if (!_matchesUrls.TryGetValue(preMatch, out var url))
        {
            return Enumerable.Empty<MatchResult>();
        }
        string preMatchHtml;
        using (var chromeMatchPage = new ChromeDriver())
        {
            chromeMatchPage.Url = url;
            await Task.Delay(TimeSpan.FromSeconds(10));
            preMatchHtml = chromeMatchPage.PageSource;
        }
        var doc = new HtmlDocument();
        doc.LoadHtml(preMatchHtml);
        var dataPastMathesCore = doc.DocumentNode.Descendants("div").FirstOrDefault(d => d.GetAttributes().Any(a => a.Name == "data-past-matches-core"));
        if (dataPastMathesCore is null)
        {
            return Enumerable.Empty<MatchResult>();
        }
        var pastMathesTableNodes = dataPastMathesCore.Descendants("table").Where(d => d.HasClass("past-matches-table"));
        List<MatchResult> matchResults = new List<MatchResult>();
        foreach (var table in pastMathesTableNodes)
        {
            var trNodes = table.Descendants("tr");
            foreach (var tr in trNodes)
            {
                var week = LastResultsParserHelper.GetMatchWeekAgo(tr);
                if (week is null)
                {
                    continue;
                }
                if (week > weeks)
                {
                    break;
                }
                var matchResult = await LastResultsParserHelper.GetMatchResult(tr);
                if (matchResult is null)
                {
                    continue;
                }
                matchResults.Add(matchResult);
            }
        }
        return matchResults;
    }

    public Task<IEnumerable<PreMatch>> GetPreMatches()
    {
        _matchesUrls.Clear();
        List<PreMatch> preMatches = new List<PreMatch>();
        using var chrome = new ChromeDriver();
        chrome.Url = "https://www.hltv.org/matches";
        var html = chrome.PageSource;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var upcomingMatchesSection = PreMatchParserHelper.GetUpcomingMatchesSection(doc.DocumentNode);
        foreach (var section in upcomingMatchesSection)
        {

            var matchDate = PreMatchParserHelper.GetMatchDate(section);
            if (matchDate is null)
            {
                continue;
            }
            var upcomingMatchNodes = PreMatchParserHelper.GetUpcomingMatches(section);
            foreach (var match in upcomingMatchNodes)
            {
                var matchTime = PreMatchParserHelper.GetMatchTime(match);
                if (matchTime is null)
                {
                    continue;
                }
                var martchDateTime = new DateTime(matchDate.Value.Year, matchDate.Value.Month, matchDate.Value.Day, matchTime.Value.Hour, matchTime.Value.Minute, 0);
                MatchMeta matchMeta = PreMatchParserHelper.GetMatchMeta(match);
                if (matchMeta == MatchMeta.Underfined)
                {
                    continue;
                }
                var teamName1 = PreMatchParserHelper.GetTeamName1(match);
                if (teamName1 is null)
                {
                    continue;
                }
                var teamName2 = PreMatchParserHelper.GetTeamName2(match);
                if (teamName2 is null)
                {
                    continue;
                }
                var matchUrl = PreMatchParserHelper.GetMatchInfoUrl(match);
                if (matchUrl is null)
                {
                    continue;
                }
                var preMatch = new PreMatch(teamName1, teamName2, martchDateTime, matchMeta);
                preMatches.Add(preMatch);
                _matchesUrls.Add(preMatch, "https://www.hltv.org" + matchUrl);
            }
        }
        return Task.FromResult(preMatches.AsEnumerable());
    }


}
