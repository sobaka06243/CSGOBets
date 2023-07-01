using CSGOBets.Services.Models;

namespace CSGOBets.Services.Interfaces;

public interface IMatchesLoader
{
    Task<IEnumerable<MatchResult>> GetLastResults(PreMatch preMatch, int weeks);

    Task<IEnumerable<PreMatch>> GetPreMatches();
}
