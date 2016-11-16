using RabbitMQ.Client;
using RSB.Interfaces;
using RSB.Transports.RabbitMQ;
using StructureMap;

namespace RSB.Templater.IoC
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
            ))).Singleton();

            For<TemplateManagerSettings>().Use(
                new TemplateManagerSettings
                {
                    TemplatesDllPath = Properties.Settings.Default.TemplatesDllPath
                }
            );

            For<TemplaterSettings>().Use(
                new TemplaterSettings
                {
                    TemplatesPath = Properties.Settings.Default.TemplatesDirPath
                }
            );
        }
    }
}
