namespace CSGOBets.Services.Models;

public class MapResult
{
    public MapResult(Map map, string team1, string team2, int score1, int score2)
    {
        Map = map;
        Team1 = team1;
        Team2 = team2;
        Score1 = score1;
        Score2 = score2;
    }

    public Map Map { get; }

    public string Team1 { get; }

    public string Team2 { get; }

    public int Score1 { get; }

    public int Score2 { get; }
}
