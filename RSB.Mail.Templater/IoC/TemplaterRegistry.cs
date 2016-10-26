using RabbitMQ.Client;
using RSB.Interfaces;
using RSB.Transports.RabbitMQ;
using RSB.Transports.RabbitMQ.Settings;
using StructureMap;

namespace RSB.Mail.Templater
{
    class TemplaterRegistry : Registry
    {
        public TemplaterRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<IBus>().Use(new Bus(new RabbitMqTransport(
                new ConnectionFactory
                {
                    HostName = Properties.Settings.Default.RabbitHostname,
                    UserName = Properties.Settings.Default.RabbitUsername,
                    Password = Properties.Settings.Default.RabbitPassword
                }
                //RabbitMqTransportSettings.FromConfigurationFile()
            ))).Singleton();

        }
    }
}
