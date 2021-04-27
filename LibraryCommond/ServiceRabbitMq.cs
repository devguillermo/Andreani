using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using System.Text;

namespace LibraryCommond
{
    public interface IServiceRabbitMq
    {
        public bool Send(string message);
    }
    public class ServiceRabbitMq : IServiceRabbitMq
    {
        private IOptions<AppSettings> _settings;
        public ConnectionFactory Factory { get; set; }

        private string QueueSend { get; set; }

        public string QueueResponse { get; set; }

        //public string QueueSend { get; set };
        public ServiceRabbitMq(IOptions<AppSettings> settings)
        {
            _settings = settings;
            Factory = new ConnectionFactory { 
                UserName = _settings.Value.UserNameRabbit ,
                Password = _settings.Value.PasswordRabbit,
                VirtualHost = _settings.Value.VirtualHost,
                HostName = _settings.Value.HostNameRabbit
             };

            this.QueueSend = _settings.Value.QueueSend;
            this.QueueResponse = _settings.Value.Queuelistener;


        }

        public ServiceRabbitMq(string user, string pass, string virtualHost, string host, string queueSend, string queueResponse )
        {
            
            Factory = new ConnectionFactory
            {
                UserName = user,
                Password = pass,
                VirtualHost = virtualHost,
                HostName = host
            };

            this.QueueSend = queueSend;
            this.QueueResponse = queueResponse;


        }

        public bool Send(string message)
        {
            

            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: this.QueueSend, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "", routingKey: this.QueueSend, basicProperties: null, body: body);
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                    
                }
                


            }

        }
    }
}
