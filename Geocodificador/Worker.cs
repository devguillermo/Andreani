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
using LibraryCommond;

namespace Geocodificador
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ServiceRabbitMq serviceRabbitMq;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;//172.17.0.3
            serviceRabbitMq = new ServiceRabbitMq("andres", "Amarela0304", "/", "RabbitGuille", "ResponseGeolocalizar", "geolocalizar");
            //serviceRabbitMq = new ServiceRabbitMq("andres", "Amarela0304", "/", "172.17.0.3", "ResponseGeolocalizar", "geolocalizar");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);



                using (var connection = serviceRabbitMq.Factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "geolocalizar",
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
                    channel.BasicConsume(queue: "geolocalizar",
                                         autoAck: true,
                                         consumer: consumer);

                    await Task.Delay(1000, stoppingToken);


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

            //Task<string> task = await servicio.GetAsync(param);
            _logger.LogInformation("Esperando respuesta OMS ");
            string responseOMS = await servicio.GetAsync(param);
            _logger.LogInformation("Respuesta OMS : " + geolocalizar.id.ToString());

           // responseOMS = responseOMS.Replace("[", "").Replace("]", "");

            char[] MyChar = { ']'};
            char[] MyChar2 = { '[' };

            responseOMS = responseOMS.TrimStart(MyChar2);
            responseOMS  = responseOMS.TrimEnd(MyChar);
            

            
            try
            {
                ResponseOps res = JsonSerializer.Deserialize<ResponseOps>(responseOMS);
                _logger.LogInformation("RESPONDIENDO, RESPONDIENDO, RESPONDIENDO, RESPONDIENDO, RESPONDIENDO,");
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
