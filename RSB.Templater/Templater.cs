using System.IO;
using NLog;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace RSB.Templater
{
    public class Templater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TemplaterSettings _settings;

        public Templater(TemplaterSettings settings)
        {
            _settings = settings;

            var config = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            RazorEngineService.Create(config);
        }

        public void AddTemplate<T>()
        {
            AddTemplateAndCompile<T>(_settings.TemplatesPath);
        }

        public string CreateTemplateBody<T>(T contract)
        {
            var typeName = contract.GetType().Name;
            return Engine.Razor.RunCompile(typeName, contract.GetType(), contract);
        }

        private static void AddTemplateAndCompile<T>(string templatesPath)
        {
            var typeName = typeof(T).Name;
            var templatePath = Path.Combine(templatesPath, typeName) + ".cshtml";

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Cannot find file {templatePath}.");

            Engine.Razor.AddTemplate(typeName, File.ReadAllText(templatePath));
            Engine.Razor.Compile(typeName, typeof(T));
            Logger.Debug("Compiled template: {0}", typeof(T).Name);
        }

    }
}
