//using RabbitMQ.Client;
//using RSB.Interfaces;
//using RSB.Transports.RabbitMQ;
//using StructureMap;

//namespace RSB.MailSender.IoC
//{
//    class MailSenderRegistry : Registry
//    {
//        public MailSenderRegistry()
//        {
//            Scan(scan =>
//            {
//                scan.TheCallingAssembly();
//                scan.WithDefaultConventions();
//            });

//            For<IBus>().Use(new Bus(new RabbitMqTransport(
//                new ConnectionFactory
//                {
//                    HostName = Properties.Settings.Default.RabbitHostname,
//                    UserName = Properties.Settings.Default.RabbitUsername,
//                    Password = Properties.Settings.Default.RabbitPassword
//                }
//            ))).Singleton();

//            For<TemplateManagerSettings>().Use(
//                new TemplateManagerSettings
//                {
//                    TemplatesDll = Properties.Settings.Default.TemplatesDll
//                }
//            );

//            For<MailSenderSettings>().Use(
//                new MailSenderSettings
//                {
//                    TemplatesPath = Properties.Settings.Default.TemplatesDir,
//                    Hostname = Properties.Settings.Default.SenderHostname,
//                    Port = Properties.Settings.Default.SenderPort,
//                    Username = Properties.Settings.Default.SenderUsername,
//                    Password = Properties.Settings.Default.SenderPassword
//                }
//            );

//        }
//    }
//}
