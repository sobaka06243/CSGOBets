namespace CSGOBets.Services.Models;

public class PreMatch
{
    public PreMatch(string team1, string team2, DateTime matchDate)
    {
        Team1 = team1;
        Team2 = team2;
        MatchDate = matchDate;
    }

    public string Team1 { get; }

    public string Team2 { get; }

    public DateTime MatchDate { get; }
}
