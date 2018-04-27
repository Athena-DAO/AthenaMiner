using Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace QueueConsumer
{

    public class Consumer : IQueueConsumer
    {
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private ConnectionFactory connectionFactory;
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private IMessageRecieved messageRecieved;
        private string queueName;

        private void InitializeConnectionFactory(string hostName)
        {
            connectionFactory = new ConnectionFactory()
            {
                HostName = hostName
            };
        }

        private void CreateConnection()
        {
            connection = connectionFactory.CreateConnection();
        }

        private void SetUpChannel(string queueName)
        {
            channel = connection.CreateModel();
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.BasicQos(0, 1, false);
        }

        private void SetUpConsumer()
        {
            consumer = new EventingBasicConsumer(channel);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            messageRecieved.ProcessMessage(Encoding.UTF8.GetString(e.Body));
            channel.BasicAck(e.DeliveryTag, false);
            autoResetEvent.Set();
        }

        public void Consume(IMessageRecieved messageRecieved)
        {
            this.messageRecieved = messageRecieved;
            consumer.Received += Consumer_Received;
            channel.BasicConsume(queueName, false, consumer);
            while (true)
            {
                autoResetEvent.WaitOne();
            }
        }

        public Consumer(string hostName, string queueName)
        {
            this.queueName = queueName;

            InitializeConnectionFactory(hostName);
            CreateConnection();
            SetUpChannel(queueName);
            SetUpConsumer();
        }
    }
}
