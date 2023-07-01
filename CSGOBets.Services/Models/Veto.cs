namespace CSGOBets.Services.Models;

public class Veto
{
    public Veto(string team, VetoAction action, Map map)
    {
        Team = team;
        Action = action;
        Map = map;
    }

    public string Team { get; }

    public VetoAction Action { get; }

    public Map Map { get; }
}
