using System;
using DynamicProxy.Core.Abstract;
using System.Reflection;

namespace DynamicProxy.Core.Factory
{
    public class DynamicProxyFactory : IDynamicProxyFactory
    {
        private readonly IProxyBuilder _proxyBuilder;

        public DynamicProxyFactory()
        {
            _proxyBuilder = new DecoratorProxyBuilder();
        }
        public DynamicProxyFactory(IProxyBuilder proxyBuilder)
        {
            _proxyBuilder = proxyBuilder;
        }
        public Type BuildProxyType(object obj) => BuildProxyType(obj.GetType());
        public Type BuildProxyType<T>() => BuildProxyType(typeof(T));

        public Type BuildProxyType(Type type)
        {
            var proxyType = _proxyBuilder.Build(type);
            return proxyType;
        }
        public object BuildProxyObject(object sourceObject)
        {
            var proxyType = BuildProxyType(sourceObject.GetType());
            var sourceField = proxyType.GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic);
            var proxyObject = Activator.CreateInstance(proxyType);
            sourceField.SetValue(proxyObject, sourceObject);
            return sourceObject;
        }
    }
}
