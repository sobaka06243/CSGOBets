namespace CSGOBets.Services.Models;

public class PreMatch
{
    public PreMatch(string team1, string team2, DateTime matchDate, MatchMeta meta)
    {
        Team1 = team1;
        Team2 = team2;
        MatchDate = matchDate;
        Meta = meta;
    }

    public string Team1 { get; }

    public string Team2 { get; }

    public DateTime MatchDate { get; }

    public MatchMeta Meta { get; }
}
