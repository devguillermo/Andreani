using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using LibraryCommond;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Geocodificador.Service;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;

namespace Geocodificador
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ServiceRabbitMq serviceRabbitMq;
        private IConfiguration configuration;

        private string request;
        private string response;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            this.configuration = configuration;

            var user = configuration["AppSettings:UserNameRabbit"];
            var passwoed = configuration["AppSettings:PasswordRabbit"];
            var host = configuration["AppSettings:HostNameRabbit"];
            var vhost = configuration["AppSettings:VirtualHost"];
            request = configuration["AppSettings:Queuelistener"];
            response = configuration["AppSettings:QueueSend"];

            _logger = logger;
            serviceRabbitMq = new ServiceRabbitMq(user, passwoed, vhost, host, response, request);
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var connection = serviceRabbitMq.Factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: request,
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            _logger.LogInformation("Mensaje Recivido: {message}", message);

                            GetGeocodificadorAsync(message);


                        };
                        channel.BasicConsume(queue: request,
                                             autoAck: true,
                                             consumer: consumer);

                        await Task.Delay(1000, stoppingToken);


                    }

                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                {
                    _logger.LogInformation("Error de Coneccion {time}, Mensaje: {message},  {num}", DateTimeOffset.Now, ex.Message, ex.InnerException.Source);
                 
                }
                

                

                await Task.Delay(1000, stoppingToken);
            }

        }


        private async Task GetGeocodificadorAsync(string message)
        {
            Geolocalizar geolocalizar = JsonSerializer.Deserialize<Geolocalizar>(message);



            HttpClient httpClient = new HttpClient();
            httpServicio servicio = new httpServicio(httpClient);

            string param = geolocalizar.parametro();

            _logger.LogInformation("Esperando respuesta OMS ");
            string responseOMS = await servicio.GetAsync(param);
            _logger.LogInformation("Respuesta OMS : " + geolocalizar.id.ToString());

            char[] MyChar = { ']'};
            char[] MyChar2 = { '[' };

            responseOMS = responseOMS.TrimStart(MyChar2);
            responseOMS  = responseOMS.TrimEnd(MyChar);
            

            
            try
            {
                ResponseOps res = JsonSerializer.Deserialize<ResponseOps>(responseOMS);
                _logger.LogInformation("RESPONDIENDO");
                geolocalizar.ResponseOps = res;

                string jsonGeo = JsonSerializer.Serialize(geolocalizar);

                serviceRabbitMq.Send(jsonGeo);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error: " + ex.Message);
                //throw;
            }
            


        }
    }
}
