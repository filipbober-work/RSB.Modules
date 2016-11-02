using RabbitMQ.Client;
using RSB.Interfaces;
using RSB.Transports.RabbitMQ;
using StructureMap;

namespace RSB.Mail.Templater.IoC
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

            For<MailManagerSettings>().Use(
                new MailManagerSettings
                {
                    TemplatesDll = Properties.Settings.Default.TemplatesDll
                }
            );

            For<MailSenderSettings>().Use(
                new MailSenderSettings
                {
                    TemplatesPath = Properties.Settings.Default.TemplatesDir,
                    Hostname = Properties.Settings.Default.SenderHostname,
                    Port = Properties.Settings.Default.SenderPort,
                    Username = Properties.Settings.Default.SenderUsername,
                    Password = Properties.Settings.Default.SenderPassword
                }
            );

        }
    }
}
