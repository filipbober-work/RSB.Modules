using System;

namespace RSB.Modules.Templater.Common.Contracts
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class TemplateAttribute : Attribute
    {
        public string Title { get; }

        public TemplateAttribute(string title = "")
        {
            Title = title;
       }
    }
}