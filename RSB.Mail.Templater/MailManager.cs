using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RSB.Interfaces;
using RSB.Mail.Templater.Models;
using System.Reflection;
using System.Web.Razor.Tokenizer.Symbols;

namespace RSB.Mail.Templater
{
    public class MailManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MailSender _mailSender;
        private readonly IBus _bus;

        private bool _isInitialized;

        public MailManager(MailSender mailSender, IBus bus)
        {
            _mailSender = mailSender;
            _bus = bus;
        }

        public void Start()
        {
            if (_isInitialized)
                return;

            InitializeTemplates();

            _isInitialized = true;
        }


        // TODO: Temp
        static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(MailAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        private void InitializeTemplates()
        {
            // We get the current assembly through the current class
            var currentAssembly = this.GetType().GetTypeInfo().Assembly;

            // we filter the defined classes according to the interfaces they implement
            var iDisposableAssemblies = currentAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IDisposable))).ToList();

            //var def = iDisposableAssemblies[0].GetGenericTypeDefinition();
            //var x = def.MakeGenericType();
            //typeof(x);


            //Type customType = iDisposableAssemblies[0];
            //Type genericListType = typeof(List<>);
            //Type customListType = genericListType.MakeGenericType(customType);
            //IList customListInstance = (IList) Activator.CreateInstance(customListType);
            //customListInstance.Add(customListInstance);




            //foreach (var t in iDisposableAssemblies)
            //{
            //    //    MethodInfo method = typeof(MailManager).GetMethod("RegisterTemplate");
            //    //    MethodInfo generic = method.MakeGenericMethod(t);
            //    //    generic.Invoke(this, null);



            //    var method = typeof(MailManager).GetMethod(nameof(RegisterTemplate));
            //    method.MakeGenericMethod(t).Invoke(this, null);

            //}

            //var v =TypesImplementingInterface(typeof(IMailMessage));

            //RegisterTemplate<def.generictype>();



            //generic.Invoke(this, null);

            var types = GetTypesWithHelpAttribute(currentAssembly);
            foreach (var t in types)
            //foreach (var t in iDisposableAssemblies)
            {
                MethodInfo method = typeof(MailManager).GetMethod(nameof(RegisterTemplate));
                MethodInfo generic = method.MakeGenericMethod(t);
                generic.Invoke(this, null);
            }

            //RegisterTemplate<SendUserRegisteredMail>();
        }

        // TODO: This thould be private
        public void RegisterTemplate<T>() where T : IMailMessage, new()
        {
            _mailSender.AddTemplate<T>();
            _bus.RegisterAsyncQueueHandler<T>(async msg => await SendEmailAsync(msg));
        }

        private async Task SendEmailAsync(IMailMessage message)
        {
            try
            {
                await _mailSender.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while sending message");
            }
        }
    }
}