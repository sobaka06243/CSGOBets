namespace CSGOBets.Services.Models;

public class MatchInfo
{
    public MatchInfo(PreMatch match, IEnumerable<MatchResult> results)
    {
        Match = match;
        Results = results;
    }

    public PreMatch Match { get; }

    public IEnumerable<MatchResult> Results { get; }
}
