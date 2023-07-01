namespace CSGOBets.Services.Models;

public class MatchResult
{
    public MatchResult(string team1, string team2, IEnumerable<Veto> vetoes, IEnumerable<MapResult> maps)
    {
        Team1 = team1;
        Team2 = team2;
        Vetoes = vetoes;
        Maps = maps;
    }

    public string Team1 { get; }

    public string Team2 { get; }

    public IEnumerable<Veto> Vetoes { get; }

    public IEnumerable<MapResult> Maps { get; }
}
