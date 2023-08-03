using CSGOBets.HLTVParser.Services;
using CSGOBets.Services.Models;

var parser = new MatchesParser();
var preMathes = await parser.GetPreMatches();
using var rabbitMqSender = new RabbitMqSender();
rabbitMqSender.PurgeQueue();
foreach (var preMatch in preMathes)
{
    rabbitMqSender.Send(new MatchInfo(preMatch, await parser.GetLastResults(preMatch, 2)));
}