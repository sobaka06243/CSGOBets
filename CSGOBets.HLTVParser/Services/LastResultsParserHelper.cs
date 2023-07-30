using CSGOBets.Services.Models;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace CSGOBets.HLTVParser.Services;

public static class LastResultsParserHelper
{

    public static async Task<MatchResult?> GetMatchResult(HtmlNode node)
    {
        var matchHref = LastResultsParserHelper.GetMatchHref(node);
        if (matchHref is null)
        {
            return null;
        }
        using var chromeMatchResultPage = new ChromeDriver();
        chromeMatchResultPage.Url = "https://www.hltv.org" + matchHref;
        Random rand = new Random();
        await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 10)));
        var resultMatchHtml = chromeMatchResultPage.PageSource;
        var resultMatchDoc = new HtmlDocument();
        resultMatchDoc.LoadHtml(resultMatchHtml);
        var vetoBoxNode = LastResultsParserHelper.GetVetoBox(resultMatchDoc.DocumentNode);
        if (vetoBoxNode is null)
        {
            return null;
        }
        var vetoNodes = LastResultsParserHelper.GetVetoNodes(vetoBoxNode);
        if (vetoNodes.Count() != 7)
        {
            return null;
        }
        List<Veto> vetoActions = new List<Veto>();
        foreach (var vetoNode in vetoNodes)
        {
            var veto = LastResultsParserHelper.GetVeto(vetoNode);
            if (veto is null)
            {
                return null;
            }
            vetoActions.Add(veto);
        }
        var mapHolderNodes = resultMatchDoc.DocumentNode.Descendants("div").Where(d => d.HasClass("mapholder"));
        bool isMapFinded = true;
        List<MapResult> mapResults = new List<MapResult>();
        string team1 = string.Empty;
        string team2 = string.Empty;
        foreach (var mapHolder in mapHolderNodes)
        {
            var mapNameNode = mapHolder.Descendants("div").FirstOrDefault(d => d.HasClass("mapname"));
            if (mapNameNode is null)
            {
                isMapFinded = false;
                break;
            }
            string mapName = mapNameNode.InnerText;
            var map = GetMap(mapName);
            if (map is null)
            {
                isMapFinded = false;
                break;
            }
            var resultsLeftNode = mapHolder.Descendants().FirstOrDefault(d => d.HasClass("results-left"));
            if (resultsLeftNode is null)
            {
                isMapFinded = false;
                break;
            }
            var tmpTeam1 = GetTeamName(resultsLeftNode);
            if (tmpTeam1 is null)
            {
                isMapFinded = false;
                break;
            }
            var resultsTeamScore1Node = resultsLeftNode.Descendants("div").FirstOrDefault(d => d.HasClass("results-team-score"));
            if (resultsTeamScore1Node is null)
            {
                isMapFinded = false;
                break;
            }
            var resultsRightNode = mapHolder.Descendants().FirstOrDefault(d => d.HasClass("results-right"));
            if (resultsRightNode is null)
            {
                isMapFinded = false;
                break;
            }
            var tmpTeam2 = GetTeamName(resultsRightNode);
            if (tmpTeam2 is null)
            {
                isMapFinded = false;
                break;
            }
            var resultsTeamScore2Node = resultsRightNode.Descendants("div").FirstOrDefault(d => d.HasClass("results-team-score"));
            if (resultsTeamScore2Node is null)
            {
                isMapFinded = false;
                break;
            }
            if (resultsTeamScore1Node.InnerText == "-" || resultsTeamScore2Node.InnerText == "-")
            {
                break;
            }
            if (!int.TryParse(resultsTeamScore1Node.InnerText, out var score1))
            {
                isMapFinded = false;
                break;
            }
            if (!int.TryParse(resultsTeamScore2Node.InnerText, out var score2))
            {
                isMapFinded = false;
                break;
            }
            team1 = tmpTeam1;
            team2 = tmpTeam2;
            mapResults.Add(new MapResult(map.Value, team1, team2, score1, score2));
        }
        if (!isMapFinded)
        {
            return null;
        }

        var lineupNodes = resultMatchDoc.DocumentNode.Descendants("div").Where(d => d.HasClass("lineup"));
        if (lineupNodes.Count() != 2)
        {
            return null;
        }
        List<(string Team, int Rank)> teamInfo = new List<(string, int)>();
        foreach (var lineupNode in lineupNodes)
        {
            var boxHeadline = lineupNode.Descendants("div").FirstOrDefault(d => d.HasClass("box-headline"));
            if (boxHeadline is null)
            {
                return null;
            }
            var boxHeadlineHrefs = boxHeadline.Descendants("a");
            var teamName = boxHeadlineHrefs.FirstOrDefault()?.InnerText;
            if (teamName is null)
            {
                return null;
            }
            var rankStr = boxHeadlineHrefs.LastOrDefault()?.InnerText;
            if (rankStr is null)
            {
                return null;
            }
            rankStr = Regex.Match(rankStr, @"\d+").Value;
            if (rankStr is null)
            {
                return null;
            }
            if (!int.TryParse(rankStr, out var rank))
            {
                return null;
            }
            teamInfo.Add(new (teamName, rank));
        }
        var teamRank1 = teamInfo.FirstOrDefault(i => i.Team == team1).Rank;
        var teamRank2 = teamInfo.FirstOrDefault(i => i.Team == team2).Rank;
        if(teamRank1 == 0 || teamRank2 == 0)
        {
            return null;
        }
        return new MatchResult(team1, team2, teamRank1, teamRank2,  vetoActions, mapResults);
    }

    public static int? GetMatchWeekAgo(HtmlNode node)
    {
        var pastMatchesTimeAgoNode = node.Descendants("div").FirstOrDefault(d => d.HasClass("past-matches-time-ago"));
        if (pastMatchesTimeAgoNode is null)
        {
            return null;
        }
        int week;
        if (string.IsNullOrEmpty(pastMatchesTimeAgoNode.InnerText))
        {
            week = 0;
        }
        else
        {
            var strWeek = pastMatchesTimeAgoNode.InnerText.FirstOrDefault();
            if (!int.TryParse(strWeek.ToString(), out week))
            {
                return null;
            }
        }
        return week;
    }

    public static string? GetMatchHref(HtmlNode node)
    {
        return node.Descendants("a").LastOrDefault(d => d.GetAttributes().Any(a => a.Name == "data-link-tracking-page" && a.Value == "Matchpage"))?.Attributes.FirstOrDefault(a => a.Name == "href")?.Value;
    }

    public static HtmlNode? GetVetoBox(HtmlNode node)
    {
        return node.Descendants("div").LastOrDefault(d => d.HasClass("veto-box"));
    }

    public static IEnumerable<HtmlNode> GetVetoNodes(HtmlNode node)
    {
        var parentVetoBoxNode = node.Descendants("div").FirstOrDefault();
        if (parentVetoBoxNode is null)
        {
            return new List<HtmlNode>();
        }
        return parentVetoBoxNode.Descendants("div");
    }

    public static Veto? GetVeto(HtmlNode node)
    {
        var splittedVetoNode = node.InnerText.Split(" ");
        string team = string.Empty;
        for (int i = 1; i < splittedVetoNode.Length - 2; i++)
        {
            team += splittedVetoNode[i];
            if (i + 1 != splittedVetoNode.Length - 2)
            {
                team += " ";
            }
        }
        if (splittedVetoNode.Length == 0)
        {
            return null;
        }
        var strVetoOrder = splittedVetoNode[0];
        string strMap;
        if (strVetoOrder.Contains('7'))
        {
            if (splittedVetoNode.Length < 1)
            {
                return null;
            }
            strMap = splittedVetoNode[1];
        }
        else
        {
            var tmpMap = splittedVetoNode.LastOrDefault();
            if (tmpMap is null)
            {
                return null;
            }
            strMap = tmpMap;
        }
        var map = GetMap(strMap);
        if (map is null)
        {
            return null;
        }
        if (node.InnerText.Contains("removed"))
        {
            return new Veto(team, VetoAction.Removed, map.Value);
        }
        else if (node.InnerText.Contains("picked"))
        {
            return new Veto(team, VetoAction.Picked, map.Value);
        }
        else if (node.InnerText.Contains("was left over"))
        {
            return new Veto(string.Empty, VetoAction.WasLeftOver, map.Value);
        }
        else
        {
            return null;
        }
    }

    public static Map? GetMap(string strMap)
    {
        switch (strMap)
        {
            case "Mirage":
                return Map.Mirage;
            case "Inferno":
                return Map.Inferno;
            case "Nuke":
                return Map.Nuke;
            case "Overpass":
                return Map.Overpass;
            case "Vertigo":
                return Map.Vertigo;
            case "Ancient":
                return Map.Ancient;
            case "Anubis":
                return Map.Anubis;
            default:
                return null;
        }
    }

    public static Map? GetMap(HtmlNode node)
    {
        var mapNameNode = node.Descendants("div").FirstOrDefault(d => d.HasClass("mapname"));
        if (mapNameNode is null)
        {
            return null;
        }
        string mapName = mapNameNode.InnerText;
        return GetMap(mapName);
    }

    public static string? GetTeamName(HtmlNode node)
    {
        var resultsTeamName1Node = node.Descendants("div").FirstOrDefault(d => d.HasClass("results-teamname"));
        if (resultsTeamName1Node is null)
        {
            return null;
        }
        return resultsTeamName1Node.InnerText;
    }
}
