using LibraryCommond;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api_Geo.Models;

namespace Api_Geo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IOptions<AppSettings> _settings;
        private IServiceMongo _serviceMongo;
        public Worker(IOptions<AppSettings> settings, IServiceMongo serviceMongo, ILogger<Worker> logger)
        {
            _logger = logger;
            _settings = settings;
            _serviceMongo = serviceMongo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            string queue = _settings.Value.QueueResponse;
            string VirtualHost = _settings.Value.VirtualHost;

            ConnectionFactory factory = new ConnectionFactory();

            factory.UserName = _settings.Value.UserNameRabbit;
            factory.Password = _settings.Value.PasswordRabbit;
            factory.VirtualHost = _settings.Value.VirtualHost;
            factory.HostName = _settings.Value.HostNameRabbit;
            
            Console.WriteLine("Virtul h : " + VirtualHost);


            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Escuchando mensajes en Virtual Host {virtual}, Queue: {queue}: {time}", VirtualHost, queue, DateTimeOffset.Now);



                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation("Mensaje Recivido desde Geocodificador: {message}", message);
                        SaveGeocodificacion(message);



                    };
                    channel.BasicConsume(queue: queue,
                                         autoAck: true,
                                         consumer: consumer);

                    await Task.Delay(1000, stoppingToken);


                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stops at: {time}", DateTimeOffset.Now);

            await base.StopAsync(cancellationToken);
        }

        private void SaveGeocodificacion(string message)
        {

            RequestGeo geolocalizar = JsonSerializer.Deserialize<RequestGeo>(message);

            _serviceMongo.UpdateDocumentRequest(geolocalizar.ResponseOps, geolocalizar.idStr);
        }

    }
}
