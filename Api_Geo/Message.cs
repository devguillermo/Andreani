using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Api_Geo
{
    public interface IMessage
    {
        public void Send(string message, string queue);
        public string Get(string messaje, string queue);
    }
        

    class Message : IMessage
    {
        private ConnectionFactory factory;
        public Message()
        {
            this.factory = new ConnectionFactory();
           
        }

        public event EventHandler<EventMessageReceived> MessageReceived;

        
        protected virtual void OnRaiseEventMessageReceived(EventMessageReceived e)
        {
            EventHandler<EventMessageReceived> raiseEvent = MessageReceived;

            if (raiseEvent != null)
            {
                raiseEvent(this, e);
            }
        }



        public void Send(string message, string queue)
        {

            factory = new ConnectionFactory();
           
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                

            }


        }

        public string Get(string messaje, string queue)
        {

            ConnectionFactory factory = new ConnectionFactory();
            
            string message = "";

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
                    message = Encoding.UTF8.GetString(body);

                    OnRaiseEventMessageReceived(new EventMessageReceived(message));
                   
                };
                channel.BasicConsume(queue: queue,
                                     autoAck: true,
                                     consumer: consumer);



            }

            return message;
        }

    }
}