using CSGOBets.Services.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace CSGOBets.HLTVParser.Services;

public class RabbitMqSender : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public RabbitMqSender()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Send(object message)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        var byteMessage = Encoding.UTF8.GetBytes(jsonMessage);
        _channel.QueueDeclare(queue: "MatchInfo",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _channel.BasicPublish(exchange: string.Empty,
                             routingKey: "MatchInfo",
                             basicProperties: null,
                             body: byteMessage);
        
    }

    public void PurgeQueue()
    {
        _channel.QueuePurge("MatchInfo");
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
