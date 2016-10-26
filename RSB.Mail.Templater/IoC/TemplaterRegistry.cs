using RabbitMQ.Client;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;
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

            For<MailSenderSettings>().Use(
                new MailSenderSettings
                {
                    Hostname = Properties.Settings.Default.SenderHostname,
                    HostAddress = Properties.Settings.Default.SenderAddress,
                    Port = Properties.Settings.Default.SenderPort,
                    Username = Properties.Settings.Default.SenderUsername,
                    Password = Properties.Settings.Default.SenderPassword
                }
            );

            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
                //BaseTemplateType = typeof(MailMessage)
            };
            For<IRazorEngineService>().Use( RazorEngineService.Create(config)).Singleton();
        }
    }
}
