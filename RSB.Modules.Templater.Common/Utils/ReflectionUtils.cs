using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using RSB.Modules.Templater.Common.Contracts;

namespace RSB.Modules.Templater.Common.Utils
{
    public class ReflectionUtils
    {
        public static ITemplateRequest<T> InstantiateTemplateRequest<T>() where T : new()
        {
            var requestType = BuildDynamicRequestType<T>();
            var requestInstance = Activator.CreateInstance(requestType);
            var custDataAsTemplate = requestInstance as ITemplateRequest<T>;

            if (custDataAsTemplate == null)
                return null;

            custDataAsTemplate.Variables = new T();
            return custDataAsTemplate;
        }

        public static ITemplateResponse<T> InstantiateTemplateResponse<T>() where T : new()
        {
            var responseType = BuildDynamicResponseType<T>();
            var responseInstance = Activator.CreateInstance(responseType);
            var custDataAsTemplate = responseInstance as ITemplateResponse<T>;

            return custDataAsTemplate;
        }

        public static Type BuildDynamicRequestType<T>()
        {
            var appDomain = Thread.GetDomain();
            var assemblyName = new AssemblyName
            {
                Name = "RequestAssembly" + Guid.NewGuid()
            };

            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName,
                                                            AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");

            var typeBuilder = moduleBuilder.DefineType("FillUserRegisteredTemplateRequest",
                                                            TypeAttributes.Public);

            var variablesBldr = typeBuilder.DefineField("_variables",
                                                            typeof(T),
                                                            FieldAttributes.Private);

            var variablesPropBldr = typeBuilder.DefineProperty("Variables",
                                                             PropertyAttributes.HasDefault,
                                                             typeof(T),
                                                             null);

            MethodAttributes getSetAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig | MethodAttributes.Virtual;

            var variablesGetPropMthdBldr =
                typeBuilder.DefineMethod("get_Variables",
                                           getSetAttr,
                                           typeof(T),
                                           Type.EmptyTypes);

            // ReSharper disable once InconsistentNaming
            var variablesGetIL = variablesGetPropMthdBldr.GetILGenerator();

            variablesGetIL.Emit(OpCodes.Ldarg_0);
            variablesGetIL.Emit(OpCodes.Ldfld, variablesBldr);
            variablesGetIL.Emit(OpCodes.Ret);

            var variablesSetPropMthdBldr =
                typeBuilder.DefineMethod("set_Variables",
                                           getSetAttr,
                                           null,
                                           new[] { typeof(T) });

            // ReSharper disable once InconsistentNaming
            var variablesSetIL = variablesSetPropMthdBldr.GetILGenerator();

            variablesSetIL.Emit(OpCodes.Ldarg_0);
            variablesSetIL.Emit(OpCodes.Ldarg_1);
            variablesSetIL.Emit(OpCodes.Stfld, variablesBldr);
            variablesSetIL.Emit(OpCodes.Ret);

            variablesPropBldr.SetGetMethod(variablesGetPropMthdBldr);
            variablesPropBldr.SetSetMethod(variablesSetPropMthdBldr);

            typeBuilder.AddInterfaceImplementation(typeof(ITemplateRequest<T>));

            var result = typeBuilder.CreateType();
            return result;
        }

        public static Type BuildDynamicResponseType<T>()
        {
            var appDomain = Thread.GetDomain();
            var assemblyName = new AssemblyName
            {
                Name = "ResponseAssembly" + Guid.NewGuid()
            };

            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName,
                                                            AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");

            var typeBuilder = moduleBuilder.DefineType("FillUserRegisteredTemplateResponse",
                                                            TypeAttributes.Public);

            var textBldr = typeBuilder.DefineField("_text",
                                                            typeof(string),
                                                            FieldAttributes.Private);

            var textPropBldr = typeBuilder.DefineProperty("Text",
                                                             PropertyAttributes.HasDefault,
                                                             typeof(string),
                                                             null);

            MethodAttributes getSetAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig | MethodAttributes.Virtual;

            var textGetPropMthdBldr =
                typeBuilder.DefineMethod("get_Text",
                                           getSetAttr,
                                           typeof(string),
                                           Type.EmptyTypes);

            // ReSharper disable once InconsistentNaming
            var textGetIL = textGetPropMthdBldr.GetILGenerator();

            textGetIL.Emit(OpCodes.Ldarg_0);
            textGetIL.Emit(OpCodes.Ldfld, textBldr);
            textGetIL.Emit(OpCodes.Ret);

            var variablesSetPropMthdBldr =
                typeBuilder.DefineMethod("set_Text",
                                           getSetAttr,
                                           null,
                                           new[] { typeof(string) });

            // ReSharper disable once InconsistentNaming
            var textSetIL = variablesSetPropMthdBldr.GetILGenerator();

            textSetIL.Emit(OpCodes.Ldarg_0);
            textSetIL.Emit(OpCodes.Ldarg_1);
            textSetIL.Emit(OpCodes.Stfld, textBldr);
            textSetIL.Emit(OpCodes.Ret);

            textPropBldr.SetGetMethod(textGetPropMthdBldr);
            textPropBldr.SetSetMethod(variablesSetPropMthdBldr);

            typeBuilder.AddInterfaceImplementation(typeof(ITemplateResponse<T>));

            var result = typeBuilder.CreateType();
            return result;
        }
    }
}