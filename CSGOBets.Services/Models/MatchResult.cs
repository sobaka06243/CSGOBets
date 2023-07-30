namespace CSGOBets.Services.Models;

public class MatchResult
{
    public MatchResult(string team1, string team2, int teamRank1, int teamRank2, IEnumerable<Veto> vetoes, IEnumerable<MapResult> maps)
    {
        Team1 = team1;
        Team2 = team2;
        TeamRank1 = teamRank1;
        TeamRank2 = teamRank2;
        Vetoes = vetoes;
        Maps = maps;
    }

    public string Team1 { get; }

    public string Team2 { get; }

    public int TeamRank1 { get; }

    public int TeamRank2 { get; }

    public IEnumerable<Veto> Vetoes { get; }

    public IEnumerable<MapResult> Maps { get; }
}
