using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DynamicProxy.Core.Abstract;

namespace DynamicProxy.Core.Factory
{
    public class DecoratorProxyBuilder : IProxyBuilder
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;
        private FieldBuilder _sourceFieldBuilder;
        private Type _interceptorType;
        private Type _baseType;

        public DecoratorProxyBuilder(Type interceptorType = null)
        {
            _interceptorType = interceptorType ?? typeof(DefaultInterceptor);
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(Guid.NewGuid().ToString());
        }

        public Type Build(Type type)
        {
            var interfaces = type.GetInterfaces().ToList();
            if (type.IsInterface)
            {
                interfaces.Add(type);
            }

            _baseType = type;
            _typeBuilder = _moduleBuilder.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public, null, interfaces.ToArray());
            _sourceFieldBuilder = _typeBuilder.DefineField("_source", type, FieldAttributes.Private);

            if (type.IsGenericType)
            {
                var generics = type.GetGenericArguments();
                _typeBuilder.DefineGenericParameters(generics.Select(genericBuilder => genericBuilder.Name).ToArray());
            }

            var members = _baseType.GetInterfaces().SelectMany(baseInterface => baseInterface.GetMembers()).ToList();
            foreach (var member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        if (((MethodBase)member).IsSpecialName) continue;
                        BuildMethod(member as MethodInfo);
                        break;
                    case MemberTypes.Event:
                        BuildEvent(member as EventInfo);
                        break;
                    case MemberTypes.Property:
                        BuildProperty(member as PropertyInfo);
                        break;
                }
            }

            var proxyType = _typeBuilder.CreateType();
            return proxyType;
        }
        private PropertyBuilder BuildProperty(PropertyInfo prototype)
        {
            var propertyBuilder = _typeBuilder.DefineProperty(prototype.Name, prototype.Attributes, prototype.PropertyType, null);
            var propertyGetMethod = prototype.GetGetMethod();
            var propertySetMethod = prototype.GetSetMethod();

            if (propertyGetMethod != null)
            {
                var getMethodBuilder = BuildMethod(propertyGetMethod);
                propertyBuilder.SetGetMethod(getMethodBuilder);
            }

            if (propertySetMethod != null)
            {
                var setMethodBuilder = BuildMethod(propertySetMethod);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }
            return propertyBuilder;
        }
        private EventBuilder BuildEvent(EventInfo prototype)
        {
            var eventBuilder = _typeBuilder.DefineEvent(prototype.Name, EventAttributes.None, prototype.EventHandlerType);

            var addMethod = prototype.GetAddMethod();
            var removeMethod = prototype.GetRemoveMethod();

            var proxyAddMethod = BuildMethod(addMethod);
            var proxyRemoveMethod = BuildMethod(removeMethod);

            eventBuilder.SetAddOnMethod(proxyAddMethod);
            eventBuilder.SetRemoveOnMethod(proxyRemoveMethod);

            return eventBuilder;
        }
        private MethodBuilder BuildMethod(MethodInfo prototype)
        {
            var methodParameters = prototype.GetParameters();
            var methodBuilder = _typeBuilder.DefineMethod(prototype.Name
                    , MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final
                    , prototype.CallingConvention
                    , prototype.ReturnType
                    , prototype.ReturnParameter?.GetRequiredCustomModifiers()
                    , prototype.ReturnParameter?.GetOptionalCustomModifiers()
                    , prototype.GetParameters().Select(parameter => parameter.ParameterType).ToArray()
                    , prototype.GetParameters().Select(parameter => parameter.GetRequiredCustomModifiers()).ToArray()
                    , prototype.GetParameters().Select(parameter => parameter.GetOptionalCustomModifiers()).ToArray());


            var interceptorType = _interceptorType;
            var interceptorCtor = interceptorType.GetConstructor(Type.EmptyTypes);
            var interceptMethod = interceptorType.GetMethod("Intercept");
            var dictionaryCtor = typeof(Dictionary<string, object>).GetConstructor(new Type[0]);
            var dictionaryAddMethod = typeof(Dictionary<string, object>).GetMethod("Add", new[] { typeof(string), typeof(object) });
            var getMethodInfoMethod = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) });
            var getMethod = typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) });
            var ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.DeclareLocal(typeof(MethodInfo));
            ilGenerator.DeclareLocal(_interceptorType);
            ilGenerator.DeclareLocal(typeof(Dictionary<object, string>));
            ilGenerator.Emit(OpCodes.Ldtoken, prototype.DeclaringType);
            ilGenerator.Emit(OpCodes.Ldstr, prototype.Name);
            ilGenerator.Emit(OpCodes.Call, getMethod);
            ilGenerator.Emit(OpCodes.Stloc_0);
            ilGenerator.Emit(OpCodes.Newobj, interceptorCtor);
            ilGenerator.Emit(OpCodes.Stloc_1);
            ilGenerator.Emit(OpCodes.Newobj, dictionaryCtor);
            ilGenerator.Emit(OpCodes.Stloc_2);

            for (var index = 0; index < methodParameters.Length; index++)
            {
                ilGenerator.Emit(OpCodes.Ldloc_2);
                ilGenerator.Emit(OpCodes.Ldstr, methodParameters[index].Name);
                ilGenerator.Emit(OpCodes.Ldarg, index + 1);

                if (methodParameters[index].ParameterType.IsPrimitive)
                {
                    ilGenerator.Emit(OpCodes.Box, methodParameters[index].ParameterType);
                }
                ilGenerator.Emit(OpCodes.Callvirt, dictionaryAddMethod);
            }

            ///* LOAD INPUT PARAMETERS INTO STACK */
            ilGenerator.Emit(OpCodes.Ldloc_1); 
            ilGenerator.Emit(OpCodes.Ldarg_0); 
            ilGenerator.Emit(OpCodes.Ldarg_0); 
            ilGenerator.Emit(OpCodes.Ldfld, _sourceFieldBuilder); 
            ilGenerator.Emit(OpCodes.Ldloc_0); 
            ilGenerator.Emit(OpCodes.Ldloc_2); 
            ilGenerator.Emit(OpCodes.Call, interceptMethod); 

            if (prototype.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Pop);
            }
            else if (prototype.ReturnType.IsPrimitive)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, prototype.ReturnType);
            }

            /* Return from the current method */
            ilGenerator.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }
}