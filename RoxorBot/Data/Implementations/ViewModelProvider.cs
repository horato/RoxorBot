using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Input;
using Prism.Commands;
using RoxorBot.Data.Attributes;

namespace RoxorBot.Data.Implementations
{
    public static class ViewModelProvider
    {
        public const string CommandSuffix = "Command";

        private static readonly Dictionary<Assembly, ModuleBuilder> Builders;

        static ViewModelProvider()
        {
            Builders = new Dictionary<Assembly, ModuleBuilder>();
        }

        private static ModuleBuilder GetModuleBuilder(Assembly assembly)
        {
            if (!Builders.ContainsKey(assembly))
                Builders.Add(assembly, CreateNewModuleBuilder());

            return Builders[assembly];
        }

        internal static Type CreateViewModel<T>()
        {
            return CreateViewModel(typeof(T));
        }

        private static ModuleBuilder CreateNewModuleBuilder()
        {
            var assemblyName = new AssemblyName($"RoxorBot.DynamicTypes.{Guid.NewGuid():N}");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assembly.DefineDynamicModule(assemblyName.Name);
        }

        public static Type CreateViewModel(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var typeBuilder = CreateNewTypeBuilder(type);
            CreateConstructors(typeBuilder, type);

            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var containsMyCommandAttribute = method.GetCustomAttribute<CommandAttribute>() != null;
                if (!containsMyCommandAttribute)
                    continue;

                var canMethod = GetCanMethod(method, type);

                var propertyName = $"{method.Name}{CommandSuffix}";
                var field = typeBuilder.DefineField($"_{propertyName}", typeof(ICommand), FieldAttributes.Private);

                var getMethod = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(ICommand), null);
                var ilGenerator = getMethod.GetILGenerator();
                CreateGetterBody(ilGenerator, field, method, canMethod);

                var commandProperty = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, typeof(ICommand), null);
                commandProperty.SetGetMethod(getMethod);
            }

            var newViewModelType = typeBuilder.CreateType();
            return newViewModelType;
        }

        private static MethodInfo GetCanMethod(MethodInfo method, Type type)
        {
            return type.GetMethods().Where(x => x.Name == $"Can{method.Name}").SingleOrDefault(x => !x.GetParameters().Any() && x.ReturnType == typeof(bool));
        }

        private static void CreateConstructors(TypeBuilder typeBuilder, Type type)
        {
            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(CanAccessFromDescendant).ToList();
            if (!ctors.Any())
                throw new InvalidOperationException("No valid constructors found");

            foreach (var baseConstructor in ctors)
                BuildConstructor(typeBuilder, baseConstructor);
        }

        private static ConstructorBuilder BuildConstructor(TypeBuilder type, ConstructorInfo baseConstructor)
        {
            var parameters = baseConstructor.GetParameters();
            var constructorBuilder = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters.Select(x => x.ParameterType).ToArray());
            for (var index = 0; index < parameters.Length; ++index)
                constructorBuilder.DefineParameter(index + 1, parameters[index].Attributes, parameters[index].Name);
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            for (var index = 0; index < parameters.Length; ++index)
                ilGenerator.Emit(OpCodes.Ldarg_S, index + 1);
            ilGenerator.Emit(OpCodes.Call, baseConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            return constructorBuilder;
        }

        public static bool CanAccessFromDescendant(MethodBase method)
        {
            if (!method.IsPublic && !method.IsFamily)
                return method.IsFamilyOrAssembly;
            return true;
        }

        private static TypeBuilder CreateNewTypeBuilder(Type type)
        {
            var moduleBuilder = GetModuleBuilder(type.Assembly);
            var types = new List<Type>();

            var name = $"{type.Name}_{Guid.NewGuid():N}";
            return moduleBuilder.DefineType(name, TypeAttributes.Public, type, types.ToArray());
        }

        private static void CreateGetterBody(ILGenerator ilGenerator, FieldBuilder commandField, MethodInfo methodToExecute, MethodInfo canMethod)
        {
            ilGenerator.DeclareLocal(typeof(ICommand));
            ilGenerator.DeclareLocal(typeof(bool));

            var returnLabel = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Nop);

            //if(command field == null)
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, commandField);
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Ceq);
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Ceq);
            ilGenerator.Emit(OpCodes.Stloc_1);

            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Brtrue_S, returnLabel);

            //command field = new DelegateCommand(new Action(method, canmethod))
            var actionConstructor = CreateActionConstructor();
            var delegateCommandConstructor = CreateDelegateCommandConstructor(canMethod != null);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldftn, methodToExecute);
            ilGenerator.Emit(OpCodes.Newobj, actionConstructor);
            if (canMethod != null)
            {
                var funcBoolConstructor = CreateFuncBoolConstructor();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldftn, canMethod);
                ilGenerator.Emit(OpCodes.Newobj, funcBoolConstructor);
            }
            ilGenerator.Emit(OpCodes.Newobj, delegateCommandConstructor);
            ilGenerator.Emit(OpCodes.Stfld, commandField);

            var endLabel = ilGenerator.DefineLabel();
            //return command field
            ilGenerator.MarkLabel(returnLabel);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, commandField);
            ilGenerator.Emit(OpCodes.Stloc_0);
            ilGenerator.Emit(OpCodes.Br_S, endLabel);

            //function end
            ilGenerator.MarkLabel(endLabel);
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static ConstructorInfo CreateActionConstructor()
        {
            var inputParameters = new[]
            {
                typeof(object),
                typeof(IntPtr)
            };
            return typeof(Action).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, inputParameters, null);
        }

        private static ConstructorInfo CreateDelegateCommandConstructor(bool includeCanMethod)
        {
            var inputParameters = new List<Type>
            {
                typeof(Action)
            };
            if (includeCanMethod)
                inputParameters.Add(typeof(Func<bool>));

            return typeof(DelegateCommand).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, inputParameters.ToArray(), null);
        }

        private static ConstructorInfo CreateFuncBoolConstructor()
        {
            var inputParameters = new[]
            {
                typeof(object),
                typeof(IntPtr)
            };
            return typeof(Func<bool>).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, inputParameters, null);
        }
    }
}
