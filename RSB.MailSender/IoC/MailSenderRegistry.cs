﻿using RabbitMQ.Client;
using RSB.Interfaces;
using RSB.Transports.RabbitMQ;
using StructureMap;

namespace RSB.MailSender.IoC
{
    class MailSenderRegistry : Registry
    {
        public MailSenderRegistry()
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

            For<MailManagerSettings>().Use(
                new MailManagerSettings()
            );

        For<MailSenderSettings>().Use(
                new MailSenderSettings
                {
                    Hostname = Properties.Settings.Default.MailHostname,
                    Address = Properties.Settings.Default.MailAddress,
                    Username = Properties.Settings.Default.MailUsername,
                    Password = Properties.Settings.Default.MailPassword,
                    Port = Properties.Settings.Default.MailPort
                }
            );

        }
    }
}